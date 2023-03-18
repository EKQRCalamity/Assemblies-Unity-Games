using System;
using System.Collections;
using System.Collections.Generic;
using Framework.EditorScripts.EnemiesBalance;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Effects.Entity.BlobShadow;
using Gameplay.GameControllers.Environment;
using Gameplay.GameControllers.Penitent;
using Gameplay.UI;
using Sirenix.OdinInspector;
using Tools.Level.Effects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tools.Level.Layout;

[DefaultExecutionOrder(-1)]
public class LevelInitializer : MonoBehaviour
{
	public enum GuiltConfigurationEnum
	{
		Default,
		OverridePosition,
		NotInThisScene,
		DontGenerateGuilt
	}

	public const float SLEEP_TIMESCALE = 0.1f;

	[BoxGroup("Designer Settings", true, false, 0)]
	[SerializeField]
	private Color EditorBackgroundColor;

	[BoxGroup("Advanced Settings", true, false, 0)]
	[SerializeField]
	private BlobShadowManager blobShadowManager;

	[BoxGroup("Advanced Settings", true, false, 0)]
	[SerializeField]
	public Color levelShadowColor;

	[BoxGroup("Advanced Settings", true, false, 0)]
	[SerializeField]
	public bool hideShadows;

	[BoxGroup("Color Settings", true, false, 0)]
	[SerializeField]
	public bool useLevelEffectsDatabase;

	[ShowIf("useLevelEffectsDatabase", true)]
	[BoxGroup("Color Settings", true, false, 0)]
	[SerializeField]
	public LEVEL_COLOR_CONFIGS levelEffectsTemplate;

	[BoxGroup("Color Settings", true, false, 0)]
	[SerializeField]
	public Color colorizeColor;

	[BoxGroup("Color Settings", true, false, 0)]
	[SerializeField]
	[Range(0f, 1f)]
	public float colorizeAmount;

	[BoxGroup("Color Settings", true, false, 0)]
	[SerializeField]
	public Color colorizeMultColor = Color.white;

	[BoxGroup("Advanced Settings", true, false, 0)]
	public LocalReflectionConfig localReflectionConfig;

	[BoxGroup("Advanced Settings", true, false, 0)]
	public bool hasReflections;

	[BoxGroup("Advanced Settings", true, false, 0)]
	[SerializeField]
	public ScreenMaterialEffectsManager.SCREEN_EFFECTS screenEffect;

	[BoxGroup("Advanced Settings", true, false, 0)]
	[SerializeField]
	public float sleepTime;

	[BoxGroup("Designer Settings", true, false, 0)]
	[SerializeField]
	private bool DisplayZoneTitle = true;

	[BoxGroup("Designer Settings", true, false, 0)]
	[SerializeField]
	[ShowIf("DisplayZoneTitle", true)]
	public List<string> ignoreTitleIfComingFromScenes = new List<string>();

	[BoxGroup("Designer Settings", true, false, 0)]
	[SerializeField]
	public GuiltConfigurationEnum GuiltConfiguration;

	[BoxGroup("Designer Settings", true, false, 0)]
	[SerializeField]
	[ShowIf("ShowPositionOverrider", true)]
	public Transform guiltPositionOverrider;

	private EnemyStatsImporter enemyStatsImporter;

	[NonSerialized]
	public float TimeScaleReal = 1f;

	private static bool IsInitilizated;

	public bool OverrideGuiltPosition => GuiltConfiguration == GuiltConfigurationEnum.OverridePosition && (bool)guiltPositionOverrider;

	public bool UseDefaultGuiltSystem => GuiltConfiguration == GuiltConfigurationEnum.Default || (GuiltConfiguration == GuiltConfigurationEnum.OverridePosition && !guiltPositionOverrider);

	public bool LevelDebug { get; private set; }

	public bool IsSleeping { get; set; }

	public LevelEffectsStore LevelEffectsStore { get; private set; }

	public BlobShadowManager BlobShadowManager => blobShadowManager;

	public EnemyStatsImporter EnemyStatsImporter => enemyStatsImporter;

	public float TimeScale
	{
		get
		{
			return TimeScaleReal;
		}
		set
		{
			if (Math.Abs(value - TimeScaleReal) > Mathf.Epsilon)
			{
				TimeScaleReal = value;
			}
		}
	}

	private bool ShowPositionOverrider()
	{
		return GuiltConfiguration == GuiltConfigurationEnum.OverridePosition;
	}

