using System.Collections;
using UnityEngine;

public class HouseLevel : Level
{
	private LevelProperties.House properties;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	[SerializeField]
	private PlayerDeathEffect[] playerTutorialEffects;

	[SerializeField]
	private HouseElderKettle elderDialoguePoint;

	[SerializeField]
	private GameObject tutorialGameObject;

	[SerializeField]
	private int dialoguerVariableID;

	public override Levels CurrentLevel => Levels.House;

	public override Scenes CurrentScene => Scenes.scene_level_house_elder_kettle;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.House.GetMode(base.mode);
		properties.OnStateChange += base.zHack_OnStateChanged;
		properties.OnBossDeath += base.zHack_OnWin;
		base.timeline = properties.CreateTimeline(base.mode);
		goalTimes = properties.goalTimes;
		properties.OnBossDamaged += base.timeline.DealDamage;
		base.PartialInit();
	}

	protected override void Start()
	{
		base.Start();
		if (PlayerData.Data.CheckLevelsHaveMinDifficulty(new Levels[1] { Levels.Devil }, Mode.Hard))
		{
			Dialoguer.SetGlobalFloat(dialoguerVariableID, 8f);
		}
		else if (PlayerData.Data.CountLevelsHaveMinDifficulty(Level.world1BossLevels, Mode.Hard) + PlayerData.Data.CountLevelsHaveMinDifficulty(Level.world2BossLevels, Mode.Hard) + PlayerData.Data.CountLevelsHaveMinDifficulty(Level.world3BossLevels, Mode.Hard) + PlayerData.Data.CountLevelsHaveMinDifficulty(Level.world4BossLevels, Mode.Hard) > 0)
		{
			Dialoguer.SetGlobalFloat(dialoguerVariableID, 7f);
		}
		else if (PlayerData.Data.IsHardModeAvailable)
		{
			Dialoguer.SetGlobalFloat(dialoguerVariableID, 6f);
		}
		else if (PlayerData.Data.CheckLevelsCompleted(Level.world2BossLevels))
		{
			Dialoguer.SetGlobalFloat(dialoguerVariableID, 5f);
		}
		else if (PlayerData.Data.CheckLevelsCompleted(Level.world1BossLevels))
		{
			Dialoguer.SetGlobalFloat(dialoguerVariableID, 4f);
		}
		else if (PlayerData.Data.CountLevelsCompleted(Level.world1BossLevels) > 1)
		{
			Dialoguer.SetGlobalFloat(dialoguerVariableID, 3f);
		}
		else if (PlayerData.Data.IsTutorialCompleted)
		{
			Dialoguer.SetGlobalFloat(dialoguerVariableID, 2f);
		}
		else if (Dialoguer.GetGlobalFloat(dialoguerVariableID) == 0f)
		{
			tutorialGameObject.SetActive(value: false);
			base.Ending = true;
		}
		SceneLoader.OnLoaderCompleteEvent += SelectMusic;
		AddDialoguerEvents();
	}

	private void SelectMusic()
	{
		if (PlayerData.Data.pianoAudioEnabled)
		{
			AudioManager.PlayBGMPlaylistManually(goThroughPlaylistAfter: false);
		}
		else
		{
			AudioManager.PlayBGM();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		SceneLoader.OnLoaderCompleteEvent -= SelectMusic;
		RemoveDialoguerEvents();
		playerTutorialEffects = null;
	}

	public void AddDialoguerEvents()
	{
		Dialoguer.events.onMessageEvent += OnDialoguerMessageEvent;
		Dialoguer.events.onEnded += OnDialogueEndedHandler;
		Dialoguer.events.onInstantlyEnded += OnDialogueEndedHandler;
	}

	private void OnDialogueEndedHandler()
	{
		base.Ending = false;
	}

	public void RemoveDialoguerEvents()
	{
		Dialoguer.events.onMessageEvent -= OnDialoguerMessageEvent;
	}

	public void StartTutorial()
	{
		AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		playerTutorialEffects[0].gameObject.SetActive(value: true);
		playerTutorialEffects[0].transform.position = player.transform.position;
		player.gameObject.SetActive(value: false);
		playerTutorialEffects[0].animator.SetTrigger("OnStartTutorial");
		player = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		if (player != null)
		{
			playerTutorialEffects[1].gameObject.SetActive(value: true);
			playerTutorialEffects[1].transform.position = player.transform.position;
			player.gameObject.SetActive(value: false);
			playerTutorialEffects[1].animator.SetTrigger("OnStartTutorial");
		}
	}

	private void OnDialoguerMessageEvent(string message, string metadata)
	{
		if (message == "ElderKettleFirstWeapon")
		{
			tutorialGameObject.SetActive(value: true);
			StartCoroutine(power_up_cr());
		}
		if (message == "EndJoy")
		{
		}
		if (!(message == "Sleep"))
		{
		}
	}

	private IEnumerator power_up_cr()
	{
		yield return new WaitForSeconds(0.15f);
		AudioManager.Play("sfx_potion_poof");
		AbstractPlayerController[] array = players;
		foreach (AbstractPlayerController abstractPlayerController in array)
		{
			if (!(abstractPlayerController == null))
			{
				abstractPlayerController.animator.Play("Power_Up");
			}
		}
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(housePattern_cr());
	}

	private IEnumerator housePattern_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		if (Dialoguer.GetGlobalFloat(dialoguerVariableID) == 0f)
		{
			elderDialoguePoint.BeginDialogue();
		}
	}
}
