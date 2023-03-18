using System;
using System.Collections.Generic;
using Framework.EditorScripts.BossesBalance;
using Framework.EditorScripts.EnemiesBalance;
using Framework.FrameworkCore;
using UnityEngine;

namespace Framework.Managers;

public class GameModeManager : GameSystem, PersistentInterface
{
	[Serializable]
	public enum GAME_MODES
	{
		MENU,
		NEW_GAME,
		NEW_GAME_PLUS,
		BOSS_RUSH,
		DEMAKE
	}

	[Serializable]
	public class GameModePersistenceData : PersistentManager.PersistentData
	{
		public GAME_MODES gameMode = GAME_MODES.NEW_GAME;

		public int NewGamePlusUpgrades;

		public GameModePersistenceData()
			: base("ID_GAMEMMODE")
		{
		}
	}

	public Core.SimpleEvent OnEnterMenuMode;

	public Core.SimpleEvent OnEnterNewGameMode;

	public Core.SimpleEvent OnEnterNewGamePlusMode;

	public Core.SimpleEvent OnEnterDemakeMode;

	public Core.SimpleEvent OnExitMenuMode;

	public Core.SimpleEvent OnExitNewGameMode;

	public Core.SimpleEvent OnExitNewGamePlusMode;

	public Core.SimpleEvent OnExitDemakeMode;

	public const string SCENE_GAMEPLUS = "d07z01s03";

	private const string NEW_GAME_BALANCE_CHART_PATH = "Enemies Balance Charts/EnemiesBalanceChartNewGame";

	private const string NEW_GAME_PLUS_BALANCE_CHART_PATH = "Enemies Balance Charts/EnemiesBalanceChartNewGamePlus";

	private const string NEW_GAME_BOSS_CHART = "Bosses Balance Charts/BossesBalanceChartNewGame";

	private const string NEW_GAME_PLUS_BOSS_CHART = "Bosses Balance Charts/BossesBalanceChartNewGamePlus";

	private const string BOSS_RUSH_NORMAL_BOSS_CHART = "Bosses Balance Charts/BossesBalanceChartBossRushNormal";

	private const string BOSS_RUSH_HARD_BOSS_CHART = "Bosses Balance Charts/BossesBalanceChartBossRushHard";

	private EnemiesBalanceChart newGameEnemiesBalanceChart;

	private EnemiesBalanceChart newGamePlusEnemiesBalanceChart;

	private BossesBalanceChart newGameBossesBalanceChart;

	private BossesBalanceChart newGamePlusBossesBalanceChart;

	private BossesBalanceChart bossRushNormalBalanceChart;

	private BossesBalanceChart bossRushHardBalanceChart;

	private GAME_MODES currentMode;

	private int NewGamePlusUpgrades;

	private const string PERSITENT_ID = "ID_GAMEMMODE";

	public override void Initialize()
	{
		currentMode = GAME_MODES.MENU;
		newGameEnemiesBalanceChart = Resources.Load<EnemiesBalanceChart>("Enemies Balance Charts/EnemiesBalanceChartNewGame");
		newGamePlusEnemiesBalanceChart = Resources.Load<EnemiesBalanceChart>("Enemies Balance Charts/EnemiesBalanceChartNewGamePlus");
		newGameBossesBalanceChart = Resources.Load<BossesBalanceChart>("Bosses Balance Charts/BossesBalanceChartNewGame");
		newGamePlusBossesBalanceChart = Resources.Load<BossesBalanceChart>("Bosses Balance Charts/BossesBalanceChartNewGamePlus");
		bossRushNormalBalanceChart = Resources.Load<BossesBalanceChart>("Bosses Balance Charts/BossesBalanceChartBossRushNormal");
		bossRushHardBalanceChart = Resources.Load<BossesBalanceChart>("Bosses Balance Charts/BossesBalanceChartBossRushHard");
	}

	public override void AllInitialized()
	{
		Core.Persistence.AddPersistentManager(this);
	}