	private void Awake()
	{
		if (!IsInitilizated)
		{
			if (!Core.ready)
			{
				Debug.Log("==============  LevelInitializer");
				Singleton<Core>.Instance.Initialize();
			}
			else
			{
				IsInitilizated = true;
			}
		}
		LevelEffectsStore = GetComponentInChildren<LevelEffectsStore>();
		LevelDebug = !Core.Events.GetFlag("EXECUTED_FROM_MAINMENU");
		IsSleeping = false;
		Core.Logic.SetCurrentLevelConfig(this);
		if (useLevelEffectsDatabase)
		{
			LevelColorEffectData levelEffects = Core.LevelManager.GetLevelEffects(levelEffectsTemplate);
			SetLevelEffectsData(levelEffects);
		}
		SetEnemyStats();
	}

	private void SetLevelEffectsData(LevelColorEffectData levelEffectsData)
	{
		colorizeAmount = levelEffectsData.colorizeAmount;
		colorizeColor = levelEffectsData.colorizeColor;
		colorizeMultColor = levelEffectsData.colorizeMultColor;
	}

	private void SetLevelScreenEffect(ScreenMaterialEffectsManager.SCREEN_EFFECTS e)
	{
		Core.Logic.CameraManager.ScreenEffectsManager.SetEffect(e);
	}

	private void Start()
	{
		ApplyBackgroundColor();
		SetLevelScreenEffect(screenEffect);
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
		if (!IsInitilizated)
		{
			string text = SceneManager.GetActiveScene().name;
			text = text.Split('_')[0];
			Core.SpawnManager.PrepareForSpawnFromMenu();
			Core.LevelManager.ChangeLevel(text, startFromEditor: true);
			IsInitilizated = true;
		}
	}

	private void SetEnemyStats()
	{
		List<EnemyBalanceItem> enemiesBalanceItems = Core.GameModeManager.GetCurrentEnemiesBalanceChart().EnemiesBalanceItems;
		if (enemiesBalanceItems.Count > 0)
		{
			enemyStatsImporter = new EnemyStatsImporter(enemiesBalanceItems);
		}
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
		ColorizePlayer();
		ApplyReflectionConfig();
	}

	private void ColorizePlayer()
	{
		MasterShaderEffects componentInChildren = Core.Logic.Penitent.GetComponentInChildren<MasterShaderEffects>();
		if (!(componentInChildren == null))
		{
			if (Math.Abs(colorizeAmount) < Mathf.Epsilon)
			{
				componentInChildren.DeactivateColorize();
			}
			else
			{
				componentInChildren.SetColorizeData(colorizeColor, colorizeMultColor, colorizeAmount);
			}
		}
	}

	public void ApplyLevelColorEffects(MasterShaderEffects effects)
	{
		effects.SetColorizeData(colorizeColor, colorizeMultColor, colorizeAmount);
	}

	private void Update()
	{
		Time.timeScale = TimeScaleReal;
	}

	private void ApplyBackgroundColor()
	{
		if (Camera.main == null)
		{
			Debug.LogWarning("Imposible to change background color. Camera not found.");
		}
		else
		{
			Camera.main.backgroundColor = EditorBackgroundColor;
		}
	}

	private void ApplyReflectionConfig()
	{
		PIDI_2DReflection reflections = Core.Logic.Penitent.reflections;
		if (!(reflections == null))
		{
			if (hasReflections)
			{
				reflections.enabled = true;
				reflections.SetLocalReflectionConfig(localReflectionConfig);
			}
			else
			{
				reflections.enabled = false;
			}
		}
	}

	private IEnumerator SleepTimeCoroutine(float sleepTimeLapse)
	{
		TimeScale = 0.1f;
		yield return new WaitForSecondsRealtime(sleepTimeLapse);
		IsSleeping = false;
		sleepTime = 0f;
		if (!UIController.instance.Paused)
		{
			TimeScale = 1f;
		}
	}

	public void SleepTime()
	{
		if (!IsSleeping && !UIController.instance.Paused)
		{
			IsSleeping = true;
			StartCoroutine(SleepTimeCoroutine(sleepTime));
		}
	}

	public bool ShowZoneTitle(Framework.FrameworkCore.Level oldLevel)
	{
		bool flag = false;
		if (oldLevel != null)
		{
			flag = ignoreTitleIfComingFromScenes.Contains(oldLevel.LevelName);
		}
		return DisplayZoneTitle && !flag;
	}
}
