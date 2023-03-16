using System.Collections;
using UnityEngine;

public class StartScreen : AbstractMonoBehaviour
{
	public class InitialLoadData
	{
		public bool forceOriginalTitleScreen;
	}

	public enum State
	{
		Animating,
		MDHR_Splash,
		Title
	}

	public static InitialLoadData initialLoadData;

	public AudioClip[] SelectSound;

	[SerializeField]
	private Animator mdhrSplash;

	[SerializeField]
	private SpriteRenderer fader;

	[SerializeField]
	private GameObject titleAnimation;

	[SerializeField]
	private GameObject titleAnimationDLC;

	private CupheadInput.AnyPlayerInput input;

	private bool shouldLoadSlotSelect;

	private const string PATH = "Audio/TitleScreenAudio";

	public State state { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		UnityEngine.Debug.Log("Build version " + Application.version);
		Cuphead.Init();
		CupheadTime.Reset();
		PauseManager.Reset();
		shouldLoadSlotSelect = false;
		PlayerData.inGame = false;
		PlayerManager.ResetPlayers();
	}

	private void Start()
	{
		if (PlatformHelper.PreloadSettingsData)
		{
			SettingsData.ApplySettingsOnStartup();
		}
		if (AudioNoiseHandler.Instance != null)
		{
			AudioNoiseHandler.Instance.OpticalSound();
		}
		if (StartScreenAudio.Instance == null)
		{
			StartScreenAudio startScreenAudio = Object.Instantiate(Resources.Load("Audio/TitleScreenAudio")) as StartScreenAudio;
			startScreenAudio.name = "StartScreenAudio";
		}
		SettingsData.ApplySettingsOnStartup();
		FrameDelayedCallback(StartFrontendSnapshot, 1);
		StartCoroutine(loop_cr());
	}

	private void Update()
	{
		switch (state)
		{
		case State.MDHR_Splash:
			UpdateSplashMDHR();
			break;
		case State.Title:
			UpdateTitleScreen();
			break;
		}
	}

	private void UpdateSplashMDHR()
	{
	}

	private void UpdateTitleScreen()
	{
		if (shouldLoadSlotSelect)
		{
			AudioManager.Play("ui_playerconfirm");
			AudioManager.Play("level_select");
			SceneLoader.LoadScene(Scenes.scene_slot_select, SceneLoader.Transition.Iris, SceneLoader.Transition.Fade, SceneLoader.Icon.None);
			base.enabled = false;
		}
	}

	private void onPlayerJoined(PlayerId playerId)
	{
		shouldLoadSlotSelect = true;
	}

	private IEnumerator loop_cr()
	{
		yield return new WaitForSeconds(1f);
		AudioManager.Play("mdhr_logo_sting");
		yield return StartCoroutine(tweenRenderer_cr(fader, 1f));
		mdhrSplash.Play("Logo");
		yield return mdhrSplash.WaitForAnimationToEnd(this, "Logo");
		AudioManager.SnapshotReset(Scenes.scene_title.ToString(), 0.3f);
		if (!CreditsScreen.goodEnding)
		{
			AudioManager.PlayBGM();
		}
		else if (DLCManager.DLCEnabled() && !forceOriginalTitleScreen())
		{
			AudioManager.StartBGMAlternate(0);
			titleAnimation.SetActive(value: false);
			titleAnimationDLC.SetActive(value: true);
		}
		else
		{
			AudioManager.PlayBGMPlaylistManually(goThroughPlaylistAfter: true);
		}
		initialLoadData = null;
		CreditsScreen.goodEnding = true;
		SettingsData.Data.hasBootedUpGame = true;
		yield return StartCoroutine(tweenRenderer_cr(mdhrSplash.GetComponent<SpriteRenderer>(), 0.4f));
		state = State.Title;
		PlayerManager.OnPlayerJoinedEvent += onPlayerJoined;
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerOne, canJoin: true, promptBeforeJoin: false);
	}

	private void OnDestroy()
	{
		PlayerManager.OnPlayerJoinedEvent -= onPlayerJoined;
	}

	private IEnumerator tweenRenderer_cr(SpriteRenderer renderer, float time)
	{
		float t = 0f;
		Color c = renderer.color;
		c.a = 1f;
		yield return null;
		while (t < time)
		{
			c.a = 1f - t / time;
			renderer.color = c;
			t += Time.deltaTime;
			yield return null;
		}
		c.a = 0f;
		renderer.color = c;
		yield return null;
	}

	private bool forceOriginalTitleScreen()
	{
		if (initialLoadData != null)
		{
			return initialLoadData.forceOriginalTitleScreen;
		}
		return SettingsData.Data.forceOriginalTitleScreen;
	}

	protected virtual void StartFrontendSnapshot()
	{
		AudioManager.HandleSnapshot(AudioManager.Snapshots.FrontEnd.ToString(), 0.15f);
	}
}
