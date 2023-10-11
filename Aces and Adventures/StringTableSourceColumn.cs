using System;
using System.Collections.Generic;
using Google.Apis.Sheets.v4.Data;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

[Serializable]
public class StringTableSourceColumn : StringTableColumn
{
	public override string horizontalAlignment => "RIGHT";

	public override bool isLocked => true;

	public override bool isPulled => true;

	public override SimpleCellData PushHeader(StringTable table)
	{
		LocaleIdentifier projectLocaleIdentifier = LocalizationUtil.ProjectLocaleIdentifier;
		return new SimpleCellData(projectLocaleIdentifier.ToString());
	}

	public override SimpleCellData PushCellData(StringTableEntry entry)
	{
		SimpleCellData pushedSource = entry.GetSourceEntry().GetPushedSource();
		if (entry.IsManuallyTranslated())
		{
			string lastPulledSource = entry.GetLastPulledSource();
			if (lastPulledSource != null && lastPulledSource != pushedSource.value)
			{
				entry.GetMetadata<TranslationModeMeta>().translationIsOutOfDate = true;
			}
		}
		if (entry.AutomatedTranslationChecked() && !entry.UsesManualTranslation())
		{
			entry.ClearAutomatedTranslationCheck();
		}
		return pushedSource;
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
			if (!item.UsesManualTranslation())
			{
				GridRange range = new GridRange
				{
					StartRowIndex = row,
					EndRowIndex = row + 1,
					StartColumnIndex = columnIndex,
					EndColumnIndex = columnIndex + 1,
					SheetId = sheetId
				};
				yield return GoogleUtil.Sheets.SetBackgroundColorRequest(range, StringTableColumn.HighlightColor);
				yield return GoogleUtil.Sheets.SetTextFormatRequest(range, GoogleUtil.Sheets.NewColor(0f));
			}
		}
	}

	public override void PullCellData(StringTableEntry entry, CellData cellData)
	{
		entry.SetLastPulledSource(cellData);
	}
}
