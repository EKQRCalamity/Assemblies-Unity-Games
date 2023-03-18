using System;
using UnityEngine;

namespace Framework.Map;

public class CellData
{
	public EditorMapCellData.CellType Type;

	public CellKey CellKey;

	public Rect Bounding = Rect.zero;

	public ZoneKey ZoneId;

	public bool Revealed;

	public bool NGPlus;

	public bool IgnoredForMapPercentage;

	public bool[] Walls = new bool[4];

	public bool[] Doors = new bool[4];

	public CellData(CellKey cellKey, EditorMapCellData editor)
	{
		CellKey = cellKey;
		Type = editor.Type;
		Bounding = editor.CalculatedWorldBounding;
		ZoneId = new ZoneKey(editor.ZoneId);
		NGPlus = editor.NGPlus;
		IgnoredForMapPercentage = editor.IgnoredForMapPercentage;
		Array.Copy(editor.Doors, Doors, 4);
		Array.Copy(editor.CalculateWalls, Walls, 4);
		Revealed = false;
	}
}
