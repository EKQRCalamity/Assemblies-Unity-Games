using System.Collections;
using UnityEngine;

public class RetroArcadeLevel : Level
{
	private LevelProperties.RetroArcade properties;

	public static float TOTAL_POINTS;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	[SerializeField]
	private RetroArcadeTrafficManager trafficManager;

	[SerializeField]
	private RetroArcadeTentacleManager tentacleManager;

	[SerializeField]
	private RetroArcadeSnakeManager snakeManager;

	[SerializeField]
	private RetroArcadeSheriffManager sheriffManager;

	[SerializeField]
	private RetroArcadeChaserManager chaserManager;

	[SerializeField]
	private RetroArcadeBouncyManager bouncyManager;

	[SerializeField]
	private RetroArcadeAlienManager alienManager;

	[SerializeField]
	private RetroArcadeCaterpillarManager caterpillarManager;

	[SerializeField]
	private RetroArcadeRobotManager robotManager;

	[SerializeField]
	private RetroArcadePaddleShip paddleShip;

	[SerializeField]
	private RetroArcadeQShip qShip;

	[SerializeField]
	private RetroArcadeUFO ufo;

	[SerializeField]
	private RetroArcadeToadManager toadManager;

	[SerializeField]
	private RetroArcadeMissileMan missileMan;

	[SerializeField]
	private RetroArcadeWorm worm;

	[SerializeField]
	private RetroArcadeBigPlayer bigCuphead;

	[SerializeField]
	private RetroArcadeBigPlayer bigMugman;

	public ArcadePlayerController playerPrefab;

	public override Levels CurrentLevel => Levels.RetroArcade;

	public override Scenes CurrentScene => Scenes.scene_level_retro_arcade;

