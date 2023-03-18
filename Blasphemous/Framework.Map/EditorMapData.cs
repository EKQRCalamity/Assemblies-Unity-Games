using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Framework.Map;

[CreateAssetMenu(menuName = "Blasphemous/Map Data")]
public class EditorMapData : SerializedScriptableObject
{
	public enum EditorModeType
	{
		Logic,
		World,
		Secrets
	}

	public enum OnClickCell
	{
		Select,
		Create,
		ChangeZone,
		ChangeWalls,
		ChangeAll,
		Delete
	}

	[Serializable]
	public class MyDistrictHelper
	{
		public string ZoneName;

		public bool Show = true;
	}

	[NonSerialized]
	[HideInInspector]
	[OdinSerialize]
	public EditorMapCellGrid Cells = new EditorMapCellGrid();

	[NonSerialized]
	[HideInInspector]
	[OdinSerialize]
	public Dictionary<string, EditorMapCellGrid> Secrets = new Dictionary<string, EditorMapCellGrid>();

	[OnValueChanged("SizeChanged", false)]
	public int GridSizeX;

	[OnValueChanged("SizeChanged", false)]
	public int GridSizeY;

	public Vector2 GlobalOffset = Vector2.zero;

	public Vector2 CellSize = Vector2.zero;

	[EnumToggleButtons]
	public EditorModeType EditorMode;

	[EnumToggleButtons]
	[LabelText("Click On Cell")]
	public OnClickCell OnClickCellkBehaviour;

	[LabelText("")]
	public EditorMapCellData NewCell = new EditorMapCellData();

	[LabelText("Selected Cell")]
	public EditorMapCellData SelectedCell;

	[HideInInspector]
	public bool inspectorShowGlobalData = true;

	public Dictionary<string, Vector2> DistrictOffset = new Dictionary<string, Vector2>();

	[HideInInspector]
	public bool inspectorShowDistrictShow = true;

	[OdinSerialize]
	[HideInInspector]
	public List<MyDistrictHelper> DistrictShowed = new List<MyDistrictHelper>();

	public void RestoreDefaults()
	{
		SelectedCell = null;
		EditorMode = EditorModeType.Logic;
		OnClickCellkBehaviour = OnClickCell.Select;
		if (Secrets == null)
		{
			Secrets = new Dictionary<string, EditorMapCellGrid>();
		}
	}

	public void AddNewSecret(string idSecret)
	{
		Secrets[idSecret] = new EditorMapCellGrid();
	}

	private void SizeChanged()
	{
	}
}
