using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Sheets.v4.Data;
using UnityEngine.Localization.Tables;

[Serializable]
public abstract class StringTableColumn
{
	public static readonly Color HighlightColor = GoogleUtil.Sheets.NewColor(1f, 1f, 0.83f);

	public static readonly Color ErrorColor = GoogleUtil.Sheets.NewColor(1f, 0.8f, 0.8f);

	public int columnIndex;

	public virtual bool isLocked => false;

	public virtual bool isPulled => true;

	public virtual bool isPushed => true;

	public virtual int width => 900;

	public virtual bool useBandedRange => true;

	public virtual bool usesTextWrapping => true;

	public virtual string horizontalAlignment => "LEFT";

	public virtual string verticalAlignment => "TOP";

	public abstract SimpleCellData PushHeader(StringTable table);

	public abstract SimpleCellData PushCellData(StringTableEntry entry);

	public abstract void PullCellData(StringTableEntry entry, CellData cellData);

	public IEnumerable<SimpleCellData> GetPushedSimpleCellData(StringTable table)
	{
		yield return PushHeader(table);
		foreach (StringTableEntry item in table.EntriesOrderedByKey())
		{
			yield return PushCellData(item);
		}
	}

	public IEnumerable<CellData> GetPushedCellData(StringTable table)
	{
		foreach (SimpleCellData pushedSimpleCellDatum in GetPushedSimpleCellData(table))
		{
			yield return pushedSimpleCellDatum.ToCellData();
		}
	}

	public virtual IEnumerable<Request> GetPushRequests(StringTable table)
	{
		yield return GoogleUtil.Sheets.SetCellDataByColumnRequest(table.SheetId(), columnIndex, GetPushedCellData(table), "userEnteredValue,note");
		GridRange range = GetGridRange(table);
		GridRange headerRange = GetHeaderRange(table);
		if (isLocked)
		{
			foreach (Request item in GoogleUtil.Sheets.AddProtectedRangeRequests(range, LocalizationUtil.USERS))
			{
				yield return item;
			}
			if (useBandedRange)
			{
				yield return GoogleUtil.Sheets.AddBandingRequest(range, GoogleUtil.Sheets.NewColor(0.25f), GoogleUtil.Sheets.NewColor(0.4f), GoogleUtil.Sheets.NewColor(0.1f));
			}
			yield return GoogleUtil.Sheets.SetTextFormatRequest(range, GoogleUtil.Sheets.NewColor(1f));
			yield return GoogleUtil.Sheets.UpdateBordersRequest(range, new Border
			{
				Color = GoogleUtil.Sheets.NewColor(0.75f),
				Width = 1,
				Style = "SOLID"
			});
		}
		else
		{
			foreach (Request item2 in GoogleUtil.Sheets.AddProtectedRangeRequests(headerRange, LocalizationUtil.USERS))
			{
				yield return item2;
			}
			if (useBandedRange)
			{
				yield return GoogleUtil.Sheets.AddBandingRequest(range, GoogleUtil.Sheets.NewColor(1f), GoogleUtil.Sheets.NewColor(0.9f), GoogleUtil.Sheets.NewColor(0.1f));
			}
			else
			{
				yield return GoogleUtil.Sheets.SetBackgroundColorRequest(headerRange, GoogleUtil.Sheets.NewColor(0.1f));
			}
		}
		yield return GoogleUtil.Sheets.UpdateDimensionRequest(GetDimensionRange(table), (int)Math.Round((float)width * (2105f / (float)table.GetColumns().Sum((StringTableColumn c) => c.width))));
		yield return GoogleUtil.Sheets.SetTextHorizontalAlignmentRequest(range, horizontalAlignment);
		yield return GoogleUtil.Sheets.SetTextVerticalAlignmentRequest(range, verticalAlignment);
		yield return GoogleUtil.Sheets.SetTextFormatRequest(headerRange, GoogleUtil.Sheets.NewColor(1f), true);
		if (usesTextWrapping)
		{
			GoogleUtil.Sheets.WrapStrategy? wrapMode2 = table.GetWrapMode();
			if (wrapMode2.HasValue)
			{
				GoogleUtil.Sheets.WrapStrategy wrapMode = wrapMode2.GetValueOrDefault();
				yield return GoogleUtil.Sheets.SetTextWrapping(range, wrapMode);
			}
		}
	}

	public GridRange GetGridRange(StringTable table, bool includeHeader = true)
	{
		return new GridRange
		{
			SheetId = table.SheetId(),
			StartColumnIndex = columnIndex,
			EndColumnIndex = columnIndex + 1,
			StartRowIndex = ((!includeHeader) ? 1 : 0),
			EndRowIndex = table.Count + includeHeader.ToInt()
		};
	}

	public GridRange GetHeaderRange(StringTable table)
	{
		return new GridRange
		{
			SheetId = table.SheetId(),
			StartColumnIndex = columnIndex,
			EndColumnIndex = columnIndex + 1,
			StartRowIndex = 0,
			EndRowIndex = 1
		};
	}

	public DimensionRange GetDimensionRange(StringTable table)
	{
		return new DimensionRange
		{
			Dimension = "COLUMNS",
			StartIndex = columnIndex,
			EndIndex = columnIndex + 1,
			SheetId = table.SheetId()
		};
	}
}
