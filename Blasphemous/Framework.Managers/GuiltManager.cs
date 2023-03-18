using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Framework.FrameworkCore;
using Framework.Map;
using Gameplay.GameControllers.Penitent;
using Gameplay.UI;
using Tools.DataContainer;
using Tools.Level.Interactables;
using Tools.Level.Layout;
using UnityEngine;

namespace Framework.Managers;

public class GuiltManager : GameSystem, PersistentInterface
{
	[Serializable]
	public class GuiltDrop
	{
		public string id;

		public string scene;

		public Vector3 position;

		public List<string> linkedIds = new List<string>();

		public bool isLinked;

		public CellKey cellKey;
	}

	[Serializable]
	public class GuiltPersistenceData : PersistentManager.PersistentData
	{
		public List<GuiltDrop> guiltDrops;

		public bool DropSingleGuilt;

		public bool DropTearsAlongWithGuilt;

		public float DroppedTears;

		public GuiltPersistenceData()
			: base("ID_GUILT")
		{
		}
	}

	private const string GUILD_RESOURCE_CONFIG = "GuiltConfig";

	private GuiltConfigData guiltConfig;

	private List<GuiltDrop> guiltDrops = new List<GuiltDrop>();

	public const int GLOBAL_SAFE_POSITION_X = -513;

	public const int GLOBAL_SAFE_POSITION_Y = 11;

	public const string GLOBAL_SAFE_LEVEL = "D01Z02S01";

	public bool DropSingleGuilt;

	public bool DropTearsAlongWithGuilt;

	public float DroppedTears;

	private const string PERSITENT_ID = "ID_GUILT";

	private Penitent Penitent { get; set; }

	public override void Start()
	{
		guiltConfig = Resources.Load<GuiltConfigData>("GuiltConfig");
		InitializeAll();
	}

	public override void AllInitialized()
	{
		Core.Persistence.AddPersistentManager(this);
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		SpawnManager.OnPlayerSpawn += OnPenitentReady;
	}

	private void InitializeAll()
	{
		guiltDrops.Clear();
	}

