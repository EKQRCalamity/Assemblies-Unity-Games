using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Framework.Dialog;
using Framework.FrameworkCore;
using Gameplay.UI;
using Sirenix.Utilities;
using UnityEngine;

namespace Framework.Managers;

public class EventManager : GameSystem, PersistentInterface
{
	public delegate void StandardEvent(string id, string parameter);

	public delegate void StandardFlag(string flag, bool flagActive);

	[Serializable]
	public class FlagPersistenceData : PersistentManager.PersistentData
	{
		public Dictionary<string, bool> flags = new Dictionary<string, bool>();

		public Dictionary<string, bool> flagsPreserve = new Dictionary<string, bool>();

		public bool IsMiriamQuestStarted;

		public bool IsMiriamQuestFinished;

		public List<string> MiriamClosedPortals = new List<string>();

		public FlagPersistenceData()
			: base("ID_EVENT_MANAGER")
		{
		}
	}

	private Dictionary<string, FlagObject> flags = new Dictionary<string, FlagObject>();

	private const string MIRIAM_QUEST_CONFIG = "Dialog/ST205_MIRIAM/MiriamQuest";

	private MiriamsConfig MiriamQuest;

	private List<string> MiriamClosedPortals = new List<string>();

	public const string EVENT_PERSITENT_ID = "ID_EVENT_MANAGER";

	public GameObject LastCreatedObject { get; private set; }

	public bool IsMiriamQuestStarted { get; private set; }

	public bool IsMiriamQuestFinished { get; private set; }

	public string MiriamCurrentScenePortal { get; private set; }

	public string MiriamCurrentScenePortalToReturn { get; private set; }

	public string MiriamCurrentSceneDestination { get; private set; }

	public event StandardEvent OnEventLaunched;

	public event StandardFlag OnFlagChanged;

	public override void AllInitialized()
	{
		Core.Persistence.AddPersistentManager(this);
		LevelManager.OnBeforeLevelLoad += OnBeforeLevelLoad;
		LevelManager.OnLevelPreLoaded += OnLevelPreLoaded;
		LevelManager.OnLevelLoaded += OnLevelLoaded;
	}

	public override void Start()
	{
		ResetPersistence();
	}

	public override void Dispose()
	{
		base.Dispose();
		LevelManager.OnBeforeLevelLoad -= OnBeforeLevelLoad;
		LevelManager.OnLevelPreLoaded -= OnLevelPreLoaded;
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
	}

	public void LaunchEvent(string id, string parameter = "")
	{
		if (!id.IsNullOrWhitespace() && this.OnEventLaunched != null)
		{
			string text = id.Replace(' ', '_').ToUpper();
			string text2 = parameter.Replace(' ', '_').ToUpper();
			if (parameter.IsNullOrWhitespace())
			{
				text2 = "NO_PARAMS";
			}
			Log.Trace("Events", "Event " + text + " with parameter " + text2 + " has been raised.");
			this.OnEventLaunched(text, text2);
		}
	}

	public void SetFlag(string id, bool b, bool forcePreserve = false)
	{
		if (!string.IsNullOrEmpty(id))
		{
			string formattedId = GetFormattedId(id);
			if (!flags.ContainsKey(formattedId))
			{
				FlagObject flagObject = new FlagObject();
				flagObject.id = formattedId;
				flagObject.preserveInNewGamePlus = forcePreserve;
				flags[formattedId] = flagObject;
				Debug.Log("-- Create flag " + id);
			}
			flags[formattedId].value = b;
			if (forcePreserve && !flags[formattedId].preserveInNewGamePlus)
			{
				Debug.LogWarning("-- Created flag " + id + " set force to preserve");
				flags[formattedId].preserveInNewGamePlus = forcePreserve;
			}
			Log.Trace("Events", "Flag " + formattedId + " has been set to " + b.ToString().ToUpper() + ".");
			if (this.OnFlagChanged != null)
			{
				this.OnFlagChanged(formattedId, b);
			}
			if (b && flags[formattedId].addToPercentage)
			{
				Core.AchievementsManager.CheckProgressToAC46();
			}
		}
	}

