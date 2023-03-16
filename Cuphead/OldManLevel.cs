using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OldManLevel : Level
{
	private LevelProperties.OldMan properties;

	private int EffectReset = Animator.StringToHash("Reset");

	private int EffectResetPink = Animator.StringToHash("ResetPink");

	private const float CAM_END_POS_X = -460f;

	private const float CAM_MOVE_TIME = 3f;

	private const int CAM_PHASE2_BOUNDS_LEFT = 1002;

	private const int CAM_PHASE2_BOUNDS_RIGHT = 85;

	private const int PHASE2_BOUNDS_LEFT = 1249;

	private const int PHASE2_BOUNDS_RIGHT = 331;

	private const float IRIS_TIME = 0.9f;

	[SerializeField]
	private Image fader;

	[SerializeField]
	private GameObject[] hairObjects;

	[SerializeField]
	private OldManLevelScubaGnome scubaGnomePrefab;

	[SerializeField]
	private GameObject mainPlatform;

	public OldManLevelPlatformManager platformManager;

	[SerializeField]
	private OldManLevelOldMan oldMan;

	[SerializeField]
	private OldManLevelSockPuppetHandler sockPuppet;

	[SerializeField]
	private OldManLevelGnomeLeader gnomeLeader;

	[SerializeField]
	private OldManLevelGnomeClimber gnomeClimberPrefab;

	[SerializeField]
	private OldManLevelSpikeFloor[] spikes;

	[SerializeField]
	private GameObject mountainBG;

	[SerializeField]
	private GameObject cloudLeft;

	[SerializeField]
	private GameObject cloudRight;

	[SerializeField]
	private GameObject stomachBG;

	[SerializeField]
	private Collider2D phaseTransitionTrigger;

	[SerializeField]
	private GameObject mainPit;

	[SerializeField]
	private GameObject bleachers;

	private List<OldManLevelSpikeFloor> gnomesSpawned;

	private PatternString climberPosString;

	public bool playedFirstSpikeSound;

	private bool firstAttack;

	private float[] climberXPosition = new float[10] { -1006f, -766f, -792f, -562.2f, -590.8f, -357f, -377f, -147.7f, -163.2f, 63.2f };

	private List<Effect> smokeFXPool = new List<Effect>();

	[SerializeField]
	private Effect smokePrefab;

	private List<Effect> sparkleFXPool = new List<Effect>();

	[SerializeField]
	private Effect sparklePrefab;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortraitMain;

	[SerializeField]
	private Sprite _bossPortraitPhaseTwo;

	[SerializeField]
	private Sprite _bossPortraitPhaseThree;

	[SerializeField]
	private string _bossQuoteMain;

	[SerializeField]
	private string _bossQuotePhaseTwo;

	[SerializeField]
	private string _bossQuotePhaseThree;

	public override Levels CurrentLevel => Levels.OldMan;

	public override Scenes CurrentScene => Scenes.scene_level_old_man;

	public override Sprite BossPortrait
	{
		get
		{
			switch (properties.CurrentState.stateName)
			{
			case LevelProperties.OldMan.States.Main:
				return _bossPortraitMain;
			case LevelProperties.OldMan.States.SockPuppet:
				return _bossPortraitPhaseTwo;
			case LevelProperties.OldMan.States.GnomeLeader:
				return _bossPortraitPhaseThree;
			default:
				Debug.LogError(string.Concat("Couldn't find portrait for state ", properties.CurrentState.stateName, ". Using Main."));
				return _bossPortraitMain;
			}
		}
	}

	public override string BossQuote
	{
		get
		{
			switch (properties.CurrentState.stateName)
			{
			case LevelProperties.OldMan.States.Main:
				return _bossQuoteMain;
			case LevelProperties.OldMan.States.SockPuppet:
				return _bossQuotePhaseTwo;
			case LevelProperties.OldMan.States.GnomeLeader:
				return _bossQuotePhaseThree;
			default:
				Debug.LogError(string.Concat("Couldn't find quote for state ", properties.CurrentState.stateName, ". Using Main."));
				return _bossQuoteMain;
			}
		}
	}

	protected override void PartialInit()
	{
		properties = LevelProperties.OldMan.GetMode(base.mode);
		properties.OnStateChange += base.zHack_OnStateChanged;
		properties.OnBossDeath += base.zHack_OnWin;
		base.timeline = properties.CreateTimeline(base.mode);
		goalTimes = properties.goalTimes;
		properties.OnBossDamaged += base.timeline.DealDamage;
		base.PartialInit();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortraitMain = null;
		_bossPortraitPhaseTwo = null;
		_bossPortraitPhaseThree = null;
		WORKAROUND_NullifyFields();
	}

	protected override void Start()
	{
		base.Start();
		firstAttack = true;
		platformManager.LevelInit(properties);
		oldMan.LevelInit(properties);
		sockPuppet.LevelInit(properties);
		gnomeLeader.LevelInit(properties);
		climberPosString = new PatternString(properties.CurrentState.climberGnomes.gnomePositionStrings);
		for (int i = 0; i < spikes.Length; i++)
		{
			spikes[i].SetProperties(properties);
			spikes[i].SetID(i);
		}
		gnomeLeader.gameObject.SetActive(value: false);
		AudioManager.FadeSFXVolume("sfx_dlc_omm_p3_stomachacid_amb_loop", 0f, 0f);
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		if (properties.CurrentState.stateName == LevelProperties.OldMan.States.SockPuppet)
		{
			StopAllCoroutines();
			StartCoroutine(phase_2_trans_cr());
		}
		else if (properties.CurrentState.stateName == LevelProperties.OldMan.States.GnomeLeader)
		{
			StopAllCoroutines();
			StartCoroutine(phase_3_trans_cr());
		}
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(oldmanPattern_cr());
		StartCoroutine(gnome_turrets_cr());
		StartCoroutine(climbers_cr());
	}

	protected override void OnPreWin()
	{
		if (Level.Current.mode == Mode.Easy)
		{
			sockPuppet.OnPhase3();
		}
	}

	public void CreateFX(Vector3 pos, bool isSparkle, bool isPink)
	{
		Effect effect = null;
		List<Effect> list = ((!isSparkle) ? smokeFXPool : sparkleFXPool);
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i].inUse)
			{
				effect = list[i];
				break;
			}
		}
		if (effect == null)
		{
			effect = ((!isSparkle) ? smokePrefab.Create(pos) : sparklePrefab.Create(pos));
			list.Add(effect);
		}
		effect.Initialize(pos);
		effect.animator.Play((!isPink) ? EffectReset : EffectResetPink);
		effect.inUse = true;
	}

	private void ClearFX(List<Effect> pool)
	{
		for (int i = 0; i < pool.Count; i++)
		{
			if (pool[i].inUse)
			{
				pool[i].removeOnEnd = true;
			}
			else
			{
				Object.Destroy(pool[i].gameObject);
			}
		}
	}

	private IEnumerator oldmanPattern_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		while (true)
		{
			yield return StartCoroutine(nextPattern_cr());
			yield return null;
		}
	}

	private IEnumerator nextPattern_cr()
	{
		LevelProperties.OldMan.Pattern p = properties.CurrentState.NextPattern;
		while (p == LevelProperties.OldMan.Pattern.Camel && firstAttack)
		{
			p = properties.CurrentState.NextPattern;
		}
		firstAttack = false;
		switch (p)
		{
		default:
			yield return CupheadTime.WaitForSeconds(this, 1f);
			break;
		case LevelProperties.OldMan.Pattern.Spit:
			yield return StartCoroutine(spit_cr());
			break;
		case LevelProperties.OldMan.Pattern.Duck:
			yield return StartCoroutine(duck_cr());
			break;
		case LevelProperties.OldMan.Pattern.Camel:
			yield return StartCoroutine(camel_cr());
			break;
		}
	}

	private IEnumerator spit_cr()
	{
		while (oldMan.state != OldManLevelOldMan.State.Idle)
		{
			yield return null;
		}
		oldMan.Spit();
		while (oldMan.state != OldManLevelOldMan.State.Idle)
		{
			yield return null;
		}
	}

	private IEnumerator duck_cr()
	{
		while (oldMan.state != OldManLevelOldMan.State.Idle)
		{
			yield return null;
		}
		oldMan.Goose();
		while (oldMan.state != OldManLevelOldMan.State.Idle)
		{
			yield return null;
		}
	}

	private IEnumerator camel_cr()
	{
		while (oldMan.state != OldManLevelOldMan.State.Idle)
		{
			yield return null;
		}
		oldMan.Bear();
		while (oldMan.state != OldManLevelOldMan.State.Idle)
		{
			yield return null;
		}
	}

	private IEnumerator gnome_turrets_cr()
	{
		LevelProperties.OldMan.Turret p = properties.CurrentState.turret;
		gnomesSpawned = new List<OldManLevelSpikeFloor>();
		PatternString appearString = new PatternString(p.appearOrder);
		while (true)
		{
			if (!p.gnomesOn)
			{
				yield return null;
				continue;
			}
			yield return CupheadTime.WaitForSeconds(this, p.appearDelayRange.RandomFloat());
			gnomesSpawned.RemoveAll((OldManLevelSpikeFloor g) => g.spikeState != OldManLevelSpikeFloor.SpikeState.Gnomed);
			if (gnomesSpawned.Count < p.maxCount)
			{
				int appearOrder2 = 0;
				do
				{
					appearOrder2 = appearString.PopInt();
					yield return null;
				}
				while (spikes[appearOrder2].spikeState != 0);
				spikes[appearOrder2].SpawnGnome();
				gnomesSpawned.Add(spikes[appearOrder2]);
			}
			yield return null;
		}
	}

	private IEnumerator climbers_cr()
	{
		LevelProperties.OldMan.ClimberGnomes p = properties.CurrentState.climberGnomes;
		while (true)
		{
			yield return null;
			int pos = climberPosString.PopInt();
			int platform = 4 - pos / 2;
			if (!platformManager.PlatformRemoved(platform))
			{
				OldManLevelGnomeClimber oldManLevelGnomeClimber = gnomeClimberPrefab.Spawn();
				float facing = ((pos % 2 != 0) ? 1 : (-1));
				Transform platform2 = platformManager.GetPlatform(platform);
				oldManLevelGnomeClimber.Init(climberXPosition[pos], facing, platform2, p);
				platformManager.AttachGnome(platform, oldManLevelGnomeClimber);
			}
			yield return CupheadTime.WaitForSeconds(this, p.spawnDelayRange.RandomFloat());
		}
	}

	public void ActivatePhase2Beard()
	{
		GameObject[] array = hairObjects;
		foreach (GameObject gameObject in array)
		{
			gameObject.SetActive(value: true);
		}
	}

	private IEnumerator phase_2_trans_cr()
	{
		oldMan.EndPhase1();
		ClearFX(sparkleFXPool);
		ClearFX(smokeFXPool);
		yield return oldMan.animator.WaitForAnimationToStart(this, "Phase_Trans");
		oldMan.StopAllCoroutines();
		yield return null;
		OldManLevelSpikeFloor[] array = spikes;
		foreach (OldManLevelSpikeFloor oldManLevelSpikeFloor in array)
		{
			oldManLevelSpikeFloor.Exit();
		}
		platformManager.EndPhase();
		while (oldMan.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.68421054f)
		{
			yield return null;
		}
		Level.Current.SetBounds(1249, 331, null, null);
		CupheadLevelCamera.Current.ChangeHorizontalBounds(1002, 85);
		Vector3 cameraEndPos = new Vector3(-460f, 0f, 0f);
		StartCoroutine(CupheadLevelCamera.Current.slide_camera_cr(cameraEndPos, 3f));
		StartCoroutine(move_clouds_cr(3f));
		yield return CupheadTime.WaitForSeconds(this, 2f);
		oldMan.OnPhase2();
		yield return CupheadTime.WaitForSeconds(this, 2f);
		bleachers.SetActive(value: true);
		yield return null;
	}

	private IEnumerator move_clouds_cr(float time)
	{
		float leftStartPos = cloudLeft.transform.localPosition.x;
		float rightStartPos = cloudRight.transform.localPosition.x;
		float t = 0f;
		while (t < time)
		{
			t += (float)CupheadTime.Delta;
			cloudLeft.transform.localPosition = new Vector3(EaseUtils.EaseInOutSine(leftStartPos, -720f, t / time), cloudLeft.transform.localPosition.y);
			cloudRight.transform.localPosition = new Vector3(EaseUtils.EaseInOutSine(rightStartPos, 420f, t / time), cloudRight.transform.localPosition.y);
			yield return null;
		}
		cloudLeft.transform.localPosition = new Vector3(-720f, cloudLeft.transform.localPosition.y);
		cloudRight.transform.localPosition = new Vector3(420f, cloudRight.transform.localPosition.y);
	}

	public bool InPhase2()
	{
		return properties.CurrentState.stateName == LevelProperties.OldMan.States.SockPuppet;
	}

	private IEnumerator phase_3_trans_cr()
	{
		Object.Destroy(platformManager.gameObject);
		sockPuppet.OnPhase3();
		AbstractPlayerController p1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		LevelPlayerWeaponManager weaponManagerP1 = p1.GetComponent<LevelPlayerWeaponManager>();
		LevelPlayerMotor motorP1 = p1.GetComponent<LevelPlayerMotor>();
		weaponManagerP1.InterruptSuper();
		LevelPlayerWeaponManager weaponManagerP2 = null;
		LevelPlayerMotor motorP2 = null;
		AbstractPlayerController p3 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		if (p3 != null)
		{
			motorP2 = p3.GetComponent<LevelPlayerMotor>();
			weaponManagerP2 = p3.GetComponent<LevelPlayerWeaponManager>();
			weaponManagerP2.InterruptSuper();
		}
		yield return new WaitForEndOfFrame();
		while (sockPuppet.transState != OldManLevelSockPuppetHandler.TransitionState.PlatformDestroyed)
		{
			yield return null;
		}
		mainPlatform.SetActive(value: false);
		phaseTransitionTrigger.gameObject.SetActive(value: true);
		mainPit.SetActive(value: false);
		if ((bool)motorP1)
		{
			motorP1.OnTrampolineKnockUp(-2.3f);
		}
		if ((bool)motorP2)
		{
			motorP2.OnTrampolineKnockUp(-2.3f);
		}
		bool readyToGo = false;
		while (!readyToGo)
		{
			if ((p1.IsDead || phaseTransitionTrigger.bounds.Contains(p1.transform.position + Vector3.down * 10f)) && (PlayerManager.GetPlayer(PlayerId.PlayerTwo) == null || PlayerManager.GetPlayer(PlayerId.PlayerTwo).IsDead || phaseTransitionTrigger.bounds.Contains(PlayerManager.GetPlayer(PlayerId.PlayerTwo).transform.position + Vector3.down * 10f)))
			{
				readyToGo = true;
				sockPuppet.SwallowedPlayers();
			}
			if (p1.IsDead || phaseTransitionTrigger.bounds.Contains(p1.transform.position + Vector3.down * 10f))
			{
				p1.gameObject.SetActive(value: false);
			}
			if ((PlayerManager.GetPlayer(PlayerId.PlayerTwo) == null || PlayerManager.GetPlayer(PlayerId.PlayerTwo).IsDead || phaseTransitionTrigger.bounds.Contains(PlayerManager.GetPlayer(PlayerId.PlayerTwo).transform.position + Vector3.down * 10f)) && PlayerManager.GetPlayer(PlayerId.PlayerTwo) != null)
			{
				PlayerManager.GetPlayer(PlayerId.PlayerTwo).gameObject.SetActive(value: false);
			}
			yield return null;
		}
		PlayerDeathEffect[] ghosts = Object.FindObjectsOfType(typeof(PlayerDeathEffect)) as PlayerDeathEffect[];
		PlayerDeathEffect[] array = ghosts;
		foreach (PlayerDeathEffect playerDeathEffect in array)
		{
			playerDeathEffect.transform.position += Vector3.up * 5000f;
		}
		PlayerSuperGhost[] superGhosts = Object.FindObjectsOfType(typeof(PlayerSuperGhost)) as PlayerSuperGhost[];
		PlayerSuperGhost[] array2 = superGhosts;
		foreach (PlayerSuperGhost playerSuperGhost in array2)
		{
			Object.Destroy(playerSuperGhost.gameObject);
		}
		PlayerSuperGhostHeart[] superGhostHearts = Object.FindObjectsOfType(typeof(PlayerSuperGhostHeart)) as PlayerSuperGhostHeart[];
		PlayerSuperGhostHeart[] array3 = superGhostHearts;
		foreach (PlayerSuperGhostHeart obj in array3)
		{
			Object.Destroy(obj);
		}
		while (sockPuppet.transState != OldManLevelSockPuppetHandler.TransitionState.InStomach)
		{
			yield return null;
		}
		yield return StartCoroutine(iris_cr());
		if (!p1.IsDead)
		{
			motorP1.EnableInput();
		}
		p3 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		if (p3 != null && !p3.IsDead)
		{
			motorP2.EnableInput();
		}
		StartCoroutine(scuba_gnomes_cr());
		yield return null;
	}

	private IEnumerator iris_cr()
	{
		LevelPauseGUI pauseGUI = GameObject.Find("Level_UI").GetComponentInChildren<LevelPauseGUI>();
		pauseGUI.ForceDisablePause(value: true);
		Animator faderAni = fader.GetComponent<Animator>();
		Color c2 = fader.color;
		c2.a = 1f;
		fader.color = c2;
		faderAni.SetTrigger("Iris_In");
		yield return faderAni.WaitForAnimationToEnd(this, "Iris_In");
		yield return new WaitForSeconds(0.9f);
		SetupStomach();
		faderAni.SetTrigger("Iris_Out");
		gnomeLeader.StartGnomeLeader();
		yield return faderAni.WaitForAnimationToEnd(this, "Iris_Out");
		pauseGUI.ForceDisablePause(value: false);
		c2 = fader.color;
		c2.a = 0f;
		fader.color = c2;
	}

	private void SetupStomach()
	{
		Level.Current.SetBounds(1249, 331, null, null);
		gnomeLeader.gameObject.SetActive(value: true);
		phaseTransitionTrigger.gameObject.SetActive(value: false);
		mountainBG.SetActive(value: false);
		stomachBG.SetActive(value: true);
		sockPuppet.FinishPuppet();
		AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		if (!player.IsDead)
		{
			player.gameObject.SetActive(value: true);
			LevelPlayerMotor component = player.GetComponent<LevelPlayerMotor>();
			component.ClearBufferedInput();
			component.ForceLooking(new Trilean2(1, 1));
			player.GetComponent<LevelPlayerAnimationController>().ResetMoveX();
			component.OnRevive(gnomeLeader.platformPositions[1].position + Vector3.up * 1000f);
			component.CancelReviveBounce();
			component.EnableInput();
		}
		AbstractPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		if (player2 != null && !player2.IsDead)
		{
			player2.gameObject.SetActive(value: true);
			LevelPlayerMotor component2 = player2.GetComponent<LevelPlayerMotor>();
			component2 = player2.GetComponent<LevelPlayerMotor>();
			component2.ClearBufferedInput();
			component2.ForceLooking(new Trilean2(1, 1));
			player2.GetComponent<LevelPlayerAnimationController>().ResetMoveX();
			component2.OnRevive(gnomeLeader.platformPositions[3].position + Vector3.up * 1000f);
			component2.CancelReviveBounce();
			component2.EnableInput();
		}
		SFX_StomachLoop();
	}

	private IEnumerator scuba_gnomes_cr()
	{
		bool onLeft = Rand.Bool();
		LevelProperties.OldMan.ScubaGnomes p = properties.CurrentState.scubaGnomes;
		PatternString scubaTypeString = new PatternString(p.scubaTypeString);
		PatternString spawnDelayString = new PatternString(p.spawnDelayString);
		PatternString dartParryableString = new PatternString(p.dartParryableString);
		float offset = 50f;
		while (true)
		{
			float xPos = ((!onLeft) ? (CupheadLevelCamera.Current.Bounds.xMax - offset) : (CupheadLevelCamera.Current.Bounds.xMin + offset));
			OldManLevelScubaGnome scubaGnome = scubaGnomePrefab.Spawn();
			scubaGnome.Init(new Vector3(xPos, CupheadLevelCamera.Current.Bounds.yMin), PlayerManager.GetNext(), scubaTypeString.PopLetter() == 'A', onLeft, dartParryableString.PopLetter() == 'P', p, gnomeLeader);
			yield return CupheadTime.WaitForSeconds(this, spawnDelayString.PopFloat());
			onLeft = !onLeft;
			yield return null;
		}
	}

	private void SFX_StomachLoop()
	{
		base.transform.position = stomachBG.transform.position;
		AudioManager.PlayLoop("sfx_dlc_omm_p3_stomachacid_amb_loop");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_stomachacid_amb_loop");
		AudioManager.FadeSFXVolume("sfx_dlc_omm_p3_stomachacid_amb_loop", 1f, 1f);
	}

	private void WORKAROUND_NullifyFields()
	{
		platformManager = null;
		fader = null;
		hairObjects = null;
		scubaGnomePrefab = null;
		mainPlatform = null;
		oldMan = null;
		sockPuppet = null;
		gnomeLeader = null;
		gnomeClimberPrefab = null;
		spikes = null;
		mountainBG = null;
		cloudLeft = null;
		cloudRight = null;
		stomachBG = null;
		phaseTransitionTrigger = null;
		mainPit = null;
		bleachers = null;
		gnomesSpawned = null;
		climberPosString = null;
		climberXPosition = null;
		_bossPortraitMain = null;
		_bossPortraitPhaseTwo = null;
		_bossPortraitPhaseThree = null;
		_bossQuoteMain = null;
		_bossQuotePhaseTwo = null;
		_bossQuotePhaseThree = null;
	}
}