	private void OnPenitentReady(Penitent penitent)
	{
		if (!Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.BOSS_RUSH) && !Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE))
		{
			Penitent = penitent;
			Penitent penitent2 = Penitent;
			penitent2.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent2.OnDead, new Core.SimpleEvent(OnPenitentDead));
			Penitent.Stats.Fervour.MaxFactor = GetFervourMaxFactor();
		}
	}

	private void OnPenitentDead()
	{
		if (Core.Logic.CurrentLevelConfig.GuiltConfiguration != LevelInitializer.GuiltConfigurationEnum.DontGenerateGuilt && Core.Logic.Penitent.GuiltDrop)
		{
			AddDrop();
			if (DropTearsAlongWithGuilt)
			{
				DroppedTears = Core.Logic.Penitent.Stats.Purge.Current;
				Core.Logic.Penitent.Stats.Purge.Current = 0f;
			}
		}
		Core.InventoryManager.OnPenitentDead();
		if ((bool)Penitent)
		{
			Penitent penitent = Penitent;
			penitent.OnDead = (Core.SimpleEvent)Delegate.Remove(penitent.OnDead, new Core.SimpleEvent(OnPenitentDead));
		}
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		guiltDrops.ForEach(delegate(GuiltDrop drop)
		{
			InstanciateDrop(drop);
		});
	}

	public void OnDropTaken(string ID)
	{
		GuiltDrop guiltDrop = guiltDrops.FirstOrDefault((GuiltDrop x) => x.id == ID && x.scene == Core.LevelManager.currentLevel.LevelName);
		if (guiltDrop != null)
		{
			foreach (string linkedId in guiltDrop.linkedIds)
			{
				GuiltDrop guiltDrop2 = guiltDrops.FirstOrDefault((GuiltDrop x) => x.id == linkedId && x.scene == Core.LevelManager.currentLevel.LevelName);
				if (guiltDrop2 != null)
				{
					guiltDrops.Remove(guiltDrop2);
					continue;
				}
				Debug.LogError("Linked Drop taken NOT found ID:" + ID + "  IdLinked:" + linkedId + "  Scene" + Core.LevelManager.currentLevel.LevelName + "  Current: " + Core.LevelManager.currentLevel.LevelName);
			}
			guiltDrops.Remove(guiltDrop);
			UpdateGuilt(whenDead: false);
		}
		else
		{
			Debug.LogError("Drop taken NOT found ID:" + ID + " Scene" + Core.LevelManager.currentLevel.LevelName + "  Current: " + Core.LevelManager.currentLevel.LevelName);
		}
	}

	public float GetFervourGainFactor()
	{
		float result = guiltConfig.fervourGainFactor.Evaluate(guiltDrops.Count);
		if (guiltDrops.Count > 0 && DropSingleGuilt)
		{
			result = guiltConfig.fervourGainFactor.Evaluate(guiltConfig.maxDeathsDrops);
		}
		return result;
	}

	public float GetFervourMaxFactor()
	{
		if (!guiltConfig)
		{
			return 0f;
		}
		float result = guiltConfig.fervourMaxFactor.Evaluate(guiltDrops.Count);
		if (guiltDrops.Count > 0 && DropSingleGuilt)
		{
			result = guiltConfig.fervourMaxFactor.Evaluate(guiltConfig.maxDeathsDrops);
		}
		return result;
	}

	public float GetPurgeGainFactor()
	{
		float result = guiltConfig.purgeGainFactor.Evaluate(guiltDrops.Count);
		if (guiltDrops.Count > 0 && DropSingleGuilt)
		{
			result = guiltConfig.purgeGainFactor.Evaluate(guiltConfig.maxDeathsDrops);
		}
		return result;
	}

	public ReadOnlyCollection<GuiltDrop> GetAllDrops()
	{
		return guiltDrops.AsReadOnly();
	}

	public List<GuiltDrop> GetAllCurrentMapDrops()
	{
		return guiltDrops.ToList();
	}

	public int GetDropsCount()
	{
		int num = guiltDrops.Count;
		if (num > 0 && DropSingleGuilt)
		{
			num = guiltConfig.maxDeathsDrops;
		}
		return num;
	}

	public void ResetGuilt(bool restoreDropTears)
	{
		if (restoreDropTears && DropTearsAlongWithGuilt)
		{
			Core.Logic.Penitent.Stats.Purge.Current += Core.GuiltManager.DroppedTears;
			Core.GuiltManager.DroppedTears = 0f;
		}
		guiltDrops.Clear();
		InteractableGuiltDrop[] array = UnityEngine.Object.FindObjectsOfType<InteractableGuiltDrop>();
		InteractableGuiltDrop[] array2 = array;
		foreach (InteractableGuiltDrop interactableGuiltDrop in array2)
		{
			UnityEngine.Object.Destroy(interactableGuiltDrop.gameObject);
		}
		UpdateGuilt(whenDead: true);
	}

	public void AddGuilt()
	{
		GuiltDrop drop = AddDrop();
		InstanciateDrop(drop);
		UpdateGuilt(whenDead: true);
	}

	public void DEBUG_AddAllGuilts()
	{
		for (int i = 0; i < guiltConfig.maxDeathsDrops; i++)
		{
			GuiltDrop guiltDrop = AddDrop();
		}
	}

	private void UpdateGuilt(bool whenDead)
	{
		if (Core.Logic.Penitent != null)
		{
			Core.Logic.Penitent.Stats.Fervour.MaxFactor = GetFervourMaxFactor();
		}
		UIController.instance.UpdateGuiltLevel(whenDead);
	}

	private GuiltDrop AddDrop()
	{
		if (guiltDrops.Count >= guiltConfig.maxDeathsDrops)
		{
			return null;
		}
		if (DropSingleGuilt && guiltDrops.Count == 1)
		{
			ResetGuilt(restoreDropTears: false);
		}
		GuiltDrop drop = new GuiltDrop();
		do
		{
			drop.id = Guid.NewGuid().ToString();
		}
		while (guiltDrops.FirstOrDefault((GuiltDrop p) => p.id == drop.id) != null);
		if (Core.Logic.CurrentLevelConfig.OverrideGuiltPosition)
		{
			drop.position = Core.Logic.CurrentLevelConfig.guiltPositionOverrider.position;
			drop.scene = Core.LevelManager.currentLevel.LevelName;
		}
		else if (Core.LevelManager.LastSafeGuiltLevel == string.Empty)
		{
			Debug.LogWarning("No safe position for Guilt, placing on safe level");
			drop.position = new Vector3(-513f, 11f);
			drop.scene = "D01Z02S01";
		}
		else
		{
			drop.position = Core.LevelManager.LastSafeGuiltPosition;
			drop.scene = Core.LevelManager.LastSafeGuiltLevel;
			drop.cellKey = Core.LevelManager.LastSafeGuiltCellKey;
		}
		if (drop.cellKey == null)
		{
			drop.cellKey = Core.NewMapManager.GetCellKeyFromPosition(drop.scene, drop.position);
		}
		if (drop.cellKey == null)
		{
			drop.cellKey = Core.NewMapManager.GetPlayerCell();
			Debug.LogWarning(string.Concat("[Debug only message] Unable to find CellKey for new guilt drop. SCENE:", drop.scene, "  POS:", drop.position, " LEVEL:", Core.LevelManager.currentLevel.LevelName, "  OVERRIDE:", Core.Logic.CurrentLevelConfig.OverrideGuiltPosition));
		}
		foreach (GuiltDrop guiltDrop in guiltDrops)
		{
			if (!guiltDrop.isLinked && guiltDrop.scene == drop.scene)
			{
				float magnitude = (guiltDrop.position - drop.position).magnitude;
				if (magnitude <= guiltConfig.MaxDistanceToLink)
				{
					drop.isLinked = true;
					guiltDrop.linkedIds.Add(drop.id);
					break;
				}
			}
		}
		guiltDrops.Add(drop);
		return drop;
	}

	private void InstanciateDrop(GuiltDrop drop)
	{
		if (drop != null && !drop.isLinked && drop.scene == Core.LevelManager.currentLevel.LevelName)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(guiltConfig.dropPrefab, drop.position, Quaternion.identity);
			gameObject.GetComponent<InteractableGuiltDrop>().SetDropId(drop.id);
		}
	}

	public int GetOrder()
	{
		return 10;
	}

	public string GetPersistenID()
	{
		return "ID_GUILT";
	}

	public void ResetPersistence()
	{
		InitializeAll();
		DropSingleGuilt = false;
		DropTearsAlongWithGuilt = false;
		DroppedTears = 0f;
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		GuiltPersistenceData guiltPersistenceData = new GuiltPersistenceData();
		guiltPersistenceData.guiltDrops = new List<GuiltDrop>(guiltDrops);
		guiltPersistenceData.DropSingleGuilt = DropSingleGuilt;
		guiltPersistenceData.DropTearsAlongWithGuilt = DropTearsAlongWithGuilt;
		guiltPersistenceData.DroppedTears = DroppedTears;
		return guiltPersistenceData;
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		GuiltPersistenceData guiltPersistenceData = (GuiltPersistenceData)data;
		guiltDrops = new List<GuiltDrop>(guiltPersistenceData.guiltDrops);
		foreach (GuiltDrop guiltDrop in guiltDrops)
		{
			if (guiltDrop.cellKey == null)
			{
				guiltDrop.cellKey = Core.NewMapManager.GetCellKeyFromPosition(guiltDrop.position);
			}
		}
		DropSingleGuilt = guiltPersistenceData.DropSingleGuilt;
		DropTearsAlongWithGuilt = guiltPersistenceData.DropTearsAlongWithGuilt;
		DroppedTears = guiltPersistenceData.DroppedTears;
	}

	public override void OnGUI()
	{
		DebugResetLine();
		DebugDrawTextLine("GuiltManager -------------------------------------");
		DebugDrawTextLine("-- Config max drops: " + guiltConfig.maxDeathsDrops);
		DebugDrawTextLine("   Config max distance to link: " + guiltConfig.MaxDistanceToLink);
		DebugDrawTextLine("-- Current Fervour Gain Factor: " + GetFervourGainFactor());
		DebugDrawTextLine("   Current Fervour Max Factor: " + GetFervourMaxFactor());
		DebugDrawTextLine("   Current Purge Gain Factor: " + GetPurgeGainFactor());
		DebugDrawTextLine("-- DROPS: " + guiltDrops.Count);
		foreach (GuiltDrop guiltDrop in guiltDrops)
		{
			string text = "    ID: " + guiltDrop.id + " Scene:" + guiltDrop.scene + "  POS: " + guiltDrop.position.ToString();
			if (guiltDrop.isLinked)
			{
				text += "  LINKED";
			}
			DebugDrawTextLine(text);
			if (guiltDrop.linkedIds.Count <= 0)
			{
				continue;
			}
			foreach (string linkedId in guiltDrop.linkedIds)
			{
				DebugDrawTextLine("        LINKED ID: " + linkedId);
			}
		}
	}
}
