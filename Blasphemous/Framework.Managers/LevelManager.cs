using System.Collections;
using System.Collections.Generic;
using System.IO;
using Com.LuisPedroFonseca.ProCamera2D;
using Framework.FrameworkCore;
using Framework.Map;
using Framework.Util;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Penitent;
using Gameplay.UI;
using Gameplay.UI.Console;
using Gameplay.UI.Widgets;
using Tools.Level.Effects;
using Tools.Level.Interactables;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.Managers;

public class LevelManager : GameSystem
{
	public enum LevelStatus
	{
		Unloaded,
		Unloading,
		Loaded,
		Loading,
		Activated,
		Activating,
		Deactivated,
		Deactivating
	}

	public delegate void EmptyDelegate();

	public delegate void SceneDelegate(Scene unityScene);

	public delegate void LevelDelegate(Level oldLevel, Level newLevel);

	public enum CinematicsChangeLevel
	{
		No,
		Intro,
		Outro
	}

	public const string GENERIC_ELEMENTS_SCENE_NAME = "GenericElements";

	public const string PRELOAD_PLATFORM_PREFIX = "Preload_";

	private bool mustWaitToCacheScene;

	private Level preloadLevel;

	private const float SCENE_FADE_DELAY = 0.25f;

	private const int MAX_DISTANCE = 999;

	private const int STREAMING_MAX_DISTANCE = 4;

	private const int STREAMING_DISTANCE_TO_LOAD = 2;

	private string lastZoneName = string.Empty;

	private Dictionary<string, Level> levels = new Dictionary<string, Level>();

	private ScriptableLevelEffects levelEffectsDatabase;

	public bool InsideChangeLevel { get; private set; }

	public Level currentLevel { get; private set; }

	public Level lastLevel { get; private set; }

	public Door PendingrDoorToExit { get; set; }

	public Vector3 LastSafePosition { get; private set; }

	public string LastSafeLevel { get; private set; }

	public Vector3 LastSafeGuiltPosition { get; private set; }

	public string LastSafeGuiltLevel { get; private set; }

	public CellKey LastSafeGuiltCellKey { get; private set; }

	public CinematicsChangeLevel InCinematicsChangeLevel { get; private set; }

	public static event EmptyDelegate OnMenuLoaded;

	public static event EmptyDelegate OnGenericsElementsLoaded;

	public static event LevelDelegate OnBeforeLevelLoad;

	public static event LevelDelegate OnLevelPreLoaded;

	public static event LevelDelegate OnLevelLoaded;

