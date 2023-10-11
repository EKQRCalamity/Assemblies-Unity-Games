using System;
using System.Linq;
using Google.Apis.Sheets.v4.Data;
using UnityEngine.Localization.Tables;

[Serializable]
public abstract class StringTableEnumColumn<M, E> : StringTableColumn where M : class, IMetadataEnum<E> where E : struct, IConvertible
{
	private static int? _Width;

	private static int Width
	{
		get
		{
			int valueOrDefault = _Width.GetValueOrDefault();
			if (!_Width.HasValue)
			{
				valueOrDefault = EnumUtil<E>.Values.Select(EnumUtil.Name).Concat(typeof(E).GetUILabel()).MaxBy((string s) => s.Length)
					.Length * 9;
				_Width = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public override bool usesTextWrapping => false;

	public override int width => Width;

	protected virtual void _OnCellDataMissing(StringTableEntry entry)
	{
	}

	protected virtual void _PostProcessPullCellData(StringTableEntry entry, CellData cellData, M meta)
	{
	}

	public override SimpleCellData PushHeader(StringTable table)
	{
		return new SimpleCellData(typeof(E).GetUILabel(), typeof(E).GetAttribute<UIFieldAttribute>()?.tooltip);
	}

	public override SimpleCellData PushCellData(StringTableEntry entry)
	{
		return new SimpleCellData(EnumUtil.Name(entry.GetMetadata<M>()?.value));
	}

	public override void PullCellData(StringTableEntry entry, CellData cellData)
	{
		if (!cellData.HasValue())
		{
			_OnCellDataMissing(entry);
			return;
		}
		M orAddMetadata = entry.GetOrAddMetadata<M>();
		orAddMetadata.value = EnumUtil<E>.ParseToNearest(cellData.Value());
		_PostProcessPullCellData(entry, cellData, orAddMetadata);
	}
}
