using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Framework.FrameworkCore;
using I2.Loc;
using Tools.DataContainer;
using Tools.Level;
using Tools.Level.Actionables;
using Tools.Level.Interactables;
using UnityEngine;

namespace Framework.Managers;

public class MapManager : GameSystem, PersistentInterface
{
	public class DataMapCell
	{
		public bool crawled;

		public Bounds worldBounds { get; private set; }

		public Rect textureBounds { get; private set; }

		public Bounds mapBounds { get; private set; }

		public DataMapCell(DataMapReveal reference, Bounds cell)
		{
			worldBounds = new Bounds(new Vector3(cell.center.x, cell.center.y, 0f), new Vector3(cell.size.x, cell.size.y, 100f));
			Vector2 vector = reference.WorldToMaskCoordinates(cell.min);
			vector = new Vector2(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
			Vector2 vector2 = reference.WorldToMaskCoordinates(cell.max);
			vector2 = new Vector2(Mathf.Ceil(vector2.x), Mathf.Ceil(vector2.y));
			Vector2 vector3 = vector2 - vector;
			textureBounds = new Rect(vector.x, vector.y, vector3.x, vector3.y);
			Vector3 vector4 = reference.WorldToTexture(cell.center);
			Vector3 vector5 = reference.WorldToTexture(cell.size);
			mapBounds = new Bounds(new Vector3(vector4.x, vector4.y, 0f), new Vector3(vector5.x, vector5.y, 100f));
		}
	}

	public enum MapElementType
	{
		Reclinatory,
		Gate,
		Door,
		Teleport,
		MeaCulpa
	}

	public class ElementsRevealed
	{
		public bool activatedOrOpen;

		public Vector3 pos { get; private set; }

		public MapElementType element { get; private set; }

		public ElementsRevealed(Vector3 position, MapElementType typeEleement)
		{
			pos = position;
			element = typeEleement;
			activatedOrOpen = false;
		}
	}

	public class DataMapReveal
	{
		public Sprite mask;

		public Sprite map;

		public Vector3 pos;

		public string domain = string.Empty;

		public string zone = string.Empty;

		public float orthogonalFactor;

		public int height;

		public int width;

		public bool updated;

		public bool updatedAnyTime;

		public Bounds mapBounds;

		public List<DataMapCell> cells = new List<DataMapCell>();

		public Dictionary<string, ElementsRevealed> elements = new Dictionary<string, ElementsRevealed>();

		public Vector3 GetWorldPosition()
		{
			return WorldToTexture(new Vector3(pos.x, pos.y, 0f));
		}

		public Vector2 GetWorldPosition2()
		{
			return WorldToTexture(new Vector2(pos.x, pos.y));
		}

		public Vector3 WorldToTexture(Vector3 world)
		{
			return new Vector3(world.x * orthogonalFactor, world.y * orthogonalFactor, world.z);
		}

		public Vector2 WorldToMaskCoordinates(Vector3 world)
		{
			Vector3 vector = world - pos;
			vector *= orthogonalFactor;
			vector += new Vector3((float)width / 2f, (float)height / 2f, 0f);
			return new Vector2(vector.x, vector.y);
		}

		public List<DataMapCell> MarkAndGetMapCellFromWorld(Vector3 worldPosition, bool forceUpdate = false)
		{
			List<DataMapCell> list = new List<DataMapCell>();
			Vector3 point = new Vector3(worldPosition.x, worldPosition.y, 0f);
			foreach (DataMapCell cell in cells)
			{
				if ((!cell.crawled || forceUpdate) && cell.worldBounds.Contains(point))
				{
					cell.crawled = true;
					list.Add(cell);
				}
			}
			return list;
		}

		public bool CellContainsMapPos(Vector2 mapPos)
		{
			Vector3 point = new Vector3(mapPos.x, mapPos.y, 0f);
			bool flag = mapBounds.Contains(point);
			if (flag)
			{
				flag = false;
				{
					foreach (DataMapCell cell in cells)
					{
						if (cell.mapBounds.Contains(point))
						{
							return true;
						}
					}
					return flag;
				}
			}
			return flag;
		}

