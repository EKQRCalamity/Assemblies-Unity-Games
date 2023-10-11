using System;
using System.Collections.Generic;
using Google.Apis.Sheets.v4.Data;
using UnityEngine.Localization.Tables;

[Serializable]
public class StringTableTargetColumn : StringTableColumn
{
	public override bool useBandedRange => false;

	public override SimpleCellData PushHeader(StringTable table)
	{
		return new SimpleCellData(table.LocaleIdentifier.ToString());
	}

	public override SimpleCellData PushCellData(StringTableEntry entry)
	{
		return new SimpleCellData(entry.Value, entry.GetComment(fallbackToSharedEntry: true) ?? entry.GetContextComment());
	}

	public override IEnumerable<Request> GetPushRequests(StringTable table)
	{
		foreach (Request pushRequest in base.GetPushRequests(table))
		{
			yield return pushRequest;
		}
		int row = 0;
		int sheetId = table.SheetId();
		foreach (StringTableEntry item in table.EntriesOrderedByKey())
		{
			int num = row + 1;
			row = num;
			TranslationModeMeta metadata = item.GetMetadata<TranslationModeMeta>();
			if (metadata != null && metadata.value == TranslationMode.Manual)
			{
				yield return GoogleUtil.Sheets.SetBackgroundColorRequest(new GridRange
				{
					StartRowIndex = row,
					EndRowIndex = row + 1,
					StartColumnIndex = columnIndex,
					EndColumnIndex = columnIndex + 1,
					SheetId = sheetId
				}, metadata.translationIsOutOfDate ? StringTableColumn.ErrorColor : StringTableColumn.HighlightColor);
			}
			else if (!item.UsesManualTranslation() || item.AutomatedTranslationChecked())
			{
				yield return GoogleUtil.Sheets.SetBackgroundColorRequest(new GridRange
				{
					StartRowIndex = row,
					EndRowIndex = row + 1,
					StartColumnIndex = columnIndex,
					EndColumnIndex = columnIndex + 1,
					SheetId = sheetId
				}, StringTableColumn.HighlightColor);
			}
		}
	}

	public override void PullCellData(StringTableEntry entry, CellData cellData)
	{
		string text = cellData.Value();
		if (!(entry.Value == text))
		{
			entry.Value = text;
			entry.UpdateIsSmart();
			TranslationModeMeta orAddMetadata = entry.GetOrAddMetadata<TranslationModeMeta>();
			orAddMetadata.value = (entry.Value.HasVisibleCharacter() ? TranslationMode.Manual : ((orAddMetadata.value != TranslationMode.Manual) ? orAddMetadata.value : TranslationMode.Auto));
			if (orAddMetadata.value == TranslationMode.Manual)
			{
				orAddMetadata.translationIsOutOfDate = false;
				entry.ClearAutomatedTranslationCheck();
			}
		}
	}
}
