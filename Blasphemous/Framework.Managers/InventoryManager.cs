using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Framework.FrameworkCore;
using Framework.Inventory;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Damage;
using I2.Loc;
using Tools.Level.Actionables;
using UnityEngine;

namespace Framework.Managers;

public class InventoryManager : GameSystem, PersistentInterface
{
	public enum ItemType
	{
		Relic,
		Prayer,
		Bead,
		Quest,
		Collectible,
		Sword
	}

	public delegate void ItemEvent(string item);

	[Serializable]
	public class InventoryPersistenceData : PersistentManager.PersistentData
	{
		public List<string> ownRellics = new List<string>();

		public string[] wearRellics = new string[3];

		public List<string> ownBeads = new List<string>();

		public string[] wearBeads = new string[8];

		public List<string> ownQuestItems = new List<string>();

		public List<string> ownCollectibleItems = new List<string>();

		public List<string> ownPrayers = new List<string>();

		public string[] wearPrayers = new string[1];

		public Dictionary<string, int> prayerDecipher = new Dictionary<string, int>();

		public List<string> ownSwords = new List<string>();

		public string[] wearSwords = new string[1];

		public bool[] keys = new bool[4];

		public InventoryPersistenceData()
			: base("ID_INVENTORY")
		{
		}
	}

	private GameObject mainObject;

	private ItemType _LastAddedObjectType = ItemType.Bead;

	private const string LANGUAGE_PREFAB_NAME = "Inventory/Languages";

	public static readonly string[] LANGUAGE_ELEMENT_LIST = new string[3] { "caption", "description", "lore" };

	private LanguageSource language;

	private static string currentLanguage = string.Empty;

	public const int NUM_RELLICS_SLOTS = 3;

	private Dictionary<string, Relic> allRellics;

	private List<Relic> ownRellics;

	private Relic[] wearRellics = new Relic[3];

	public const int MAX_BEADS_SLOTS = 8;

	private Dictionary<string, RosaryBead> allBeads;

	private List<RosaryBead> ownBeads;

	private RosaryBead[] wearBeads = new RosaryBead[8];

	private Dictionary<string, QuestItem> allQuestItems;

	private List<QuestItem> ownQuestItems;

	public const int MAX_PRAYERS_SLOTS = 1;

	private Dictionary<string, Prayer> allPrayers;

	private List<Prayer> ownPrayers;

	private Prayer[] wearPrayers = new Prayer[1];

	private Dictionary<string, Framework.Inventory.CollectibleItem> allCollectibleItems;

	private List<Framework.Inventory.CollectibleItem> ownCollectibleItems;

	public const int NUM_SWORDS_SLOTS = 1;

	public const string TRUE_SWORD_ID = "HE201";

	private Dictionary<string, Sword> allSwords;

	private List<Sword> ownSwords;

	private Sword[] wearSwords = new Sword[1];

	public const int NUM_BOSS_KEYS = 4;

	private bool[] ownBossKeys = new bool[4];

	private float PlayerHealth = -1f;

	private float NumOfFlasks = -1f;

	private List<string> ObjectsConvertedToTears = new List<string>();

	private static bool forceReload = true;

	private static List<string> cachedRelicsId = new List<string>();

	private static List<string> cachedQuestItemsId = new List<string>();

	private static List<string> cachedPrayersId = new List<string>();

	private static List<string> cachedBeadsId = new List<string>();

	private static List<string> cachedCollectibleItemsId = new List<string>();

	private static List<string> cachedSwordsId = new List<string>();

	private const string PERSITENT_ID = "ID_INVENTORY";

	public BaseInventoryObject LastAddedObject { get; private set; }

	public ItemType LastAddedObjectType
	{
		get
		{
			AnyLastUsedObjectUntilLastCalled = false;
			return _LastAddedObjectType;
		}
		private set
		{
			_LastAddedObjectType = value;
			AnyLastUsedObjectUntilLastCalled = true;
		}
	}

	public bool AnyLastUsedObjectUntilLastCalled { get; private set; }

	public TearsObject TearsGenericObject { get; private set; }

	public event ItemEvent OnItemEquiped;

	public override void Start()
	{
		language = GetLanguageSource();
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLocalizationChange;
		PenitentDamageArea.OnHitGlobal = (PenitentDamageArea.PlayerHitEvent)Delegate.Combine(PenitentDamageArea.OnHitGlobal, new PenitentDamageArea.PlayerHitEvent(OnPenitentHit));
		InitializeObjects();
	}

	public override void Update()
	{
		base.Update();
		if (Core.Logic == null || !(Core.Logic.Penitent != null))
		{
			return;
		}
		if (PlayerHealth != Core.Logic.Penitent.Stats.Life.Current)
		{
			if (PlayerHealth != -1f)
			{
				PlayerHealth = Core.Logic.Penitent.Stats.Life.Current;
				SendPenitentHealthChanged(PlayerHealth, wearBeads);
				SendPenitentHealthChanged(PlayerHealth, wearRellics);
				SendPenitentHealthChanged(PlayerHealth, wearPrayers);
				SendPenitentHealthChanged(PlayerHealth, wearSwords);
			}
			else
			{
				PlayerHealth = Core.Logic.Penitent.Stats.Life.Current;
			}
		}
		if (NumOfFlasks != Core.Logic.Penitent.Stats.Flask.Current)
		{
			if (NumOfFlasks != -1f)
			{
				NumOfFlasks = Core.Logic.Penitent.Stats.Flask.Current;
				SendNumberOfCurrentFlasksChanged(NumOfFlasks, wearBeads);
				SendNumberOfCurrentFlasksChanged(NumOfFlasks, wearRellics);
				SendNumberOfCurrentFlasksChanged(NumOfFlasks, wearPrayers);
				SendNumberOfCurrentFlasksChanged(NumOfFlasks, wearSwords);
			}
			else
			{
				NumOfFlasks = Core.Logic.Penitent.Stats.Flask.Current;
			}
		}
	}

