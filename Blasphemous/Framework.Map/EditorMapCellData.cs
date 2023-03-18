using System;
using UnityEngine;

namespace Framework.Map;

[Serializable]
public class EditorMapCellData
{
	public enum CellType
	{
		Normal,
		PrieDieu,
		Teleport,
		MeaCulpa,
		Soledad,
		Nacimiento,
		Confessor,
		FuenteFlask,
		MiriamPortal
	}

	public enum CellSide
	{
		North,
		South,
		West,
		East
	}

	public bool NGPlus;

	public bool IgnoredForMapPercentage;

	public ZoneKey ZoneId = new ZoneKey();

	[HideInInspector]
	public CellType Type;

	[HideInInspector]
	public bool[] Walls = new bool[4];

	[HideInInspector]
	public bool[] Doors = new bool[4];

	[HideInInspector]
	public bool[] CalculateWalls = new bool[4];

	[HideInInspector]
	public float[] WorldOffset = new float[4];

	[HideInInspector]
	public Rect CalculatedWorldBounding = Rect.zero;

	public EditorMapCellData()
	{
		Walls = new bool[4];
		Doors = new bool[4];
		CalculateWalls = new bool[4];
		WorldOffset = new float[4];
	}

	public EditorMapCellData(EditorMapCellData other)
	{
		ZoneId = new ZoneKey(other.ZoneId);
		Type = other.Type;
		Array.Copy(other.Walls, Walls, 4);
		Array.Copy(other.Doors, Doors, 4);
	}
}
