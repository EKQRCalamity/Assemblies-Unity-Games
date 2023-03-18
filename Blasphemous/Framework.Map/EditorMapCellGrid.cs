using System;
using System.Collections.Generic;
using Sirenix.Serialization;

namespace Framework.Map;

[Serializable]
public class EditorMapCellGrid
{
	[OdinSerialize]
	public Dictionary<CellKey, EditorMapCellData> CellsDict = new Dictionary<CellKey, EditorMapCellData>();

	public EditorMapCellData this[CellKey index]
	{
		get
		{
			EditorMapCellData value = null;
			if (index != null)
			{
				CellsDict.TryGetValue(index, out value);
			}
			return value;
		}
		set
		{
			CellsDict[index] = value;
		}
	}
}
