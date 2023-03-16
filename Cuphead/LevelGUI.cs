using System;
using UnityEngine;

public class LevelGUI : AbstractMonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private LevelPauseGUI pause;

	[SerializeField]
	private LevelGameOverGUI gameOver;

	[SerializeField]
	private OptionsGUI optionsPrefab;

	[SerializeField]
	private RectTransform optionsRoot;

	[SerializeField]
	private RestartTowerConfirmGUI restartTowerConfirmPrefab;

	[SerializeField]
	private RectTransform restartTowerConfirmRoot;

	[SerializeField]
	private AchievementsGUI achievementsPrefab;

	[SerializeField]
	private RectTransform achievementsRoot;

	private OptionsGUI options;

	private AchievementsGUI achievements;

	private RestartTowerConfirmGUI restartTowerConfirm;

	[Space(10f)]
	[SerializeField]
	private CupheadUICamera uiCameraPrefab;

	private CupheadUICamera uiCamera;

	public static LevelGUI Current { get; private set; }

	public Canvas Canvas => canvas;

	public static event Action DebugOnDisableGuiEvent;

	public static void DebugDisableGUI()
	{
		if (LevelGUI.DebugOnDisableGuiEvent != null)
		{
			LevelGUI.DebugOnDisableGuiEvent();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Current = this;
	}

	private void Start()
	{
		uiCamera = UnityEngine.Object.Instantiate(uiCameraPrefab);
		uiCamera.transform.SetParent(base.transform);
		uiCamera.transform.ResetLocalTransforms();
		canvas.worldCamera = uiCamera.camera;
	}

	private void OnDestroy()
	{
		pause = null;
		options = null;
		restartTowerConfirm = null;
		if (Current == this)
		{
			Current = null;
		}
	}

	public void LevelInit()
	{
		options = optionsPrefab.InstantiatePrefab<OptionsGUI>();
		options.rectTransform.SetParent(optionsRoot, worldPositionStays: false);
		if (PlatformHelper.ShowAchievements)
		{
			achievements = achievementsPrefab.InstantiatePrefab<AchievementsGUI>();
			achievements.rectTransform.SetParent(achievementsRoot, worldPositionStays: false);
		}
		if (Level.IsTowerOfPower)
		{
			restartTowerConfirm = restartTowerConfirmPrefab.InstantiatePrefab<RestartTowerConfirmGUI>();
			restartTowerConfirm.rectTransform.SetParent(restartTowerConfirmRoot, worldPositionStays: false);
		}
		pause.Init(checkIfDead: true, options, achievements, restartTowerConfirm);
	}
}