	public bool GetFlag(string id)
	{
		string formattedId = GetFormattedId(id);
		bool result = false;
		if (flags.ContainsKey(formattedId))
		{
			result = flags[formattedId].value;
		}
		return result;
	}

	public string GetFormattedId(string id)
	{
		return id.Replace(' ', '_').ToUpper();
	}

	public void PrepareForNewGamePlus()
	{
		foreach (KeyValuePair<string, FlagObject> flag in flags)
		{
			if (flag.Value.value && !flag.Value.preserveInNewGamePlus)
			{
				Debug.Log("Reset flag for ng+ " + flag.Key);
				flag.Value.value = false;
			}
			if (flag.Value.value && flag.Value.preserveInNewGamePlus)
			{
				Debug.Log("Preserve flag for ng+ " + flag.Key);
			}
		}
		IsMiriamQuestStarted = false;
		IsMiriamQuestFinished = false;
		MiriamClosedPortals = new List<string>();
		MiriamCurrentScenePortal = string.Empty;
		MiriamCurrentScenePortalToReturn = string.Empty;
		MiriamCurrentSceneDestination = string.Empty;
	}

	private void OnBeforeLevelLoad(Level oldLevel, Level newLevel)
	{
		if (Core.LevelManager.InCinematicsChangeLevel == LevelManager.CinematicsChangeLevel.No)
		{
			PlayMakerFSM.BroadcastEvent("ON LEVEL UNLOAD");
		}
	}

	private void OnLevelPreLoaded(Level oldLevel, Level newLevel)
	{
		if (Core.LevelManager.InCinematicsChangeLevel == LevelManager.CinematicsChangeLevel.No)
		{
			PlayMakerFSM.BroadcastEvent("ON LEVEL LOADED");
		}
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		switch (Core.LevelManager.InCinematicsChangeLevel)
		{
		case LevelManager.CinematicsChangeLevel.No:
			PlayMakerFSM.BroadcastEvent("ON LEVEL READY");
			break;
		case LevelManager.CinematicsChangeLevel.Outro:
			PlayMakerFSM.BroadcastEvent("POST CUTSCENE");
			break;
		}
	}

	public float GetPercentageCompletition()
	{
		float num = 0f;
		foreach (FlagObject item in flags.Values.Where((FlagObject x) => x.addToPercentage && x.value))
		{
			num += GameConstants.PercentageValues[item.percentageType];
		}
		return num;
	}

	public bool StartMiriamQuest()
	{
		bool result = false;
		if (IsMiriamQuestStarted)
		{
			Debug.LogError("*** StartMiriamQuest. quest was started");
		}
		else if (IsMiriamQuestFinished)
		{
			Debug.LogError("*** StartMiriamQuest. quest was finished");
		}
		else
		{
			IsMiriamQuestStarted = true;
			result = true;
		}
		return result;
	}

	public bool FinishMiriamQuest()
	{
		bool result = false;
		if (CheckStatusMiriamQuest())
		{
			IsMiriamQuestFinished = true;
			result = true;
		}
		return result;
	}

	public ReadOnlyCollection<string> GetMiriamClosedPortals()
	{
		return MiriamClosedPortals.AsReadOnly();
	}

	public bool ActivateMiriamPortalAndTeleport(bool UseFade = true)
	{
		bool result = false;
		if (CheckStatusMiriamQuest())
		{
			if (!MiriamCurrentScenePortal.IsNullOrWhitespace())
			{
				Debug.LogWarning("ActivateMiriamPortalAndTeleport and have current portal " + MiriamCurrentScenePortal);
			}
			int count = MiriamClosedPortals.Count;
			if (count >= MiriamQuest.Scenes.Count)
			{
				Debug.LogError("ActivateMiriamPortalAndTeleport no more portals in quest");
			}
			else if (MiriamClosedPortals.Contains(Core.LevelManager.currentLevel.LevelName))
			{
				Debug.LogError("ActivateMiriamPortalAndTeleport " + Core.LevelManager.currentLevel.LevelName + " is closed.");
			}
			else
			{
				MiriamCurrentScenePortal = Core.LevelManager.currentLevel.LevelName;
				MiriamCurrentScenePortalToReturn = MiriamCurrentScenePortal;
				GameModeManager gameModeManager = Core.GameModeManager;
				gameModeManager.OnEnterMenuMode = (Core.SimpleEvent)Delegate.Combine(gameModeManager.OnEnterMenuMode, new Core.SimpleEvent(ClearMiriamChallengeSpawn));
				result = true;
				MiriamCurrentSceneDestination = MiriamQuest.Scenes[count];
				Core.SpawnManager.SpawnFromMiriam(MiriamCurrentSceneDestination, string.Empty, UseFade);
			}
		}
		return result;
	}

