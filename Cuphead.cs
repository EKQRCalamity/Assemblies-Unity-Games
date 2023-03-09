using Rewired;
using Rewired.UI.ControlMapper;
using UnityEngine;

public class Cuphead : AbstractMonoBehaviour
{
	private const string PATH = "Core/CupheadCore";

	private static bool didLightInit;

	private static bool didFullInit;

	[SerializeField]
	private AudioNoiseHandler noiseHandler;

	[SerializeField]
	private InputManager rewired;

	public ControlMapper controlMapper;

	[SerializeField]
	private CupheadEventSystem eventSystem;

	[SerializeField]
	private CupheadRenderer renderer;

	[SerializeField]
	private ScoringEditorData scoringProperties;

	[SerializeField]
	private AchievementToastManager achievementToastManagerPrefab;

	public static Cuphead Current { get; private set; }

	public ScoringEditorData ScoringProperties => scoringProperties;

	public AchievementToastManager achievementToastManager { get; private set; }

	public static void Init(bool lightInit = false)
	{
		if (Current == null)
		{
			Object.Instantiate(Resources.Load<Cuphead>("Core/CupheadCore"));
		}
		else
		{
			if (!didLightInit)
			{
				return;
			}
			didLightInit = false;
		}
		if (lightInit)
		{
			didLightInit = true;
			return;
		}
		Current.rewired.gameObject.SetActive(value: true);
		Current.eventSystem.gameObject.SetActive(value: true);
		Current.controlMapper.gameObject.SetActive(value: true);
		PlayerManager.Awake();
		if (!PlatformHelper.PreloadSettingsData)
		{
			OnlineManager.Instance.Init();
		}
		PlmManager.Instance.Init();
		PlayerManager.Init();
		didFullInit = true;
	}

	protected override void Awake()
	{
		base.Awake();
		if (Current == null)
		{
			Current = this;
			base.gameObject.name = base.gameObject.name.Replace("(Clone)", string.Empty);
			Object.DontDestroyOnLoad(base.gameObject);
			noiseHandler = Object.Instantiate(noiseHandler);
			noiseHandler.transform.SetParent(base.transform);
			bool hasBootedUpGame = SettingsData.Data.hasBootedUpGame;
			if (PlatformHelper.ShowAchievements)
			{
				achievementToastManager = Object.Instantiate(achievementToastManagerPrefab);
				achievementToastManager.transform.SetParent(base.transform);
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (Current == this)
		{
			Current = null;
		}
	}

	private void Update()
	{
		if (didFullInit)
		{
			PlayerManager.Update();
		}
		Cursor.visible = !Screen.fullScreen;
	}
}
