using System;
using System.Collections.ObjectModel;
using DG.Tweening;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.FrameworkCore.Attributes.Logic;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Effects.NPCs.BloodDecals;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Environment.Breakable;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Spawn;
using Gameplay.UI;
using Tools.Level.Layout;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.Managers;

public class LogicManager : GameSystem
{
	public delegate void StateEvent(LogicStates state);

	public static Core.SimpleEvent GoToMainMenu;

	public Core.SimpleEvent OnUsePrieDieu;

	public StateEvent OnStateChanged;

	private LogicStates previousState;

	public const string MENU_LEVEL_NAME = "MainMenu";

	private const string ATTRACK_NAME = "AttrackMode";

	public const string CREDITS_LEVEL_NAME = "D07Z01S04";

	public const string ARENA_LEVELS_PREFIX = "D19Z01";

	public const string LAKE_LEVEL_PREFIX = "D13Z01";

	public const string PAINTING_LEVEL_PREFIX = "D18Z01";

	public const string HW_LEVEL_PREFIX = "D24Z01";

	private float currentScale;

	public LogicStates CurrentState { get; private set; }

	public bool DebugExecutionEnabled { get; set; }

	public ScreenFreezeManager ScreenFreeze => UnityEngine.Object.FindObjectOfType<ScreenFreezeManager>();

	public CameraShakeManager CameraShakeManager { get; private set; }

	public EnemySpawner EnemySpawner { get; private set; }

	public ExecutionController ExecutionController { get; private set; }

	public BreakableManager BreakableManager { get; set; }

	public LevelInitializer CurrentLevelConfig { get; private set; }

	public int CurrentLevelDifficult { get; set; }

	public int CurrentLevelNumber { get; set; }

	public Penitent Penitent { get; set; }

	public PenitentSpawner PenitentSpawner { get; set; }

	public float PlayerCurrentLife { get; set; }

	public CameraManager CameraManager => CameraManager.Instance;

	public PermaBloodStorage PermaBloodStore { get; private set; }

	public float FXVolumen { get; set; }

	public bool IsPaused { get; private set; }

	public void SetState(LogicStates newState)
	{
		if (newState != CurrentState)
		{
			previousState = CurrentState;
			CurrentState = newState;
			if (OnStateChanged != null)
			{
				OnStateChanged(CurrentState);
			}
			Log.Trace("Logic", "Game state has been set to " + newState.ToString().ToUpper());
		}
	}

	public void SetPreviousState()
	{
		SetState(previousState);
	}

	public override void Initialize()
	{
		PermaBloodStore = new PermaBloodStorage();
		EnemySpawner = new EnemySpawner();
		BreakableManager = new BreakableManager();
		PenitentSpawner = new PenitentSpawner();
		ExecutionController = new ExecutionController();
		CameraShakeManager = new CameraShakeManager();
		FXVolumen = -1f;
		PlayerCurrentLife = -1f;
		IsPaused = false;
	}

	public bool IsMenuScene()
	{
		string text = string.Empty;
		if (Core.LevelManager.currentLevel != null)
		{
			text = Core.LevelManager.currentLevel.LevelName;
		}
		return text == "MainMenu";
	}

	public bool IsAttrackScene()
	{
		string name = SceneManager.GetActiveScene().name;
		return name == "AttrackMode";
	}

	public void LoadAttrackScene()
	{
		if (UIController.instance != null)
		{
			UIController.instance.HideBossHealth();
		}
		Core.LevelManager.ChangeLevel("AttrackMode");
	}

	public void LoadMenuScene(bool useFade = true)
	{
		if (GoToMainMenu != null)
		{
			GoToMainMenu();
		}
		if (UIController.instance != null)
		{
			UIController.instance.HideBossHealth();
		}
		LevelManager levelManager = Core.LevelManager;
		string levelName = "MainMenu";
		bool useFade2 = useFade;
		levelManager.ChangeLevel(levelName, startFromEditor: false, useFade2);
		Core.GameModeManager.ChangeMode(GameModeManager.GAME_MODES.MENU);
	}

