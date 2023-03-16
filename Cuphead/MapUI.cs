using System.Collections;
using UnityEngine;

public class MapUI : AbstractMonoBehaviour
{
	[SerializeField]
	private MapPauseUI pauseUI;

	[SerializeField]
	private MapEquipUI equipUI;

	[SerializeField]
	private OptionsGUI optionsPrefab;

	[SerializeField]
	private RectTransform optionsRoot;

	[SerializeField]
	private AchievementsGUI achievementsPrefab;

	[SerializeField]
	private RectTransform achievementsRoot;

	[Space(10f)]
	[SerializeField]
	public Canvas sceneCanvas;

	[SerializeField]
	public Canvas screenCanvas;

	[SerializeField]
	public Canvas hudCanvas;

	[Space(10f)]
	[SerializeField]
	private CupheadUICamera uiCameraPrefab;

	private OptionsGUI optionsUI;

	private AchievementsGUI achievementsUI;

	private CupheadUICamera uiCamera;

	public static MapUI Current { get; private set; }

	public static MapUI Create()
	{
		return Object.Instantiate(Map.Current.MapResources.mapUI);
	}

	protected override void Awake()
	{
		base.Awake();
		Current = this;
		CupheadEventSystem.Init();
		LevelGUI.DebugOnDisableGuiEvent += OnDisableGUI;
	}

	private void Start()
	{
		uiCamera = Object.Instantiate(uiCameraPrefab);
		uiCamera.transform.SetParent(base.transform);
		uiCamera.transform.ResetLocalTransforms();
		screenCanvas.worldCamera = uiCamera.camera;
		sceneCanvas.worldCamera = CupheadMapCamera.Current.camera;
		hudCanvas.worldCamera = CupheadMapCamera.Current.camera;
		StartCoroutine(HandleReturnToMapTooltipEvents());
	}

	private void OnDestroy()
	{
		LevelGUI.DebugOnDisableGuiEvent -= OnDisableGUI;
		if (Current == this)
		{
			Current = null;
		}
		pauseUI = null;
		equipUI = null;
		optionsPrefab = null;
	}

	public void Init(MapPlayerController[] players)
	{
		optionsUI = optionsPrefab.InstantiatePrefab<OptionsGUI>();
		optionsUI.rectTransform.SetParent(optionsRoot, worldPositionStays: false);
		if (PlatformHelper.ShowAchievements)
		{
			achievementsUI = achievementsPrefab.InstantiatePrefab<AchievementsGUI>();
			achievementsUI.rectTransform.SetParent(achievementsRoot, worldPositionStays: false);
		}
		pauseUI.Init(checkIfDead: false, optionsUI, achievementsUI);
		equipUI.Init(checkIfDead: false);
	}

	private void OnDisableGUI()
	{
		hudCanvas.enabled = false;
	}

	public void Refresh()
	{
		optionsUI.SetupButtons();
	}

	private void Update()
	{
		if (!MapEventNotification.Current.showing && MapEventNotification.Current.EventQueue.Count > 0)
		{
			MapEventNotification.Current.EventQueue.Dequeue()();
		}
	}

	private IEnumerator HandleReturnToMapTooltipEvents()
	{
		yield return new WaitForSeconds(1f);
		if (PlayerData.Data.shouldShowBoatmanTooltip)
		{
			MapEventNotification.Current.ShowTooltipEvent(TooltipEvent.Boatman);
			PlayerData.Data.shouldShowBoatmanTooltip = false;
			PlayerData.SaveCurrentFile();
		}
		if (PlayerData.Data.shouldShowShopkeepTooltip)
		{
			MapEventNotification.Current.ShowTooltipEvent(TooltipEvent.ShopKeep);
			PlayerData.Data.shouldShowShopkeepTooltip = false;
			PlayerData.SaveCurrentFile();
		}
		if (PlayerData.Data.shouldShowTurtleTooltip)
		{
			MapEventNotification.Current.ShowTooltipEvent(TooltipEvent.Turtle);
			PlayerData.Data.shouldShowTurtleTooltip = false;
			PlayerData.SaveCurrentFile();
		}
		if (PlayerData.Data.shouldShowForkTooltip)
		{
			MapEventNotification.Current.ShowTooltipEvent(TooltipEvent.Professional);
			PlayerData.Data.shouldShowForkTooltip = false;
			PlayerData.SaveCurrentFile();
		}
	}
}
