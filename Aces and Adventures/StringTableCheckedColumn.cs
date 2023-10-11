using System;
using Google.Apis.Sheets.v4.Data;
using UnityEngine.Localization.Tables;

[Serializable]
public class StringTableCheckedColumn : StringTableEnumColumn<ItemTranslationChecked, ItemTranslationChecked.Checked>
{
	protected override void _OnCellDataMissing(StringTableEntry entry)
	{
		entry.ClearAutomatedTranslationCheck();
	}

	protected override void _PostProcessPullCellData(StringTableEntry entry, CellData cellData, ItemTranslationChecked meta)
	{
		if (meta.value == ItemTranslationChecked.Checked.Y)
		{
			if (entry.IsManuallyTranslated())
			{
				entry.GetMetadata<TranslationModeMeta>().translationIsOutOfDate = false;
				entry.ClearAutomatedTranslationCheck();
				entry.StringTable().MarkDirtyForPush();
			}
			else
			{
				meta.lastAutomatedTranslation = (entry.Value.HasVisibleCharacter() ? entry.Value : meta.lastAutomatedTranslation);
			}
		}
		else
		{
			entry.ClearAutomatedTranslationCheck();
		}
	}
}
