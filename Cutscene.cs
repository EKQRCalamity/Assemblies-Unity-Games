using UnityEngine;

public abstract class Cutscene : AbstractPausableComponent
{
	public enum Mode
	{
		Scene,
		Level
	}

	[SerializeField]
	private GameObject translationText;

	private static Scenes sceneAfterCutscene;

	private static Levels levelAfterCutscene;

	private static Mode mode;

	protected CutsceneGUI gui;

	public static Cutscene Current { get; private set; }

	public static SceneLoader.Properties transitionProperties { get; private set; }

	static Cutscene()
	{
		transitionProperties = new SceneLoader.Properties();
	}

	public static void Load(Levels level, Scenes cutscene, SceneLoader.Transition transitionStart, SceneLoader.Transition transitionEnd, SceneLoader.Icon icon = SceneLoader.Icon.Hourglass)
	{
		transitionProperties.transitionStart = transitionStart;
		transitionProperties.transitionEnd = transitionEnd;
		transitionProperties.icon = icon;
		mode = Mode.Level;
		levelAfterCutscene = level;
		SceneLoader.LoadScene(cutscene, transitionStart, transitionEnd, SceneLoader.Icon.None);
	}

	public static void Load(Scenes scene, Scenes cutscene, SceneLoader.Transition transitionStart, SceneLoader.Transition transitionEnd, SceneLoader.Icon icon = SceneLoader.Icon.Hourglass)
	{
		transitionProperties.transitionStart = transitionStart;
		transitionProperties.transitionEnd = transitionEnd;
		transitionProperties.icon = icon;
		mode = Mode.Scene;
		sceneAfterCutscene = scene;
		SceneLoader.LoadScene(cutscene, transitionStart, transitionEnd, SceneLoader.Icon.None);
	}

	protected override void Awake()
	{
		base.Awake();
		Current = this;
		Cuphead.Init();
		CreateUI();
	}

	protected virtual void Start()
	{
		CupheadTime.SetAll(1f);
		InterruptingPrompt.SetCanInterrupt(canInterrupt: true);
		CreateCamera();
		gui.CutseneInit();
		SetRichPresence();
		if (translationText != null)
		{
			translationText.SetActive(Localization.language != Localization.Languages.English);
		}
	}

	private void CreateUI()
	{
		gui = Object.FindObjectOfType<CutsceneGUI>();
		if (gui == null)
		{
			gui = Resources.Load<CutsceneGUI>("UI/Cutscene_UI").InstantiatePrefab<CutsceneGUI>();
		}
	}

	private void CreateCamera()
	{
		CupheadCutsceneCamera cupheadCutsceneCamera = Object.FindObjectOfType<CupheadCutsceneCamera>();
		cupheadCutsceneCamera.Init();
	}

	protected virtual void OnCutsceneOver()
	{
		switch (mode)
		{
		case Mode.Scene:
			SceneLoader.LoadScene(sceneAfterCutscene, SceneLoader.Transition.Fade, transitionProperties.transitionEnd, transitionProperties.icon);
			break;
		case Mode.Level:
			SceneLoader.LoadLevel(levelAfterCutscene, SceneLoader.Transition.Fade);
			break;
		}
	}

	public void Skip()
	{
		OnCutsceneOver();
	}

	protected virtual void SetRichPresence()
	{
		OnlineManager.Instance.Interface.SetRichPresence(PlayerId.Any, "Cutscene", active: true);
	}

	public bool IsTranslationTextActive()
	{
		return translationText.activeSelf;
	}
}
