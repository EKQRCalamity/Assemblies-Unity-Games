using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Penitent;
using Gameplay.UI;
using Sirenix.Utilities;
using Tools.Level;
using Tools.Level.Interactables;
using Tools.Level.Layout;
using UnityEngine;

namespace Framework.Managers;

public class SpawnManager : GameSystem, PersistentInterface
{
	public delegate void SpawnEvent(Penitent penitent);

	public delegate void TeleportEvent(string spawnId);

	public enum PosibleSpawnPoints
	{
		None,
		Menu,
		Editor,
		PrieDieu,
		Teleport,
		Door,
		SafePosition,
		CustomPosition,
		TeleportPrieDieu,
		Miriam
	}

	[Serializable]
	public class CheckPointPersistenceData : PersistentManager.PersistentData
	{
		public string activePrieDieuScene = string.Empty;

		public string activePrieDieuId = string.Empty;

		public PosibleSpawnPoints pendingSpawn;

		public string spawnId = string.Empty;

		public List<string> activeTeleports = new List<string>();

		public CheckPointPersistenceData()
			: base("ID_CHECKPOINT_MANAGER")
		{
		}
	}

	private const string TELEPORT_RESOURCE_DIR = "Teleport/";

	private const string PENITENT_PREFAB = "Core/Penitent";

	private string activePrieDieuScene = string.Empty;

	private string activePrieDieuId = string.Empty;

	private PosibleSpawnPoints pendingSpawn;

	private string spawnId = string.Empty;

	private Vector3 safePosition;

	private List<TeleportDestination> TeleportList;

	private Dictionary<string, TeleportDestination> TeleportDict;

	private Penitent penitentPrefab;

	private Door currentDoor;

	public bool FirstSpanw;

	private string customLevel = string.Empty;

	private Vector3 customPosition = Vector3.zero;

	private EntityOrientation customOrientation = EntityOrientation.Left;

	private bool customIsMiriam;

	public const string CHECK_PERSITENT_ID = "ID_CHECKPOINT_MANAGER";

	private static bool forceReload = true;

	private static List<string> cachedTeleportId = new List<string>();

	public string InitialScene { get; set; }

	public bool AutomaticRespawn { get; set; }

	public bool IgnoreNextAutomaticRespawn { get; set; }

	public bool HidePlayerInNextSpawn { get; set; }

	public PrieDieu ActivePrieDieu
	{
		set
		{
			if (!(value == null))
			{
				value.Ligthed = true;
				activePrieDieuScene = Core.LevelManager.currentLevel.LevelName;
				activePrieDieuId = value.GetPersistenID();
			}
		}
	}

	public static event SpawnEvent OnPlayerSpawn;

	public static event TeleportEvent OnTeleport;

	public static event TeleportEvent OnTeleportPrieDieu;

	public override void Start()
	{
		HidePlayerInNextSpawn = false;
		FirstSpanw = false;
		AutomaticRespawn = false;
		IgnoreNextAutomaticRespawn = false;
		customLevel = string.Empty;
		customIsMiriam = false;
		LoadAllTeleports();
	}

	public override void AllInitialized()
	{
		Core.Persistence.AddPersistentManager(this);
	}

	public void Teleport(string teleportId)
	{
		if (TeleportDict.ContainsKey(teleportId))
		{
			Teleport(TeleportDict[teleportId]);
		}
		else
		{
			Debug.LogError("The key " + teleportId + "is not present in the teleports dictionary!");
		}
	}

	public void Teleport(TeleportDestination teleport)
	{
		Color? color = null;
		if (teleport.UseCustomLoadColor)
		{
			color = teleport.CustomLoadColor;
		}
		string sceneName = teleport.sceneName;
		PosibleSpawnPoints spawnType = PosibleSpawnPoints.Teleport;
		string teleportName = teleport.teleportName;
		bool useFade = teleport.UseFade;
		Color? color2 = color;
		SpawnPlayer(sceneName, spawnType, teleportName, useFade, forceLoad: false, color2);
	}

	public void SpawnFromMiriam(string targetScene, string targetTeleport, bool useFade)
	{
		SpawnPlayer(targetScene, PosibleSpawnPoints.Miriam, targetTeleport, useFade, forceLoad: true);
	}

