using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;

public class SceneLoader : AbstractMonoBehaviour
{
	public abstract class Context
	{
	}

	public delegate void FadeHandler(float time);

	public enum Transition
	{
		Iris,
		Fade,
		Blur,
		None
	}

	public enum Icon
	{
		None,
		Random,
		Cuphead_Head,
		Cuphead_Running,
		Cuphead_Jumping,
		Screen_OneMoment,
		Hourglass,
		HourglassBroken
	}

	public class Properties
	{
		public const float FADE_START_DEFAULT = 0.4f;

		public const float FADE_END_DEFAULT = 0.4f;

		public Icon icon;

		public Transition transitionStart;

		public Transition transitionEnd;

		public float transitionStartTime;

		public float transitionEndTime;

		public Properties()
		{
			Reset();
		}

		public void Reset()
		{
			icon = Icon.Hourglass;
			transitionStart = Transition.Fade;
			transitionEnd = Transition.Fade;
			transitionStartTime = 0.4f;
			transitionEndTime = 0.4f;
		}
	}

	private const string SCENE_LOADER_PATH = "UI/Scene_Loader";

	private const float ICON_IN_TIME = 0.4f;

	private const float ICON_OUT_TIME = 0.6f;

	private const float ICON_WAIT_TIME = 1f;

	private const float ICON_NONE_TIME = 1f;

	private const float FADER_DELAY = 0.5f;

	private const float IRIS_TIME = 0.6f;

	private readonly string LOAD_SCENE_NAME = Scenes.scene_load_helper.ToString();

	public static float EndTransitionDelay;

	public static bool IsInIrisTransition;

	public static bool IsInBlurTransition;

	private static SceneLoader _instance;

	private static string previousSceneName;

	private static bool currentlyLoading;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Image fader;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private SceneLoaderCamera camera;

	private bool doneLoadingSceneAsync;

	private float bgmVolume;

	private float bgmLevelVolume;

	private float bgmVolumeStart;

	private float bgmLevelVolumeStart;

	private float sfxVolumeStart;

	private Coroutine bgmCoroutine;

	public static bool Exists => _instance != null;