		public void CheckNewElements(Bounds worldBounds, Dictionary<Type, List<PersistentObject>> sceneData)
		{
			CheckElements<Door>(MapElementType.Door, worldBounds, sceneData);
			CheckElements<PrieDieu>(MapElementType.Reclinatory, worldBounds, sceneData);
			CheckElements<Gate>(MapElementType.Gate, worldBounds, sceneData);
			CheckElements<Teleport>(MapElementType.Teleport, worldBounds, sceneData);
		}

		public void UpdateElementsStatus()
		{
			PersistentObject[] array = UnityEngine.Object.FindObjectsOfType<PersistentObject>();
			foreach (PersistentObject persistentObject in array)
			{
				if (elements.ContainsKey(persistentObject.GetPersistenID()))
				{
					elements[persistentObject.GetPersistenID()].activatedOrOpen = persistentObject.IsOpenOrActivated();
				}
			}
		}

		private void CheckElements<T>(MapElementType elementType, Bounds worldBounds, Dictionary<Type, List<PersistentObject>> sceneData) where T : PersistentObject
		{
			Type typeFromHandle = typeof(T);
			if (!sceneData.ContainsKey(typeFromHandle))
			{
				return;
			}
			foreach (PersistentObject item in sceneData[typeFromHandle])
			{
				if (!(item == null) && !elements.ContainsKey(item.GetPersistenID()))
				{
					bool flag = true;
					if (typeFromHandle == typeof(Door))
					{
						Door door = (Door)item;
						flag = !door.autoEnter;
					}
					else if (typeFromHandle == typeof(Teleport))
					{
						Teleport teleport = (Teleport)item;
						flag = teleport.showOnMap;
					}
					if (flag && CheckAndGetObjectSafePoint(worldBounds, item.gameObject, out var finalPos))
					{
						ElementsRevealed value = new ElementsRevealed(finalPos, elementType);
						elements[item.GetPersistenID()] = value;
					}
				}
			}
		}

		private bool CheckAndGetObjectSafePoint(Bounds worldBounds, GameObject obj, out Vector3 finalPos)
		{
			bool result = false;
			finalPos = new Vector3(obj.transform.position.x, obj.transform.position.y, 0f);
			if (worldBounds.Contains(finalPos))
			{
				result = true;
				Transform transform = obj.transform.Find("MAPELEMENT");
				if ((bool)transform && transform.gameObject != null && transform.gameObject.activeInHierarchy)
				{
					finalPos = new Vector3(transform.position.x, transform.position.y, 0f);
				}
			}
			return result;
		}
	}

	[Serializable]
	public class MapPersistenceDataElements
	{
		public Vector3 pos;

		public MapElementType element;

		public bool activatedOrOpen;
	}

	[Serializable]
	public class MapPersistenceDataItem
	{
		public string filename;

		public Dictionary<string, MapPersistenceDataElements> elements = new Dictionary<string, MapPersistenceDataElements>();
	}

	[Serializable]
	public class MapPersistenceData : PersistentManager.PersistentData
	{
		public Dictionary<string, MapPersistenceDataItem> reveal = new Dictionary<string, MapPersistenceDataItem>();

		public MapPersistenceData()
			: base("ID_MAPS")
		{
		}
	}

	private Dictionary<string, string> domainLocalization = new Dictionary<string, string>();

	private Dictionary<string, string> zonesLocalization = new Dictionary<string, string>();

	private const string MAP_RESOURCE_CONFIG = "Maps/MapData";

	private MapData mapData;

	private Dictionary<string, DataMapReveal> mapReveal = new Dictionary<string, DataMapReveal>();

	private const string REG_EXP = "^D(?<domain>[0-9]{2})Z(?<zone>[0-9]{2})S(?<scene>[0-9]{2})$";

	private string currentLanguage = string.Empty;

	private Dictionary<Type, List<PersistentObject>> cacheObjects = new Dictionary<Type, List<PersistentObject>>();

	private const string PERSITENT_ID = "ID_MAPS";

	public string CurrentDomain { get; private set; }

	public string CurrentZone { get; private set; }

	public Vector3 playerMapOffset { get; set; }

	public override void Initialize()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLocalizationChange;
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		LevelManager.OnBeforeLevelLoad += OnBeforeLevelLoad;
		CurrentDomain = string.Empty;
		CurrentZone = string.Empty;
		currentLanguage = string.Empty;
		LoadAllMaps();
		cacheObjects.Clear();
		OnLocalizationChange();
		playerMapOffset = Vector3.zero;
	}

