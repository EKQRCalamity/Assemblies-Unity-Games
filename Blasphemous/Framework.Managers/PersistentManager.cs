using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Framework.FrameworkCore;
using Framework.Map;
using Framework.Penitences;
using FullSerializer;
using Gameplay.GameControllers.Entities;
using Sirenix.Utilities;
using Steamworks;
using Tools;
using UnityEngine;

namespace Framework.Managers;

public class PersistentManager : GameSystem, PersistentInterface
{
	[Serializable]
	public abstract class PersistentData
	{
		public string debugName = "Manager";

		[fsIgnore]
		public string UniqId { get; private set; }

		public PersistentData()
		{
			UniqId = string.Empty;
		}

		public PersistentData(string uniqId)
		{
			UniqId = uniqId;
		}
	}

	[Serializable]
	public class SnapShot
	{
		public Dictionary<string, PersistentData> commonElements;

		public Dictionary<string, Dictionary<string, PersistentData>> sceneElements;

		public SnapShot()
		{
			commonElements = new Dictionary<string, PersistentData>();
			sceneElements = new Dictionary<string, Dictionary<string, PersistentData>>();
		}

		public void Clear()
		{
			commonElements.Clear();
			sceneElements.Clear();
		}

		public void Copy(SnapShot other)
		{
			commonElements = new Dictionary<string, PersistentData>(other.commonElements);
			sceneElements = new Dictionary<string, Dictionary<string, PersistentData>>(other.sceneElements);
		}
	}

	private class DEBUGPersistence : PersistentData
	{
		public bool boolean1;

		public bool boolean2;

		public float value1 = 1230.4f;

		public float value2 = 9191f;

		public DEBUGPersistence(string id)
			: base(id)
		{
		}
	}

	public enum PercentageType
	{
		BossDefeated_1,
		BossDefeated_2,
		Upgraded,
		Exploration,
		Teleport_A,
		EndingA,
		ItemAdded,
		Custom,
		Map,
		Map_NgPlus,
		BossDefeated_NgPlus,
		Penitence_NgPlus,
		Teleport_B
	}

	[Serializable]
	public class PersitentPersistenceData : PersistentData
	{
		public bool Corrupted;

		public string CurrentDomain = string.Empty;

		public string CurrentZone = string.Empty;

		public bool HasBackup;

		public float Percent;

		public float Purge;

		public float Time;

		public float TimeAtSlotAscension;

		public bool IsNewGamePlus;

		public bool CanConvertToNewGamePlus;

		public int NewGamePlusUpgrades;

		public PersitentPersistenceData()
			: base("ID_PERSISTENT_MANAGER")
		{
		}
	}

	public class PublicSlotData
	{
		public PersitentPersistenceData persistence;

		public PenitenceManager.PenitencePersistenceData penitence;

		public EventManager.FlagPersistenceData flags;

		public AchievementsManager.AchievementPersistenceData achievement;
	}

	private const int NOT_INITIALIZED_SLOT = -1;

	private const string BACKUP_SUFIX = "_backup";

	private static int CurrentSaveSlot = -1;

	private fsSerializer serializer = new fsSerializer();

	private bool InsideLoadLevelProcess;

	private bool InsideSaveProcess;

	private bool InsideLoadProcess;

	private readonly List<PersistentInterface> persistenceSystems = new List<PersistentInterface>();

	private SnapShot currentSnapshot;

	private const string EVENT_PERSITENT_ID = "ID_PERSISTENT_MANAGER";

	public const string APP_SETTINGS_PATH = "/app_settings";

	private const string REWARDS_FIX_KEY = "REWARDS_FIX";

	private const int MAX_SLOTS = 3;

	private static Dictionary<string, string> SkinsAndObjects = new Dictionary<string, string>
	{
		{ "PENITENT_PE01", "RB101" },
		{ "PENITENT_PE02", "RB102" },
		{ "PENITENT_PE03", "RB103" }
	};

	private static bool FixHasBeenApplied = false;

	private float LastTimeStored { get; set; }

	private float TimeStored { get; set; }

	private float TimeAtSlotAscension { get; set; }