	public override void AllPreInitialized()
	{
		LastSafePosition = Vector3.zero;
		LastSafeLevel = string.Empty;
		LastSafeGuiltPosition = Vector3.zero;
		LastSafeGuiltLevel = string.Empty;
		LastSafeGuiltCellKey = null;
		currentLevel = null;
		lastLevel = null;
		preloadLevel = new Level("Preload_" + GetPlatformName(), bundle: false);
		mustWaitToCacheScene = false;
		levelEffectsDatabase = Resources.Load("LevelEffectsDatabase") as ScriptableLevelEffects;
		InjectGenericElements();
		for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
			string text = fileNameWithoutExtension.Split('_')[0];
			if (!levels.ContainsKey(text) && !(text == "GenericElements") && !text.StartsWith("Preload_"))
			{
				levels[text] = new Level(text);
				levels[text].Distance = 999;
			}
		}
		Application.backgroundLoadingPriority = ThreadPriority.Low;
		InCinematicsChangeLevel = CinematicsChangeLevel.No;
		InsideChangeLevel = false;
	}

	public void ChangeLevel(string levelName, bool startFromEditor = false, bool useFade = true, bool forceDeactivate = false, Color? background = null)
	{
		if (!InsideChangeLevel)
		{
			Singleton<Core>.Instance.StartCoroutine(ChangeLevelCorrutine(levelName, string.Empty, startFromEditor, useFade, forceDeactivate, background));
		}
		else
		{
			Debug.LogError("Calling a ChangeLevel when a change level is in progress");
		}
	}

	public void ChangeLevelAndPlayEvent(string levelName, string eventName, bool hideplayer = true, bool useFade = true, bool forceDeactivate = false, Color? background = null, bool Miriam = false)
	{
		if (!InsideChangeLevel)
		{
			InCinematicsChangeLevel = CinematicsChangeLevel.Intro;
			Core.SpawnManager.SetCurrentToCustomSpawnData(Miriam);
			Core.SpawnManager.HidePlayerInNextSpawn = hideplayer;
			Singleton<Core>.Instance.StartCoroutine(ChangeLevelCorrutine(levelName, eventName, startFromEditor: false, useFade, forceDeactivate, background));
		}
		else
		{
			Debug.LogError("Calling a ChangeLevel when a change level is in progress");
		}
	}

	public void RestoreFromChangeLevelAndPlayEvent(bool useFade = true, Color? background = null)
	{
		InCinematicsChangeLevel = CinematicsChangeLevel.Outro;
		Core.SpawnManager.SpawnFromCustom(useFade, background);
	}

	private IEnumerator ChangeLevelCorrutine(string levelName, string eventName, bool startFromEditor = false, bool useFade = true, bool forceDeactivate = false, Color? background = null)
	{
		InsideChangeLevel = true;
		if (!startFromEditor && !levels.ContainsKey(levelName))
		{
			Debug.LogError("LevelManager: Try to load level '" + levelName + "' that it's not in build");
		}
		else
		{
			Level level = levels[levelName];
			if (LevelManager.OnBeforeLevelLoad != null)
			{
				LevelManager.OnBeforeLevelLoad(currentLevel, level);
			}
			Core.Persistence.OnBeforeLevelLoad(currentLevel, level);
			if (currentLevel != null && (level != currentLevel || forceDeactivate))
			{
				if (currentLevel.CurrentStatus != LevelStatus.Activated)
				{
					Debug.LogError("LevelManager: Load new level and current status is " + currentLevel.CurrentStatus);
				}
				else
				{
					yield return DeactivateCurrentAndLoadNew(level, useFade, background);
				}
			}
			else
			{
				yield return LoadAndActivateLevel(level, useFade, background);
			}
			if (eventName != string.Empty)
			{
				PlayMakerFSM.BroadcastEvent(eventName);
			}
		}
		InsideChangeLevel = false;
	}

	public void ActivatePrecachedScene()
	{
		mustWaitToCacheScene = true;
	}

	public List<string> DEBUG_GetAllLevelsName()
	{
		return new List<string>(levels.Keys);
	}

	public string GetLastSceneName()
	{
		return (lastLevel == null) ? string.Empty : lastLevel.LevelName;
	}

	public LevelColorEffectData GetLevelEffects(LEVEL_COLOR_CONFIGS configType)
	{
		LevelColorEffectData value = default(LevelColorEffectData);
		if (levelEffectsDatabase.levelColorConfigs.TryGetValue(configType, out value))
		{
			return value;
		}
		levelEffectsDatabase.levelColorConfigs.TryGetValue(LEVEL_COLOR_CONFIGS.NONE, out value);
		return value;
	}

	public void SetPlayerSafePosition(Vector3 safePosition)
	{
		LastSafePosition = safePosition;
		LastSafeLevel = currentLevel.LevelName;
		if (Core.Logic.CurrentLevelConfig.UseDefaultGuiltSystem)
		{
			LastSafeGuiltPosition = safePosition;
			LastSafeGuiltLevel = currentLevel.LevelName;
			LastSafeGuiltCellKey = Core.NewMapManager.GetPlayerCell();
		}
		else if (Core.Logic.CurrentLevelConfig.OverrideGuiltPosition)
		{
			LastSafeGuiltPosition = Core.Logic.CurrentLevelConfig.guiltPositionOverrider.position;
			LastSafeGuiltLevel = currentLevel.LevelName;
			LastSafeGuiltCellKey = Core.NewMapManager.GetCellKeyFromPosition(LastSafeGuiltLevel, LastSafeGuiltPosition);
		}
	}

	private string GetPlatformName()
	{
		string result = Application.platform.ToString();
		switch (Application.platform)
		{
		case RuntimePlatform.WindowsPlayer:
		case RuntimePlatform.WindowsEditor:
			result = "Windows";
			break;
		case RuntimePlatform.LinuxPlayer:
		case RuntimePlatform.LinuxEditor:
			result = "Linux";
			break;
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.OSXPlayer:
			result = "OSX";
			break;
		}
		return result;
	}

	private IEnumerator WaitForPreloadCacheScene()
	{
		while (preloadLevel.CurrentStatus != LevelStatus.Activated)
		{
			switch (preloadLevel.CurrentStatus)
			{
			case LevelStatus.Loaded:
			case LevelStatus.Deactivated:
				yield return preloadLevel.Activate();
				break;
			case LevelStatus.Unloaded:
				yield return preloadLevel.Load(streaming: false);
				break;
			default:
				yield return null;
				break;
			}
		}
	}

	private IEnumerator DeactivateCurrentAndLoadNew(Level level, bool useFade, Color? background = null)
	{
		UIController.instance.ShowLoad(show: true, background);
		yield return new WaitForEndOfFrame();
		yield return currentLevel.DeActivate();
		yield return LoadAndActivateLevel(level, useFade, background);
	}

	private IEnumerator LoadAndActivateLevel(Level level, bool useFade, Color? background = null)
	{
		Debug.Log("---> LevelManager:  Load and activate level " + level.LevelName);
		yield return new WaitForEndOfFrame();
		ClearOldLevelParams();
		Core.Logic.SetState(LogicStates.Unresponsive);
		lastLevel = currentLevel;
		currentLevel = level;
		currentLevel.Distance = 0;
		if (lastLevel != null)
		{
			lastZoneName = lastLevel.LevelName.Substring(0, 6);
		}
		if (mustWaitToCacheScene)
		{
			yield return WaitForPreloadCacheScene();
		}
		Core.UI.ShowGamePlayUI = true;
		while (level.CurrentStatus != LevelStatus.Activated)
		{
			switch (level.CurrentStatus)
			{
			case LevelStatus.Loaded:
			case LevelStatus.Deactivated:
				yield return level.Activate();
				break;
			case LevelStatus.Unloaded:
				yield return level.Load(streaming: false);
				break;
			default:
				yield return null;
				break;
			}
		}
		InitializeLevel();
		if (!level.LevelName.Equals("D07Z01S04") && !level.LevelName.StartsWith("D19Z01"))
		{
			Singleton<Core>.Instance.StartCoroutine(StreamingLevels());
			Core.Input.SetBlocker("BLOCK_UNTIL_FPS_STABLE", blocking: true);
			float timeWaiting = 0f;
			while (1f / Time.smoothDeltaTime < 50f && timeWaiting < 3f)
			{
				timeWaiting += Time.smoothDeltaTime;
				yield return null;
			}
			Core.Input.SetBlocker("BLOCK_UNTIL_FPS_STABLE", blocking: false);
		}
		Penitent player = Core.Logic.Penitent;
		player.PlatformCharacterController.PlatformCharacterPhysics.GravityScale = 0f;
		for (int i = 0; i < 5; i++)
		{
			yield return new WaitForEndOfFrame();
		}
		float groundDist = player.PlatformCharacterController.GroundDist;
		Vector3 spawnPosition = new Vector3(player.transform.position.x, player.transform.position.y - groundDist, player.transform.position.z);
		player.transform.position = spawnPosition;
		player.Animator.Play("Idle");
		player.PlatformCharacterController.PlatformCharacterPhysics.GravityScale = 3f;
		UpdateNewCameraParams();
		Core.Logic.SetState(LogicStates.Playing);
		UIController.instance.ShowLoad(show: false, background);
		float time = ((!useFade) ? 0f : 0.6f);
		Color fadecolor = (background.HasValue ? background.Value : Color.black);
		FadeWidget.instance.StartEasyFade(fadecolor, new Color(0f, 0f, 0f, 0f), time, toBlack: false);
		Core.Input.ResetManager();
		Core.Persistence.OnLevelLoaded(lastLevel, currentLevel);
		if (LevelManager.OnLevelLoaded != null)
		{
			LevelManager.OnLevelLoaded(lastLevel, currentLevel);
		}
		if (InCinematicsChangeLevel == CinematicsChangeLevel.Outro)
		{
			InCinematicsChangeLevel = CinematicsChangeLevel.No;
		}
		if (PendingrDoorToExit != null)
		{
			PendingrDoorToExit.ExitFromThisDoor();
			PendingrDoorToExit = null;
		}
	}

	private void ClearOldLevelParams()
	{
		ProCamera2DNumericBoundaries proCamera2DNumericBoundaries = Core.Logic.CameraManager.ProCamera2DNumericBoundaries;
		proCamera2DNumericBoundaries.UseNumericBoundaries = false;
		proCamera2DNumericBoundaries.UseTopBoundary = false;
		proCamera2DNumericBoundaries.UseBottomBoundary = false;
		proCamera2DNumericBoundaries.UseLeftBoundary = false;
		proCamera2DNumericBoundaries.UseRightBoundary = false;
		Core.Logic.CameraManager.ProCamera2D.FollowVertical = true;
		Core.Logic.CameraManager.ProCamera2D.FollowHorizontal = true;
	}

	private void UpdateNewCameraParams()
	{
		Core.Logic.CameraManager.UpdateNewCameraParams();
		Core.Logic.CameraManager.CameraPlayerOffset.UpdateNewParams();
		CameraNumericBoundaries[] array = Object.FindObjectsOfType<CameraNumericBoundaries>();
		if (array.Length <= 0)
		{
			return;
		}
		bool flag = false;
		CameraNumericBoundaries[] array2 = array;
		foreach (CameraNumericBoundaries cameraNumericBoundaries in array2)
		{
			if (!cameraNumericBoundaries.notSetOnLevelLoad)
			{
				if (flag)
				{
					Debug.LogWarning("UpdateNewCameraParams " + array.Length + " CameraNumericBoundaries found, only first applied");
					break;
				}
				flag = true;
				cameraNumericBoundaries.SetBoundariesOnLevelLoad();
			}
		}
	}

	private void InitializeLevel()
	{
		SceneManager.SetActiveScene(currentLevel.GetLogicScene().Scene);
		if (LevelManager.OnLevelPreLoaded != null)
		{
			LevelManager.OnLevelPreLoaded(lastLevel, currentLevel);
		}
		if (Core.Logic.DebugExecutionEnabled)
		{
			ExecutionCommand.EnableDebugExecution();
		}
		Core.Logic.EnemySpawner.SpawnEnemiesOnLoad();
		Core.SpawnManager.SpawnPlayerOnLevelLoad();
	}

	private IEnumerator StreamingLevels()
	{
		List<Level> allLevels = new List<Level>(levels.Values);
		foreach (Level level in allLevels)
		{
			if (level.IsBundle && level.CurrentStatus == LevelStatus.Unloaded && IsInSameZone(level))
			{
				yield return level.Load(streaming: true);
			}
			else if (level.IsBundle && (level.CurrentStatus == LevelStatus.Loaded || level.CurrentStatus == LevelStatus.Deactivated) && !IsInSameZone(level) && !IsInLastZone(level))
			{
				yield return level.UnLoad();
			}
		}
	}

	private bool IsInSameZone(Level level)
	{
		string value = currentLevel.LevelName.Substring(0, 6);
		return level.LevelName.StartsWith(value);
	}

	private bool IsInLastZone(Level level)
	{
		return level.LevelName.StartsWith(lastZoneName);
	}

	private void OnBaseSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "GenericElements")
		{
			SceneManager.sceneLoaded -= OnBaseSceneLoaded;
			if (LevelManager.OnGenericsElementsLoaded != null)
			{
				LevelManager.OnGenericsElementsLoaded();
			}
		}
	}

	private void InjectGenericElements()
	{
		SceneManager.sceneLoaded += OnBaseSceneLoaded;
		if (true)
		{
			SceneManager.LoadScene("GenericElements", LoadSceneMode.Additive);
		}
	}
}