	public void SpawnFromDoor(string targetScene, string targetDoor, bool useFade)
	{
		SpawnPlayer(targetScene, PosibleSpawnPoints.Door, targetDoor, useFade);
	}

	public void SpawnFromMenu()
	{
		SpawnPlayer(string.Empty, PosibleSpawnPoints.Menu, string.Empty);
	}

	public void SpawnFromTeleportOnPrieDieu(string targetScene, bool useFade)
	{
		SpawnPlayer(targetScene, PosibleSpawnPoints.TeleportPrieDieu, string.Empty, useFade);
	}

	public void SetCurrentToCustomSpawnData(bool IsMiriam)
	{
		customIsMiriam = IsMiriam;
		customLevel = Core.LevelManager.currentLevel.LevelName;
		customPosition = Core.Logic.Penitent.GetPosition();
		customOrientation = Core.Logic.Penitent.GetOrientation();
	}

	public void SpawnFromCustom(bool usefade, Color? background = null)
	{
		if (customIsMiriam)
		{
			SpawnPlayer(Core.Events.MiriamCurrentScenePortalToReturn, PosibleSpawnPoints.Miriam, string.Empty, usefade, forceLoad: false, background);
		}
		else if (customLevel != string.Empty)
		{
			SpawnPlayer(customLevel, PosibleSpawnPoints.CustomPosition, string.Empty, usefade, forceLoad: false, background);
		}
		customLevel = string.Empty;
		customIsMiriam = false;
	}

	public void Respawn()
	{
		if (activePrieDieuScene.IsNullOrWhitespace())
		{
			Debug.LogError("Respawn: Respawn without scene, loading initial");
			activePrieDieuScene = InitialScene;
		}
		SpawnPlayer(activePrieDieuScene, PosibleSpawnPoints.PrieDieu, activePrieDieuId, usefade: true, forceLoad: true);
		UIController.instance.UpdatePurgePoints();
	}

	public void RespawnSafePosition()
	{
		Core.Events.SetFlag("CHERUB_RESPAWN", b: true);
		safePosition = Core.LevelManager.LastSafePosition;
		SpawnPlayer(Core.LevelManager.LastSafeLevel, PosibleSpawnPoints.SafePosition, string.Empty, usefade: true, forceLoad: true);
		UIController.instance.UpdatePurgePoints();
	}

	public void RespawnMiriamSameLevel(bool useFade, Color? background = null)
	{
		Core.Events.SetFlag("CHERUB_RESPAWN", b: true);
		safePosition = Core.LevelManager.LastSafePosition;
		SpawnPlayer(Core.LevelManager.currentLevel.LevelName, PosibleSpawnPoints.Miriam, string.Empty, useFade, forceLoad: true, background);
		UIController.instance.UpdatePurgePoints();
	}

	public ReadOnlyCollection<TeleportDestination> GetAllTeleports()
	{
		return TeleportList.AsReadOnly();
	}

	public ReadOnlyCollection<TeleportDestination> GetAllUIActiveTeleports()
	{
		return TeleportList.Where((TeleportDestination element) => element.isActive && element.useInUI).ToList().AsReadOnly();
	}

	public void SetTeleportActive(string teleportId, bool active)
	{
		if (TeleportDict.ContainsKey(teleportId))
		{
			TeleportDict[teleportId].isActive = active;
			Core.AchievementsManager.CheckProgressToAC46();
		}
	}

	public bool IsTeleportActive(string teleportId)
	{
		bool result = false;
		if (TeleportDict.ContainsKey(teleportId))
		{
			result = TeleportDict[teleportId].isActive;
		}
		return result;
	}

	public float GetPercentageCompletition()
	{
		float num = 0f;
		foreach (TeleportDestination item in TeleportDict.Values.Where((TeleportDestination x) => x.useInCompletition && x.isActive))
		{
			num += GameConstants.PercentageValues[item.percentageType];
		}
		return num;
	}

	public void SetActivePriedieuManually(string levelName, string activePriedieuPersistentID)
	{
		activePrieDieuScene = levelName;
		activePrieDieuId = activePriedieuPersistentID;
	}

	public string GetActivePriedieuScene()
	{
		return activePrieDieuScene;
	}