	public static float ACCURACY_BONUS { get; private set; }

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.RetroArcade.GetMode(base.mode);
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
		alienManager.LevelInit(properties);
		caterpillarManager.LevelInit(properties);
		robotManager.LevelInit(properties);
		paddleShip.LevelInit(properties);
		qShip.LevelInit(properties);
		ufo.LevelInit(properties);
		toadManager.LevelInit(properties);
		worm.LevelInit(properties);
		bouncyManager.LevelInit(properties);
		missileMan.LevelInit(properties);
		chaserManager.LevelInit(properties);
		sheriffManager.LevelInit(properties);
		snakeManager.LevelInit(properties);
		tentacleManager.LevelInit(properties);
		trafficManager.LevelInit(properties);
		ACCURACY_BONUS = properties.CurrentState.general.accuracyBonus;
		bigCuphead.Init(PlayerManager.GetPlayer(PlayerId.PlayerOne) as ArcadePlayerController);
		bigMugman.Init(PlayerManager.GetPlayer(PlayerId.PlayerTwo) as ArcadePlayerController);
	}

	protected override void CreatePlayers()
	{
		base.CreatePlayers();
	}

	protected override void Update()
	{
		base.Update();
	}

	protected override void OnLevelStart()
	{
		bigCuphead.LevelStart();
		bigMugman.LevelStart();
		StartStateCoroutine();
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		bigCuphead.OnVictory();
		bigMugman.OnVictory();
		StartStateCoroutine();
	}

	private void StartStateCoroutine()
	{
		switch (properties.CurrentState.stateName)
		{
		case LevelProperties.RetroArcade.States.Aliens:
			StartCoroutine(startAliens_cr());
			break;
		case LevelProperties.RetroArcade.States.Caterpillar:
			StartCoroutine(startCaterpillars_cr());
			break;
		case LevelProperties.RetroArcade.States.Robots:
			StartCoroutine(startRobots_cr());
			break;
		case LevelProperties.RetroArcade.States.PaddleShip:
			StartCoroutine(startPaddleShip_cr());
			break;
		case LevelProperties.RetroArcade.States.QShip:
			StartCoroutine(startQShip_cr());
			break;
		case LevelProperties.RetroArcade.States.UFO:
			StartCoroutine(startUFO_cr());
			break;
		case LevelProperties.RetroArcade.States.Toad:
			StartCoroutine(startToad_cr());
			break;
		case LevelProperties.RetroArcade.States.Worm:
			StartCoroutine(startWorm_cr());
			break;
		case LevelProperties.RetroArcade.States.Bouncy:
			StartCoroutine(startBouncy_cr());
			break;
		case LevelProperties.RetroArcade.States.Main:
		case LevelProperties.RetroArcade.States.MissileMan:
			StartCoroutine(startMissile_cr());
			break;
		case LevelProperties.RetroArcade.States.Chaser:
			StartCoroutine(startChaser_cr());
			break;
		case LevelProperties.RetroArcade.States.Sheriff:
			StartCoroutine(startSheriff_cr());
			break;
		case LevelProperties.RetroArcade.States.Snake:
			StartCoroutine(startSnake_cr());
			break;
		case LevelProperties.RetroArcade.States.Tentacle:
			StartCoroutine(startTentacle_cr());
			break;
		case LevelProperties.RetroArcade.States.Traffic:
			StartCoroutine(startTrafficUFO_cr());
			break;
		case LevelProperties.RetroArcade.States.JetpackTest:
			StartCoroutine(switchToJetpack_cr());
			break;
		case LevelProperties.RetroArcade.States.Generic:
			break;
		}
	}

	public override void OnLevelEnd()
	{
		base.OnLevelEnd();
		bigCuphead.OnVictory();
		bigMugman.OnVictory();
	}

	private IEnumerator startAliens_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		alienManager.StartAliens();
	}

	private IEnumerator startCaterpillars_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		caterpillarManager.StartCaterpillar();
	}

	private IEnumerator startRobots_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		robotManager.StartRobots();
	}

	private IEnumerator startPaddleShip_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		paddleShip.StartPaddleShip();
	}

	private IEnumerator startQShip_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		qShip.StartQShip();
	}

	private IEnumerator startUFO_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		ufo.StartUFO();
	}

	private IEnumerator startToad_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		toadManager.StartToad();
	}

	private IEnumerator startWorm_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		worm.StartWorm();
	}

	private IEnumerator startBouncy_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		bouncyManager.StartBouncy();
	}

	private IEnumerator startMissile_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		missileMan.StartMissile();
	}

	private IEnumerator switchToRocket_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		ArcadePlayerController player1 = PlayerManager.GetPlayer<ArcadePlayerController>(PlayerId.PlayerOne);
		player1.ChangeToRocket();
		if (PlayerManager.GetPlayer(PlayerId.PlayerTwo) != null)
		{
			ArcadePlayerController player2 = PlayerManager.GetPlayer<ArcadePlayerController>(PlayerId.PlayerTwo);
			player2.ChangeToRocket();
		}
	}

	private IEnumerator startChaser_cr()
	{
		ArcadePlayerController player1 = PlayerManager.GetPlayer<ArcadePlayerController>(PlayerId.PlayerOne);
		if (player1.controlScheme != ArcadePlayerController.ControlScheme.Rocket)
		{
			yield return StartCoroutine(switchToRocket_cr());
		}
		yield return CupheadTime.WaitForSeconds(this, 3f);
		chaserManager.StartChasers();
	}

	private IEnumerator startSheriff_cr()
	{
		ArcadePlayerController player1 = PlayerManager.GetPlayer<ArcadePlayerController>(PlayerId.PlayerOne);
		if (player1.controlScheme != ArcadePlayerController.ControlScheme.Rocket)
		{
			yield return StartCoroutine(switchToRocket_cr());
		}
		yield return CupheadTime.WaitForSeconds(this, 3f);
		sheriffManager.StartSheriff();
	}

	private IEnumerator startSnake_cr()
	{
		ArcadePlayerController player1 = PlayerManager.GetPlayer<ArcadePlayerController>(PlayerId.PlayerOne);
		if (player1.controlScheme != ArcadePlayerController.ControlScheme.Rocket)
		{
			yield return StartCoroutine(switchToRocket_cr());
		}
		yield return CupheadTime.WaitForSeconds(this, 3f);
		snakeManager.StartSnake();
	}

	private IEnumerator startTentacle_cr()
	{
		ArcadePlayerController player1 = PlayerManager.GetPlayer<ArcadePlayerController>(PlayerId.PlayerOne);
		if (player1.controlScheme != ArcadePlayerController.ControlScheme.Rocket)
		{
			yield return StartCoroutine(switchToRocket_cr());
		}
		yield return CupheadTime.WaitForSeconds(this, 3f);
		tentacleManager.StartTentacle();
	}

	private IEnumerator startTrafficUFO_cr()
	{
		ArcadePlayerController player1 = PlayerManager.GetPlayer<ArcadePlayerController>(PlayerId.PlayerOne);
		if (player1.controlScheme != ArcadePlayerController.ControlScheme.Rocket)
		{
			yield return StartCoroutine(switchToRocket_cr());
		}
		yield return CupheadTime.WaitForSeconds(this, 3f);
		trafficManager.StartTraffic();
	}

	private IEnumerator switchToJetpack_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		ArcadePlayerController player1 = PlayerManager.GetPlayer<ArcadePlayerController>(PlayerId.PlayerOne);
		player1.ChangeToJetpack();
		if (PlayerManager.GetPlayer(PlayerId.PlayerTwo) != null)
		{
			ArcadePlayerController player2 = PlayerManager.GetPlayer<ArcadePlayerController>(PlayerId.PlayerTwo);
			player2.ChangeToJetpack();
		}
		yield return null;
	}
}