	public override void Dispose()
	{
		cacheObjects.Clear();
		I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLocalizationChange;
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		LevelManager.OnBeforeLevelLoad -= OnBeforeLevelLoad;
	}

	private void OnBeforeLevelLoad(Level oldLevel, Level newLevel)
	{
		cacheObjects.Clear();
		playerMapOffset = Vector3.zero;
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		string levelName = newLevel.LevelName;
		cacheObjects.Clear();
		FillSceneCacheObject<Door>();
		FillSceneCacheObject<PrieDieu>();
		FillSceneCacheObject<Gate>();
		FillSceneCacheObject<Teleport>();
		if (levelName == "NONE")
		{
			return;
		}
		string domain = string.Empty;
		string zone = string.Empty;
		if (GetDomainAndZoneFromBundleName(levelName, ref domain, ref zone) && Core.Logic.CurrentLevelConfig.ShowZoneTitle(oldLevel) && domain + zone != CurrentDomain + CurrentZone)
		{
			string zoneName = GetZoneName(domain, zone);
			if (zoneName != string.Empty)
			{
				DisplayZoneName(zoneName);
				Core.AchievementsManager.AddProgressToAC21(domain, zone);
			}
		}
		CurrentDomain = domain;
		CurrentZone = zone;
	}

	public override void AllInitialized()
	{
		Core.Persistence.AddPersistentManager(this);
	}

	public string GetZoneNameFromBundle(string bundle)
	{
		string result = string.Empty;
		string domain = string.Empty;
		string zone = string.Empty;
		if (GetDomainAndZoneFromBundleName(bundle, ref domain, ref zone))
		{
			result = GetZoneName(domain, zone);
		}
		return result;
	}

	public void DisplayZoneName(string zoneName)
	{
	}

	public DataMapReveal GetNearestZone(Vector2 mapPos)
	{
		DataMapReveal dataMapReveal = null;
		float num = 0f;
		foreach (DataMapReveal value in mapReveal.Values)
		{
			float sqrMagnitude = (value.GetWorldPosition2() - mapPos).sqrMagnitude;
			if (value.CellContainsMapPos(mapPos) && (dataMapReveal == null || sqrMagnitude < num))
			{
				num = sqrMagnitude;
				dataMapReveal = value;
			}
		}
		return dataMapReveal;
	}

	public DataMapReveal GetCurrentZone()
	{
		return GetZone(CurrentDomain, CurrentZone);
	}

	public List<string> GetZonesList()
	{
		return mapReveal.Keys.ToList();
	}

	public DataMapReveal GetZone(string key)
	{
		if (mapReveal.ContainsKey(key))
		{
			return mapReveal[key];
		}
		return null;
	}

	public DataMapReveal GetZone(string domain, string zone)
	{
		return GetZone(GetDictKey(domain, zone));
	}

	public void DEBUG_RevealAllMaps(int numberOfReveals = 10)
	{
		foreach (DataMapReveal value2 in mapReveal.Values)
		{
			Graphics.CopyTexture(value2.map.texture, value2.mask.texture);
			value2.mask = Sprite.Create(value2.mask.texture, value2.mask.rect, new Vector2(0.5f, 0.5f));
			foreach (DataMapCell cell in value2.cells)
			{
				cell.crawled = true;
			}
			value2.elements.Clear();
			for (int i = 0; i < numberOfReveals; i++)
			{
				ElementsRevealed value = new ElementsRevealed(Vector3.zero, MapElementType.Door);
				value2.elements[value2.domain + value2.zone + "_" + i] = value;
			}
			value2.updatedAnyTime = true;
		}
	}

