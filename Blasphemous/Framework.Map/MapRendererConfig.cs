using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.Map;

[CreateAssetMenu(menuName = "Blasphemous/Map Renderer config")]
public class MapRendererConfig : SerializedScriptableObject
{
	[NonSerialized]
	[HideInInspector]
	public static List<EditorMapCellData.CellSide> spriteKeyOrder = new List<EditorMapCellData.CellSide>
	{
		EditorMapCellData.CellSide.West,
		EditorMapCellData.CellSide.North,
		EditorMapCellData.CellSide.East,
		EditorMapCellData.CellSide.South
	};

	[NonSerialized]
	[HideInInspector]
	public static List<string> NeededSprites = new List<string>
	{
		"____", "_W__", "_D__", "W___", "D___", "WW__", "DW__", "WD__", "DD__", "W_W_",
		"D_W_", "D_D_", "_W_W", "_D_W", "_D_D", "WWW_", "DWW_", "WDW_", "DDW_", "DWD_",
		"DDD_", "WW_W", "DW_W", "WD_W", "DD_W", "WD_D", "DD_D", "WWWW", "DWWW", "WDWW",
		"DDWW", "DWDW", "WDWD", "DDDW", "DDWD", "DDDD"
	};

	[HideInInspector]
	public Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

	public Material SpriteMaterial;

	public Material SpriteMaterialSelected;

	public Color PlayerBackgoundColor = new Color(0f, 0f, 0f);

	[BoxGroup("Movement", true, false, 0)]
	[InfoBox("In pixels per second", InfoMessageType.Info, null)]
	public float MovementSpeed = 100f;

	[BoxGroup("Movement", true, false, 0)]
	public bool CenterAtPlayer = true;

	[HideIf("CenterAtPlayer", true)]
	[BoxGroup("Movement", true, false, 0)]
	public Vector2Int centerCell = default(Vector2Int);

	[BoxGroup("Movement", true, false, 0)]
	public Vector2Int minCell = default(Vector2Int);

	[BoxGroup("Movement", true, false, 0)]
	public Vector2Int maxCell = default(Vector2Int);

	[HideInInspector]
	public Dictionary<string, Color> ZonesColor = new Dictionary<string, Color>();

	[BoxGroup("Marks", true, false, 0)]
	public bool UseMarks = true;

	[BoxGroup("Marks", true, false, 0)]
	[ShowIf("UseMarks", true)]
	public Dictionary<MapData.MarkType, Sprite> Marks = new Dictionary<MapData.MarkType, Sprite>();

	public MapRendererConfig()
	{
		foreach (string neededSprite in NeededSprites)
		{
			if (!Sprites.ContainsKey(neededSprite))
			{
				Sprites[neededSprite] = null;
			}
		}
		foreach (object value in Enum.GetValues(typeof(MapData.MarkType)))
		{
			if (!Marks.ContainsKey((MapData.MarkType)value))
			{
				Marks[(MapData.MarkType)value] = null;
			}
		}
	}

	public Color GetZoneColor(ZoneKey zoneKey)
	{
		Color result = Color.white;
		if (ZonesColor.ContainsKey(zoneKey.GetLocalizationKey()))
		{
			result = ZonesColor[zoneKey.GetLocalizationKey()];
		}
		return result;
	}
}
