using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessCastleLevel : Level
{
	private LevelProperties.ChessCastle properties;

	private static readonly int MaxAttemptsToContinue = 5;

	private static readonly int KingOfGamesDialoguerStateIndex = 36;

	private static readonly int KingOfGamesVictoryDialoguerCountIndex = 37;

	private static readonly int KingOfGamesVictoryDialoguerStateIndex = 42;

	private static readonly int KingOfGamesFinalDialogueState = 6;

	private static readonly int CastleBaseLayer = 0;

	private static readonly int CastleDoorLayer = 1;

	private static readonly int CastleFlairLayer = 2;

	private static readonly string[] LevelPrefixes = new string[5] { "Pawn", "Knight", "Bishop", "Rook", "Queen" };

	private static readonly int PlatformBaseLayer = 0;

	public static readonly Dictionary<Levels, string[]> Coins = new Dictionary<Levels, string[]>
	{
		{
			Levels.ChessPawn,
			new string[2] { "a37b3d37-a32e-4b88-a583-34489496494d", "25f15554-d229-4330-96cc-ac8a13c18ea0" }
		},
		{
			Levels.ChessKnight,
			new string[2] { "eacf4228-e200-4839-9d79-3439cfcc5824", "47f7edb1-b5c5-4afb-9acb-a46f5e6df557" }
		},
		{
			Levels.ChessBishop,
			new string[2] { "3826615a-498b-4158-af7b-0d01acbc18c8", "d52b1cc6-414c-4a7c-9f8a-250316566d58" }
		},
		{
			Levels.ChessRook,
			new string[2] { "fc2c48cd-5dec-472a-ae18-dccfc94232c6", "16732bc8-7230-467a-a9ac-ff9c62ab7657" }
		},
		{
			Levels.ChessQueen,
			new string[3] { "e0c6e8bc-0c56-4e52-a9a1-c53887f5ca4c", "19090606-09e8-4e56-92ac-e08200926b94", "39bfe6d8-0dbc-4886-9998-52c67b57969e" }
		}
	};

	private static readonly string[] DoorSounds = new string[5] { "sfx_dlc_kog_castle_door_wooddoor", "sfx_dlc_kog_castle_door_drawbridge", "sfx_dlc_kog_castle_door_tall", "sfx_dlc_kog_castle_door_portcullis", "sfx_dlc_kog_castle_door_queen" };

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	[SerializeField]
	private AbstractLevelInteractiveEntity startEntity;

	[SerializeField]
	private AbstractLevelInteractiveEntity exitEntity;

	[SerializeField]
	private PlayerDeathEffect[] playerStartLevelEffects;

	[SerializeField]
	private ChessCastleLevelKingInteractionPoint dialogueInteractionPoint;

	[SerializeField]
	private SpeechBubble speechBubble;

	[SerializeField]
	private Animator castleAnimator;

	[SerializeField]
	private Animator platformAnimator;

	[SerializeField]
	private GameObject cloudPrefab;

	[SerializeField]
	private GameObject coinPrefab;

	[SerializeField]
	private Transform coinSparkSpawnPoint;

	[SerializeField]
	private Animator kingAnimator;

	[SerializeField]
	private Effect sparkleEffect;

	[SerializeField]
	private Transform sparklesCenter;

	[SerializeField]
	private float sinePeriod;

	[SerializeField]
	private float sineAmplitude;

	[SerializeField]
	private float _rotationMultiplier;

	[SerializeField]
	private float introPanAmount;

	[SerializeField]
	private float introPanDuration;

	private bool firstEntry;

	private Levels previousLevel;

	private bool previouslyWon;

	private int attemptsToBeat;

	private Levels currentTargetLevel;

	private bool currentIsGauntlet;

	private Coroutine rotationCoroutine;

	private Coroutine gauntletSparklesCoroutine;

	private Vector2 speechBubbleBasePosition;

	private float cameraSineAccumulator;

	private Coroutine introCameraMovementCoroutine;

	private bool beginIntroPan;

	public override Levels CurrentLevel => Levels.ChessCastle;

	public override Scenes CurrentScene => Scenes.scene_level_chess_castle;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	public bool rotating { get; private set; }

	public float rotationMultiplier => _rotationMultiplier;

	protected override void PartialInit()
	{
		properties = LevelProperties.ChessCastle.GetMode(base.mode);
		properties.OnStateChange += base.zHack_OnStateChanged;
		properties.OnBossDeath += base.zHack_OnWin;
		base.timeline = properties.CreateTimeline(base.mode);
		goalTimes = properties.goalTimes;
		properties.OnBossDamaged += base.timeline.DealDamage;
		base.PartialInit();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SceneLoader.OnFadeOutStartEvent += onFadeOutStartEventHandler;
		base.OnIntroEvent += onIntroEventHandler;
		Dialoguer.events.onStarted += onDialogueStartedHandler;
		Dialoguer.events.onMessageEvent += onDialogueMessageHandler;
		Dialoguer.events.onEnded += onDialogueEndedHandler;
		Dialoguer.events.onInstantlyEnded += onDialogueEndedHandler;
		Dialoguer.events.onTextPhase += onDialogueAdvancedHandler;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		SceneLoader.OnFadeOutStartEvent -= onFadeOutStartEventHandler;
		base.OnIntroEvent -= onIntroEventHandler;
		Dialoguer.events.onStarted -= onDialogueStartedHandler;
		Dialoguer.events.onMessageEvent -= onDialogueMessageHandler;
		Dialoguer.events.onEnded -= onDialogueEndedHandler;
		Dialoguer.events.onInstantlyEnded -= onDialogueEndedHandler;
		Dialoguer.events.onTextPhase -= onDialogueAdvancedHandler;
	}

	protected override void Awake()
	{
		previousLevel = Level.PreviousLevel;
		previouslyWon = Level.Won;
		if (previouslyWon)
		{
			attemptsToBeat = PlayerData.Data.chessBossAttemptCounter;
			PlayerData.Data.ResetKingOfGamesCounter();
			PlayerData.SaveCurrentFile();
		}
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		startEntity = (exitEntity = null);
		playerStartLevelEffects = null;
		dialogueInteractionPoint = null;
		speechBubble = null;
		coinPrefab = null;
		kingAnimator = null;
		castleAnimator = null;
		platformAnimator = null;
		cloudPrefab = null;
	}

	protected override void Start()
	{
		base.Start();
		speechBubbleBasePosition = speechBubble.basePosition;
		updateDialogueState();
		bool flag = PlayerData.Data.CountLevelsCompleted(Level.kingOfGamesLevels) == Level.kingOfGamesLevels.Length;
		StartCoroutine(cloudSpawn_cr());
		AudioManager.PlayLoop("sfx_dlc_kog_castle_amb_wind_loop");
		AudioManager.FadeSFXVolumeLinear("sfx_dlc_kog_castle_amb_wind_loop", 0.4f, 1f);
		Levels levels;
		if (previouslyWon && SceneLoader.CurrentContext is GauntletContext && ((GauntletContext)SceneLoader.CurrentContext).complete)
		{
			levels = Levels.ChessQueen;
			movePlayersToDialoguePositions();
			dialogueInteractionPoint.dialogueInteraction = DialoguerDialogues.KingOfGamesVictory_WDLC;
			Dialoguer.SetGlobalFloat(KingOfGamesVictoryDialoguerStateIndex, -3f);
		}
		else if (previouslyWon && (!flag || Dialoguer.GetGlobalFloat(KingOfGamesDialoguerStateIndex) != (float)KingOfGamesFinalDialogueState))
		{
			movePlayersToDialoguePositions();
			dialogueInteractionPoint.dialogueInteraction = DialoguerDialogues.KingOfGamesVictory_WDLC;
			levels = calculatePreviousLevel();
			if (flag)
			{
				levels = Levels.ChessQueen;
				Dialoguer.SetGlobalFloat(KingOfGamesVictoryDialoguerStateIndex, -2f);
			}
			else if (attemptsToBeat < MaxAttemptsToContinue)
			{
				int num = (int)Dialoguer.GetGlobalFloat(KingOfGamesVictoryDialoguerCountIndex);
				num++;
				Dialoguer.SetGlobalFloat(KingOfGamesVictoryDialoguerCountIndex, num);
				Dialoguer.SetGlobalFloat(KingOfGamesVictoryDialoguerStateIndex, 0f);
			}
			else
			{
				Dialoguer.SetGlobalFloat(KingOfGamesVictoryDialoguerStateIndex, -1f);
			}
		}
		else
		{
			Dialoguer.SetGlobalFloat(KingOfGamesVictoryDialoguerStateIndex, -2f);
			if (flag)
			{
				if (Array.Exists(Level.kingOfGamesLevels, (Levels level) => level == previousLevel))
				{
					levels = previousLevel;
					movePlayersToDialoguePositions();
				}
				else
				{
					levels = Levels.ChessPawn;
				}
			}
			else
			{
				levels = calculateCurrentLevel();
			}
		}
		if (Dialoguer.GetGlobalFloat(KingOfGamesDialoguerStateIndex) == 0f)
		{
			firstEntry = true;
		}
		if (!previouslyWon)
		{
			introCameraMovementCoroutine = StartCoroutine(panCamera_cr());
		}
		if (firstEntry || previouslyWon)
		{
			string text = LevelPrefixes[Array.IndexOf(Level.kingOfGamesLevels, levels)];
			castleAnimator.Play(text + "Idle", CastleBaseLayer);
			castleAnimator.Play(text + "Open", CastleDoorLayer, 1f);
			if (levels == Levels.ChessBishop || levels == Levels.ChessQueen)
			{
				castleAnimator.Play(text, CastleFlairLayer, 1f);
			}
			platformAnimator.Play("Stop", PlatformBaseLayer, 1f);
			setTargetLevel(levels, gauntlet: false);
		}
		else
		{
			rotate(Levels.ChessPawn, levels, gauntlet: false, intro: true);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (introCameraMovementCoroutine == null)
		{
			cameraSineAccumulator += CupheadTime.Delta;
			Vector3 manualFloat = new Vector3(0f, sineAmplitude * Mathf.Sin(cameraSineAccumulator / sinePeriod * 2f * (float)Math.PI + (float)Math.PI / 2f));
			CupheadLevelCamera.Current.SetManualFloat(manualFloat);
		}
	}

	protected override void OnLevelStart()
	{
		if (firstEntry)
		{
			StartCoroutine(firstEntry_cr());
		}
		else if (dialogueInteractionPoint.dialogueInteraction == DialoguerDialogues.KingOfGamesVictory_WDLC)
		{
			StartCoroutine(postWinEntry_cr());
		}
	}

	protected override void OnTransitionInComplete()
	{
		bool flag = PlayerData.Data.CountLevelsCompleted(Level.kingOfGamesLevels) == Level.kingOfGamesLevels.Length;
		base.OnTransitionInComplete();
		if (flag)
		{
			AudioManager.StartBGMAlternate(1);
		}
		else if (Dialoguer.GetGlobalFloat(KingOfGamesDialoguerStateIndex) == 0f)
		{
			AudioManager.PlayBGM();
		}
		else
		{
			AudioManager.StartBGMAlternate(0);
		}
	}

	private IEnumerator panCamera_cr()
	{
		CupheadLevelCamera.Current.SetManualFloat(new Vector3(0f, 0f - introPanAmount));
		while (!beginIntroPan)
		{
			yield return null;
		}
		float elapsedTime = 0f;
		while (elapsedTime < introPanDuration)
		{
			yield return null;
			elapsedTime += (float)CupheadTime.Delta;
			CupheadLevelCamera.Current.SetManualFloat(new Vector3(0f, EaseUtils.EaseOutCubic(0f - introPanAmount, sineAmplitude, elapsedTime / introPanDuration)));
		}
		introCameraMovementCoroutine = null;
	}

	private IEnumerator firstEntry_cr()
	{
		castleAnimator.Play(LevelPrefixes[0] + "Close", CastleDoorLayer, 1f);
		startEntity.enabled = false;
		AudioSource castleIntroMusic = GameObject.Find("MUS_CastleIntro").GetComponent<AudioSource>();
		while (castleIntroMusic.isPlaying)
		{
			yield return null;
		}
		AudioManager.StartBGMAlternate(0);
	}

	private IEnumerator postWinEntry_cr()
	{
		AudioManager.StopBGM();
		AbstractLevelInteractiveEntity abstractLevelInteractiveEntity = startEntity;
		bool flag = false;
		dialogueInteractionPoint.enabled = flag;
		flag = flag;
		exitEntity.enabled = flag;
		abstractLevelInteractiveEntity.enabled = flag;
		yield return CupheadTime.WaitForSeconds(this, 0.35f);
		if (Dialoguer.GetGlobalFloat(KingOfGamesDialoguerStateIndex) != 0f)
		{
			AudioManager.StartBGMAlternate(0);
		}
		dialogueInteractionPoint.BeginDialogue();
		yield return CupheadTime.WaitForSeconds(this, 2f);
		AbstractLevelInteractiveEntity abstractLevelInteractiveEntity2 = startEntity;
		flag = true;
		dialogueInteractionPoint.enabled = flag;
		flag = flag;
		exitEntity.enabled = flag;
		abstractLevelInteractiveEntity2.enabled = flag;
	}

	private void rotate(Levels startLevel, Levels endLevel, bool gauntlet, bool intro)
	{
		if (rotationCoroutine == null)
		{
			rotationCoroutine = StartCoroutine(rotate_cr(startLevel, endLevel, gauntlet, intro));
		}
	}

	private IEnumerator rotate_cr(Levels startLevel, Levels endLevel, bool gauntlet, bool intro)
	{
		if (gauntletSparklesCoroutine != null)
		{
			StopCoroutine(gauntletSparklesCoroutine);
			gauntletSparklesCoroutine = null;
		}
		int startIndex = Array.IndexOf(Level.kingOfGamesLevels, startLevel);
		int destinationIndex = Array.IndexOf(Level.kingOfGamesLevels, endLevel);
		string startPrefix = LevelPrefixes[startIndex];
		string endPrefix = LevelPrefixes[destinationIndex];
		AbstractLevelInteractiveEntity abstractLevelInteractiveEntity = startEntity;
		bool flag = false;
		dialogueInteractionPoint.enabled = flag;
		flag = flag;
		exitEntity.enabled = flag;
		abstractLevelInteractiveEntity.enabled = flag;
		if (intro)
		{
			float normalizedTime = (float)destinationIndex / (float)Level.kingOfGamesLevels.Length - 0.25f;
			castleAnimator.Play("FullRotation", CastleBaseLayer, normalizedTime);
			castleAnimator.Play("Off", CastleDoorLayer);
			kingAnimator.Play("LeverPull", 0, 0.2f);
		}
		else
		{
			castleAnimator.Play(startPrefix + "Close", CastleDoorLayer);
			AudioManager.Play(DoorSounds[startIndex] + "_close");
			kingAnimator.SetTrigger("PullLever");
			kingAnimator.SetBool("Talking", value: false);
			yield return kingAnimator.WaitForNormalizedTime(this, 0.6101695f, "LeverPull");
			yield return castleAnimator.WaitForNormalizedTimeLooping(this, 11f / 12f, startPrefix + "Idle", CastleBaseLayer, allowEqualTime: true);
			castleAnimator.Play(startPrefix + "Start");
		}
		castleAnimator.SetInteger("Destination", destinationIndex);
		castleAnimator.SetBool("Gauntlet", gauntlet);
		castleAnimator.Play("Off", CastleFlairLayer);
		platformAnimator.Play("Start", PlatformBaseLayer);
		rotating = true;
		CupheadLevelCamera.Current.StartShake(2f);
		if (destinationIndex - startIndex != 1)
		{
			AudioManager.PlayLoop("sfx_dlc_kog_castle_kog_rotate_loop");
			AudioManager.FadeSFXVolumeLinear("sfx_dlc_kog_castle_kog_rotate_loop", 0f, 0.2f, 0.2f);
		}
		else
		{
			AudioManager.Play("sfx_dlc_kog_castle_kog_rotate");
		}
		yield return castleAnimator.WaitForAnimationToStart(this, endPrefix + "Stop", CastleBaseLayer);
		AudioManager.FadeSFXVolumeLinear("sfx_dlc_kog_castle_kog_rotate_loop", 0f, 0.2f);
		AudioManager.Play("sfx_dlc_kog_castle_kog_roateend");
		yield return castleAnimator.WaitForNormalizedTime(this, 1f, endPrefix + "Stop", CastleBaseLayer, allowEqualTime: true);
		rotating = false;
		CupheadLevelCamera.Current.EndShake(0.2f);
		castleAnimator.Play(endPrefix + "Idle", CastleBaseLayer);
		castleAnimator.Play(endPrefix + "Open", CastleDoorLayer);
		AudioManager.Play(DoorSounds[destinationIndex] + "_open");
		if (endLevel == Levels.ChessPawn || endLevel == Levels.ChessBishop || endLevel == Levels.ChessRook || endLevel == Levels.ChessQueen)
		{
			castleAnimator.Play(endPrefix, CastleFlairLayer);
		}
		platformAnimator.Play("Stop", PlatformBaseLayer);
		setTargetLevel(endLevel, gauntlet);
		AbstractLevelInteractiveEntity abstractLevelInteractiveEntity2 = startEntity;
		flag = true;
		dialogueInteractionPoint.enabled = flag;
		flag = flag;
		exitEntity.enabled = flag;
		abstractLevelInteractiveEntity2.enabled = flag;
		rotationCoroutine = null;
		if (gauntlet)
		{
			gauntletSparklesCoroutine = StartCoroutine(gauntletSparkles_cr());
		}
	}

	public void StartChessLevel()
	{
		StartCoroutine(startChessLevel_cr());
	}

	private IEnumerator startChessLevel_cr()
	{
		AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		playerStartLevelEffects[0].gameObject.SetActive(value: true);
		playerStartLevelEffects[0].transform.position = player.transform.position;
		player.gameObject.SetActive(value: false);
		playerStartLevelEffects[0].animator.SetTrigger("OnStartTutorial");
		player = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		if (player != null)
		{
			playerStartLevelEffects[1].gameObject.SetActive(value: true);
			playerStartLevelEffects[1].transform.position = player.transform.position;
			player.gameObject.SetActive(value: false);
			playerStartLevelEffects[1].animator.SetTrigger("OnStartTutorial");
		}
		AudioManager.Play("sfx_dlc_kog_castle_kog_entercastle");
		yield return CupheadTime.WaitForSeconds(this, 0.2f);
		GauntletContext context = ((!currentIsGauntlet) ? null : new GauntletContext(complete: false));
		Levels level = currentTargetLevel;
		SceneLoader.Transition transitionStart = SceneLoader.Transition.Iris;
		SceneLoader.LoadLevel(level, transitionStart, SceneLoader.Icon.Hourglass, context);
	}

	public void Exit()
	{
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (!(allPlayer == null))
			{
				allPlayer.DisableInput();
			}
		}
		SceneLoader.LoadScene(Scenes.scene_map_world_DLC, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
	}

	private IEnumerator cloudSpawn_cr()
	{
		MinMax cloudSpawnTime = new MinMax(0.8f, 1.4f);
		while (true)
		{
			float elapsedTime = 0f;
			for (float duration = cloudSpawnTime.RandomFloat(); elapsedTime < ((!rotating) ? duration : (duration / rotationMultiplier)); elapsedTime += (float)CupheadTime.Delta)
			{
				yield return null;
			}
			GameObject obj = UnityEngine.Object.Instantiate(cloudPrefab);
			obj.GetComponent<ChessCastleLevelCloud>().Initialize(this);
		}
	}

	private void AnimateCoins(int count)
	{
		StartCoroutine(coin_cr(count));
	}

	private IEnumerator coin_cr(int count)
	{
		for (int i = 0; i < count; i++)
		{
			string animationName = ((i % 2 != 0) ? "CoinB" : "CoinA");
			kingAnimator.Play(animationName, 1);
			yield return kingAnimator.WaitForAnimationToEnd(this, animationName, 1);
			AudioManager.Play("sfx_coin_pickup");
			GameObject coinSpark = UnityEngine.Object.Instantiate(coinPrefab, coinSparkSpawnPoint.position, Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)));
			Renderer coinSparkRenderer = coinSpark.GetComponent<Renderer>();
			coinSparkRenderer.sortingLayerName = "Effects";
			coinSparkRenderer.sortingOrder = 50;
			coinSpark.GetComponent<Animator>().Play("anim_level_coin_death");
		}
	}

	private IEnumerator gauntletSparkles_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.2f);
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, 0.35f);
			Vector3 position = sparklesCenter.position + (Vector3)(UnityEngine.Random.insideUnitCircle * 70f);
			Effect effect = sparkleEffect.Create(position);
			effect.GetComponent<AnimationHelper>().Speed = 1f / 3f;
			effect.GetComponent<SpriteRenderer>().sortingLayerName = "Enemies";
		}
	}

	private void onDialogueStartedHandler()
	{
		base.Ending = true;
		AudioManager.Play("sfx_dlc_kog_castle_kingvoice");
	}

	private void onDialogueMessageHandler(string message, string metadata)
	{
		switch (message)
		{
		case "GiveCoins":
		{
			if (Coins.TryGetValue(currentTargetLevel, out var value))
			{
				AnimateCoins(value.Length);
			}
			break;
		}
		case "SetupChooseLevel":
			setupChooseLevel();
			break;
		case "ChooseLevel":
		{
			revertChooseLevel();
			if (!(metadata == "-1") && Parser.IntTryParse(metadata, out var result))
			{
				if (result == 5)
				{
					rotate(currentTargetLevel, Levels.ChessPawn, gauntlet: true, intro: false);
					break;
				}
				Levels endLevel = Level.kingOfGamesLevels[result];
				rotate(currentTargetLevel, endLevel, gauntlet: false, intro: false);
			}
			break;
		}
		}
	}

	private void onDialogueEndedHandler()
	{
		base.Ending = false;
		stopTalk();
		if (SceneLoader.CurrentContext is GauntletContext && ((GauntletContext)SceneLoader.CurrentContext).complete)
		{
			OnlineManager.Instance.Interface.UnlockAchievement(PlayerId.Any, OnlineAchievementData.DLC.DefeatKOGGauntlet);
			dialogueInteractionPoint.dialogueInteraction = DialoguerDialogues.KingOfGames_WDLC;
		}
		else if (dialogueInteractionPoint.dialogueInteraction == DialoguerDialogues.KingOfGamesVictory_WDLC)
		{
			PlayerData.SaveCurrentFile();
			dialogueInteractionPoint.dialogueInteraction = DialoguerDialogues.KingOfGames_WDLC;
			if (attemptsToBeat < MaxAttemptsToContinue && Dialoguer.GetGlobalFloat(KingOfGamesDialoguerStateIndex) != (float)KingOfGamesFinalDialogueState)
			{
				rotate(currentTargetLevel, calculateCurrentLevel(), gauntlet: false, intro: false);
			}
			else
			{
				Exit();
			}
		}
		else if (firstEntry)
		{
			PlayerData.SaveCurrentFile();
			if (!startEntity.enabled)
			{
				AudioManager.Play("sfx_dlc_kog_castle_door_wooddoor_open");
			}
			castleAnimator.Play(LevelPrefixes[0] + "Open", CastleDoorLayer);
			startEntity.enabled = true;
		}
	}

	private void onDialogueAdvancedHandler(DialoguerTextData data)
	{
		if (!kingAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk"))
		{
			kingAnimator.SetTrigger("Talk");
		}
	}

	public void StartTalkAnimation()
	{
		kingAnimator.SetBool("Talking", value: true);
	}

	public void EndTalkAnimation()
	{
		kingAnimator.SetBool("Talking", value: false);
	}

	private void onIntroEventHandler()
	{
		beginIntroPan = true;
	}

	private void onFadeOutStartEventHandler(float time)
	{
		beginIntroPan = true;
	}

	private void stopTalk()
	{
		kingAnimator.SetBool("Talking", value: false);
	}

	private void updateDialogueState()
	{
		switch (PlayerData.Data.CountLevelsCompleted(Level.kingOfGamesLevels))
		{
		case 1:
			Dialoguer.SetGlobalFloat(KingOfGamesDialoguerStateIndex, 2f);
			return;
		case 2:
			Dialoguer.SetGlobalFloat(KingOfGamesDialoguerStateIndex, 3f);
			return;
		case 3:
			Dialoguer.SetGlobalFloat(KingOfGamesDialoguerStateIndex, 4f);
			return;
		case 4:
			Dialoguer.SetGlobalFloat(KingOfGamesDialoguerStateIndex, 5f);
			return;
		}
		if (Dialoguer.GetGlobalFloat(KingOfGamesDialoguerStateIndex) == 7f)
		{
			Dialoguer.SetGlobalFloat(KingOfGamesDialoguerStateIndex, KingOfGamesFinalDialogueState);
		}
	}

	private Levels calculateCurrentLevel()
	{
		Levels[] array = Level.kingOfGamesLevels;
		foreach (Levels levels in array)
		{
			if (!PlayerData.Data.CheckLevelCompleted(levels))
			{
				return levels;
			}
		}
		return Level.kingOfGamesLevels.GetLast();
	}

	private Levels calculatePreviousLevel()
	{
		Levels[] array = Level.kingOfGamesLevels;
		if (!PlayerData.Data.CheckLevelCompleted(array[0]))
		{
			return Levels.Test;
		}
		for (int i = 1; i < Level.kingOfGamesLevels.Length; i++)
		{
			if (!PlayerData.Data.CheckLevelCompleted(array[i]))
			{
				return array[i - 1];
			}
		}
		return Levels.Test;
	}

	private void setTargetLevel(Levels level, bool gauntlet)
	{
		currentTargetLevel = level;
		currentIsGauntlet = gauntlet;
	}

	private void setupChooseLevel()
	{
		int i = Array.IndexOf(Level.kingOfGamesLevels, currentTargetLevel);
		speechBubble.HideOptionByIndex(i);
		Vector2 basePosition = speechBubbleBasePosition;
		basePosition.y -= 130f;
		speechBubble.basePosition = basePosition;
	}

	private void revertChooseLevel()
	{
		speechBubble.basePosition = speechBubbleBasePosition;
	}

	private void movePlayersToDialoguePositions()
	{
		AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		if (player != null)
		{
			Vector3 position = player.transform.position;
			position.x = dialogueInteractionPoint.playerOneDialoguePosition.x;
			player.transform.position = position;
		}
		player = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		if (player != null)
		{
			Vector3 position2 = player.transform.position;
			position2.x = dialogueInteractionPoint.playerTwoDialoguePosition.x;
			player.transform.position = position2;
		}
	}
}