	private void InitializeObjects()
	{
		if (mainObject == null)
		{
			mainObject = new GameObject("Inventory Objects");
			UnityEngine.Object.DontDestroyOnLoad(mainObject);
		}
		allRellics = new Dictionary<string, Relic>();
		ownRellics = new List<Relic>();
		for (int i = 0; i < wearRellics.Length; i++)
		{
			wearRellics[i] = null;
		}
		allBeads = new Dictionary<string, RosaryBead>();
		ownBeads = new List<RosaryBead>();
		for (int j = 0; j < wearRellics.Length; j++)
		{
			wearBeads[j] = null;
		}
		allQuestItems = new Dictionary<string, QuestItem>();
		ownQuestItems = new List<QuestItem>();
		allCollectibleItems = new Dictionary<string, Framework.Inventory.CollectibleItem>();
		ownCollectibleItems = new List<Framework.Inventory.CollectibleItem>();
		allPrayers = new Dictionary<string, Prayer>();
		ownPrayers = new List<Prayer>();
		for (int k = 0; k < wearPrayers.Length; k++)
		{
			wearPrayers[k] = null;
		}
		allSwords = new Dictionary<string, Sword>();
		ownSwords = new List<Sword>();
		for (int l = 0; l < wearSwords.Length; l++)
		{
			wearSwords[l] = null;
		}
		for (int m = 0; m < ownBossKeys.Length; m++)
		{
			ownBossKeys[m] = false;
		}
		LoadBaseInventoryObjects(allRellics, ownRellics);
		LoadBaseInventoryObjects(allBeads, ownBeads);
		LoadBaseInventoryObjects(allQuestItems, ownQuestItems);
		LoadBaseInventoryObjects(allCollectibleItems, ownCollectibleItems);
		LoadBaseInventoryObjects(allPrayers, ownPrayers);
		LoadBaseInventoryObjects(allSwords, ownSwords);
		int num = allBeads.Count + allRellics.Count + allQuestItems.Count + allPrayers.Count + allCollectibleItems.Count + allSwords.Count;
		Log.Debug("Inventory", num + " items loaded succesfully.");
		Log.Debug("Inventory", allBeads.Count + " beads.");
		Log.Debug("Inventory", allRellics.Count + " relics.");
		Log.Debug("Inventory", allQuestItems.Count + " quest items.");
		Log.Debug("Inventory", allPrayers.Count + " prayers.");
		Log.Debug("Inventory", allCollectibleItems.Count + " collectibles.");
		Log.Debug("Inventory", allSwords.Count + " swords.");
		TearsGenericObject = Resources.Load<TearsObject>("Inventory/TearObject");
		currentLanguage = string.Empty;
		OnLocalizationChange();
	}

	public override void AllInitialized()
	{
		Core.Persistence.AddPersistentManager(this);
	}

