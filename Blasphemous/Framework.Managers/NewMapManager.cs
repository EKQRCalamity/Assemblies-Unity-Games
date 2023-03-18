using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Framework.FrameworkCore;
using Framework.Map;
using Gameplay.UI;
using I2.Loc;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.Managers;

public class NewMapManager : GameSystem, PersistentInterface
{
	[Serializable]
	public class NewMapPersistenceDataItem
	{
		public List<CellKey> RevealedCells;

		public List<string> Secrets;

		public Dictionary<CellKey, MapData.MarkType> Marks;

		public NewMapPersistenceDataItem()
		{
			RevealedCells = new List<CellKey>();
			Secrets = new List<string>();
			Marks = new Dictionary<CellKey, MapData.MarkType>();
		}
	}

	[Serializable]
	public class NewMapPersistenceData : PersistentManager.PersistentData
	{
		public string currentMapId;

		public Dictionary<string, NewMapPersistenceDataItem> Maps = new Dictionary<string, NewMapPersistenceDataItem>();

		public NewMapPersistenceData()
			: base("ID_NEW_MAPS")
		{
		}
	}

	private ZoneKey LastScene = new ZoneKey();

	private Dictionary<string, string> DistrictLocalization = new Dictionary<string, string>();

	private Dictionary<string, string> ZonesLocalization = new Dictionary<string, string>();

	private const string REG_EXP = "^D(?<district>[0-9]{2})Z(?<zone>[0-9]{2})S(?<scene>[0-9]{2})$";

	private const int TOTAL_NUMBER_OF_ZONES_FOR_AC21 = 23;

	private const string MAP_DIRECTORY = "New Maps/";

	private const string AC21_CONFIG_PATH = "New Maps/ZonesAC21";

	private ZonesAC21 ZonesForAC21;

	private string CurrentLanguage = string.Empty;

	private Dictionary<string, MapData> Maps = new Dictionary<string, MapData>();

	private MapData CurrentMap;

	private CellData LastPlayerCell;

	private const string PERSITENT_ID = "ID_NEW_MAPS";

	public ZoneKey CurrentScene { get; private set; }

	public ZoneKey CurrentSafeScene { get; private set; }

	public string DefaultMapID { get; private set; }

	public override void Initialize()
	{
		CreateLocaliztionDicts();
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLocalizationChange;
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		CurrentLanguage = string.Empty;
		CurrentScene = new ZoneKey();
		LoadAllMaps();
		ZonesForAC21 = Resources.Load<ZonesAC21>("New Maps/ZonesAC21");
		OnLocalizationChange();
	}