	public float PercentCompleted
	{
		get
		{
			float percentageCompletition = Core.InventoryManager.GetPercentageCompletition();
			percentageCompletition += Core.Events.GetPercentageCompletition();
			EntityStats stats = Core.Logic.Penitent.Stats;
			float num = (float)(stats.Life.GetUpgrades() + stats.Fervour.GetUpgrades() + stats.MeaCulpa.GetUpgrades()) * GameConstants.PercentageValues[PercentageType.Upgraded];
			percentageCompletition += num;
			percentageCompletition += Core.NewMapManager.GetPercentageCompletition();
			percentageCompletition += Core.SpawnManager.GetPercentageCompletition();
			if (Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.NEW_GAME_PLUS))
			{
				float percentageCompletition2 = Core.PenitenceManager.GetPercentageCompletition();
				percentageCompletition += percentageCompletition2;
			}
			return percentageCompletition;
		}
	}

	public override void Initialize()
	{
		currentSnapshot = new SnapShot();
		persistenceSystems.Clear();
		serializer = new fsSerializer();
		LastTimeStored = 0f;
		ResetPersistence();
		AddPersistentManager(this);
	}

	public void AddPersistentManager(PersistentInterface manager)
	{
		if (!persistenceSystems.Contains(manager))
		{
			persistenceSystems.Add(manager);
		}
	}

	public void ResetAll()
	{
		Log.Debug("[Persistence]", "Reset all");
		currentSnapshot.Clear();
		foreach (PersistentInterface item in persistenceSystems.OrderBy((PersistentInterface x) => x.GetOrder()))
		{
			item.ResetPersistence();
		}
		if ((bool)Core.Logic.Penitent)
		{
			EntityStats stats = Core.Logic.Penitent.Stats;
			stats.ResetPersistence();
		}
	}

	public void ResetCurrent()
	{
		Log.Debug("Persistence", "ResetCurrent - Reseting data on \"" + Core.LevelManager.currentLevel.LevelName + "\".");
		LoadSnapShot(currentSnapshot, Core.LevelManager.currentLevel.LevelName, isloading: false, CreateAndGetSaveGameInternalDir(GetAutomaticSlot()));
	}

	public void RestoreStored()
	{
		Log.Debug("[Persistence]", "RestoreStored");
		Core.Logic.Penitent.Stats.Life.SetToCurrentMax();
		Core.Logic.Penitent.Stats.Flask.SetToCurrentMax();
		if (Core.Alms.GetPrieDieuLevel() > 1)
		{
			Core.Logic.Penitent.Stats.Fervour.SetToCurrentMax();
		}
		else
		{
			Core.Logic.Penitent.Stats.Fervour.Current = 0f;
		}
		Core.InventoryManager.ResetObjectsEffects();
		SaveGame();
	}

	public void PrepareForNewGamePlus()
	{
		currentSnapshot.sceneElements.Clear();
		TimeAtSlotAscension = GetCurrentTimePlayed();
	}

	public void DEBUG_SaveAllData(int slot, int mapReveals, int elementPerScene)
	{
		Core.GuiltManager.DEBUG_AddAllGuilts();
		Core.InventoryManager.TestAddAllObjects();
		foreach (string item in Core.LevelManager.DEBUG_GetAllLevelsName())
		{
			string sceneKeyName = GetSceneKeyName(item);
			if (currentSnapshot.sceneElements.ContainsKey(sceneKeyName))
			{
				currentSnapshot.sceneElements[sceneKeyName].Clear();
			}
			currentSnapshot.sceneElements[sceneKeyName] = new Dictionary<string, PersistentData>();
			for (int i = 0; i < elementPerScene; i++)
			{
				string text = sceneKeyName + "_" + i + ToString() + "_TEST_ID_VERY_LONG_ID_LIKE_EVERY_IDS";
				DEBUGPersistence value = new DEBUGPersistence(text);
				currentSnapshot.sceneElements[sceneKeyName][text] = value;
			}
		}
		SaveGame(slot);
	}

	public void SaveGame(int slot, bool fullSave = true)
	{
		Debug.Log("**** Persistent: SAVEGAME");
		SetAutomaticSlot(slot);
		if (GetAutomaticSlot() == -1)
		{
			Debug.LogError("Trying to save slot without initializing");
			return;
		}
		string dataPath = CreateAndGetSaveGameInternalDir(slot);
		SaveSnapShot(currentSnapshot, Core.LevelManager.currentLevel.LevelName, fullSave, dataPath);
	}

	public void SaveGame(bool fullSave = true)
	{
		SaveGame(GetAutomaticSlot(), fullSave);
	}

	public bool DeleteSaveGame(int slot)
	{
		bool result = false;
		if (ExistSlot(slot))
		{
			DeleteSaveGame_Internal(GetSaveGameFile(slot), GetSaveGameInternalName(slot));
			result = true;
		}
		if (ExistBackupSlot(slot))
		{
			DeleteSaveGame_Internal(GetSaveGameBackupFile(slot), GetSaveGameBackupInternalName(slot));
			result = true;
		}
		return result;
	}

	public bool LoadGame(int slot)
	{
		SetAutomaticSlot(slot);
		bool flag = LoadGameWithOutRespawn(slot);
		if (flag)
		{
			Core.Logic.EnemySpawner.Reset();
			Core.SpawnManager.FirstSpanw = true;
			Core.SpawnManager.Respawn();
		}
		return flag;
	}

	public bool LoadGameWithOutRespawn(int slot)
	{
		Debug.Log("**** Persistent: LOADGAME");
		Core.Logic.ResetAllData();
		bool flag = LoadGameSnapShot(slot, ref currentSnapshot);
		PublicSlotData slotData = Core.Persistence.GetSlotData(slot);
		if ((!flag || slotData.persistence.Corrupted) && ExistBackupSlot(slot))
		{
			Debug.LogWarning("Loading backup for corrupted slot " + slot);
			RestoreBackup(slot);
			flag = LoadGameSnapShot(slot, ref currentSnapshot);
		}
		if (flag)
		{
			BackupSlot(slot);
			LoadSnapShot(currentSnapshot, string.Empty, isloading: true, CreateAndGetSaveGameInternalDir(slot));
		}
		return flag;
	}

	public bool ExistSlot(int slot)
	{
		string saveGameFile = GetSaveGameFile(slot);
		return File.Exists(saveGameFile);
	}

	public bool ExistBackupSlot(int slot)
	{
		string saveGameBackupFile = GetSaveGameBackupFile(slot);
		return File.Exists(saveGameBackupFile);
	}

	public PublicSlotData GetSlotData(int slot)
	{
		PublicSlotData publicSlotData = null;
		SnapShot snap = new SnapShot();
		bool corrupted = false;
		bool flag = LoadGameSnapShot(slot, ref snap);
		bool flag2 = ExistBackupSlot(slot);
		if (!flag && flag2)
		{
			flag = LoadGameBackupSnapShot(slot, ref snap);
			corrupted = true;
		}
		if (flag)
		{
			publicSlotData = new PublicSlotData();
			if (snap.commonElements.ContainsKey(GetPersistenID()))
			{
				publicSlotData.persistence = (PersitentPersistenceData)snap.commonElements[GetPersistenID()];
				ZoneKey sceneKey = new ZoneKey(publicSlotData.persistence.CurrentDomain, publicSlotData.persistence.CurrentZone, string.Empty);
				if (!Core.NewMapManager.ZoneHasName(sceneKey))
				{
					SpawnManager.CheckPointPersistenceData checkPointPersistenceData = (SpawnManager.CheckPointPersistenceData)snap.commonElements["ID_CHECKPOINT_MANAGER"];
					if (!checkPointPersistenceData.activePrieDieuScene.IsNullOrWhitespace())
					{
						publicSlotData.persistence.CurrentDomain = checkPointPersistenceData.activePrieDieuScene.Substring(0, 3);
						publicSlotData.persistence.CurrentZone = checkPointPersistenceData.activePrieDieuScene.Substring(3, 3);
					}
					else
					{
						corrupted = true;
					}
				}
				publicSlotData.persistence.CanConvertToNewGamePlus = Core.GameModeManager.CanConvertToNewGamePlus(snap);
				publicSlotData.persistence.Corrupted = corrupted;
				publicSlotData.persistence.HasBackup = flag2;
			}
			string persistenID = Core.PenitenceManager.GetPersistenID();
			if (snap.commonElements.ContainsKey(persistenID))
			{
				publicSlotData.penitence = (PenitenceManager.PenitencePersistenceData)snap.commonElements[persistenID];
			}
			persistenID = Core.Events.GetPersistenID();
			if (snap.commonElements.ContainsKey(persistenID))
			{
				publicSlotData.flags = (EventManager.FlagPersistenceData)snap.commonElements[persistenID];
			}
			persistenID = Core.AchievementsManager.GetPersistenID();
			if (snap.commonElements.ContainsKey(persistenID))
			{
				publicSlotData.achievement = (AchievementsManager.AchievementPersistenceData)snap.commonElements[persistenID];
			}
		}
		else if (ExistSlot(slot))
		{
			publicSlotData = new PublicSlotData();
			publicSlotData.persistence = new PersitentPersistenceData();
			publicSlotData.persistence.Corrupted = true;
		}
		return publicSlotData;
	}

	public static int GetAutomaticSlot()
	{
		return CurrentSaveSlot;
	}

	public static void SetAutomaticSlot(int slot)
	{
		CurrentSaveSlot = slot;
	}

	public static void ResetAutomaticSlot()
	{
		CurrentSaveSlot = -1;
	}

	public void HACK_EnableNewGamePlusInCurrent()
	{
		string text = "d07z01s03";
		if (!currentSnapshot.sceneElements.ContainsKey(text))
		{
			currentSnapshot.sceneElements[text] = new Dictionary<string, PersistentData>();
		}
		string text2 = text + "__TEST_ID_VERY_LONG_ID_LIKE_EVERY_IDS";
		DEBUGPersistence value = new DEBUGPersistence(text2);
		currentSnapshot.sceneElements[text][text2] = value;
	}

	public void OnBeforeLevelLoad(Level oldLevel, Level newLevel)
	{
		if (oldLevel != null && !oldLevel.LevelName.Equals("MainMenu") && !oldLevel.LevelName.Equals("D07Z01S04"))
		{
			Debug.Log("**** PERSITENT EVENT: OnBeforeLevelLoad " + oldLevel.LevelName);
			if (GetAutomaticSlot() == -1)
			{
				Debug.LogError("Trying to save slot without initializing");
				return;
			}
			SaveSnapShot(currentSnapshot, oldLevel.LevelName, fullSave: false, CreateAndGetSaveGameInternalDir(GetAutomaticSlot()));
			InsideLoadLevelProcess = true;
			Log.Trace("Persistence", "Persistence SAVE: " + oldLevel.LevelName);
		}
	}

	public void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		CheckAndApplyBeadFix();
		InsideLoadLevelProcess = false;
		if (newLevel != null && !newLevel.LevelName.Equals("MainMenu"))
		{
			Debug.Log("**** PERSITENT EVENT: OnLevelLoaded " + newLevel.LevelName);
			LoadSnapShot(currentSnapshot, newLevel.LevelName, isloading: false, CreateAndGetSaveGameInternalDir(GetAutomaticSlot()));
		}
	}

	private void DeleteSaveGame_Internal(string file, string internalDir)
	{
		if (File.Exists(file))
		{
			File.Delete(file);
		}
		if (!internalDir.EndsWith("/"))
		{
			internalDir += "/";
		}
		if (Directory.Exists(internalDir))
		{
			Directory.Delete(internalDir, recursive: true);
		}
	}

	private string GetSaveGameBackupInternalName(int slot)
	{
		return GetSaveGameInternalName(slot) + "_backup";
	}

	private string GetSaveGameBackupFile(int slot)
	{
		return GetSaveGameFile(slot) + "_backup";
	}

	private bool BackupSlot(int slot)
	{
		if (!ExistSlot(slot))
		{
			return false;
		}
		try
		{
			File.Copy(GetSaveGameFile(slot), GetSaveGameBackupFile(slot), overwrite: true);
			string saveGameInternalName = GetSaveGameInternalName(slot);
			string saveGameBackupInternalName = GetSaveGameBackupInternalName(slot);
			FileTools.DirectoryCopy(saveGameInternalName, saveGameBackupInternalName, copySubDirs: true, overwrite: true);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private bool RestoreBackup(int slot)
	{
		if (!ExistBackupSlot(slot))
		{
			return false;
		}
		try
		{
			File.Copy(GetSaveGameBackupFile(slot), GetSaveGameFile(slot), overwrite: true);
			string saveGameInternalName = GetSaveGameInternalName(slot);
			string saveGameBackupInternalName = GetSaveGameBackupInternalName(slot);
			FileTools.DirectoryCopy(saveGameBackupInternalName, saveGameInternalName, copySubDirs: true, overwrite: true);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private bool LoadGameSnapShot(int slot, ref SnapShot snap)
	{
		if (!ExistSlot(slot))
		{
			return false;
		}
		string saveGameFile = GetSaveGameFile(slot);
		return LoadGameSnapShotInternal(saveGameFile, ref snap);
	}

	private bool LoadGameBackupSnapShot(int slot, ref SnapShot snap)
	{
		if (!ExistBackupSlot(slot))
		{
			return false;
		}
		string saveGameBackupFile = GetSaveGameBackupFile(slot);
		return LoadGameSnapShotInternal(saveGameBackupFile, ref snap);
	}

	private bool LoadGameSnapShotInternal(string path, ref SnapShot snap)
	{
		bool flag = true;
		string input = string.Empty;
		try
		{
			string s = File.ReadAllText(path);
			byte[] bytes = Convert.FromBase64String(s);
			input = Encoding.UTF8.GetString(bytes);
		}
		catch (Exception)
		{
			flag = false;
		}
		if (flag)
		{
			fsData data;
			fsResult fsResult = fsJsonParser.Parse(input, out data);
			if (fsResult.Failed)
			{
				Debug.LogError("** LoadGame parsing error: " + fsResult.FormattedMessages);
				flag = false;
			}
			else
			{
				try
				{
					fsResult = serializer.TryDeserialize(data, ref snap);
				}
				catch (Exception ex2)
				{
					Debug.LogError("** LoadGame deserialization exception: " + ex2.Message);
					flag = false;
				}
				finally
				{
					if (fsResult.Failed)
					{
						Debug.LogError("** LoadGame deserialization error: " + fsResult.FormattedMessages);
						flag = false;
					}
				}
			}
		}
		return flag;
	}

	private bool SaveGame_Internal(int slot)
	{
		string saveGameFile = GetSaveGameFile(slot);
		Debug.Log("* SaveGame, saving slot " + slot + " to file " + saveGameFile);
		fsData data;
		fsResult fsResult = serializer.TrySerialize(currentSnapshot, out data);
		if (fsResult.Failed)
		{
			Debug.LogError("** SaveGame error: " + fsResult.FormattedMessages);
			return false;
		}
		string s = fsJsonPrinter.CompressedJson(data);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		string encryptedData = Convert.ToBase64String(bytes);
		FileTools.SaveSecure(saveGameFile, encryptedData);
		return true;
	}

	private string CreateAndGetSaveGameInternalDir(int slot)
	{
		string text = GetSaveGameInternalName(slot) + "/";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text;
	}

	private string GetSaveGameInternalName(int slot)
	{
		string fileName = "savegame_" + slot;
		return GetPathAppSettings(fileName);
	}

	private string GetSaveGameFile(int slot)
	{
		return GetSaveGameInternalName(slot) + ".save";
	}

	private void SaveSnapShot(SnapShot snapShot, string scene, bool fullSave, string dataPath)
	{
		if (InsideLoadLevelProcess)
		{
			Debug.LogError("*** SAVEERROR 1. SAVE INSIDE LOAD LEVEL PROCESS, PLEASE LOOK TRACE AND CALL A DEVELOPER");
			return;
		}
		if (InsideSaveProcess)
		{
			Debug.LogError("*** SAVEERROR 2. SAVE INSIDE SAVE PROCESS, PLEASE LOOK TRACE AND CALL A DEVELOPER");
			return;
		}
		if (InsideLoadProcess)
		{
			Debug.LogError("*** SAVEERROR 3. SAVE INSIDE LOAD PROCESS, PLEASE LOOK TRACE AND CALL A DEVELOPER");
			return;
		}
		InsideSaveProcess = true;
		string sceneKeyName = GetSceneKeyName(scene);
		if (snapShot.sceneElements.ContainsKey(sceneKeyName))
		{
			snapShot.sceneElements[sceneKeyName].Clear();
		}
		else
		{
			snapShot.sceneElements[sceneKeyName] = new Dictionary<string, PersistentData>();
		}
		PersistentObject[] array = UnityEngine.Object.FindObjectsOfType<PersistentObject>();
		PersistentObject[] array2 = array;
		foreach (PersistentObject persistentObject in array2)
		{
			if (persistentObject.IsIgnoringPersistence())
			{
				continue;
			}
			try
			{
				PersistentData currentPersistentState = persistentObject.GetCurrentPersistentState(dataPath, fullSave);
				if (currentPersistentState != null)
				{
					if (currentPersistentState.UniqId == string.Empty)
					{
						Debug.LogError("Persisten Error: Object " + persistentObject.gameObject.name + " has persistence and not UniqueId component");
					}
					else
					{
						snapShot.sceneElements[sceneKeyName][currentPersistentState.UniqId] = currentPersistentState;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("** SaveSnapShot, error persistent.GetCurrentPersistentState: " + ex.Message);
				Debug.LogException(ex);
			}
		}
		foreach (PersistentInterface item in persistenceSystems.OrderBy((PersistentInterface x) => x.GetOrder()))
		{
			try
			{
				PersistentData currentPersistentState2 = item.GetCurrentPersistentState(dataPath, fullSave);
				snapShot.commonElements[currentPersistentState2.UniqId] = currentPersistentState2;
			}
			catch (Exception ex2)
			{
				Debug.LogError("** SaveSnapShot, error item.GetCurrentPersistentState: " + ex2.Message);
				Debug.LogException(ex2);
			}
		}
		EntityStats stats = Core.Logic.Penitent.Stats;
		try
		{
			PersistentData currentPersistentState3 = stats.GetCurrentPersistentState(dataPath, fullSave);
			snapShot.commonElements[currentPersistentState3.UniqId] = currentPersistentState3;
		}
		catch (Exception ex3)
		{
			Debug.LogError("** SaveSnapShot, error stats.GetCurrentPersistentState: " + ex3.Message);
			Debug.LogException(ex3);
		}
		SaveGame_Internal(GetAutomaticSlot());
		InsideSaveProcess = false;
	}

	private void LoadSnapShot(SnapShot snapShot, string scene, bool isloading, string dataPath)
	{
		if (InsideLoadLevelProcess)
		{
			Debug.LogError("*** SAVEERROR 4. LOAD INSIDE LOAD LEVEL PROCESS, PLEASE LOOK TRACE AND CALL A DEVELOPER");
		}
		if (InsideSaveProcess)
		{
			Debug.LogError("*** SAVEERROR 5. LOAD INSIDE SAVE PROCESS, PLEASE LOOK TRACE AND CALL A DEVELOPER");
		}
		if (InsideLoadProcess)
		{
			Debug.LogError("*** SAVEERROR 6. LOAD INSIDE LOAD PROCESS, PLEASE LOOK TRACE AND CALL A DEVELOPER");
		}
		InsideLoadProcess = true;
		string sceneKeyName = GetSceneKeyName(scene);
		if (snapShot.sceneElements.ContainsKey(sceneKeyName))
		{
			Dictionary<string, PersistentData> dictionary = snapShot.sceneElements[sceneKeyName];
			PersistentObject[] array = UnityEngine.Object.FindObjectsOfType<PersistentObject>();
			PersistentObject[] array2 = array;
			foreach (PersistentObject persistentObject in array2)
			{
				if (persistentObject.IsIgnoringPersistence())
				{
					continue;
				}
				string persistenID = persistentObject.GetPersistenID();
				if (persistenID == string.Empty)
				{
					Debug.LogWarning("Persisten Error: Object " + persistentObject.gameObject.name + " has persistence and not UniqueId component");
					continue;
				}
				if (!dictionary.ContainsKey(persistenID))
				{
					Debug.Log("*** LoadSnapShot -- NO DATA FOR " + persistentObject.gameObject.name);
					continue;
				}
				try
				{
					persistentObject.SetCurrentPersistentState(dictionary[persistenID], isloading, dataPath);
				}
				catch (Exception ex)
				{
					Debug.LogError("** LoadSnapShot, error persistent.SetCurrentPersistentState: " + ex.Message);
					Debug.LogException(ex);
				}
			}
		}
		foreach (PersistentInterface item in persistenceSystems.OrderBy((PersistentInterface x) => x.GetOrder()))
		{
			if (snapShot.commonElements.ContainsKey(item.GetPersistenID()))
			{
				PersistentData data = snapShot.commonElements[item.GetPersistenID()];
				try
				{
					item.SetCurrentPersistentState(data, isloading, dataPath);
				}
				catch (Exception ex2)
				{
					Debug.LogError("** LoadSnapShot, error item.SetCurrentPersistentState: " + ex2.Message);
					Debug.LogException(ex2);
				}
			}
		}
		EntityStats stats = Core.Logic.Penitent.Stats;
		if (snapShot.commonElements.ContainsKey(stats.GetPersistenID()))
		{
			try
			{
				stats.SetCurrentPersistentState(snapShot.commonElements[stats.GetPersistenID()], isloading, dataPath);
			}
			catch (Exception ex3)
			{
				Debug.LogError("** LoadSnapShot, error stats.SetCurrentPersistentState: " + ex3.Message);
				Debug.LogException(ex3);
			}
		}
		InsideLoadProcess = false;
	}

	private string GetSceneKeyName(string scene)
	{
		return scene.ToLower();
	}

	public int GetOrder()
	{
		return 0;
	}

	public string GetPersistenID()
	{
		return "ID_PERSISTENT_MANAGER";
	}

	public float GetCurrentTimePlayed()
	{
		return TimeStored + Time.realtimeSinceStartup - LastTimeStored;
	}

	public float GetCurrentTimePlayedForAC44()
	{
		return GetCurrentTimePlayed() - TimeAtSlotAscension;
	}

	public PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		PersitentPersistenceData persitentPersistenceData = new PersitentPersistenceData();
		persitentPersistenceData.Percent = PercentCompleted;
		persitentPersistenceData.Time = TimeStored + Time.realtimeSinceStartup - LastTimeStored;
		persitentPersistenceData.TimeAtSlotAscension = TimeAtSlotAscension;
		if ((bool)Core.Logic.Penitent)
		{
			EntityStats stats = Core.Logic.Penitent.Stats;
			persitentPersistenceData.Purge = Core.Logic.Penitent.Stats.Purge.Current;
		}
		persitentPersistenceData.CurrentZone = Core.NewMapManager.CurrentSafeScene.Zone;
		persitentPersistenceData.CurrentDomain = Core.NewMapManager.CurrentSafeScene.District;
		persitentPersistenceData.IsNewGamePlus = Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.NEW_GAME_PLUS);
		persitentPersistenceData.NewGamePlusUpgrades = Core.GameModeManager.GetNewGamePlusUpgrades();
		persitentPersistenceData.CanConvertToNewGamePlus = false;
		return persitentPersistenceData;
	}

	public void SetCurrentPersistentState(PersistentData data, bool isloading, string dataPath)
	{
		PersitentPersistenceData persitentPersistenceData = (PersitentPersistenceData)data;
		if (isloading)
		{
			TimeStored = persitentPersistenceData.Time;
			TimeAtSlotAscension = persitentPersistenceData.TimeAtSlotAscension;
			LastTimeStored = Time.realtimeSinceStartup;
		}
	}

	public void ResetPersistence()
	{
		LastTimeStored = Time.realtimeSinceStartup;
		TimeStored = 0f;
		TimeAtSlotAscension = 0f;
		InsideLoadLevelProcess = false;
		InsideSaveProcess = false;
		InsideLoadProcess = false;
	}

	private void CheckAndApplyBeadFix()
	{
		if (FixHasBeenApplied)
		{
			return;
		}
		FixHasBeenApplied = true;
		if (!CheckIfBeadFixIsApplied())
		{
			for (int i = 0; i < 3; i++)
			{
				LoadGameWithOutRespawn(i);
				FixBeadsRewards(i);
			}
			MarkBeadFixApplied();
		}
	}

	private void FixBeadsRewards(int slotIndex)
	{
		bool flag = false;
		if (Core.GameModeManager.GetNewGamePlusUpgrades() == 1)
		{
			SnapShot snap = new SnapShot();
			bool flag2 = LoadGameSnapShot(slotIndex, ref snap);
			bool flag3 = ExistBackupSlot(slotIndex);
			if (!flag2 && flag3)
			{
				flag2 = LoadGameBackupSnapShot(slotIndex, ref snap);
			}
			IPenitence currentPenitence = Core.PenitenceManager.GetCurrentPenitence();
			if (flag2 && Core.GameModeManager.CanConvertToNewGamePlus(snap) && currentPenitence != null)
			{
				foreach (KeyValuePair<string, string> skinsAndObject in SkinsAndObjects)
				{
					if (Core.ColorPaletteManager.IsColorPaletteUnlocked(skinsAndObject.Key) && !Core.InventoryManager.IsRosaryBeadOwned(skinsAndObject.Value) && skinsAndObject.Key.EndsWith(currentPenitence.Id))
					{
						flag = true;
						Core.InventoryManager.AddRosaryBead(skinsAndObject.Value);
						Core.PenitenceManager.MarkCurrentPenitenceAsCompleted();
					}
				}
			}
		}
		else if (Core.GameModeManager.GetNewGamePlusUpgrades() > 1)
		{
			foreach (KeyValuePair<string, string> skinsAndObject2 in SkinsAndObjects)
			{
				if (Core.ColorPaletteManager.IsColorPaletteUnlocked(skinsAndObject2.Key) && !Core.InventoryManager.IsRosaryBeadOwned(skinsAndObject2.Value))
				{
					flag = true;
					Core.InventoryManager.AddRosaryBead(skinsAndObject2.Value);
					string[] array = skinsAndObject2.Key.Split('_');
					if (array.Length > 0)
					{
						string id = array[1];
						Core.PenitenceManager.MarkPenitenceAsCompleted(id);
					}
					else
					{
						Debug.LogError("RewardBeadsAlreadyFlagged: key: " + skinsAndObject2.Key + " isn't a valid key!");
					}
				}
			}
		}
		if (flag)
		{
			Core.Persistence.SaveGame(slotIndex, fullSave: false);
		}
	}

	private bool CheckIfBeadFixIsApplied()
	{
		bool result = false;
		string pathAppSettings = GetPathAppSettings("/app_settings");
		if (File.Exists(pathAppSettings))
		{
			Dictionary<string, fsData> dictionary = new Dictionary<string, fsData>();
			string s = File.ReadAllText(pathAppSettings);
			byte[] bytes = Convert.FromBase64String(s);
			string @string = Encoding.UTF8.GetString(bytes);
			fsData data;
			fsResult fsResult = fsJsonParser.Parse(@string, out data);
			if (fsResult.Failed && !fsResult.FormattedMessages.Equals("No input"))
			{
				Debug.LogError("CheckInFileIfBeadsRewarded: parsing error: " + fsResult.FormattedMessages);
			}
			else if (data != null)
			{
				dictionary = data.AsDictionary;
				string key = "REWARDS_FIX";
				if (dictionary.TryGetValue(key, out var value))
				{
					result = value.AsBool;
				}
			}
		}
		return result;
	}

	private void MarkBeadFixApplied()
	{
		string pathAppSettings = GetPathAppSettings("/app_settings");
		fsData fsData = ReadAppSettings(pathAppSettings);
		if (!(fsData == null) && fsData.IsDictionary)
		{
			string key = "REWARDS_FIX";
			fsData.AsDictionary[key] = new fsData(boolean: true);
			string s = fsJsonPrinter.CompressedJson(fsData);
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			string encryptedData = Convert.ToBase64String(bytes);
			FileTools.SaveSecure(pathAppSettings, encryptedData);
		}
	}

	public static string GetPathAppSettings(string fileName)
	{
		string arg = string.Empty;
		if (SteamManager.Initialized)
		{
			arg = SteamUser.GetSteamID().ToString();
		}
		string text = $"{Application.persistentDataPath}/Savegames/{arg}/";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		if (!fileName.IsNullOrWhitespace())
		{
			text += fileName;
		}
		return text;
	}

	public static fsData ReadAppSettings(string filePath)
	{
		fsData data = new fsData();
		if (TryToReadFile(filePath, out var fileData))
		{
			byte[] bytes = Convert.FromBase64String(fileData);
			string @string = Encoding.UTF8.GetString(bytes);
			fsResult fsResult = fsJsonParser.Parse(@string, out data);
			if (fsResult.Failed && !fsResult.FormattedMessages.Equals("No input"))
			{
				Debug.LogError("Parsing error: " + fsResult.FormattedMessages);
			}
		}
		return data;
	}

	public static bool TryToReadFile(string filePath, out string fileData)
	{
		if (!File.Exists(filePath))
		{
			Debug.LogError("File at path '" + filePath + "' does not exists!");
			fileData = string.Empty;
			return false;
		}
		fileData = File.ReadAllText(filePath);
		if (fileData.Length == 0)
		{
			Debug.Log("File at path '" + filePath + "' is empty.");
			return false;
		}
		return true;
	}
}