	private void ClearMiriamChallengeSpawn()
	{
		GameModeManager gameModeManager = Core.GameModeManager;
		gameModeManager.OnEnterMenuMode = (Core.SimpleEvent)Delegate.Remove(gameModeManager.OnEnterMenuMode, new Core.SimpleEvent(ClearMiriamChallengeSpawn));
		MiriamCurrentScenePortal = string.Empty;
		MiriamCurrentScenePortalToReturn = string.Empty;
		MiriamCurrentSceneDestination = string.Empty;
	}

	public bool EndMiriamPortalAndReturn(bool useFade = true)
	{
		bool result = false;
		if (CheckStatusMiriamQuest())
		{
			if (!AreInMiriamLevel())
			{
				Debug.LogError("EndThisActivatePortalAndGetScene and don't have current portal");
			}
			else
			{
				string miriamCurrentScenePortal = MiriamCurrentScenePortal;
				if (MiriamClosedPortals.Contains(MiriamCurrentScenePortal))
				{
					Debug.LogError("EndThisActivatePortalAndGetScene: Portal " + MiriamCurrentScenePortal + " was closed and want to close again");
				}
				else
				{
					MiriamClosedPortals.Add(MiriamCurrentScenePortal);
				}
				if (MiriamClosedPortals.Count == MiriamQuest.Scenes.Count)
				{
					FinishMiriamQuest();
				}
				result = true;
				MiriamCurrentScenePortal = string.Empty;
				MiriamCurrentSceneDestination = string.Empty;
				SetFlag(MiriamQuest.CutsceneFlag, b: true);
				string text = MiriamQuest.PortalFlagPrefix + MiriamClosedPortals.Count;
				SetFlag(text, b: true);
				SetFlag(text + MiriamQuest.PortalFlagSufix, b: true);
				UIController.instance.HideMiriamTimer();
				Core.LevelManager.ChangeLevelAndPlayEvent(MiriamQuest.PortalSceneName, MiriamQuest.PortalPlaymakerEventName, hideplayer: true, useFade, forceDeactivate: false, null, Miriam: true);
			}
		}
		return result;
	}