	public override void Dispose()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLocalizationChange;
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		string levelName = newLevel.LevelName;
		if (levelName == "NONE")
		{
			return;
		}
		ZoneKey zoneKey = new ZoneKey();
		if (GetZoneKeyFromBundleName(levelName, ref zoneKey) && Core.Logic.CurrentLevelConfig.ShowZoneTitle(oldLevel) && CurrentScene.GetLocalizationKey() != zoneKey.GetLocalizationKey())
		{
			string zoneName = GetZoneName(zoneKey);
			if (zoneName != string.Empty && Core.LevelManager.InCinematicsChangeLevel == LevelManager.CinematicsChangeLevel.No)
			{
				CurrentSafeScene = zoneKey;
				DisplayZoneName(zoneName);
				AddProgressToAC21(zoneKey);
			}
		}
		CurrentScene = zoneKey;
	}

	public override void AllInitialized()
	{
		Core.Persistence.AddPersistentManager(this);
	}

	public string GetCurrentDistrictName()
	{
		return GetDistrictName(CurrentScene.District);
	}

	public string GetCurrentZoneName()
	{
		return GetZoneName(CurrentScene);
	}

	public bool CanShowMapInCurrentZone()
	{
		bool result = false;
		if (CurrentScene != null && !GameConstants.DistrictsWithoutName.Contains(CurrentScene.District))
		{
			string localizationKey = CurrentScene.GetLocalizationKey();
			result = ZonesLocalization.ContainsKey(localizationKey);
		}
		return result;
	}

	public string GetDistrictName(string district)
	{
		string result = "![NO_LOC_" + district + "]";
		if (GameConstants.DistrictsWithoutName.Contains(district))
		{
			result = string.Empty;
		}
		else if (DistrictLocalization.ContainsKey(district))
		{
			result = DistrictLocalization[district];
		}
		return result;
	}

	public string GetZoneName(ZoneKey sceneKey)
	{
		string localizationKey = sceneKey.GetLocalizationKey();
		string text = "![ERROR NO KEY " + localizationKey + "]";
		if (GameConstants.DistrictsWithoutName.Contains(sceneKey.District))
		{
			text = string.Empty;
		}
		else if (ZonesLocalization.ContainsKey(localizationKey))
		{
			text = ZonesLocalization[localizationKey];
			if (text.Trim().Length == 0)
			{
				text = "[!ERROR NO LOC FOR " + localizationKey + "]";
			}
		}
		return text;
	}

	public bool ZoneHasName(ZoneKey sceneKey)
	{
		string localizationKey = sceneKey.GetLocalizationKey();
		return !GameConstants.DistrictsWithoutName.Contains(sceneKey.District) && ZonesLocalization.ContainsKey(localizationKey) && ZonesLocalization[localizationKey].Trim().Length > 0;
	}

	public string GetZoneNameFromBundle(string bundle)
	{
		string result = string.Empty;
		ZoneKey zoneKey = new ZoneKey();
		if (GetZoneKeyFromBundleName(bundle, ref zoneKey))
		{
			result = GetZoneName(zoneKey);
		}
		return result;
	}

	public void DisplayZoneName()
	{
		AddProgressToAC21(CurrentScene);
		DisplayZoneName(GetCurrentZoneName());
	}

	public void DisplayZoneName(string zone)
	{
		UIController.instance.ShowAreaPopUp(zone);
	}

	public List<SecretData> GetAllSecrets()
	{
		if (CurrentMap != null)
		{
			return CurrentMap.Secrets.Values.ToList();
		}
		return new List<SecretData>();
	}

	public bool SetSecret(string secretId, bool enable)
	{
		bool result = false;
		if (CurrentMap != null)
		{
			result = SetSecret(CurrentMap.Name, secretId, enable);
		}
		return result;
	}

	public bool SetSecret(string mapId, string secretId, bool enabled)
	{
		bool result = false;
		if (Maps.ContainsKey(mapId))
		{
			MapData mapData = Maps[mapId];
			if (mapData.Secrets.ContainsKey(secretId))
			{
				result = enabled != mapData.Secrets[secretId].Revealed;
				mapData.Secrets[secretId].Revealed = enabled;
			}
		}
		return result;
	}

	public int GetTotalCells()
	{
		int result = 0;
		if (CurrentMap != null)
		{
			List<CellData> cells = CurrentMap.Cells;
			if (Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.NEW_GAME))
			{
				cells.RemoveAll((CellData cell) => cell.NGPlus);
			}
			IEnumerable<CellData> source = cells.Where((CellData cell) => !cell.IgnoredForMapPercentage);
			result = source.Count();
		}
		return result;
	}

	public CellKey GetPlayerCell()
	{
		if (LastPlayerCell != null)
		{
			return LastPlayerCell.CellKey;
		}
		return null;
	}

	public CellKey GetCellKeyFromPosition(string scene, Vector2 position)
	{
		CellKey result = null;
		ZoneKey zoneKey = new ZoneKey();
		if (GetZoneKeyFromBundleName(scene, ref zoneKey) && CurrentMap.CellsByZone.ContainsKey(zoneKey))
		{
			foreach (CellData item in CurrentMap.CellsByZone[zoneKey])
			{
				if (item.Bounding.Contains(position))
				{
					return new CellKey(item.CellKey);
				}
			}
			return result;
		}
		return result;
	}

	public CellKey GetCellKeyFromPosition(Vector2 position)
	{
		CellKey result = null;
		if (CurrentMap != null)
		{
			foreach (CellData cell in Maps[DefaultMapID].Cells)
			{
				if (cell.Bounding.Contains(position))
				{
					return cell.CellKey;
				}
			}
			return result;
		}
		return result;
	}

	public List<CellData> GetAllRevealedCells()
	{
		return GetAllRevealedCells(CurrentMap);
	}

	public List<CellKey> GetAllRevealSecretsCells()
	{
		List<CellKey> list = new List<CellKey>();
		foreach (SecretData item in CurrentMap.Secrets.Values.Where((SecretData x) => x.Revealed))
		{
			foreach (CellKey key in item.Cells.Keys)
			{
				list.Add(key);
			}
		}
		return list;
	}

	public List<CellData> GetAllRevealedCells(string mapId)
	{
		MapData map = null;
		if (Maps.ContainsKey(mapId))
		{
			map = Maps[mapId];
		}
		return GetAllRevealedCells(map);
	}

	public List<string> GetAllMaps()
	{
		return Maps.Keys.ToList();
	}

	public string GetCurrentMap()
	{
		string result = string.Empty;
		if (CurrentMap != null)
		{
			result = CurrentMap.Name;
		}
		return result;
	}

	public bool SetCurrentMap(string mapId)
	{
		if (Maps.ContainsKey(mapId))
		{
			CurrentMap = Maps[mapId];
			return true;
		}
		return false;
	}

	public void RevealCellInCurrentPlayerPosition()
	{
		if (Core.Logic.Penitent != null)
		{
			RevealCellInPosition(Core.Logic.Penitent.transform.position);
		}
	}

	public void RevealCellInPosition(Vector2 position)
	{
		if (CurrentMap == null || !CurrentMap.CellsByZone.ContainsKey(CurrentScene))
		{
			return;
		}
		bool flag = false;
		foreach (CellData item in CurrentMap.CellsByZone[CurrentScene])
		{
			if (item.Bounding.Contains(position))
			{
				flag = flag || !item.Revealed;
				item.Revealed = true;
				LastPlayerCell = item;
			}
		}
		if (flag)
		{
			Core.AchievementsManager.CheckProgressToAC46();
		}
	}

	public void RevealAllMap()
	{
		foreach (CellData cell in CurrentMap.Cells)
		{
			cell.Revealed = true;
		}
		Core.AchievementsManager.CheckProgressToAC46();
	}

	public void RevealAllNGMap()
	{
		foreach (CellData cell in CurrentMap.Cells)
		{
			if (!cell.NGPlus)
			{
				cell.Revealed = true;
			}
		}
		Core.AchievementsManager.CheckProgressToAC46();
	}

	public List<CellData> GetUnrevealedCellsForCompletion()
	{
		bool isNGMap = Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.NEW_GAME_PLUS);
		return CurrentMap.Cells.Where((CellData c) => !c.Revealed && c.NGPlus == isNGMap && !c.IgnoredForMapPercentage).ToList();
	}

	public void RevealAllDistrict(string district)
	{
		IEnumerable<CellData> enumerable = CurrentMap.Cells.Where((CellData p) => p.ZoneId.District == district);
		foreach (CellData item in enumerable)
		{
			item.Revealed = true;
		}
		Core.AchievementsManager.CheckProgressToAC46();
	}

	public void RevealAllZone(string district, string zone)
	{
		IEnumerable<CellData> enumerable = CurrentMap.Cells.Where((CellData p) => p.ZoneId.District == district && p.ZoneId.Zone == zone);
		foreach (CellData item in enumerable)
		{
			item.Revealed = true;
		}
		Core.AchievementsManager.CheckProgressToAC46();
	}

	public float GetPercentageCompletition()
	{
		float num = 0f;
		if (CurrentMap != null)
		{
			num = GetCellPercentage(forNgPlus: false) * GameConstants.PercentageValues[PersistentManager.PercentageType.Map];
			if (Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.NEW_GAME_PLUS))
			{
				float num2 = GetCellPercentage(forNgPlus: true) * GameConstants.PercentageValues[PersistentManager.PercentageType.Map_NgPlus];
				num += num2;
			}
		}
		return num;
	}

	public bool CanAddMark(CellKey key)
	{
		bool result = false;
		if (CurrentMap != null && CurrentMap.CellsByCellKey.ContainsKey(key))
		{
			CellData cellData = CurrentMap.CellsByCellKey[key];
			result = cellData.Revealed && cellData.Type == EditorMapCellData.CellType.Normal && !CurrentMap.Marks.ContainsKey(key);
		}
		return result;
	}

	public bool IsMarkOnCell(CellKey key)
	{
		bool result = false;
		if (CurrentMap != null && CurrentMap.Marks.ContainsKey(key))
		{
			result = !MapData.MarkPrivate.Contains(CurrentMap.Marks[key]);
		}
		return result;
	}

	public bool GetMarkOnCell(CellKey key, ref MapData.MarkType type)
	{
		bool result = false;
		if (CurrentMap != null && CurrentMap.Marks.ContainsKey(key))
		{
			result = true;
			type = CurrentMap.Marks[key];
		}
		return result;
	}

	public bool GetCellType(CellKey key, ref EditorMapCellData.CellType type)
	{
		bool result = false;
		if (CurrentMap != null && CurrentMap.CellsByCellKey.ContainsKey(key))
		{
			result = true;
			type = CurrentMap.CellsByCellKey[key].Type;
		}
		return result;
	}

	public Dictionary<CellKey, List<MapData.MarkType>> GetAllMarks()
	{
		Dictionary<CellKey, List<MapData.MarkType>> dictionary = new Dictionary<CellKey, List<MapData.MarkType>>();
		if (CurrentMap != null)
		{
			foreach (GuiltManager.GuiltDrop allCurrentMapDrop in Core.GuiltManager.GetAllCurrentMapDrops())
			{
				if (allCurrentMapDrop != null && allCurrentMapDrop.cellKey != null)
				{
					dictionary[allCurrentMapDrop.cellKey] = new List<MapData.MarkType>();
					dictionary[allCurrentMapDrop.cellKey].Add(MapData.MarkType.Guilt);
				}
				else
				{
					Core.GuiltManager.ResetGuilt(restoreDropTears: true);
				}
			}
			Dictionary<CellKey, EditorMapCellData.CellType> dictionary2 = new Dictionary<CellKey, EditorMapCellData.CellType>();
			foreach (SecretData item in CurrentMap.Secrets.Values.Where((SecretData x) => x.Revealed))
			{
				foreach (CellData value in item.Cells.Values)
				{
					dictionary2[value.CellKey] = value.Type;
				}
			}
			IEnumerable<CellData> enumerable = CurrentMap.Cells.Where((CellData p) => p.Revealed);
			foreach (CellData item2 in enumerable)
			{
				EditorMapCellData.CellType cellType = item2.Type;
				if (dictionary2.ContainsKey(item2.CellKey))
				{
					cellType = dictionary2[item2.CellKey];
				}
				if (cellType != 0)
				{
					if (!dictionary.ContainsKey(item2.CellKey))
					{
						dictionary[item2.CellKey] = new List<MapData.MarkType>();
					}
					switch (cellType)
					{
					case EditorMapCellData.CellType.PrieDieu:
						dictionary[item2.CellKey].Add(MapData.MarkType.PrieDieu);
						break;
					case EditorMapCellData.CellType.Teleport:
						dictionary[item2.CellKey].Add(MapData.MarkType.Teleport);
						break;
					case EditorMapCellData.CellType.MeaCulpa:
						dictionary[item2.CellKey].Add(MapData.MarkType.MeaCulpa);
						break;
					case EditorMapCellData.CellType.Soledad:
						dictionary[item2.CellKey].Add(MapData.MarkType.Soledad);
						break;
					case EditorMapCellData.CellType.Nacimiento:
						dictionary[item2.CellKey].Add(MapData.MarkType.Nacimiento);
						break;
					case EditorMapCellData.CellType.Confessor:
						dictionary[item2.CellKey].Add(MapData.MarkType.Confessor);
						break;
					case EditorMapCellData.CellType.FuenteFlask:
						dictionary[item2.CellKey].Add(MapData.MarkType.FuenteFlask);
						break;
					case EditorMapCellData.CellType.MiriamPortal:
						dictionary[item2.CellKey].Add(MapData.MarkType.MiriamPortal);
						break;
					}
				}
			}
			{
				foreach (KeyValuePair<CellKey, MapData.MarkType> mark in CurrentMap.Marks)
				{
					if (!dictionary.ContainsKey(mark.Key))
					{
						dictionary[mark.Key] = new List<MapData.MarkType>();
					}
					dictionary[mark.Key].Add(mark.Value);
				}
				return dictionary;
			}
		}
		return dictionary;
	}

	public bool AddMarkOnCell(CellKey key, MapData.MarkType type)
	{
		bool flag = CanAddMark(key);
		if (flag)
		{
			CurrentMap.Marks[key] = type;
		}
		return flag;
	}

	public bool RemoveMarkOnCell(CellKey key)
	{
		bool result = false;
		if (CurrentMap != null)
		{
			result = CurrentMap.Marks.Remove(key);
		}
		return result;
	}

	public MapData DEBUG_GetCurrentMap()
	{
		return CurrentMap;
	}

	private float GetCellPercentage(bool forNgPlus)
	{
		float result = 0f;
		IEnumerable<CellData> source = CurrentMap.Cells.Where((CellData cell) => cell.NGPlus == forNgPlus && !cell.IgnoredForMapPercentage);
		float num = source.Count();
		if (num > 0f)
		{
			result = (float)source.Count((CellData cell) => cell.Revealed) / num;
		}
		return result;
	}

	private void AddProgressToAC21(ZoneKey zone)
	{
		string id = "ZONE_NAME_OF_" + zone.District.ToUpper() + zone.Zone.ToUpper() + "_DISPLAYED";
		if (ZonesForAC21.AllowZoneForAc21(zone) && !Core.Events.GetFlag(id))
		{
			Core.Events.SetFlag(id, b: true, forcePreserve: true);
			Core.AchievementsManager.Achievements["AC21"].AddProgress(4.347826f);
		}
	}

	private bool GetZoneKeyFromBundleName(string bundle, ref ZoneKey zoneKey)
	{
		Regex regex = new Regex("^D(?<district>[0-9]{2})Z(?<zone>[0-9]{2})S(?<scene>[0-9]{2})$");
		Match match = regex.Match(bundle);
		if (match.Success)
		{
			string district = "D" + match.Groups["district"].Value;
			string zone = "Z" + match.Groups["zone"].Value;
			string scene = "S" + match.Groups["scene"].Value;
			zoneKey = new ZoneKey(district, zone, scene);
		}
		return match.Success;
	}

	private void LoadAllMaps()
	{
		Maps.Clear();
		CurrentMap = null;
		LastPlayerCell = null;
		CurrentSafeScene = new ZoneKey();
		EditorMapData[] array = Resources.LoadAll<EditorMapData>("New Maps/");
		EditorMapData[] array2 = array;
		foreach (EditorMapData editorMapData in array2)
		{
			MapData mapData = new MapData();
			mapData.Name = editorMapData.name;
			mapData.Cells = new List<CellData>();
			mapData.Secrets = new Dictionary<string, SecretData>();
			mapData.Marks = new Dictionary<CellKey, MapData.MarkType>();
			foreach (KeyValuePair<CellKey, EditorMapCellData> item in editorMapData.Cells.CellsDict)
			{
				if (item.Value != null)
				{
					CellData cellData = new CellData(item.Key, item.Value);
					mapData.Cells.Add(cellData);
					if (!mapData.CellsByZone.ContainsKey(item.Value.ZoneId))
					{
						mapData.CellsByZone[item.Value.ZoneId] = new List<CellData>();
					}
					mapData.CellsByZone[item.Value.ZoneId].Add(cellData);
					mapData.CellsByCellKey[item.Key] = cellData;
				}
			}
			foreach (KeyValuePair<string, EditorMapCellGrid> secret in editorMapData.Secrets)
			{
				if (!mapData.Secrets.ContainsKey(secret.Key))
				{
					mapData.Secrets[secret.Key] = new SecretData();
					mapData.Secrets[secret.Key].Name = secret.Key;
				}
				foreach (KeyValuePair<CellKey, EditorMapCellData> item2 in secret.Value.CellsDict)
				{
					if (item2.Value != null)
					{
						mapData.Secrets[secret.Key].Cells[item2.Key] = new CellData(item2.Key, item2.Value);
					}
				}
			}
			Maps[editorMapData.name] = mapData;
		}
		if (Maps.Count > 0)
		{
			CurrentMap = Maps.First((KeyValuePair<string, MapData> x) => x.Key.EndsWith("DLC3")).Value;
			DefaultMapID = CurrentMap.Name;
		}
		Log.Debug("New Map", Maps.Count + " maps loaded succesfully.");
		Log.Debug("******************************************* CELLS");
		Log.Debug(" NG+: " + CurrentMap.Cells.Where((CellData cell) => cell.NGPlus).Count());
		Log.Debug(" Normal: " + CurrentMap.Cells.Where((CellData cell) => !cell.NGPlus).Count());
	}

	private List<CellData> GetAllRevealedCells(MapData map)
	{
		List<CellData> list = new List<CellData>();
		if (map != null)
		{
			foreach (CellData item in map.Cells.Where((CellData cell) => cell.Revealed))
			{
				bool flag = false;
				foreach (SecretData item2 in map.Secrets.Values.Where((SecretData x) => x.Revealed))
				{
					if (item2.Cells.ContainsKey(item.CellKey))
					{
						flag = true;
						list.Add(item2.Cells[item.CellKey]);
						break;
					}
				}
				if (!flag)
				{
					list.Add(item);
				}
			}
			return list;
		}
		return list;
	}

	private void OnLocalizationChange()
	{
		if (!(CurrentLanguage != I2.Loc.LocalizationManager.CurrentLanguage))
		{
			return;
		}
		if (CurrentLanguage != string.Empty)
		{
			Log.Debug("MapManager", "Language change, localize items to: " + I2.Loc.LocalizationManager.CurrentLanguage);
		}
		CurrentLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
		LanguageSource mainLanguageSource = LocalizationManager.GetMainLanguageSource();
		int languageIndexFromCode = mainLanguageSource.GetLanguageIndexFromCode(I2.Loc.LocalizationManager.CurrentLanguageCode);
		foreach (string item in new List<string>(DistrictLocalization.Keys))
		{
			string text = "Map/" + item;
			TermData termData = mainLanguageSource.GetTermData(text);
			if (termData == null)
			{
				Debug.LogWarning("Term " + text + " not found in Maps Localization");
			}
			else
			{
				DistrictLocalization[item] = termData.Languages[languageIndexFromCode];
			}
		}
		foreach (string item2 in new List<string>(ZonesLocalization.Keys))
		{
			string text2 = "Map/" + item2;
			TermData termData2 = mainLanguageSource.GetTermData(text2);
			if (termData2 == null)
			{
				Debug.LogWarning("Term " + text2 + " not found in Maps Localization");
			}
			else
			{
				ZonesLocalization[item2] = termData2.Languages[languageIndexFromCode];
			}
		}
	}

	private void CreateLocaliztionDicts()
	{
		DistrictLocalization.Clear();
		ZonesLocalization.Clear();
		Regex regex = new Regex("^D(?<district>[0-9]{2})Z(?<zone>[0-9]{2})S(?<scene>[0-9]{2})$");
		for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
		{
			string scenePathByBuildIndex = SceneUtility.GetScenePathByBuildIndex(i);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(scenePathByBuildIndex);
			string input = fileNameWithoutExtension.Split('_')[0];
			Match match = regex.Match(input);
			if (!match.Success)
			{
				continue;
			}
			string text = "D" + match.Groups["district"].Value;
			string zone = "Z" + match.Groups["zone"].Value;
			string scene = "S" + match.Groups["scene"].Value;
			ZoneKey zoneKey = new ZoneKey(text, zone, scene);
			if (!GameConstants.DistrictsWithoutName.Contains(text))
			{
				if (!DistrictLocalization.ContainsKey(text))
				{
					DistrictLocalization[text] = string.Empty;
				}
				string localizationKey = zoneKey.GetLocalizationKey();
				if (!ZonesLocalization.ContainsKey(localizationKey))
				{
					ZonesLocalization[localizationKey] = string.Empty;
				}
			}
		}
	}

	public int GetOrder()
	{
		return -10;
	}

	public string GetPersistenID()
	{
		return "ID_NEW_MAPS";
	}

	public void ResetPersistence()
	{
		LoadAllMaps();
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		NewMapPersistenceData newMapPersistenceData = new NewMapPersistenceData();
		newMapPersistenceData.currentMapId = string.Empty;
		if (CurrentMap != null)
		{
			newMapPersistenceData.currentMapId = CurrentMap.Name;
		}
		foreach (KeyValuePair<string, MapData> map in Maps)
		{
			if (!newMapPersistenceData.Maps.ContainsKey(map.Key))
			{
				newMapPersistenceData.Maps[map.Key] = new NewMapPersistenceDataItem();
			}
			foreach (CellData cell in map.Value.Cells)
			{
				if (cell.Revealed)
				{
					newMapPersistenceData.Maps[map.Key].RevealedCells.Add(cell.CellKey);
				}
			}
			foreach (KeyValuePair<string, SecretData> secret in map.Value.Secrets)
			{
				if (secret.Value.Revealed)
				{
					newMapPersistenceData.Maps[map.Key].Secrets.Add(secret.Key);
				}
			}
			newMapPersistenceData.Maps[map.Key].Marks = new Dictionary<CellKey, MapData.MarkType>(map.Value.Marks);
		}
		return newMapPersistenceData;
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		foreach (KeyValuePair<string, MapData> map in Maps)
		{
			foreach (CellData cell in map.Value.Cells)
			{
				cell.Revealed = false;
			}
			foreach (SecretData value2 in map.Value.Secrets.Values)
			{
				value2.Revealed = false;
			}
			map.Value.Marks.Clear();
		}
		NewMapPersistenceData newMapPersistenceData = (NewMapPersistenceData)data;
		PassRevealedCells(newMapPersistenceData, string.Empty, string.Empty);
		PassRevealedSecrets(newMapPersistenceData, string.Empty, string.Empty);
		PassMarks(newMapPersistenceData, string.Empty, string.Empty);
		CurrentMap = null;
		if (!(newMapPersistenceData.currentMapId != string.Empty))
		{
			return;
		}
		CurrentMap = Maps.First((KeyValuePair<string, MapData> x) => x.Key.EndsWith("DLC3")).Value;
		if (!newMapPersistenceData.currentMapId.EndsWith("DLC3"))
		{
			MapData value = null;
			if (Maps.TryGetValue(newMapPersistenceData.currentMapId, out value))
			{
				PassRevealedCells(newMapPersistenceData, value.Name, CurrentMap.Name);
				PassRevealedSecrets(newMapPersistenceData, value.Name, CurrentMap.Name);
				PassMarks(newMapPersistenceData, value.Name, CurrentMap.Name);
			}
		}
	}

	private void PassRevealedCells(NewMapPersistenceData dataSource, string sorceMapKey = "", string destinationMapKey = "")
	{
		foreach (KeyValuePair<string, NewMapPersistenceDataItem> map in dataSource.Maps)
		{
			if (!string.IsNullOrEmpty(sorceMapKey) && map.Key != sorceMapKey)
			{
				continue;
			}
			if (Maps.ContainsKey(map.Key))
			{
				MapData mapData = Maps[map.Key];
				if (!string.IsNullOrEmpty(destinationMapKey))
				{
					mapData = Maps[destinationMapKey];
				}
				foreach (CellKey storedCellKey in map.Value.RevealedCells)
				{
					CellData cellData = mapData.Cells.FirstOrDefault((CellData x) => x.CellKey.Equals(storedCellKey));
					if (cellData != null)
					{
						cellData.Revealed = true;
					}
					else
					{
						Debug.LogError("*** MAP Persistence: Cell Key " + storedCellKey.ToString() + " not found in map " + map.Key);
					}
				}
			}
			else
			{
				Debug.LogError("*** MAP Persistence: Map " + map.Key + " not found");
			}
		}
	}

	private void PassRevealedSecrets(NewMapPersistenceData dataSource, string sorceMapKey = "", string destinationMapKey = "")
	{
		foreach (KeyValuePair<string, NewMapPersistenceDataItem> map in dataSource.Maps)
		{
			if (!string.IsNullOrEmpty(sorceMapKey) && map.Key != sorceMapKey)
			{
				continue;
			}
			if (Maps.ContainsKey(map.Key))
			{
				MapData mapData = Maps[map.Key];
				if (!string.IsNullOrEmpty(destinationMapKey))
				{
					mapData = Maps[destinationMapKey];
				}
				foreach (string secret in map.Value.Secrets)
				{
					if (mapData.Secrets.ContainsKey(secret))
					{
						mapData.Secrets[secret].Revealed = true;
					}
					else
					{
						Debug.LogError("*** MAP Persistence: Secret " + secret + " not found in map " + map.Key);
					}
				}
			}
			else
			{
				Debug.LogError("*** MAP Persistence: Map " + map.Key + " not found");
			}
		}
	}

	private void PassMarks(NewMapPersistenceData dataSource, string sorceMapKey = "", string destinationMapKey = "")
	{
		foreach (KeyValuePair<string, NewMapPersistenceDataItem> map in dataSource.Maps)
		{
			if (!string.IsNullOrEmpty(sorceMapKey) && map.Key != sorceMapKey)
			{
				continue;
			}
			if (Maps.ContainsKey(map.Key))
			{
				MapData mapData = Maps[map.Key];
				if (!string.IsNullOrEmpty(destinationMapKey))
				{
					mapData = Maps[destinationMapKey];
				}
				mapData.Marks = new Dictionary<CellKey, MapData.MarkType>(map.Value.Marks);
			}
			else
			{
				Debug.LogError("*** MAP Persistence: Map " + map.Key + " not found");
			}
		}
	}

	public override void OnGUI()
	{
		DebugResetLine();
		DebugDrawTextLine("NewMapManager -------------------------------------");
		if (CurrentMap == null)
		{
			DebugDrawTextLine("  NO MAP");
			return;
		}
		DebugDrawTextLine("--Current Map: " + CurrentMap.Name);
		CellKey playerCell = GetPlayerCell();
		if (CurrentScene != null)
		{
			DebugDrawTextLine("--Current SCENE: " + CurrentScene.GetKey());
		}
		else
		{
			DebugDrawTextLine("--Current SCENE: NONE");
		}
		if (playerCell != null)
		{
			DebugDrawTextLine("--Current CELL  X:" + playerCell.X + "   Y:" + playerCell.Y);
			DebugDrawTextLine("    TYPE:" + LastPlayerCell.Type);
		}
		else
		{
			DebugDrawTextLine("--Current CELL: NONE");
		}
		DebugDrawTextLine("--Number of revealed: " + GetAllRevealedCells().Count);
		DebugDrawTextLine("--Secrets");
		foreach (KeyValuePair<string, SecretData> secret in CurrentMap.Secrets)
		{
			DebugDrawTextLine("    " + secret.Value.Name + ": " + secret.Value.Revealed);
		}
	}
}