	public static SceneLoader instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = (UnityEngine.Object.Instantiate(Resources.Load("UI/Scene_Loader")) as GameObject).GetComponent<SceneLoader>();
			}
			return _instance;
		}
	}

	public static Levels CurrentLevel { get; private set; }

	public static string SceneName { get; private set; }

	public static Properties properties { get; private set; }

	public static Context CurrentContext { get; private set; }

	public static bool CurrentlyLoading => currentlyLoading;

	public static event FadeHandler OnFadeInStartEvent;

	public static event Action OnFadeInEndEvent;

	public static event FadeHandler OnFadeOutStartEvent;

	public static event Action OnFadeOutEndEvent;

	public static event FadeHandler OnFaderValue;

	public static event Action OnLoaderCompleteEvent;

	static SceneLoader()
	{
		SceneName = string.Empty;
		CurrentLevel = Levels.Veggies;
		properties = new Properties();
	}

	public static void LoadScene(string sceneName, Transition transitionStart, Transition transitionEnd, Icon icon = Icon.Hourglass, Context context = null)
	{
		Scenes result = Scenes.scene_start;
		if (EnumUtils.TryParse<Scenes>(sceneName, out result))
		{
			LoadScene(result, transitionStart, transitionEnd, icon, context);
		}
	}

	public static void LoadScene(Scenes scene, Transition transitionStart, Transition transitionEnd, Icon icon = Icon.Hourglass, Context context = null)
	{
		if (!currentlyLoading)
		{
			InterruptingPrompt.SetCanInterrupt(canInterrupt: false);
			properties.transitionStart = transitionStart;
			properties.transitionEnd = transitionEnd;
			properties.icon = icon;
			EndTransitionDelay = 0.6f;
			previousSceneName = SceneName;
			SceneName = scene.ToString();
			CurrentContext = context;
			instance.load();
		}
	}

	public static void LoadLevel(Levels level, Transition transitionStart, Icon icon = Icon.Hourglass, Context context = null)
	{
		CurrentLevel = level;
		LoadScene(LevelProperties.GetLevelScene(level), transitionStart, Transition.Iris, icon, context);
	}

	public static void LoadDicePalaceLevel(DicePalaceLevels dicePalaceLevel)
	{
		Levels level = (CurrentLevel = LevelProperties.GetDicePalaceLevel(dicePalaceLevel));
		LoadScene(LevelProperties.GetLevelScene(level), Transition.Fade, Transition.Fade, Icon.None);
	}

	public static void SetCurrentLevel(Levels level)
	{
		CurrentLevel = level;
	}

	public static void ContinueTowerOfPower()
	{
		int cURRENT_TURN = TowerOfPowerLevelGameInfo.CURRENT_TURN;
		int tURN_COUNTER = TowerOfPowerLevelGameInfo.TURN_COUNTER;
		if (cURRENT_TURN == tURN_COUNTER)
		{
			TowerOfPowerLevelGameInfo.TURN_COUNTER++;
		}
		if (TowerOfPowerLevelGameInfo.TURN_COUNTER == TowerOfPowerLevelGameInfo.allStageSpaces.Count)
		{
			TowerOfPowerLevelGameInfo.GameInfo.CleanUp();
			LoadLastMap();
		}
		else
		{
			LoadScene(LevelProperties.GetLevelScene(Levels.TowerOfPower), Transition.Fade, Transition.Fade, Icon.None);
		}
	}

	public static void ResetTheTowerOfPower()
	{
		TowerOfPowerLevelGameInfo.ResetTowerOfPower();
		LoadScene(LevelProperties.GetLevelScene(Levels.TowerOfPower), Transition.Fade, Transition.Fade, Icon.None);
	}

	public static void ReloadLevel()
	{
		if (Level.IsTowerOfPower)
		{
			if (TowerOfPowerLevelGameInfo.IsTokenLeft(0))
			{
				TowerOfPowerLevelGameInfo.PLAYER_STATS[0].HP = 3;
				TowerOfPowerLevelGameInfo.PLAYER_STATS[0].BonusHP = 3;
				TowerOfPowerLevelGameInfo.PLAYER_STATS[0].SuperCharge = 0f;
				TowerOfPowerLevelGameInfo.ReduceToken(0);
			}
			else
			{
				TowerOfPowerLevelGameInfo.PLAYER_STATS[0].HP = 0;
				TowerOfPowerLevelGameInfo.PLAYER_STATS[0].BonusHP = 0;
				TowerOfPowerLevelGameInfo.PLAYER_STATS[0].SuperCharge = 0f;
			}
			if (PlayerManager.Multiplayer)
			{
				if (TowerOfPowerLevelGameInfo.IsTokenLeft(1))
				{
					TowerOfPowerLevelGameInfo.PLAYER_STATS[1].HP = 3;
					TowerOfPowerLevelGameInfo.PLAYER_STATS[1].BonusHP = 3;
					TowerOfPowerLevelGameInfo.PLAYER_STATS[1].SuperCharge = 0f;
					TowerOfPowerLevelGameInfo.ReduceToken(1);
				}
				else
				{
					TowerOfPowerLevelGameInfo.PLAYER_STATS[1].HP = 0;
					TowerOfPowerLevelGameInfo.PLAYER_STATS[1].BonusHP = 0;
					TowerOfPowerLevelGameInfo.PLAYER_STATS[1].SuperCharge = 0f;
				}
			}
		}
		else
		{
			if (Level.IsDicePalace)
			{
				LoadDicePalaceLevel(DicePalaceLevels.DicePalaceMain);
				return;
			}
			if (Level.IsGraveyard)
			{
				LoadScene(LevelProperties.GetLevelScene(CurrentLevel), Transition.Fade, Transition.Blur, Icon.None);
				return;
			}
			if (Level.IsChessBoss)
			{
				if (CurrentContext is GauntletContext && !((GauntletContext)CurrentContext).complete)
				{
					Scenes scene = Scenes.scene_level_chess_pawn;
					Transition transitionStart = Transition.Fade;
					Transition transitionEnd = Transition.Iris;
					GauntletContext context = new GauntletContext(complete: false);
					LoadScene(scene, transitionStart, transitionEnd, Icon.Hourglass, context);
					return;
				}
				PlayerData.Data.IncrementKingOfGamesCounter();
				PlayerData.SaveCurrentFile();
			}
		}
		float transitionStartTime = properties.transitionStartTime;
		properties.transitionStartTime = 0.25f;
		LoadLevel(CurrentLevel, Transition.Fade, Icon.None);
		properties.transitionStartTime = transitionStartTime;
	}

	public static void LoadLastMap()
	{
		if (Level.IsGraveyard)
		{
			LoadScene(PlayerData.Data.CurrentMap, Transition.Fade, Transition.Blur);
			IsInBlurTransition = true;
			return;
		}
		Scenes scene = PlayerData.Data.CurrentMap;
		if (Level.IsChessBoss)
		{
			PlayerData.Data.IncrementKingOfGamesCounter();
			PlayerData.SaveCurrentFile();
			if (PlayerData.Data.CountLevelsCompleted(Level.kingOfGamesLevels) == Level.kingOfGamesLevels.Length)
			{
				scene = Scenes.scene_level_chess_castle;
			}
		}
		LoadScene(scene, Transition.Iris, Transition.Iris);
	}

	public static void TransitionOut()
	{
		TransitionOut(properties.transitionStart);
	}

	public static void TransitionOut(Transition transition)
	{
		TransitionOut(transition, properties.transitionStartTime);
	}

	public static void TransitionOut(Transition transition, float time)
	{
		properties.transitionStart = transition;
		properties.transitionStartTime = time;
		instance.Out();
	}

	protected override void Awake()
	{
		base.Awake();
		_instance = this;
		SetIconAlpha(0f);
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void load()
	{
		if (SceneName != Scenes.scene_slot_select.ToString() && SceneName != Scenes.scene_cutscene_dlc_saltbaker_prebattle.ToString())
		{
			AudioManager.HandleSnapshot(AudioManager.Snapshots.Loadscreen.ToString(), 5f);
		}
		StartCoroutine(loop_cr());
	}

	private void In()
	{
		StartCoroutine(in_cr());
	}

	private void Out()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			if (SceneLoader.OnFadeOutEndEvent != null)
			{
				SceneLoader.OnFadeOutEndEvent();
			}
		}
		else
		{
			StartCoroutine(out_cr());
		}
	}

	private void UpdateProgress(float progress)
	{
	}

	private void SetIconAlpha(float a)
	{
		SetImageAlpha(icon, a);
	}

	private void SetFaderAlpha(float a)
	{
		SetImageAlpha(fader, a);
	}

	private void SetImageAlpha(Image i, float a)
	{
		Color color = i.color;
		color.a = a;
		i.color = color;
	}

	private IEnumerator loop_cr()
	{
		currentlyLoading = true;
		yield return StartCoroutine(in_cr());
		StartCoroutine(load_cr());
		yield return StartCoroutine(iconFadeIn_cr());
		while (!doneLoadingSceneAsync)
		{
			yield return null;
		}
		if (SceneName != Scenes.scene_slot_select.ToString())
		{
			AudioManager.SnapshotReset(SceneName, 0.15f);
		}
		AsyncOperation op = Resources.UnloadUnusedAssets();
		while (!op.isDone)
		{
			yield return null;
		}
		yield return StartCoroutine(iconFadeOut_cr());
		yield return StartCoroutine(out_cr());
		properties.Reset();
		currentlyLoading = false;
	}

	private IEnumerator load_cr()
	{
		doneLoadingSceneAsync = false;
		GC.Collect();
		if (SceneName != previousSceneName && SceneName != Scenes.scene_slot_select.ToString())
		{
			string text = null;
			if (!Array.Exists(Level.kingOfGamesLevelsWithCastle, (Levels level) => LevelProperties.GetLevelScene(level) == SceneName))
			{
				text = Scenes.scene_level_chess_castle.ToString();
			}
			AssetBundleLoader.UnloadAssetBundles();
			AssetLoader<SpriteAtlas>.UnloadAssets(text);
			if (SceneName != Scenes.scene_cutscene_dlc_saltbaker_prebattle.ToString())
			{
				AssetLoader<AudioClip>.UnloadAssets();
			}
			AssetLoader<Texture2D[]>.UnloadAssets();
		}
		if (SceneName == Scenes.scene_title.ToString())
		{
			DLCManager.RefreshDLC();
		}
		AssetLoaderOption atlasOption = AssetLoaderOption.None();
		if (SceneName == Scenes.scene_level_chess_castle.ToString())
		{
			atlasOption = AssetLoaderOption.PersistInCacheTagged(SceneName);
		}
		string[] preloadAtlases = AssetLoader<SpriteAtlas>.GetPreloadAssetNames(SceneName);
		string[] preloadMusic = AssetLoader<AudioClip>.GetPreloadAssetNames(SceneName);
		if (SceneName != previousSceneName && (preloadAtlases.Length > 0 || preloadMusic.Length > 0))
		{
			AsyncOperation intermediateSceneAsyncOp = SceneManager.LoadSceneAsync(LOAD_SCENE_NAME);
			while (!intermediateSceneAsyncOp.isDone)
			{
				yield return null;
			}
			for (int k = 0; k < preloadAtlases.Length; k++)
			{
				yield return AssetLoader<SpriteAtlas>.LoadAsset(preloadAtlases[k], atlasOption);
			}
			AssetLoaderOption musicOption = AssetLoaderOption.None();
			for (int j = 0; j < preloadMusic.Length; j++)
			{
				yield return AssetLoader<AudioClip>.LoadAsset(preloadMusic[j], musicOption);
			}
			Coroutine[] persistentAssetsCoroutines = DLCManager.LoadPersistentAssets();
			if (persistentAssetsCoroutines != null)
			{
				for (int i = 0; i < persistentAssetsCoroutines.Length; i++)
				{
					yield return persistentAssetsCoroutines[i];
				}
			}
			yield return null;
		}
		AsyncOperation async = SceneManager.LoadSceneAsync(SceneName);
		while (!async.isDone || AssetBundleLoader.loadCounter > 0)
		{
			UpdateProgress(async.progress);
			yield return null;
		}
		doneLoadingSceneAsync = true;
	}

	private IEnumerator in_cr()
	{
		switch (properties.transitionStart)
		{
		default:
			if (SceneName != Scenes.scene_slot_select.ToString() && SceneName != Scenes.scene_cutscene_dlc_saltbaker_prebattle.ToString())
			{
				FadeOutBGM(0.6f);
			}
			yield return StartCoroutine(irisIn_cr());
			break;
		case Transition.Fade:
			if (SceneName != Scenes.scene_slot_select.ToString() && SceneName != Scenes.scene_level_graveyard.ToString() && SceneName != Scenes.scene_cutscene_dlc_saltbaker_prebattle.ToString() && (CurrentLevel != Levels.Saltbaker || SceneName != Scenes.scene_win.ToString()))
			{
				FadeOutBGM(properties.transitionEndTime);
			}
			yield return StartCoroutine(faderFadeIn_cr());
			break;
		case Transition.Blur:
			yield return StartCoroutine(blurIn_cr());
			break;
		case Transition.None:
			SetFaderAlpha(1f);
			break;
		}
	}

	private IEnumerator out_cr()
	{
		yield return null;
		switch (properties.transitionEnd)
		{
		default:
			yield return StartCoroutine(irisOut_cr());
			break;
		case Transition.Fade:
			yield return StartCoroutine(faderFadeOut_cr());
			break;
		case Transition.Blur:
			yield return StartCoroutine(blurOut_cr());
			break;
		case Transition.None:
			SetFaderAlpha(0f);
			break;
		}
		if (SceneName != Scenes.scene_slot_select.ToString() && !Level.IsGraveyard && SceneName != Scenes.scene_cutscene_dlc_saltbaker_prebattle.ToString())
		{
			ResetBgmVolume();
		}
		if (SceneLoader.OnLoaderCompleteEvent != null)
		{
			SceneLoader.OnLoaderCompleteEvent();
		}
		SceneLoader.OnLoaderCompleteEvent = null;
	}

	private IEnumerator irisIn_cr()
	{
		IsInIrisTransition = true;
		Animator animator = fader.GetComponent<Animator>();
		animator.SetTrigger("Iris_In");
		SetFaderAlpha(1f);
		if (SceneLoader.OnFadeInStartEvent != null)
		{
			SceneLoader.OnFadeInStartEvent(0.6f);
		}
		yield return new WaitForSeconds(0.6f);
		if (SceneLoader.OnFadeInEndEvent != null)
		{
			SceneLoader.OnFadeInEndEvent();
		}
	}

	private IEnumerator irisOut_cr()
	{
		Animator animator = fader.GetComponent<Animator>();
		animator.SetTrigger("Iris_Out");
		SetFaderAlpha(1f);
		if (SceneLoader.OnFadeOutStartEvent != null)
		{
			SceneLoader.OnFadeOutStartEvent(0.6f);
		}
		yield return new WaitForSeconds(0.6f);
		if (SceneLoader.OnFadeOutEndEvent != null)
		{
			SceneLoader.OnFadeOutEndEvent();
		}
		IsInIrisTransition = false;
	}

	private IEnumerator faderFadeIn_cr()
	{
		IsInIrisTransition = false;
		SetFaderAlpha(0f);
		Animator animator = fader.GetComponent<Animator>();
		animator.SetTrigger("Black");
		if (SceneLoader.OnFadeInStartEvent != null)
		{
			SceneLoader.OnFadeInStartEvent(properties.transitionStartTime);
		}
		yield return StartCoroutine(imageFade_cr(fader, properties.transitionStartTime, 0f, 1f));
		if (SceneLoader.OnFadeInEndEvent != null)
		{
			SceneLoader.OnFadeInEndEvent();
		}
	}

	private IEnumerator faderFadeOut_cr()
	{
		if (SceneLoader.OnFadeOutStartEvent != null)
		{
			SceneLoader.OnFadeOutStartEvent(properties.transitionEndTime);
		}
		yield return StartCoroutine(imageFade_cr(fader, properties.transitionEndTime, 1f, 0f));
		if (SceneLoader.OnFadeOutEndEvent != null)
		{
			SceneLoader.OnFadeOutEndEvent();
		}
	}

	private IEnumerator blurIn_cr()
	{
		IsInBlurTransition = true;
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		AbstractCupheadGameCamera cam = ((!(CupheadLevelCamera.Current != null)) ? ((AbstractCupheadGameCamera)CupheadMapCamera.Current) : ((AbstractCupheadGameCamera)CupheadLevelCamera.Current));
		cam.StartBlur(0.5f, 2f);
		AudioManager.ChangeBGMPitch(0.9f, 0.5f);
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		cam.EndBlur(0.5f);
		AudioManager.ChangeBGMPitch(1f, 0.5f);
		yield return CupheadTime.WaitForSeconds(this, 1f);
		cam.StartBlur(3f, 5f);
		AudioManager.ChangeBGMPitch(0.7f, 7f);
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		properties.transitionStartTime = 3f;
		FadeOutBGM(6f);
		yield return StartCoroutine(faderFadeIn_cr());
	}

	private IEnumerator blurOut_cr()
	{
		IsInBlurTransition = true;
		AbstractCupheadGameCamera cam = ((!(CupheadLevelCamera.Current != null)) ? ((AbstractCupheadGameCamera)CupheadMapCamera.Current) : ((AbstractCupheadGameCamera)CupheadLevelCamera.Current));
		cam.StartBlur(0.01f, 5f);
		yield return new WaitForSeconds(0.015f);
		cam.EndBlur(2.5f, 5f);
		properties.transitionEndTime = 2f;
		yield return StartCoroutine(faderFadeOut_cr());
		cam.StartBlur(0.5f, 5f);
		yield return new WaitForSeconds(0.5f);
		cam.EndBlur(0.5f, 5f);
		yield return new WaitForSeconds(0.5f);
		IsInBlurTransition = false;
	}

	private IEnumerator iconFadeIn_cr()
	{
		if (properties.icon == Icon.None)
		{
			SetIconAlpha(0f);
			yield break;
		}
		Animator animator = icon.GetComponent<Animator>();
		animator.SetTrigger(properties.icon.ToString());
		yield return StartCoroutine(imageFade_cr(icon, 0.4f, 0f, 1f, interruptOnLoad: true));
	}

	private IEnumerator iconFadeOut_cr()
	{
		if (properties.icon == Icon.None)
		{
			SetIconAlpha(0f);
			yield return new WaitForSeconds(0.6f);
			yield break;
		}
		float startAlpha = icon.color.a;
		yield return StartCoroutine(imageFade_cr(icon, 0.6f * startAlpha, startAlpha, 0f));
		if (startAlpha < 1f)
		{
			yield return new WaitForSeconds(0.6f * (1f - startAlpha));
		}
	}

	private IEnumerator imageFade_cr(Image image, float time, float start, float end, bool interruptOnLoad = false)
	{
		float t = 0f;
		SetImageAlpha(image, start);
		while (t < time && (!interruptOnLoad || !doneLoadingSceneAsync))
		{
			float val = Mathf.Lerp(start, end, t / time);
			SetImageAlpha(image, val);
			t += Time.deltaTime;
			if (SceneLoader.OnFaderValue != null)
			{
				SceneLoader.OnFaderValue(t / time);
			}
			if (interruptOnLoad)
			{
				EndTransitionDelay = val * 0.6f;
			}
			yield return null;
		}
		SetImageAlpha(image, end);
		if (interruptOnLoad && !doneLoadingSceneAsync)
		{
			EndTransitionDelay = 0.6f;
		}
	}

	private IEnumerator fadeBGM_cr(float time)
	{
		if (AudioNoiseHandler.Instance != null)
		{
			AudioNoiseHandler.Instance.OpticalSound();
		}
		bgmVolumeStart = AudioManager.bgmOptionsVolume;
		bgmVolume = AudioManager.bgmOptionsVolume;
		sfxVolumeStart = AudioManager.sfxOptionsVolume;
		float t = 0f;
		while (t < time)
		{
			AudioManager.bgmOptionsVolume = Mathf.Lerp(t: t / time, a: bgmVolume, b: -80f);
			t += Time.deltaTime;
			yield return null;
		}
		AudioManager.bgmOptionsVolume = -80f;
		AudioManager.StopBGM();
	}

	private void FadeOutBGM(float time)
	{
		if (bgmCoroutine != null)
		{
			StopCoroutine(bgmCoroutine);
		}
		bgmCoroutine = StartCoroutine(fadeBGM_cr(time));
	}

	public void ResetBgmVolume()
	{
		if (bgmCoroutine != null)
		{
			StopCoroutine(bgmCoroutine);
		}
		AudioManager.bgmOptionsVolume = bgmVolumeStart;
		AudioManager.sfxOptionsVolume = sfxVolumeStart;
	}
}