	public static List<string> GetAllGameModesNames()
	{
		return new List<string>(Enum.GetNames(typeof(GAME_MODES)));
	}

	public GAME_MODES GetCurrentGameMode()
	{
		return currentMode;
	}

	public string GetCurrentGameModeName()
	{
		return Enum.GetName(typeof(GAME_MODES), currentMode);
	}

	public bool CheckGameModeActive(string mode)
	{
		if (GameModeExists(mode))
		{
			return currentMode == (GAME_MODES)Enum.Parse(typeof(GAME_MODES), mode, ignoreCase: true);
		}
		return false;
	}

	public bool GameModeExists(string mode)
	{
		foreach (string allGameModesName in GetAllGameModesNames())
		{
			if (allGameModesName.Equals(mode.ToUpperInvariant()))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCurrentMode(GAME_MODES mode)
	{
		return currentMode == mode;
	}

	public void ChangeMode(string newMode)
	{
		if (!GameModeExists(newMode))
		{
			Debug.Log("GameModeManager:: ChangeMode: A Game Mode with name: " + newMode + " doesn't exist!");
		}
		else
		{
			ChangeMode((GAME_MODES)Enum.Parse(typeof(GAME_MODES), newMode, ignoreCase: true));
		}
	}

	public void ChangeMode(GAME_MODES newMode)
	{
		if (newMode == currentMode)
		{
			Debug.Log("GameModeManager:: ChangeMode: current game mode and new game mode are the same! Current game mode: " + currentMode);
			return;
		}
		GAME_MODES gAME_MODES = currentMode;
		Debug.Log(string.Concat("GameModeManager:: ChangeMode: changing current mode to ", newMode, "! Old mode: ", gAME_MODES));
		ExitMode(currentMode);
		currentMode = newMode;
		EnterMode(currentMode);
	}

	public int GetNewGamePlusUpgrades()
	{
		return NewGamePlusUpgrades;
	}

	public EnemiesBalanceChart GetCurrentEnemiesBalanceChart()
	{
		switch (currentMode)
		{
		case GAME_MODES.NEW_GAME:
			return newGameEnemiesBalanceChart;
		case GAME_MODES.NEW_GAME_PLUS:
			return newGamePlusEnemiesBalanceChart;
		default:
			Debug.Log(string.Concat("GetCurrentEnemiesBalanceChart: current Game Mode is '", currentMode, "'! Returning base game Enemies Balance Chart by default"));
			return newGameEnemiesBalanceChart;
		}
	}

	public BossesBalanceChart GetCurrentBossesBalanceChart()
	{
		switch (currentMode)
		{
		case GAME_MODES.NEW_GAME:
			return newGameBossesBalanceChart;
		case GAME_MODES.NEW_GAME_PLUS:
			return newGamePlusBossesBalanceChart;
		case GAME_MODES.BOSS_RUSH:
			if (Core.BossRushManager.GetCurrentBossRushMode() == BossRushManager.BossRushCourseMode.NORMAL)
			{
				return bossRushNormalBalanceChart;
			}
			return bossRushHardBalanceChart;
		default:
			Debug.Log(string.Concat("GetCurrentEnemiesBossesChart: current Game Mode is '", currentMode, "'! Returning base game Bosses Balance Chart by default"));
			return newGameBossesBalanceChart;
		}
	}

	public bool CanConvertToNewGamePlus(PersistentManager.SnapShot snapshot)
	{
		bool flag = false;
		return snapshot.sceneElements.ContainsKey("d07z01s03");
	}

	public void ConvertCurrentGameToPlus()
	{
		ChangeMode(GAME_MODES.NEW_GAME_PLUS);
		NewGamePlusUpgrades++;
		Core.SpawnManager.PrepareForNewGamePlus("D17Z01S01");
		Core.Persistence.PrepareForNewGamePlus();
		Core.PenitenceManager.DeactivateCurrentPenitence();
		Core.Alms.ResetPersistence();
		Core.InventoryManager.PrepareForNewGamePlus();
		Core.GuiltManager.ResetGuilt(restoreDropTears: false);
		Core.Logic.Penitent.Stats.Purge.Current = 0f;
		Core.Logic.Penitent.Stats.Flask.SetPermanentBonus(0f);
		Core.Logic.Penitent.Stats.FlaskHealth.SetPermanentBonus(0f);
		Core.Logic.Penitent.Stats.BeadSlots.SetPermanentBonus(0f);
		Core.Logic.Penitent.Stats.Fervour.SetPermanentBonus(0f);
		Core.Logic.Penitent.Stats.Life.SetPermanentBonus(0f);
		Core.Events.PrepareForNewGamePlus();
		Core.NewMapManager.ResetPersistence();
		Core.AchievementsManager.PrepareForNewGamePlus();
	}

	public bool ShouldProgressAchievements()
	{
		return currentMode == GAME_MODES.NEW_GAME || currentMode == GAME_MODES.NEW_GAME_PLUS;
	}

	private void EnterMode(GAME_MODES m)
	{
		switch (m)
		{
		case GAME_MODES.MENU:
			OnEnterMenu();
			break;
		case GAME_MODES.NEW_GAME:
			OnEnterNewGame();
			break;
		case GAME_MODES.NEW_GAME_PLUS:
			OnEnterNewGamePlus();
			break;
		case GAME_MODES.DEMAKE:
			OnEnterDemake();
			break;
		case GAME_MODES.BOSS_RUSH:
			break;
		}
	}

	private void OnEnterMenu()
	{
		if (OnEnterMenuMode != null)
		{
			OnEnterMenuMode();
		}
	}

	private void OnEnterNewGame()
	{
		if (OnEnterNewGameMode != null)
		{
			OnEnterNewGameMode();
		}
	}

	private void OnEnterNewGamePlus()
	{
		if (OnEnterNewGamePlusMode != null)
		{
			OnEnterNewGamePlusMode();
		}
	}

	private void OnEnterDemake()
	{
		if (OnEnterDemakeMode != null)
		{
			OnEnterDemakeMode();
		}
	}

	private void ExitMode(GAME_MODES m)
	{
		switch (m)
		{
		case GAME_MODES.MENU:
			OnExitMenu();
			break;
		case GAME_MODES.NEW_GAME:
			OnExitNewGame();
			break;
		case GAME_MODES.NEW_GAME_PLUS:
			OnExitNewGamePlus();
			break;
		case GAME_MODES.DEMAKE:
			OnExitDemake();
			break;
		case GAME_MODES.BOSS_RUSH:
			break;
		}
	}

	private void OnExitMenu()
	{
		if (OnExitMenuMode != null)
		{
			OnExitMenuMode();
		}
	}

	private void OnExitNewGame()
	{
		if (OnExitNewGameMode != null)
		{
			OnExitNewGameMode();
		}
	}

	private void OnExitNewGamePlus()
	{
		if (OnExitNewGamePlusMode != null)
		{
			OnExitNewGamePlusMode();
		}
	}

	private void OnExitDemake()
	{
		if (OnExitDemakeMode != null)
		{
			OnExitDemakeMode();
		}
	}

	public int GetOrder()
	{
		return 0;
	}

	public string GetPersistenID()
	{
		return "ID_GAMEMMODE";
	}

	public void ResetPersistence()
	{
		ChangeMode(GAME_MODES.NEW_GAME);
		NewGamePlusUpgrades = 0;
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		GameModePersistenceData gameModePersistenceData = new GameModePersistenceData();
		gameModePersistenceData.gameMode = currentMode;
		gameModePersistenceData.NewGamePlusUpgrades = NewGamePlusUpgrades;
		return gameModePersistenceData;
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		GameModePersistenceData gameModePersistenceData = (GameModePersistenceData)data;
		NewGamePlusUpgrades = gameModePersistenceData.NewGamePlusUpgrades;
		ChangeMode(gameModePersistenceData.gameMode);
	}
}