	public void LoadCreditsScene(bool useFade = true)
	{
		if (UIController.instance != null)
		{
			UIController.instance.HideBossHealth();
		}
		LevelManager levelManager = Core.LevelManager;
		string levelName = "D07Z01S04";
		bool useFade2 = useFade;
		levelManager.ChangeLevel(levelName, startFromEditor: false, useFade2);
	}

	public void ResetAllData()
	{
		Core.Persistence.ResetAll();
		Core.Logic.EnemySpawner.Reset();
		Core.Logic.BreakableManager.Reset();
	}

	public void SetCurrentLevelConfig(LevelInitializer newLevel)
	{
		CurrentLevelConfig = newLevel;
	}

	public void UsePrieDieu()
	{
		if (OnUsePrieDieu != null)
		{
			OnUsePrieDieu();
		}
	}

	public void ResetPlayer()
	{
	}

	public override void OnGUI()
	{
		DebugResetLine();
		if (Penitent == null)
		{
			return;
		}
		DebugDrawTextLine("******    Stats");
		string format = "{0,-25} {1,10} {2,10} {3,10} {4,10}";
		DebugDrawTextLine(string.Format(format, "Variable", "Base", "Current", "Bonus", "Final"));
		DebugDrawTextLine("------------------------------------------------------------------------------");
		EntityStats stats = Penitent.Stats;
		string format2 = "{0,-25} {1,10:F2} {2,10:F2} {3,10:F2} {4,10:F2}";
		Array values = Enum.GetValues(typeof(EntityStats.StatsTypes));
		Array.Sort(values);
		foreach (EntityStats.StatsTypes item in values)
		{
			Framework.FrameworkCore.Attributes.Logic.Attribute byType = stats.GetByType(item);
			float num = 0f;
			if (byType.IsVariable())
			{
				VariableAttribute variableAttribute = null;
				variableAttribute = (VariableAttribute)byType;
				num = variableAttribute.Current;
			}
			DebugDrawTextLine(string.Format(format2, item.ToString(), MaxFloat(byType.Base), num, byType.Bonus, MaxFloat(byType.Final)));
		}
		DebugResetLine();
		DebugDrawTextLine("******    Bonus", 600);
		foreach (EntityStats.StatsTypes item2 in values)
		{
			Framework.FrameworkCore.Attributes.Logic.Attribute byType2 = stats.GetByType(item2);
			ReadOnlyCollection<RawBonus> rawBonus = byType2.GetRawBonus();
			if (rawBonus.Count <= 0 && !(byType2.PermanetBonus > 0f))
			{
				continue;
			}
			DebugDrawTextLine(item2.ToString() + " stat, permanet " + byType2.PermanetBonus, 600);
			foreach (RawBonus item3 in rawBonus)
			{
				DebugDrawTextLine("...Base:" + item3.Base + "  Multyplier:" + item3.Multiplier, 600);
			}
		}
	}

	private float MaxFloat(float value)
	{
		float num = 999999f;
		return (!(value > num)) ? value : num;
	}

	public void PauseGame()
	{
		if (!IsPaused)
		{
			IsPaused = true;
			currentScale = 1f;
			CurrentLevelConfig.TimeScale = 0f;
			RuntimeManager.GetBus("bus:/ALLSFX/SFX").setPaused(paused: true);
		}
	}

	public void ResumeGame()
	{
		if (IsPaused)
		{
			IsPaused = false;
			RuntimeManager.GetBus("bus:/ALLSFX/SFX").setPaused(paused: false);
			CurrentLevelConfig.TimeScale = currentScale;
			DOTween.defaultTimeScaleIndependent = false;
		}
	}

	public bool IsSlowMode()
	{
		return Time.timeScale != 1f;
	}
}