	public string GetActivePriedieuId()
	{
		return activePrieDieuId;
	}

	public void PrepareForSpawnFromEditor()
	{
		pendingSpawn = PosibleSpawnPoints.Editor;
	}

	public void PrepareForSpawnFromMenu()
	{
		pendingSpawn = PosibleSpawnPoints.Menu;
	}

	public void SetInitialSpawn(string level)
	{
		activePrieDieuScene = level;
	}

	public void PrepareForCommandSpawn(string scene)
	{
		activePrieDieuId = string.Empty;
		spawnId = string.Empty;
		activePrieDieuScene = scene;
		pendingSpawn = PosibleSpawnPoints.PrieDieu;
	}

	public void PrepareForNewGamePlus(string initialScene)
	{
		ResetPersistence();
		activePrieDieuScene = initialScene;
		pendingSpawn = PosibleSpawnPoints.PrieDieu;
	}

	public void PrepareForBossRush()
	{
		pendingSpawn = PosibleSpawnPoints.PrieDieu;
	}

	public void SpawnPlayerOnLevelLoad(bool createNewInstance = true)
	{
		bool flag = false;
		PrieDieu[] array = UnityEngine.Object.FindObjectsOfType<PrieDieu>();
		currentDoor = null;
		string empty = string.Empty;
		Vector3 position = Vector3.zero;
		EntityOrientation orientation = EntityOrientation.Right;
		switch (pendingSpawn)
		{
		case PosibleSpawnPoints.PrieDieu:
		{
			PrieDieu prieDieu = array.FirstOrDefault((PrieDieu p) => p.GetPersistenID() == spawnId);
			if ((bool)prieDieu)
			{
				position = prieDieu.transform.position;
				orientation = prieDieu.spawnOrientation;
				flag = true;
			}
			break;
		}
		case PosibleSpawnPoints.TeleportPrieDieu:
			if (array.Length > 0)
			{
				PrieDieu prieDieu2 = array[0];
				position = prieDieu2.transform.position;
				orientation = prieDieu2.spawnOrientation;
				ActivePrieDieu = prieDieu2;
				flag = true;
			}
			break;
		case PosibleSpawnPoints.Teleport:
		{
			Teleport[] source = UnityEngine.Object.FindObjectsOfType<Teleport>();
			Teleport teleport = source.FirstOrDefault((Teleport p) => p.telportName == spawnId);
			if ((bool)teleport)
			{
				position = teleport.transform.position;
				orientation = teleport.spawnOrientation;
				flag = true;
			}
			break;
		}
		case PosibleSpawnPoints.Miriam:
		{
			MiriamStart[] array2 = UnityEngine.Object.FindObjectsOfType<MiriamStart>();
			MiriamStart miriamStart = null;
			if (array2.Length > 0)
			{
				miriamStart = array2[0];
			}
			if (!miriamStart)
			{
				MiriamPortal[] array3 = UnityEngine.Object.FindObjectsOfType<MiriamPortal>();
				if (array3 != null && array3.Length > 0)
				{
					position = array3[0].transform.position;
					orientation = array3[0].Orientation();
					flag = true;
				}
			}
			else
			{
				position = miriamStart.transform.position;
				orientation = miriamStart.spawnOrientation;
				flag = true;
			}
			break;
		}
		case PosibleSpawnPoints.Door:
		{
			Door[] source2 = UnityEngine.Object.FindObjectsOfType<Door>();
			currentDoor = source2.FirstOrDefault((Door p) => p.identificativeName == spawnId);
			if ((bool)currentDoor)
			{
				position = ((!(currentDoor.spawnPoint != null)) ? currentDoor.transform.position : currentDoor.spawnPoint.position);
				orientation = currentDoor.exitOrientation;
				flag = true;
			}
			break;
		}
		case PosibleSpawnPoints.Editor:
		{
			DebugSpawn debugSpawn = UnityEngine.Object.FindObjectOfType<DebugSpawn>();
			if ((bool)debugSpawn)
			{
				position = debugSpawn.transform.position;
				orientation = EntityOrientation.Right;
				flag = true;
				empty = debugSpawn.initialCommands;
			}
			else if (array.Length > 0)
			{
				position = array[0].transform.position;
				orientation = array[0].spawnOrientation;
				flag = true;
			}
			break;
		}
		case PosibleSpawnPoints.Menu:
			flag = true;
			break;
		case PosibleSpawnPoints.SafePosition:
			flag = true;
			position = safePosition;
			orientation = EntityOrientation.Left;
			break;
		case PosibleSpawnPoints.CustomPosition:
			flag = true;
			position = customPosition;
			orientation = customOrientation;
			break;
		case PosibleSpawnPoints.None:
			Debug.LogWarning("SpawnManager: Pending spawn IS NONE");
			break;
		}
		if (!flag)
		{
			Debug.LogWarning("SpawnManager: Pending spawn " + pendingSpawn.ToString() + " with id " + spawnId + "not found, trying spanw first PrieDeu or Debug or Zero");
			if (array.Length > 0)
			{
				position = array[0].transform.position;
				orientation = array[0].spawnOrientation;
			}
			else
			{
				DebugSpawn debugSpawn2 = UnityEngine.Object.FindObjectOfType<DebugSpawn>();
				if ((bool)debugSpawn2)
				{
					position = debugSpawn2.transform.position;
					orientation = EntityOrientation.Right;
				}
			}
		}
		CreatePlayer(position, orientation, createNewInstance);
		if (Core.Events.GetFlag("CHERUB_RESPAWN"))
		{
			Core.Logic.Penitent.CherubRespawn();
		}
		if (FirstSpanw)
		{
			FirstSpanw = false;
			UIController.instance.UpdatePurgePoints();
			UIController.instance.UpdateGuiltLevel(whenDead: true);
		}
	}