	public bool TeleportPenitentToGoal()
	{
		bool result = true;
		if (CheckStatusMiriamQuest())
		{
			if (!AreInMiriamLevel())
			{
				Debug.LogError("TeleportPenitentToGoal and we are not in Miriam Level!");
				result = false;
			}
			else
			{
				Vector2 position = GameObject.Find("GoalPoint").transform.position;
				Core.Logic.Penitent.Teleport(position);
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	public bool CancelMiriamPortalAndReturn(bool useFade = true)
	{
		bool result = false;
		if (CheckStatusMiriamQuest())
		{
			if (!AreInMiriamLevel())
			{
				Debug.LogError("CancelMiriamPortalAndReturn and don't have current portal");
			}
			else
			{
				string miriamCurrentScenePortalToReturn = MiriamCurrentScenePortalToReturn;
				MiriamCurrentScenePortal = string.Empty;
				MiriamCurrentSceneDestination = string.Empty;
				UIController.instance.HideMiriamTimer();
				Core.SpawnManager.SpawnFromMiriam(miriamCurrentScenePortalToReturn, string.Empty, useFade);
			}
		}
		return result;
	}

	public bool IsMiriamPortalEnabled(string levelName)
	{
		bool result = false;
		if (IsMiriamQuestStarted && !IsMiriamQuestFinished)
		{
			result = !MiriamClosedPortals.Contains(levelName);
		}
		return result;
	}

	public bool AreInMiriamLevel()
	{
		return IsMiriamQuestStarted && !MiriamCurrentScenePortal.IsNullOrWhitespace();
	}

	private bool CheckStatusMiriamQuest()
	{
		bool result = false;
		if (!IsMiriamQuestStarted)
		{
			Debug.LogError("*** MiriamQuest. quest was not started");
		}
		else if (IsMiriamQuestFinished)
		{
			Debug.LogError("*** EndMiriamQuest. quest was finished");
		}
		else
		{
			result = true;
		}
		return result;
	}

	public int GetOrder()
	{
		return 0;
	}

	public string GetPersistenID()
	{
		return "ID_EVENT_MANAGER";
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		FlagPersistenceData flagPersistenceData = new FlagPersistenceData();
		foreach (KeyValuePair<string, FlagObject> flag in flags)
		{
			flagPersistenceData.flags.Add(flag.Key, flag.Value.value);
			flagPersistenceData.flagsPreserve.Add(flag.Key, flag.Value.preserveInNewGamePlus);
		}
		flagPersistenceData.IsMiriamQuestFinished = IsMiriamQuestFinished;
		flagPersistenceData.IsMiriamQuestStarted = IsMiriamQuestStarted;
		flagPersistenceData.MiriamClosedPortals = new List<string>(MiriamClosedPortals);
		return flagPersistenceData;
	}

	private bool FlagNeedToBePersistent(string id)
	{
		bool flag = false;
		foreach (string item in GameConstants.FLAGS_ENDINGS_NEED_TO_BE_PERSISTENT)
		{
			if (id.EndsWith(item))
			{
				flag = true;
				break;
			}
		}
		return flag && !GameConstants.IGNORE_FLAG_TO_BE_PERSISTENT.Contains(id);
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		FlagPersistenceData flagPersistenceData = (FlagPersistenceData)data;
		flags.ForEach(delegate(KeyValuePair<string, FlagObject> p)
		{
			p.Value.value = false;
		});
		foreach (KeyValuePair<string, bool> flag in flagPersistenceData.flags)
		{
			if (flags.ContainsKey(flag.Key))
			{
				flags[flag.Key].value = flag.Value;
				continue;
			}
			FlagObject flagObject = new FlagObject();
			flagObject.id = flag.Key;
			flagObject.value = flag.Value;
			if (flagPersistenceData.flagsPreserve.ContainsKey(flag.Key))
			{
				flagObject.preserveInNewGamePlus = flagPersistenceData.flagsPreserve[flag.Key];
			}
			if (!flagObject.preserveInNewGamePlus && FlagNeedToBePersistent(flag.Key))
			{
				flagObject.preserveInNewGamePlus = true;
			}
			flags[flag.Key] = flagObject;
		}
		if (isloading)
		{
			IsMiriamQuestFinished = flagPersistenceData.IsMiriamQuestFinished;
			IsMiriamQuestStarted = flagPersistenceData.IsMiriamQuestStarted;
			MiriamClosedPortals = new List<string>(flagPersistenceData.MiriamClosedPortals);
		}
	}

	public void ResetPersistence()
	{
		flags.Clear();
		Log.Trace("Events", "The event manager has been reseted sucessfully.");
		FlagObjectList[] array = Resources.LoadAll<FlagObjectList>("Dialog/");
		FlagObjectList[] array2 = array;
		foreach (FlagObjectList flagObjectList in array2)
		{
			foreach (FlagObject flag in flagObjectList.flagList)
			{
				if (flags.ContainsKey(flag.id))
				{
					Debug.LogError("Duplicate Flag " + flag.id + " List:" + flags[flag.id].sourceList + ", " + flagObjectList.name);
				}
				else
				{
					FlagObject flagObject = new FlagObject(flag);
					flags[flagObject.id] = flagObject;
				}
			}
		}
		MiriamQuest = Resources.Load<MiriamsConfig>("Dialog/ST205_MIRIAM/MiriamQuest");
		Debug.Log(" Miriam Quest scenes " + MiriamQuest.Scenes.Count);
		IsMiriamQuestFinished = false;
		IsMiriamQuestStarted = false;
		MiriamClosedPortals = new List<string>();
		Log.Debug("EventManager", flags.Count + " initial flags loaded succesfully.");
	}
}
