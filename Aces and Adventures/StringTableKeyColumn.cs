using System;
using Google.Apis.Sheets.v4.Data;
using UnityEngine.Localization.Tables;

[Serializable]
public class StringTableKeyColumn : StringTableColumn
{
	public override bool isLocked => true;

	public override bool usesTextWrapping => false;

	public override int width => 300;

	public override SimpleCellData PushHeader(StringTable table)
	{
		return new SimpleCellData("Key");
	}

	public override SimpleCellData PushCellData(StringTableEntry entry)
	{
		return new SimpleCellData(entry.Key, entry.KeyId.ToString());
	}

	public override void PullCellData(StringTableEntry entry, CellData cellData)
	{
	}
}