	private void LoadAllTeleports()
	{
		TeleportDestination[] source = Resources.LoadAll<TeleportDestination>("Teleport/");
		TeleportList = new List<TeleportDestination>(source.OrderBy((TeleportDestination x) => x.order));
		TeleportDict = new Dictionary<string, TeleportDestination>();
		foreach (TeleportDestination teleport in TeleportList)
		{
			teleport.isActive = teleport.activeAtStart;
			TeleportDict[teleport.id] = teleport;
		}
	}

	private void CreatePlayer(Vector3 position, EntityOrientation orientation, bool createNewInstance)
	{
		if (!penitentPrefab)
		{
			penitentPrefab = Resources.Load<Penitent>("Core/Penitent");
		}
		if (createNewInstance)
		{
			Core.Logic.Penitent = UnityEngine.Object.Instantiate(penitentPrefab, position, Quaternion.identity);
		}
		else
		{
			Core.Logic.Penitent.transform.position = position;
		}
		if (HidePlayerInNextSpawn)
		{
			Core.Logic.Penitent.SpriteRenderer.enabled = false;
			Core.Logic.Penitent.DamageArea.enabled = false;
			Core.Logic.Penitent.Status.CastShadow = false;
			Core.Logic.Penitent.Physics.EnableColliders(enable: false);
			Core.Logic.Penitent.Physics.EnablePhysics(enable: false);
			HidePlayerInNextSpawn = false;
		}
		Core.Logic.Penitent.SetOrientation(orientation);
		if (SpawnManager.OnPlayerSpawn != null)
		{
			SpawnManager.OnPlayerSpawn(Core.Logic.Penitent);
		}
		switch (pendingSpawn)
		{
		case PosibleSpawnPoints.Teleport:
			if (SpawnManager.OnTeleport != null)
			{
				SpawnManager.OnTeleport(spawnId);
			}
			break;
		case PosibleSpawnPoints.TeleportPrieDieu:
			if (SpawnManager.OnTeleportPrieDieu != null)
			{
				SpawnManager.OnTeleportPrieDieu(spawnId);
			}
			break;
		case PosibleSpawnPoints.Door:
			if ((bool)currentDoor)
			{
				Core.LevelManager.PendingrDoorToExit = currentDoor;
			}
			break;
		}
		currentDoor = null;
	}