	public override void Dispose()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLocalizationChange;
		PenitentDamageArea.OnHitGlobal = (PenitentDamageArea.PlayerHitEvent)Delegate.Remove(PenitentDamageArea.OnHitGlobal, new PenitentDamageArea.PlayerHitEvent(OnPenitentHit));
	}

	private void LoadBaseInventoryObjects<T>(Dictionary<string, T> allDict, List<T> ownList) where T : BaseInventoryObject
	{
		string name = typeof(T).Name;
		Transform transform = mainObject.transform.Find(name);
		if (transform == null)
		{
			GameObject gameObject = new GameObject(name);
			gameObject.transform.SetParent(mainObject.transform);
			transform = gameObject.transform;
		}
		else
		{
			foreach (Transform item in transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		UnityEngine.Object[] array = Resources.LoadAll("Inventory/" + name);
		UnityEngine.Object[] array2 = array;
		foreach (UnityEngine.Object @object in array2)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(@object, transform) as GameObject;
			if (gameObject2 == null)
			{
				Log.Error("Inventory", "Error instanciating prefab " + @object.name);
				continue;
			}
			T component = gameObject2.GetComponent<T>();
			if ((UnityEngine.Object)component == (UnityEngine.Object)null)
			{
				Log.Error("Inventory", "Error on prefab " + gameObject2.name + " hasn't " + name + " component, skip");
				UnityEngine.Object.Destroy(gameObject2);
			}
			else
			{
				gameObject2.name = component.id;
				allDict[component.id] = component;
				if (component.carryonstart)
				{
					AddObject(component, ownList);
				}
			}
		}
	}

	public void OnDamageInflicted(Hit hit)
	{
		SendHitEvent(hit, wearBeads);
		SendHitEvent(hit, wearRellics);
		SendHitEvent(hit, wearPrayers);
		SendHitEvent(hit, wearSwords);
	}

	public void OnEnemyKilled(Enemy e)
	{
		SendKillEnemyEvent(e, wearBeads);
		SendKillEnemyEvent(e, wearRellics);
		SendKillEnemyEvent(e, wearPrayers);
		SendKillEnemyEvent(e, wearSwords);
	}

	public void OnPenitentHit(Penitent damaged, Hit hit)
	{
		SendReceiveHitEvent(hit, wearBeads);
		SendReceiveHitEvent(hit, wearRellics);
		SendReceiveHitEvent(hit, wearPrayers);
		SendReceiveHitEvent(hit, wearSwords);
	}

	public void OnBreakBreakable(BreakableObject breakable)
	{
		SendBreakBreakableEvent(breakable, wearBeads);
		SendBreakBreakableEvent(breakable, wearRellics);
		SendBreakBreakableEvent(breakable, wearPrayers);
		SendBreakBreakableEvent(breakable, wearSwords);
	}

	public void OnPenitentDead()
	{
		SendOnPenitentDead(wearBeads);
		SendOnPenitentDead(wearPrayers);
		SendOnPenitentDead(wearPrayers);
		SendOnPenitentDead(wearSwords);
	}

	private void SendOnPenitentDead<T>(T[] ownObject) where T : BaseInventoryObject
	{
		T[] array = (T[])ownObject.Clone();
		T[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			T val = array2[i];
			if ((bool)(UnityEngine.Object)val)
			{
				val.PenitentDead();
			}
		}
	}

	private void SendBreakBreakableEvent<T>(BreakableObject breakable, T[] ownObject) where T : BaseInventoryObject
	{
		for (int i = 0; i < ownObject.Length; i++)
		{
			T val = ownObject[i];
			if ((bool)(UnityEngine.Object)val)
			{
				val.BreakableBreak(breakable);
			}
		}
	}

	private void SendHitEvent<T>(Hit hit, T[] ownObject) where T : BaseInventoryObject
	{
		for (int i = 0; i < ownObject.Length; i++)
		{
			T val = ownObject[i];
			if ((bool)(UnityEngine.Object)val)
			{
				val.HitEnemy(hit);
			}
		}
	}

	private void SendKillEnemyEvent<T>(Enemy e, T[] ownObject) where T : BaseInventoryObject
	{
		for (int i = 0; i < ownObject.Length; i++)
		{
			T val = ownObject[i];
			if ((bool)(UnityEngine.Object)val)
			{
				val.KillEnemy(e);
			}
		}
	}

	private void SendReceiveHitEvent<T>(Hit hit, T[] ownObject) where T : BaseInventoryObject
	{
		for (int i = 0; i < ownObject.Length; i++)
		{
			T val = ownObject[i];
			if ((bool)(UnityEngine.Object)val)
			{
				val.HitReceived(hit);
			}
		}
	}

	private void SendPenitentHealthChanged<T>(float life, T[] ownObject) where T : BaseInventoryObject
	{
		for (int i = 0; i < ownObject.Length; i++)
		{
			T val = ownObject[i];
			if ((bool)(UnityEngine.Object)val)
			{
				val.PenitentHealthChanged(life);
			}
		}
	}

	private void SendNumberOfCurrentFlasksChanged<T>(float newNumberOfFlasks, T[] ownObject) where T : BaseInventoryObject
	{
		for (int i = 0; i < ownObject.Length; i++)
		{
			T val = ownObject[i];
			if ((bool)(UnityEngine.Object)val)
			{
				val.NumberOfCurrentFlasksChanged(newNumberOfFlasks);
			}
		}
	}

	public BaseInventoryObject AddBaseObjectOrTears(BaseInventoryObject inbObject)
	{
		BaseInventoryObject result = inbObject;
		if (!AddBaseObject(inbObject))
		{
			string internalId = inbObject.GetInternalId();
			if (!ObjectsConvertedToTears.Contains(internalId))
			{
				ObjectsConvertedToTears.Add(internalId);
			}
			result = TearsGenericObject;
			Core.Logic.Penitent.Stats.Purge.Current += TearsGenericObject.TearsForDuplicatedObject;
		}
		return result;
	}

	public BaseInventoryObject GetBaseObjectOrTears(string idObj, ItemType itemType)
	{
		BaseInventoryObject baseInventoryObject = GetBaseObject(idObj, itemType);
		string internalId = baseInventoryObject.GetInternalId();
		if (ObjectsConvertedToTears.Contains(internalId))
		{
			baseInventoryObject = TearsGenericObject;
		}
		return baseInventoryObject;
	}

	public BaseInventoryObject GetBaseObject(string idObj, ItemType itemType)
	{
		BaseInventoryObject result = null;
		switch (itemType)
		{
		case ItemType.Relic:
			result = Core.InventoryManager.GetRelic(idObj);
			break;
		case ItemType.Collectible:
			result = Core.InventoryManager.GetCollectibleItem(idObj);
			break;
		case ItemType.Quest:
			result = Core.InventoryManager.GetQuestItem(idObj);
			break;
		case ItemType.Prayer:
			result = Core.InventoryManager.GetPrayer(idObj);
			break;
		case ItemType.Bead:
			result = Core.InventoryManager.GetRosaryBead(idObj);
			break;
		case ItemType.Sword:
			result = Core.InventoryManager.GetSword(idObj);
			break;
		}
		return result;
	}

	public bool AddBaseObject(BaseInventoryObject inbObject)
	{
		bool result = false;
		if (inbObject.GetType() == typeof(Relic))
		{
			result = AddRelic((Relic)inbObject);
		}
		if (inbObject.GetType() == typeof(RosaryBead))
		{
			result = AddRosaryBead((RosaryBead)inbObject);
		}
		if (inbObject.GetType() == typeof(QuestItem))
		{
			result = AddQuestItem((QuestItem)inbObject);
		}
		if (inbObject.GetType() == typeof(Prayer))
		{
			result = AddPrayer((Prayer)inbObject);
		}
		if (inbObject.GetType() == typeof(Framework.Inventory.CollectibleItem))
		{
			result = AddCollectibleItem(inbObject.id);
		}
		if (inbObject.GetType() == typeof(Sword))
		{
			result = AddSword(inbObject.id);
		}
		return result;
	}

	public void TestAddAllObjects()
	{
		allBeads.Values.ToList().ForEach(delegate(RosaryBead item)
		{
			AddBaseObject(item);
		});
		allPrayers.Values.ToList().ForEach(delegate(Prayer item)
		{
			AddBaseObject(item);
		});
		allQuestItems.Values.ToList().ForEach(delegate(QuestItem item)
		{
			AddBaseObject(item);
		});
		allRellics.Values.ToList().ForEach(delegate(Relic item)
		{
			AddBaseObject(item);
		});
		allCollectibleItems.Values.ToList().ForEach(delegate(Framework.Inventory.CollectibleItem item)
		{
			AddBaseObject(item);
		});
		allSwords.Values.ToList().ForEach(delegate(Sword item)
		{
			AddBaseObject(item);
		});
	}

	public void AddAll(ItemType itemType)
	{
		switch (itemType)
		{
		case ItemType.Bead:
			allBeads.Values.ToList().ForEach(delegate(RosaryBead item)
			{
				AddBaseObject(item);
			});
			break;
		case ItemType.Collectible:
			allCollectibleItems.Values.ToList().ForEach(delegate(Framework.Inventory.CollectibleItem item)
			{
				AddBaseObject(item);
			});
			break;
		case ItemType.Prayer:
			allPrayers.Values.ToList().ForEach(delegate(Prayer item)
			{
				AddBaseObject(item);
			});
			break;
		case ItemType.Quest:
			allQuestItems.Values.ToList().ForEach(delegate(QuestItem item)
			{
				AddBaseObject(item);
			});
			break;
		case ItemType.Relic:
			allRellics.Values.ToList().ForEach(delegate(Relic item)
			{
				AddBaseObject(item);
			});
			break;
		case ItemType.Sword:
			allSwords.Values.ToList().ForEach(delegate(Sword item)
			{
				AddBaseObject(item);
			});
			break;
		default:
			Debug.LogError("Unknown AddAll param: " + itemType);
			break;
		}
	}

	public void RemoveAll(ItemType itemType)
	{
		switch (itemType)
		{
		case ItemType.Bead:
			allBeads.Values.ToList().ForEach(delegate(RosaryBead item)
			{
				RemoveBaseObject(item);
			});
			break;
		case ItemType.Collectible:
			allCollectibleItems.Values.ToList().ForEach(delegate(Framework.Inventory.CollectibleItem item)
			{
				RemoveBaseObject(item);
			});
			break;
		case ItemType.Prayer:
			allPrayers.Values.ToList().ForEach(delegate(Prayer item)
			{
				RemoveBaseObject(item);
			});
			break;
		case ItemType.Quest:
			allQuestItems.Values.ToList().ForEach(delegate(QuestItem item)
			{
				RemoveBaseObject(item);
			});
			break;
		case ItemType.Relic:
			allRellics.Values.ToList().ForEach(delegate(Relic item)
			{
				RemoveBaseObject(item);
			});
			break;
		case ItemType.Sword:
			allSwords.Values.ToList().ForEach(delegate(Sword item)
			{
				RemoveBaseObject(item);
			});
			break;
		default:
			Debug.LogError("Unknown AddAll param: " + itemType);
			break;
		}
	}

	public void PrepareForNewGamePlus()
	{
		RemoveObjectForNewGamePlus(ownRellics);
		RemoveObjectForNewGamePlus(ownBeads);
		RemoveObjectForNewGamePlus(ownQuestItems);
		RemoveObjectForNewGamePlus(ownPrayers);
		RemoveObjectForNewGamePlus(ownCollectibleItems);
		RemoveObjectForNewGamePlus(ownSwords);
		for (int i = 0; i < 4; i++)
		{
			Core.InventoryManager.RemoveBossKey(i);
		}
		RemoveEquipableObjects();
	}

	public void RemoveEquipableObjects()
	{
		UnequipAll(wearBeads);
		UnequipAll(wearPrayers);
		UnequipAll(wearRellics);
		UnequipAll(wearSwords);
	}

	public void RemoveBeads()
	{
		UnequipAll(wearBeads);
	}

	public void RemovePrayers()
	{
		UnequipAll(wearPrayers);
	}

	public bool RemoveBaseObject(BaseInventoryObject inbObject)
	{
		bool result = false;
		if (inbObject.GetType() == typeof(Relic))
		{
			result = RemoveRelic((Relic)inbObject);
		}
		if (inbObject.GetType() == typeof(RosaryBead))
		{
			result = RemoveRosaryBead((RosaryBead)inbObject);
		}
		if (inbObject.GetType() == typeof(QuestItem))
		{
			result = RemoveQuestItem((QuestItem)inbObject);
		}
		if (inbObject.GetType() == typeof(Prayer))
		{
			result = RemovePrayer((Prayer)inbObject);
		}
		if (inbObject.GetType() == typeof(Framework.Inventory.CollectibleItem))
		{
			result = RemoveCollectibleItem(inbObject.id);
		}
		if (inbObject.GetType() == typeof(Sword))
		{
			result = RemoveSword((Sword)inbObject);
		}
		return result;
	}

	public bool IsBaseObjectEquipped(BaseInventoryObject inbObject)
	{
		bool result = false;
		if (inbObject.GetType() == typeof(Relic))
		{
			result = IsRelicEquipped((Relic)inbObject);
		}
		if (inbObject.GetType() == typeof(RosaryBead))
		{
			result = IsRosaryBeadEquipped((RosaryBead)inbObject);
		}
		if (inbObject.GetType() == typeof(QuestItem))
		{
			result = IsQuestItemOwned((QuestItem)inbObject);
		}
		if (inbObject.GetType() == typeof(Prayer))
		{
			result = IsPrayerEquipped((Prayer)inbObject);
		}
		if (inbObject.GetType() == typeof(Framework.Inventory.CollectibleItem))
		{
			result = IsCollectibleItemOwned(inbObject.id);
		}
		if (inbObject.GetType() == typeof(Sword))
		{
			result = IsSwordOwned(inbObject.id);
		}
		return result;
	}

	public bool EquipBaseObject(BaseInventoryObject inbObject, int slot)
	{
		bool result = false;
		if (inbObject.GetType() == typeof(Relic))
		{
			result = SetRelicInSlot(slot, (Relic)inbObject);
		}
		if (inbObject.GetType() == typeof(RosaryBead))
		{
			result = SetRosaryBeadInSlot(slot, (RosaryBead)inbObject);
		}
		if (inbObject.GetType() == typeof(Prayer))
		{
			result = SetPrayerInSlot(slot, (Prayer)inbObject);
		}
		if (inbObject.GetType() == typeof(Sword))
		{
			result = SetSwordInSlot(slot, inbObject.id);
		}
		return result;
	}

	public int GetBaseObjectEquippedSlot(BaseInventoryObject inbObject)
	{
		int result = -1;
		if (inbObject.GetType() == typeof(Relic))
		{
			result = GetRelicSlot((Relic)inbObject);
		}
		if (inbObject.GetType() == typeof(RosaryBead))
		{
			result = GetRosaryBeadSlot((RosaryBead)inbObject);
		}
		if (inbObject.GetType() == typeof(Prayer))
		{
			result = GetPrayerInSlot((Prayer)inbObject);
		}
		if (inbObject.GetType() == typeof(Sword))
		{
			result = GetSwordInSlot((Sword)inbObject);
		}
		return result;
	}

	public Relic GetRelic(string idRelic)
	{
		Relic value = null;
		allRellics.TryGetValue(idRelic.ToUpper(), out value);
		return value;
	}

	public ReadOnlyCollection<Relic> GetAllRelics()
	{
		return GetDictValueReadOnly(allRellics);
	}

	public ReadOnlyCollection<Relic> GetRelicsOwned()
	{
		return ownRellics.AsReadOnly();
	}

	public bool AddRelic(string idRelic)
	{
		return AddRelic(GetRelic(idRelic.ToUpper()));
	}

	public bool AddRelic(Relic relic)
	{
		bool flag = (bool)relic && !ownRellics.Contains(relic);
		if (flag)
		{
			AddObject(relic, ownRellics);
		}
		return flag;
	}

	public bool RemoveRelic(string idRelic)
	{
		return RemoveRelic(GetRelic(idRelic.ToUpper()));
	}

	public bool RemoveRelic(Relic relic)
	{
		bool flag = (bool)relic && ownRellics.Contains(relic);
		if (flag)
		{
			Unequip(relic, wearRellics);
			RemoveObject(relic, ownRellics);
		}
		return flag;
	}

	public bool IsRelicOwned(string idRelic)
	{
		return IsRelicOwned(GetRelic(idRelic.ToUpper()));
	}

	public bool IsRelicOwned(Relic relic)
	{
		bool result = false;
		if (relic != null)
		{
			result = ownRellics.Contains(relic);
		}
		return result;
	}

	public bool IsRelicEquipped(string idRelic)
	{
		return IsRelicEquipped(GetRelic(idRelic.ToUpper()));
	}

	public bool IsRelicEquipped(Relic relic)
	{
		bool flag = false;
		if (relic != null)
		{
			for (int i = 0; i < 3; i++)
			{
				flag = wearRellics[i] == relic;
				if (flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	public bool IsAnyRelicEquipped()
	{
		bool result = false;
		for (int i = 0; i < 3; i++)
		{
			if ((bool)wearRellics[i] && wearRellics[i].IsEquiped)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public int GetRelicSlot(Relic relic)
	{
		int result = -1;
		if (relic != null)
		{
			for (int i = 0; i < 3; i++)
			{
				if (wearRellics[i] == relic)
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	public Relic GetRelicInSlot(int slot)
	{
		if (slot >= 0 && slot < 3)
		{
			return wearRellics[slot];
		}
		return null;
	}

	public bool SetRelicInSlot(int slot, string idRelic)
	{
		return SetRelicInSlot(slot, GetRelic(idRelic.ToUpper()));
	}

	public bool SetRelicInSlot(int slot, Relic relic)
	{
		bool flag = slot >= 0 && slot < 3;
		if (flag)
		{
			if (wearRellics[slot] != null)
			{
				Unequip(wearRellics[slot], wearRellics);
			}
			if ((bool)relic)
			{
				Equip(relic, slot, wearRellics);
			}
		}
		return flag;
	}

	public RosaryBead GetRosaryBead(string idRosaryBead)
	{
		RosaryBead value = null;
		allBeads.TryGetValue(idRosaryBead.ToUpper(), out value);
		return value;
	}

	public ReadOnlyCollection<RosaryBead> GetAllRosaryBeads()
	{
		return GetDictValueReadOnly(allBeads);
	}

	public ReadOnlyCollection<RosaryBead> GetRosaryBeadOwned()
	{
		return ownBeads.AsReadOnly();
	}

	public bool AddRosaryBead(string idRosaryBead)
	{
		return AddRosaryBead(GetRosaryBead(idRosaryBead.ToUpper()));
	}

	public bool AddRosaryBead(RosaryBead rosaryBead)
	{
		bool flag = (bool)rosaryBead && !ownBeads.Contains(rosaryBead);
		if (flag)
		{
			AddObject(rosaryBead, ownBeads);
		}
		return flag;
	}

	public bool RemoveRosaryBead(string idRosaryBead)
	{
		return RemoveRosaryBead(GetRosaryBead(idRosaryBead.ToUpper()));
	}

	public bool RemoveRosaryBead(RosaryBead bead)
	{
		bool flag = (bool)bead && ownBeads.Contains(bead);
		if (flag)
		{
			Unequip(bead, wearBeads);
			RemoveObject(bead, ownBeads);
		}
		return flag;
	}

	public bool IsRosaryBeadOwned(string idRosaryBead)
	{
		return IsRosaryBeadOwned(GetRosaryBead(idRosaryBead.ToUpper()));
	}

	public bool IsRosaryBeadOwned(RosaryBead rosaryBead)
	{
		bool result = false;
		if (rosaryBead != null)
		{
			result = ownBeads.Contains(rosaryBead);
		}
		return result;
	}

	public bool IsRosaryBeadEquipped(string idRosaryBead)
	{
		return IsRosaryBeadEquipped(GetRosaryBead(idRosaryBead.ToUpper()));
	}

	public bool IsRosaryBeadEquipped(RosaryBead bead)
	{
		bool flag = false;
		if (bead != null)
		{
			for (int i = 0; i < 8; i++)
			{
				flag = wearBeads[i] == bead;
				if (flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	public bool IsAnyRosaryBeadEquiped()
	{
		bool result = false;
		for (int i = 0; i < 8; i++)
		{
			if ((bool)wearBeads[i] && wearBeads[i].IsEquiped)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public RosaryBead GetRosaryBeadInSlot(int slot)
	{
		if (slot >= 0 && slot < 8)
		{
			return wearBeads[slot];
		}
		return null;
	}

	public int GetRosaryBeadSlot(RosaryBead bead)
	{
		int result = -1;
		if (bead != null)
		{
			for (int i = 0; i < 8; i++)
			{
				if (wearBeads[i] == bead)
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	public int GetRosaryBeadSlots()
	{
		int result = 0;
		if ((bool)Core.Logic.Penitent)
		{
			result = (int)Core.Logic.Penitent.Stats.BeadSlots.Final;
		}
		return result;
	}

	public bool SetRosaryBeadInSlot(int slot, string idRosaryBead)
	{
		return SetRosaryBeadInSlot(slot, GetRosaryBead(idRosaryBead.ToUpper()));
	}

	public bool SetRosaryBeadInSlot(int slot, RosaryBead bead)
	{
		bool flag = slot >= 0 && slot < wearBeads.Length;
		if (flag)
		{
			if (wearBeads[slot] != null)
			{
				Unequip(wearBeads[slot], wearBeads);
			}
			if ((bool)bead)
			{
				Equip(bead, slot, wearBeads);
			}
		}
		return flag;
	}

	public ReadOnlyCollection<QuestItem> GetAllQuestItems()
	{
		return GetDictValueReadOnly(allQuestItems);
	}

	public QuestItem GetQuestItem(string idQuestItem)
	{
		QuestItem value = null;
		allQuestItems.TryGetValue(idQuestItem.ToUpper(), out value);
		return value;
	}

	public ReadOnlyCollection<QuestItem> GetQuestItemOwned()
	{
		return ownQuestItems.AsReadOnly();
	}

	public bool AddQuestItem(string idQuestItem)
	{
		return AddQuestItem(GetQuestItem(idQuestItem.ToUpper()));
	}

	public bool AddQuestItem(QuestItem questItem)
	{
		bool result = false;
		if ((bool)questItem && !ownQuestItems.Contains(questItem))
		{
			result = true;
			AddObject(questItem, ownQuestItems);
		}
		return result;
	}

	public bool RemoveQuestItem(string idQuestItem)
	{
		return RemoveQuestItem(GetQuestItem(idQuestItem.ToUpper()));
	}

	public bool RemoveQuestItem(QuestItem questItem)
	{
		bool result = false;
		if ((bool)questItem && ownQuestItems.Contains(questItem))
		{
			result = true;
			RemoveObject(questItem, ownQuestItems);
		}
		return result;
	}

	public bool IsQuestItemOwned(string idQuestItem)
	{
		return IsQuestItemOwned(GetQuestItem(idQuestItem.ToUpper()));
	}

	public bool IsQuestItemOwned(QuestItem questItem)
	{
		bool result = false;
		if (questItem != null)
		{
			result = ownQuestItems.Contains(questItem);
		}
		return result;
	}

	public ReadOnlyCollection<Prayer> GetAllPrayers()
	{
		return GetDictValueReadOnly(allPrayers);
	}

	public Prayer GetPrayer(string idPrayer)
	{
		Prayer value = null;
		allPrayers.TryGetValue(idPrayer.ToUpper(), out value);
		return value;
	}

	public ReadOnlyCollection<Prayer> GetPrayersOwned()
	{
		return ownPrayers.AsReadOnly();
	}

	public ReadOnlyCollection<Prayer> GetPrayersOwned(Prayer.PrayerType prayerType)
	{
		return ownPrayers.FindAll((Prayer prayer) => prayer.prayerType == prayerType).AsReadOnly();
	}

	public bool AddPrayer(string idPrayer)
	{
		return AddPrayer(GetPrayer(idPrayer.ToUpper()));
	}

	public bool AddPrayer(Prayer prayer)
	{
		bool result = false;
		if ((bool)prayer && !ownPrayers.Contains(prayer))
		{
			result = true;
			AddObject(prayer, ownPrayers);
		}
		return result;
	}

	public bool RemovePrayer(string idPrayer)
	{
		return RemovePrayer(GetPrayer(idPrayer.ToUpper()));
	}

	public bool RemovePrayer(Prayer prayer)
	{
		bool flag = (bool)prayer && ownPrayers.Contains(prayer);
		if (flag)
		{
			Unequip(prayer, wearPrayers);
			RemoveObject(prayer, ownPrayers);
		}
		return flag;
	}

	public bool IsPrayerOwned(string idPrayer)
	{
		return IsPrayerOwned(GetPrayer(idPrayer.ToUpper()));
	}

	public bool IsPrayerOwned(Prayer prayer)
	{
		bool result = false;
		if (prayer != null)
		{
			result = ownPrayers.Contains(prayer);
		}
		return result;
	}

	public bool IsPrayerEquipped(string idPrayer)
	{
		return IsPrayerEquipped(GetPrayer(idPrayer.ToUpper()));
	}

	public bool IsAnyPrayerEquipped()
	{
		bool result = false;
		for (int i = 0; i < 1; i++)
		{
			if ((bool)wearPrayers[i] && wearPrayers[i].IsEquiped)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool IsPrayerEquipped(Prayer prayer)
	{
		bool flag = false;
		if (prayer != null)
		{
			for (int i = 0; i < 1; i++)
			{
				flag = wearPrayers[i] == prayer;
				if (flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	public Prayer GetPrayerInSlot(int slot)
	{
		if (slot >= 0 && slot < 1)
		{
			return wearPrayers[slot];
		}
		return null;
	}

	public int GetPrayerInSlot(Prayer prayer)
	{
		int result = -1;
		if (prayer != null)
		{
			for (int i = 0; i < 1; i++)
			{
				if (wearPrayers[i] == prayer)
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	public bool SetPrayerInSlot(int slot, string idPrayer)
	{
		return SetPrayerInSlot(slot, GetPrayer(idPrayer.ToUpper()));
	}

	public bool SetPrayerInSlot(int slot, Prayer prayer)
	{
		bool flag = slot >= 0 && slot < 1;
		if (flag)
		{
			if (wearPrayers[slot] != null)
			{
				Unequip(wearPrayers[slot], wearPrayers);
			}
			if ((bool)prayer && prayer.IsDeciphered())
			{
				Equip(prayer, slot, wearPrayers);
			}
		}
		return flag;
	}

	public ReadOnlyCollection<Framework.Inventory.CollectibleItem> GetAllCollectibleItems()
	{
		return GetDictValueReadOnly(allCollectibleItems);
	}

	public Framework.Inventory.CollectibleItem GetCollectibleItem(string idCollectibleItem)
	{
		Framework.Inventory.CollectibleItem value = null;
		allCollectibleItems.TryGetValue(idCollectibleItem.ToUpper(), out value);
		return value;
	}

	public ReadOnlyCollection<Framework.Inventory.CollectibleItem> GetCollectibleItemOwned()
	{
		return ownCollectibleItems.AsReadOnly();
	}

	public bool AddCollectibleItem(string idCollectibleItem)
	{
		return AddCollectibleItem(GetCollectibleItem(idCollectibleItem.ToUpper()));
	}

	public bool AddCollectibleItem(Framework.Inventory.CollectibleItem collectibleItem)
	{
		bool result = false;
		if ((bool)collectibleItem && !ownCollectibleItems.Contains(collectibleItem))
		{
			result = true;
			AddObject(collectibleItem, ownCollectibleItems);
		}
		return result;
	}

	public bool RemoveCollectibleItem(string idCollectibleItem)
	{
		return RemoveCollectibleItem(GetCollectibleItem(idCollectibleItem.ToUpper()));
	}

	public bool RemoveCollectibleItem(Framework.Inventory.CollectibleItem collectibleItem)
	{
		bool result = false;
		if ((bool)collectibleItem && ownCollectibleItems.Contains(collectibleItem))
		{
			result = true;
			RemoveObject(collectibleItem, ownCollectibleItems);
		}
		return result;
	}

	public bool IsCollectibleItemOwned(string idCollectibleItem)
	{
		return IsCollectibleItemOwned(GetCollectibleItem(idCollectibleItem.ToUpper()));
	}

	public bool IsCollectibleItemOwned(Framework.Inventory.CollectibleItem collectibleItem)
	{
		bool result = false;
		if (collectibleItem != null)
		{
			result = ownCollectibleItems.Contains(collectibleItem);
		}
		return result;
	}

	public Sword GetSword(string idSword)
	{
		Sword value = null;
		if (allSwords != null)
		{
			allSwords.TryGetValue(idSword.ToUpper(), out value);
		}
		return value;
	}

	public ReadOnlyCollection<Sword> GetAllSwords()
	{
		return GetDictValueReadOnly(allSwords);
	}

	public ReadOnlyCollection<Sword> GetSwordsOwned()
	{
		return ownSwords.AsReadOnly();
	}

	public bool AddSword(string idSword)
	{
		return AddSword(GetSword(idSword.ToUpper()));
	}

	public bool AddSword(Sword sword)
	{
		bool flag = (bool)sword && !ownSwords.Contains(sword);
		if (flag)
		{
			AddObject(sword, ownSwords);
		}
		return flag;
	}

	public bool RemoveSword(string idSword)
	{
		return RemoveSword(GetSword(idSword.ToUpper()));
	}

	public bool RemoveSword(Sword sword)
	{
		bool flag = (bool)sword && ownSwords.Contains(sword);
		if (flag)
		{
			Unequip(sword, wearSwords);
			RemoveObject(sword, ownSwords);
		}
		return flag;
	}

	public bool IsSwordOwned(string idSword)
	{
		return IsSwordOwned(GetSword(idSword.ToUpper()));
	}

	public bool IsSwordOwned(Sword sword)
	{
		bool result = false;
		if (sword != null)
		{
			result = ownSwords.Contains(sword);
		}
		return result;
	}

	public bool IsSwordEquipped(string idSword)
	{
		return IsSwordEquipped(GetSword(idSword.ToUpper()));
	}

	public bool IsSwordEquipped(Sword sword)
	{
		bool flag = false;
		if (sword != null)
		{
			for (int i = 0; i < 1; i++)
			{
				flag = wearSwords[i] == sword;
				if (flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	public bool IsAnySwordHeartEquiped()
	{
		bool result = false;
		for (int i = 0; i < 1; i++)
		{
			if ((bool)wearSwords[i] && wearSwords[i].IsEquiped)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool IsTrueSwordHeartEquiped()
	{
		bool result = false;
		for (int i = 0; i < 1; i++)
		{
			if ((bool)wearSwords[i] && wearSwords[i].IsEquiped && wearSwords[i].id.Equals("HE201"))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public int GetSwordInSlot(Sword sword)
	{
		int result = -1;
		if (sword != null)
		{
			for (int i = 0; i < 1; i++)
			{
				if (wearSwords[i] == sword)
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	public Sword GetSwordInSlot(int slot)
	{
		if (slot >= 0 && slot < 1)
		{
			return wearSwords[slot];
		}
		return null;
	}

	public bool SetSwordInSlot(int slot, string idSword)
	{
		return SetSwordInSlot(slot, GetSword(idSword.ToUpper()));
	}

	public bool SetSwordInSlot(int slot, Sword sword)
	{
		bool flag = slot >= 0 && slot < 1;
		if (flag)
		{
			if (wearSwords[slot] != null)
			{
				Unequip(wearSwords[slot], wearSwords);
			}
			if ((bool)sword)
			{
				Equip(sword, slot, wearSwords);
			}
		}
		return flag;
	}

	public void AddBossKey(int slot)
	{
		if (slot >= 0 && slot < 4)
		{
			ownBossKeys[slot] = true;
		}
	}

	public void RemoveBossKey(int slot)
	{
		if (slot >= 0 && slot < 4)
		{
			ownBossKeys[slot] = false;
		}
	}

	public bool CheckBossKey(int slot)
	{
		bool result = false;
		if (slot >= 0 && slot < 4)
		{
			result = ownBossKeys[slot];
		}
		return result;
	}

	public float GetPercentageCompletition()
	{
		float num = 0f;
		num = GetPercentageCompletitionList(ownRellics);
		num += GetPercentageCompletitionList(ownBeads);
		num += GetPercentageCompletitionList(ownQuestItems);
		num += GetPercentageCompletitionList(ownCollectibleItems);
		num += GetPercentageCompletitionList(ownPrayers);
		return num + GetPercentageCompletitionList(ownSwords);
	}

	private float GetPercentageCompletitionList<T>(List<T> ownObj) where T : BaseInventoryObject
	{
		return (float)ownObj.Count((T x) => x.AddPercentageCompletition()) * GameConstants.PercentageValues[PersistentManager.PercentageType.ItemAdded];
	}

	private void OnLocalizationChange()
	{
		if (currentLanguage != I2.Loc.LocalizationManager.CurrentLanguage)
		{
			if (currentLanguage != string.Empty)
			{
				Log.Debug("Inventory", "Language change, localize items to: " + I2.Loc.LocalizationManager.CurrentLanguage);
			}
			int languageIndexFromCode = language.GetLanguageIndexFromCode(I2.Loc.LocalizationManager.CurrentLanguageCode);
			TranslateAllObjects(allBeads, languageIndexFromCode);
			TranslateAllObjects(allRellics, languageIndexFromCode);
			TranslateAllObjects(allQuestItems, languageIndexFromCode);
			TranslateAllObjects(allCollectibleItems, languageIndexFromCode);
			TranslateAllObjects(allPrayers, languageIndexFromCode);
			TranslateAllObjects(allSwords, languageIndexFromCode);
			TranslateObject(TearsGenericObject, languageIndexFromCode);
			currentLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
		}
	}

	private void TranslateAllObjects<T>(Dictionary<string, T> allObjects, int idxLanguage) where T : BaseInventoryObject
	{
		foreach (KeyValuePair<string, T> allObject in allObjects)
		{
			TranslateObject(allObject.Value, idxLanguage);
		}
	}

	private void TranslateObject<T>(T entry, int idxLanguage) where T : BaseInventoryObject
	{
		string text = entry.GetBaseTranslationID() + "_";
		string[] lANGUAGE_ELEMENT_LIST = LANGUAGE_ELEMENT_LIST;
		foreach (string text2 in lANGUAGE_ELEMENT_LIST)
		{
			string text3 = text + text2.ToUpper();
			TermData termData = language.GetTermData(text3);
			if (termData == null)
			{
				Debug.LogError("Term " + text3 + " not found in Inventory Localization");
			}
			else if (termData.Languages.ElementAtOrDefault(idxLanguage) != null)
			{
				string text4 = termData.Languages[idxLanguage];
				switch (text2)
				{
				case "caption":
					entry.caption = text4;
					break;
				case "description":
					entry.description = text4;
					break;
				case "lore":
					entry.lore = text4;
					break;
				}
			}
		}
	}

	private void AddObject<T>(T invObj, List<T> ownObj) where T : BaseInventoryObject
	{
		Core.Metrics.CustomEvent("ITEM_ADQUIRED", invObj.name);
		ownObj.Add(invObj);
		invObj.Add();
		LastAddedObjectType = invObj.GetItemType();
		LastAddedObject = invObj;
		if (invObj.AddPercentageCompletition())
		{
			Core.AchievementsManager.CheckProgressToAC46();
		}
	}

	private void RemoveObjectForNewGamePlus<T>(List<T> ownObj) where T : BaseInventoryObject
	{
		List<T> list = new List<T>(ownObj);
		foreach (T item in list)
		{
			T current = item;
			if (!current.preserveInNewGamePlus)
			{
				ownObj.Remove(current);
				current.Remove();
			}
		}
	}

	private void RemoveObject<T>(T invObj, List<T> ownObj) where T : BaseInventoryObject
	{
		ownObj.Remove(invObj);
		invObj.Remove();
	}

	private void Unequip(EquipableInventoryObject invObj, EquipableInventoryObject[] wearObj)
	{
		for (int i = 0; i < wearObj.Length; i++)
		{
			if (wearObj[i] == invObj)
			{
				invObj.UnEquip();
				wearObj[i] = null;
			}
		}
	}

	private void UnequipAll(EquipableInventoryObject[] wearObj)
	{
		for (int i = 0; i < wearObj.Length; i++)
		{
			if (wearObj[i] != null)
			{
				wearObj[i].UnEquip();
				wearObj[i] = null;
			}
		}
	}

	private void Equip<T>(T invObj, int slot, T[] wearObj) where T : EquipableInventoryObject
	{
		Core.Metrics.CustomEvent("ITEM_EQUIPED", invObj.name);
		wearObj[slot] = invObj;
		invObj.Equip();
		if (this.OnItemEquiped != null)
		{
			this.OnItemEquiped(invObj.id);
		}
	}

	private ReadOnlyCollection<T> GetDictValueReadOnly<T>(Dictionary<string, T> dictionary)
	{
		List<T> list = new List<T>();
		foreach (T value in dictionary.Values)
		{
			list.Add(value);
		}
		return list.AsReadOnly();
	}

	public static List<string> GetAllRelicsId()
	{
		ReloadBaseElemetsIDIfNeed<Relic>(cachedRelicsId);
		return cachedRelicsId;
	}

	public static List<string> GetAllBeadsId()
	{
		ReloadBaseElemetsIDIfNeed<RosaryBead>(cachedBeadsId);
		return cachedBeadsId;
	}

	public static List<string> GetAllQuestItemsId()
	{
		ReloadBaseElemetsIDIfNeed<QuestItem>(cachedQuestItemsId);
		return cachedQuestItemsId;
	}

	public static List<string> GetAllPrayersId()
	{
		ReloadBaseElemetsIDIfNeed<Prayer>(cachedPrayersId);
		return cachedPrayersId;
	}

	public static List<string> GetAllCollectibleItemsId()
	{
		ReloadBaseElemetsIDIfNeed<Framework.Inventory.CollectibleItem>(cachedCollectibleItemsId);
		return cachedCollectibleItemsId;
	}

	public static List<string> GetAllSwordsId()
	{
		ReloadBaseElemetsIDIfNeed<Sword>(cachedSwordsId);
		return cachedSwordsId;
	}

	public static LanguageSource GetLanguageSource()
	{
		GameObject asset = ResourceManager.pInstance.GetAsset<GameObject>("Inventory/Languages");
		return (!asset) ? null : asset.GetComponent<LanguageSource>();
	}

	private static void ReloadBaseElemetsIDIfNeed<BaseClass>(List<string> list) where BaseClass : BaseInventoryObject
	{
		if (forceReload || list.Count == 0)
		{
			list.Clear();
			UnityEngine.Object[] array = Resources.LoadAll("Inventory/" + typeof(BaseClass).Name);
			UnityEngine.Object[] array2 = array;
			foreach (UnityEngine.Object original in array2)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
				BaseClass component = gameObject.GetComponent<BaseClass>();
				if ((UnityEngine.Object)component != (UnityEngine.Object)null)
				{
					list.Add(component.id);
				}
				UnityEngine.Object.Destroy(gameObject);
			}
			list.Sort();
		}
		forceReload = false;
	}

	public int GetOrder()
	{
		return 0;
	}

	public string GetPersistenID()
	{
		return "ID_INVENTORY";
	}

	public void ResetPersistence()
	{
		ResetObjectsEffects();
		InitializeObjects();
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		InventoryPersistenceData inventoryPersistenceData = new InventoryPersistenceData();
		inventoryPersistenceData.ownRellics = GetListId(ownRellics);
		for (int i = 0; i < wearRellics.Length; i++)
		{
			inventoryPersistenceData.wearRellics[i] = GetIdIfAny(wearRellics[i]);
		}
		inventoryPersistenceData.ownBeads = GetListId(ownBeads);
		for (int j = 0; j < wearBeads.Length; j++)
		{
			inventoryPersistenceData.wearBeads[j] = GetIdIfAny(wearBeads[j]);
		}
		inventoryPersistenceData.ownQuestItems = GetListId(ownQuestItems);
		inventoryPersistenceData.ownCollectibleItems = GetListId(ownCollectibleItems);
		inventoryPersistenceData.ownSwords = GetListId(ownSwords);
		for (int k = 0; k < wearSwords.Length; k++)
		{
			inventoryPersistenceData.wearSwords[k] = GetIdIfAny(wearSwords[k]);
		}
		inventoryPersistenceData.ownPrayers = GetListId(ownPrayers);
		for (int l = 0; l < wearPrayers.Length; l++)
		{
			inventoryPersistenceData.wearPrayers[l] = GetIdIfAny(wearPrayers[l]);
		}
		foreach (KeyValuePair<string, Prayer> allPrayer in allPrayers)
		{
			if (allPrayer.Value.CurrentDecipher != 0)
			{
				inventoryPersistenceData.prayerDecipher[allPrayer.Value.id] = allPrayer.Value.CurrentDecipher;
			}
		}
		for (int m = 0; m < ownBossKeys.Length; m++)
		{
			inventoryPersistenceData.keys[m] = ownBossKeys[m];
		}
		return inventoryPersistenceData;
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		InventoryPersistenceData inventoryPersistenceData = (InventoryPersistenceData)data;
		ownRellics = GetInstanceList(inventoryPersistenceData.ownRellics, GetRelic);
		for (int i = 0; i < inventoryPersistenceData.wearRellics.Length; i++)
		{
			string idRelic = inventoryPersistenceData.wearRellics[i];
			SetRelicInSlot(i, idRelic);
		}
		ownBeads = GetInstanceList(inventoryPersistenceData.ownBeads, GetRosaryBead);
		for (int j = 0; j < inventoryPersistenceData.wearBeads.Length; j++)
		{
			string idRosaryBead = inventoryPersistenceData.wearBeads[j];
			SetRosaryBeadInSlot(j, idRosaryBead);
		}
		ownQuestItems = GetInstanceList(inventoryPersistenceData.ownQuestItems, GetQuestItem);
		ownCollectibleItems = GetInstanceList(inventoryPersistenceData.ownCollectibleItems, GetCollectibleItem);
		ownSwords = GetInstanceList(inventoryPersistenceData.ownSwords, GetSword);
		for (int k = 0; k < inventoryPersistenceData.wearSwords.Length; k++)
		{
			string idSword = inventoryPersistenceData.wearSwords[k];
			SetSwordInSlot(k, idSword);
		}
		ownPrayers = GetInstanceList(inventoryPersistenceData.ownPrayers, GetPrayer);
		for (int l = 0; l < inventoryPersistenceData.wearPrayers.Length; l++)
		{
			string idPrayer = inventoryPersistenceData.wearPrayers[l];
			SetPrayerInSlot(l, idPrayer);
		}
		foreach (KeyValuePair<string, Prayer> allPrayer in allPrayers)
		{
			Prayer value = allPrayer.Value;
			value.ResetDecipher();
			if (inventoryPersistenceData.prayerDecipher.ContainsKey(value.id))
			{
				value.AddDecipher(inventoryPersistenceData.prayerDecipher[value.id]);
			}
		}
		for (int m = 0; m < inventoryPersistenceData.keys.Length; m++)
		{
			ownBossKeys[m] = inventoryPersistenceData.keys[m];
		}
	}

	public void ResetObjectsEffects()
	{
		ResetEffects(ownBeads);
		ResetEffects(ownCollectibleItems);
		ResetEffects(ownPrayers);
		ResetEffects(ownQuestItems);
		ResetEffects(ownRellics);
		ResetEffects(ownSwords);
	}

	private void ResetEffects<T>(List<T> ownObjects) where T : BaseInventoryObject
	{
		List<T> list = new List<T>(ownObjects);
		foreach (T item in list)
		{
			T current = item;
			current.Reset();
		}
	}

	private string GetIdIfAny<T>(T element) where T : BaseInventoryObject
	{
		string result = string.Empty;
		if ((bool)(UnityEngine.Object)element)
		{
			result = element.id;
		}
		return result;
	}

	private List<string> GetListId<T>(List<T> instanceList) where T : BaseInventoryObject
	{
		List<string> list = new List<string>();
		foreach (T instance in instanceList)
		{
			list.Add(instance.id);
		}
		return list;
	}

	private List<T> GetInstanceList<T>(List<string> idList, Func<string, T> funcGet) where T : BaseInventoryObject
	{
		List<T> list = new List<T>();
		foreach (string id in idList)
		{
			T val = funcGet(id);
			if ((bool)(UnityEngine.Object)val)
			{
				list.Add(val);
				val.Add();
			}
			else
			{
				Debug.LogError("*** Inventory Persistence, missing ID:" + id);
			}
		}
		return list;
	}
}