	public bool DigCurrentMask(Vector3 worldPosition, bool forceUpdate = false)
	{
		Vector3 worldPosition2 = worldPosition + playerMapOffset;
		bool result = false;
		DataMapReveal zone = GetZone(CurrentDomain, CurrentZone);
		if (zone != null)
		{
			bool flag = false;
			foreach (DataMapCell item in zone.MarkAndGetMapCellFromWorld(worldPosition2, forceUpdate))
			{
				Rect textureBounds = item.textureBounds;
				if (textureBounds.size.x > 0f && textureBounds.min.x > 0f && textureBounds.max.x < (float)zone.width && textureBounds.size.y > 0f && textureBounds.min.y > 0f && textureBounds.max.y < (float)zone.height)
				{
					Color[] pixels = zone.map.texture.GetPixels((int)textureBounds.min.x, (int)textureBounds.min.y, (int)textureBounds.size.x, (int)textureBounds.size.y);
					zone.mask.texture.SetPixels((int)textureBounds.min.x, (int)textureBounds.min.y, (int)textureBounds.size.x, (int)textureBounds.size.y, pixels);
					zone.mask.texture.Apply();
					zone.mask = Sprite.Create(zone.mask.texture, zone.mask.rect, new Vector2(0.5f, 0.5f));
					result = true;
					zone.CheckNewElements(item.worldBounds, cacheObjects);
					flag = true;
				}
				else
				{
					Debug.LogError("*** DigCurrentMask texture asking outside bounds: " + zone.domain + zone.zone);
				}
			}
			if (flag)
			{
				zone.updated = true;
				zone.updatedAnyTime = true;
			}
			zone.UpdateElementsStatus();
		}
		return result;
	}

	public string GetCurrentDomainName()
	{
		return GetDomainName(CurrentDomain);
	}

	public string GetCurrentZoneName()
	{
		return GetZoneName(CurrentDomain, CurrentZone);
	}

	public string GetDomainName(string domain)
	{
		string result = "![NO_LOC_" + domain + "]";
		if (domainLocalization.ContainsKey(domain))
		{
			result = domainLocalization[domain];
		}
		return result;
	}

	public string GetZoneName(string domain, string zone)
	{
		string dictKey = GetDictKey(domain, zone);
		string text = "![ERROR NO KEY " + dictKey + "]";
		if (zonesLocalization.ContainsKey(dictKey))
		{
			text = zonesLocalization[dictKey];
			if (text.Trim().Length == 0)
			{
				text = "[!ERROR NO ZONE NAME " + dictKey + "]";
			}
		}
		return text;
	}

	private bool GetDomainAndZoneFromBundleName(string bundle, ref string domain, ref string zone)
	{
		Regex regex = new Regex("^D(?<domain>[0-9]{2})Z(?<zone>[0-9]{2})S(?<scene>[0-9]{2})$");
		Match match = regex.Match(bundle);
		if (match.Success)
		{
			domain = "D" + match.Groups["domain"].Value;
			zone = "Z" + match.Groups["zone"].Value;
		}
		return match.Success;
	}

	private string GetDictKey(string domain, string zone)
	{
		return domain + "_" + zone;
	}

	private void LoadAllMaps()
	{
	}

	private void FillSceneCacheObject<T>() where T : PersistentObject
	{
		Type typeFromHandle = typeof(T);
		T[] array = UnityEngine.Object.FindObjectsOfType<T>();
		foreach (T item in array)
		{
			if (!cacheObjects.ContainsKey(typeFromHandle))
			{
				cacheObjects[typeFromHandle] = new List<PersistentObject>();
			}
			cacheObjects[typeFromHandle].Add(item);
		}
	}

	private void OnLocalizationChange()
	{
		if (currentLanguage != I2.Loc.LocalizationManager.CurrentLanguage)
		{
			if (currentLanguage != string.Empty)
			{
				Log.Debug("MapManager", "Language change, localize items to: " + I2.Loc.LocalizationManager.CurrentLanguage);
			}
			currentLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
			LanguageSource mainLanguageSource = LocalizationManager.GetMainLanguageSource();
			int languageIndexFromCode = mainLanguageSource.GetLanguageIndexFromCode(I2.Loc.LocalizationManager.CurrentLanguageCode);
			domainLocalization.Clear();
			zonesLocalization.Clear();
		}
	}

	public int GetOrder()
	{
		return 10;
	}

	public string GetPersistenID()
	{
		return "ID_MAPS";
	}

	public void ResetPersistence()
	{
		LoadAllMaps();
		playerMapOffset = Vector3.zero;
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		return new MapPersistenceData();
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		MapPersistenceData mapPersistenceData = (MapPersistenceData)data;
		foreach (KeyValuePair<string, MapPersistenceDataItem> item in mapPersistenceData.reveal)
		{
			string[] array = item.Key.Split('_');
			if (array.Length == 2)
			{
				Core.NewMapManager.RevealAllZone(array[0], array[1]);
			}
		}
	}
}