	private void SpawnPlayer(string level, PosibleSpawnPoints spawnType, string id, bool usefade = true, bool forceLoad = false, Color? background = null)
	{
		pendingSpawn = spawnType;
		spawnId = id;
		bool flag = level == Core.LevelManager.currentLevel.LevelName;
		if (spawnType == PosibleSpawnPoints.Menu || (!forceLoad && flag))
		{
			if (spawnType == PosibleSpawnPoints.Teleport)
			{
				Teleport[] source = UnityEngine.Object.FindObjectsOfType<Teleport>();
				Teleport teleport = source.FirstOrDefault((Teleport p) => p.telportName == spawnId);
				if ((bool)teleport)
				{
					Core.Logic.Penitent.transform.position = teleport.transform.position;
					Core.Logic.Penitent.SetOrientation(teleport.spawnOrientation);
					if (SpawnManager.OnTeleport != null)
					{
						SpawnManager.OnTeleport(spawnId);
					}
				}
				else
				{
					Debug.LogWarning("** TELEPORT inside level " + spawnId + " Not found");
				}
			}
			else
			{
				SpawnPlayerOnLevelLoad(!flag);
			}
		}
		else
		{
			LevelManager levelManager = Core.LevelManager;
			bool useFade = usefade;
			levelManager.ChangeLevel(level, startFromEditor: false, useFade, forceLoad, background);
		}
	}

	public string GetPersistenID()
	{
		return "ID_CHECKPOINT_MANAGER";
	}

	public int GetOrder()
	{
		return 0;
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		CheckPointPersistenceData checkPointPersistenceData = new CheckPointPersistenceData();
		if (activePrieDieuScene.IsNullOrWhitespace())
		{
			Debug.LogWarning("PersistentManager get activePrieDieuScene, no scene yet");
			checkPointPersistenceData.activePrieDieuScene = InitialScene;
		}
		else
		{
			checkPointPersistenceData.activePrieDieuScene = activePrieDieuScene;
		}
		checkPointPersistenceData.activePrieDieuId = activePrieDieuId;
		Debug.Log($"<color=red> Active priedieu scene: {activePrieDieuScene}\n activePriedieuId{activePrieDieuId}</color>");
		checkPointPersistenceData.pendingSpawn = pendingSpawn;
		checkPointPersistenceData.spawnId = spawnId;
		foreach (TeleportDestination teleport in TeleportList)
		{
			if (teleport.isActive)
			{
				checkPointPersistenceData.activeTeleports.Add(teleport.id);
			}
		}
		return checkPointPersistenceData;
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		CheckPointPersistenceData checkPointPersistenceData = (CheckPointPersistenceData)data;
		activePrieDieuScene = checkPointPersistenceData.activePrieDieuScene;
		activePrieDieuId = checkPointPersistenceData.activePrieDieuId;
		if (isloading)
		{
			pendingSpawn = PosibleSpawnPoints.PrieDieu;
			spawnId = activePrieDieuId;
			TeleportList.ForEach(delegate(TeleportDestination element)
			{
				element.isActive = element.activeAtStart;
			});
			{
				foreach (string activeTeleport in checkPointPersistenceData.activeTeleports)
				{
					if (TeleportDict.ContainsKey(activeTeleport))
					{
						TeleportDict[activeTeleport].isActive = true;
					}
				}
				return;
			}
		}
		pendingSpawn = checkPointPersistenceData.pendingSpawn;
		spawnId = checkPointPersistenceData.spawnId;
		if (pendingSpawn == PosibleSpawnPoints.TeleportPrieDieu)
		{
			PrieDieu[] array = UnityEngine.Object.FindObjectsOfType<PrieDieu>();
			if (array.Length > 0)
			{
				ActivePrieDieu = array[0];
			}
		}
	}

	public void ResetPersistence()
	{
		activePrieDieuScene = string.Empty;
		activePrieDieuId = string.Empty;
		pendingSpawn = PosibleSpawnPoints.None;
		spawnId = string.Empty;
		LoadAllTeleports();
	}

	public static List<string> GetAllTeleportsId()
	{
		if (forceReload || cachedTeleportId.Count == 0)
		{
			cachedTeleportId.Clear();
			TeleportDestination[] source = Resources.LoadAll<TeleportDestination>("Teleport/");
			foreach (TeleportDestination item in source.OrderBy((TeleportDestination x) => x.order))
			{
				cachedTeleportId.Add(item.id);
			}
			cachedTeleportId.Sort();
		}
		forceReload = false;
		return cachedTeleportId;
	}
}
