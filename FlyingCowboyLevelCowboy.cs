using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCowboyLevelCowboy : LevelProperties.FlyingCowboy.Entity
{
	public enum State
	{
		Idle,
		Wait,
		SnakeAttack,
		BeamAttack,
		Vacuum,
		Ricochet,
		PhaseTrans
	}

	public class VacuumForce : PlanePlayerMotor.Force
	{
		private PlanePlayerController player;

		private Transform aimPointTransform;

		private float strength;

		private float currentStrength;

		private float timeToFullStrength;

		private float elapsedTime;

		public override Vector2 force
		{
			get
			{
				if (player == null)
				{
					return Vector2.zero;
				}
				return (player.center - aimPointTransform.position).normalized * currentStrength;
			}
		}

		public VacuumForce(PlanePlayerController player, Transform aimPointTransform, float strength, float timeToFullStrength)
			: base(Vector2.zero, enabled: true)
		{
			this.player = player;
			this.aimPointTransform = aimPointTransform;
			this.strength = strength;
			currentStrength = 0f;
			this.timeToFullStrength = timeToFullStrength;
			elapsedTime = 0f;
		}

		public void UpdateStrength(float deltaTime)
		{
			elapsedTime += deltaTime;
			currentStrength = Mathf.Lerp(0f, strength, elapsedTime / timeToFullStrength);
		}
	}

	private const int SNAKE_BULLET_COUNT = 2;

	private const int DEBRIS_SPAWN_COUNT = 6;

	private const int DEBRIS_CURVE_SPAWN_COUNT = 4;

	private static readonly int SaloonAnimatorLayer = 1;

	private static readonly int PosterAnimatorLayer = 2;

	private static readonly int DoorsAnimatorLayer = 3;

	private static readonly int WheelSmokeAnimatorLayer = 4;

	private static readonly int TransitionSmokeLayer = 5;

	[SerializeField]
	private SpriteRenderer posterRenderer;

	[SerializeField]
	private FlyingCowboyLevelUFO ufoPrefab;

	[SerializeField]
	private FlyingCowboyLevelBird birdPrefab;

	[SerializeField]
	private Transform birdStartPosition;

	[SerializeField]
	private Transform birdEndPosition;

	[SerializeField]
	private TriggerZone birdSafetyZone;

	[SerializeField]
	private FlyingCowboyLevelBackshot backshotPrefab;

	[SerializeField]
	private Transform[] snakeTopRoot;

	[SerializeField]
	private Transform[] snakeBottomRoot;

	[SerializeField]
	private FlyingCowboyLevelOilBlob oilBlobPrefab;

	[SerializeField]
	private Effect snakeOilMuzzleFXPrefab;

	[SerializeField]
	private GameObject cactus;

	[SerializeField]
	private Vector2 wobbleRadius;

	[SerializeField]
	private Vector2 wobbleDuration;

	[SerializeField]
	private GameObject saloonCollidersParent;

	[SerializeField]
	private SpriteRenderer lanternARenderer;

	[SerializeField]
	private SpriteRenderer lanternBRenderer;

	[SerializeField]
	private SpriteRenderer[] saloonTransitionDisableRenderers;

	[SerializeField]
	private SpriteRenderer frontWheelRenderer;

	[SerializeField]
	private SpriteRenderer backWheelRenderer;

	[SerializeField]
	private FlyingCowboyLevelDebris[] smallVacuumDebrisPrefabs;

	[SerializeField]
	private FlyingCowboyLevelDebris[] mediumVacuumDebrisPrefabs;

	[SerializeField]
	private FlyingCowboyLevelDebris[] largeVacuumDebrisPrefabs;

	[SerializeField]
	private FlyingCowboyLevelRicochetDebris ricochetPrefab;

	[SerializeField]
	private AbstractProjectile ricochetUpPrefab;

	[SerializeField]
	private Transform ricochetUpSpawnPoint;

	[SerializeField]
	private BasicProjectile coinProjectile;

	[SerializeField]
	private Transform pistolShootRoot;

	[SerializeField]
	private int debrisSpawnHorizontalSpacing = 140;

	[SerializeField]
	private Transform vacuumDebrisAimTransform;

	[SerializeField]
	private Transform vacuumDebrisDisappearTransform;

	[SerializeField]
	private Transform vacuumSpawnTop;

	[SerializeField]
	private Transform vacuumSpawnBottom;

	[SerializeField]
	private SpriteRenderer bigVacuumRenderer;

	[SerializeField]
	private SpriteRenderer transitionVacuumRenderer;

	[SerializeField]
	private SpriteRenderer regularVacuumRenderer;

	[SerializeField]
	private SpriteRenderer bigHoseRenderer;

	[SerializeField]
	private SpriteRenderer transitionHoseRenderer;

	[SerializeField]
	private SpriteRenderer regularHoseRenderer;

	[SerializeField]
	private SpriteRenderer phase2PuffARenderer;

	[SerializeField]
	private SpriteRenderer phase2PuffBRenderer;

	private bool introBirdTriggered;

	private FlyingCowboyLevelBird introBird;

	private bool phase2Trigger;

	private bool endTransitionTrigger;

	private bool phase3Trigger;

	private Vector3 initialSaloonPosition;

	private Vector2 wobbleTimeElapsed;

	private Vector3[] topPositions;

	private Vector3[] bottomPositions;

	private Vector3[] sidePositions;

	private Vector3[] topCurvePositions;

	private Vector3[] bottomCurvePositions;

	private Vector3 phase2BasePosition;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private List<FlyingCowboyLevelDebris> allDebris;

	private FlyingCowboyLevelUFO ufo;

	private PatternString snakeOilShotsPerAttackString;

	private PatternString snakeOffsetString;

	private PatternString snakeWidthString;

	private PatternString backshotHighSpawnPosition;

	private PatternString backshotLowSpawnPosition;

	private PatternString backshotSpawnDelay;

	private PatternString backshotBulletParryable;

	private PatternString backshotAnticipationStartDistancePattern;

	private int debrisTopMainIndex;

	private int debrisBottomMainIndex;

	private int debrisSideMainIndex;

	private VacuumForce forcePlayer1;

	private VacuumForce forcePlayer2;

	private PatternString debrisCurveString;

	private PatternString debrisParryString;

	private PatternString ricochetParryString;

	private int nextSafeShoot;

	private bool posterFlyAwayTriggered;

	private Coroutine transitionVacuumAttackCoroutine;

	private Coroutine vacuumSizeCoroutine;

	public State state { get; private set; }

	public bool IsDead { get; private set; }

	public bool onBottom { get; private set; }

	private void OnEnable()
	{
		SceneLoader.OnFadeOutStartEvent += onFadeOutStartEvent;
		PlayerManager.OnPlayerJoinedEvent += onPlayerJoinedEvent;
	}

	private void OnDisable()
	{
		SceneLoader.OnFadeOutStartEvent -= onFadeOutStartEvent;
		PlayerManager.OnPlayerJoinedEvent -= onPlayerJoinedEvent;
	}

	private void Start()
	{
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		allDebris = new List<FlyingCowboyLevelDebris>();
		topPositions = new Vector3[6];
		sidePositions = new Vector3[6];
		bottomPositions = new Vector3[6];
		topCurvePositions = new Vector3[4];
		bottomCurvePositions = new Vector3[4];
		LevelProperties.FlyingCowboy.State currentState = base.properties.CurrentState;
		debrisCurveString = new PatternString(currentState.debris.debrisCurveShotString);
		debrisParryString = new PatternString(currentState.debris.debrisParryString);
		ricochetParryString = new PatternString(currentState.ricochet.splitParryString);
		backshotHighSpawnPosition = new PatternString(currentState.backshotEnemy.highSpawnPosition);
		backshotLowSpawnPosition = new PatternString(currentState.backshotEnemy.lowSpawnPosition);
		backshotSpawnDelay = new PatternString(currentState.backshotEnemy.spawnDelay);
		backshotBulletParryable = new PatternString(currentState.backshotEnemy.bulletParryString);
		backshotAnticipationStartDistancePattern = new PatternString(currentState.backshotEnemy.anticipationStartDistance);
		SetupDebrisSpawnPoints();
		StartCoroutine(wobble_cr());
		introBird = birdPrefab.Spawn(birdEndPosition.position);
		introBird.InitializeIntro(birdEndPosition.position);
		SFX_COWGIRL_COWGIRL_WheelConstantLoop();
	}

	private void Update()
	{
		if (forcePlayer1 != null)
		{
			forcePlayer1.UpdateStrength(CupheadTime.Delta);
		}
		if (forcePlayer2 != null)
		{
			forcePlayer2.UpdateStrength(CupheadTime.Delta);
		}
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void SetupDebrisSpawnPoints()
	{
		sidePositions = new Vector3[6];
		float num = (vacuumSpawnTop.position.y - vacuumSpawnBottom.position.y) / 5f;
		for (int i = 0; i < 6; i++)
		{
			float x = vacuumSpawnBottom.position.x;
			float y = ((i != 5) ? (vacuumSpawnBottom.position.y + num * (float)i) : vacuumSpawnTop.position.y);
			ref Vector3 reference = ref sidePositions[i];
			reference = new Vector3(x, y);
		}
		float num2 = ((float)debrisSpawnHorizontalSpacing - vacuumSpawnTop.position.x) / 6f;
		for (int j = 0; j < 6; j++)
		{
			float x2 = vacuumSpawnTop.position.x + num2 + num2 * (float)j;
			float y2 = vacuumSpawnTop.position.y;
			ref Vector3 reference2 = ref topPositions[j];
			reference2 = new Vector3(x2, y2);
		}
		for (int k = 0; k < 4; k++)
		{
			float x3 = vacuumSpawnTop.position.x + num2 + num2 * (float)(6 + k);
			float y3 = vacuumSpawnTop.position.y;
			ref Vector3 reference3 = ref topCurvePositions[k];
			reference3 = new Vector3(x3, y3);
		}
		for (int l = 0; l < 6; l++)
		{
			float x4 = vacuumSpawnBottom.position.x + num2 + num2 * (float)l;
			float y4 = vacuumSpawnBottom.position.y;
			ref Vector3 reference4 = ref bottomPositions[l];
			reference4 = new Vector3(x4, y4);
		}
		for (int m = 0; m < 4; m++)
		{
			float x5 = vacuumSpawnTop.position.x + num2 + num2 * (float)(6 + m);
			float y5 = vacuumSpawnTop.position.y;
			ref Vector3 reference5 = ref topCurvePositions[m];
			reference5 = new Vector3(x5, y5);
		}
		for (int n = 0; n < 4; n++)
		{
			float x6 = vacuumSpawnBottom.position.x + num2 + num2 * (float)(6 + n);
			float y6 = vacuumSpawnBottom.position.y;
			ref Vector3 reference6 = ref bottomCurvePositions[n];
			reference6 = new Vector3(x6, y6);
		}
	}

	public override void LevelInit(LevelProperties.FlyingCowboy properties)
	{
		base.LevelInit(properties);
		Level.Current.OnIntroEvent += onIntroEventHandler;
		snakeOilShotsPerAttackString = new PatternString(properties.CurrentState.snakeAttack.shotsPerAttack);
		snakeOffsetString = new PatternString(properties.CurrentState.snakeAttack.snakeOffsetString);
		snakeWidthString = new PatternString(properties.CurrentState.snakeAttack.snakeWidthString);
		debrisTopMainIndex = Random.Range(0, properties.CurrentState.debris.debrisTopSpawn.Length);
		debrisBottomMainIndex = Random.Range(0, properties.CurrentState.debris.debrisBottomSpawn.Length);
		debrisSideMainIndex = Random.Range(0, properties.CurrentState.debris.debrisSideSpawn.Length);
		initialSaloonPosition = base.transform.position;
		state = State.Idle;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void onIntroEventHandler()
	{
		StartCoroutine(intro_cr());
	}

	private IEnumerator intro_cr()
	{
		StartCoroutine(introBird_cr(0f));
		yield return CupheadTime.WaitForSeconds(this, 0.25f);
		base.animator.Play("Intro", 0);
		yield return base.animator.WaitForAnimationToEnd(this, "Intro", 0);
		StartCoroutine(main_cr());
	}

	private IEnumerator introBird_cr(float time)
	{
		if (!introBirdTriggered)
		{
			introBirdTriggered = true;
			yield return CupheadTime.WaitForSeconds(this, time);
			introBird.MoveIntro(birdStartPosition.position, base.properties.CurrentState.bird);
			introBird = null;
		}
	}

	private IEnumerator main_cr()
	{
		LevelProperties.FlyingCowboy.Cart p = base.properties.CurrentState.cart;
		PatternString pattern = new PatternString(p.cartAttackString);
		StartCoroutine(spawnBirdEnemies_cr());
		while (pattern.GetString() != "S")
		{
			pattern.PopString();
			yield return null;
		}
		while (true)
		{
			if (state != 0 || phase2Trigger)
			{
				yield return null;
				continue;
			}
			yield return null;
			switch (pattern.GetString())
			{
			case "M":
				StartCoroutine(wait_cr());
				break;
			case "S":
				StartCoroutine(snakeAttack_cr());
				StartCoroutine(spawnBackshotEnemy_cr());
				break;
			case "B":
				StartCoroutine(beamAttack_cr());
				StartCoroutine(spawnBackshotEnemy_cr());
				break;
			}
			pattern.PopString();
			yield return null;
		}
	}

	private IEnumerator breakableRecoveryPhase1_cr(float duration)
	{
		float t = 0f;
		while (t < duration && !phase2Trigger)
		{
			t += (float)CupheadTime.Delta;
			yield return null;
		}
	}

	private IEnumerator wobble_cr()
	{
		while (true)
		{
			wobbleTimeElapsed.x += CupheadTime.Delta;
			wobbleTimeElapsed.y += CupheadTime.Delta;
			if (wobbleTimeElapsed.x >= 2f * wobbleDuration.x)
			{
				wobbleTimeElapsed.x -= 2f * wobbleDuration.x;
			}
			float tx = ((!(wobbleTimeElapsed.x > wobbleDuration.x)) ? (wobbleTimeElapsed.x / wobbleDuration.x) : (1f - (wobbleTimeElapsed.x - wobbleDuration.x) / wobbleDuration.x));
			if (wobbleTimeElapsed.y >= 2f * wobbleDuration.y)
			{
				wobbleTimeElapsed.y -= 2f * wobbleDuration.y;
			}
			float ty = ((!(wobbleTimeElapsed.y > wobbleDuration.y)) ? (wobbleTimeElapsed.y / wobbleDuration.y) : (1f - (wobbleTimeElapsed.y - wobbleDuration.y) / wobbleDuration.y));
			Vector3 position = initialSaloonPosition;
			position.x += EaseUtils.EaseInOutSine(wobbleRadius.x, 0f - wobbleRadius.x, tx);
			position.y += EaseUtils.EaseInOutSine(wobbleRadius.y, 0f - wobbleRadius.y, ty);
			base.transform.position = position;
			yield return null;
		}
	}

	private IEnumerator wait_cr()
	{
		state = State.Wait;
		LevelProperties.FlyingCowboy.Cart p = base.properties.CurrentState.cart;
		base.animator.SetBool("OnHide", value: true);
		string animationBaseName = ((!onBottom) ? "HideToLow" : "HideToHigh");
		yield return base.animator.WaitForNormalizedTime(this, 1f, animationBaseName + "Start");
		base.animator.Play((!onBottom) ? "ToOpen" : "ToClosed", DoorsAnimatorLayer);
		yield return CupheadTime.WaitForSeconds(this, p.cartPopinTime);
		onBottom = !onBottom;
		base.animator.SetBool("IsLow", onBottom);
		yield return base.animator.WaitForAnimationToStart(this, animationBaseName + "End");
		base.animator.SetBool("OnHide", value: false);
		yield return base.animator.WaitForAnimationToEnd(this, animationBaseName + "End");
		state = State.Idle;
	}

	private IEnumerator snakeAttack_cr()
	{
		LevelProperties.FlyingCowboy.SnakeAttack p = base.properties.CurrentState.snakeAttack;
		state = State.SnakeAttack;
		string animationPrefix = "SnakeOil" + ((!onBottom) ? "_High" : "_Low") + ".";
		base.animator.SetTrigger("OnSnakeOil");
		base.animator.SetBool("SnakeInitialDelay", value: false);
		int shotsPerAttack = snakeOilShotsPerAttackString.PopInt();
		for (int shotCount = 0; shotCount < shotsPerAttack && (!phase2Trigger || shotCount <= 0); shotCount++)
		{
			if (shotCount > 0)
			{
				yield return base.animator.WaitForAnimationToEnd(this, animationPrefix + "SnakeOilShoot");
				yield return CupheadTime.WaitForSeconds(this, p.attackDelay);
				base.animator.SetTrigger("OnSnakeShoot");
			}
			yield return base.animator.WaitForAnimationToStart(this, animationPrefix + "SnakeOilShoot");
		}
		base.animator.SetTrigger("OnSnakeEnd");
		yield return base.animator.WaitForAnimationToEnd(this, animationPrefix + "SnakeOilExit");
		yield return StartCoroutine(breakableRecoveryPhase1_cr(p.attackRecovery));
		state = State.Idle;
	}

	private void animationEvent_SnakeShoot()
	{
		LevelProperties.FlyingCowboy.SnakeAttack snakeAttack = base.properties.CurrentState.snakeAttack;
		float num = snakeOffsetString.PopFloat();
		float num2 = snakeWidthString.PopFloat();
		AbstractPlayerController next = PlayerManager.GetNext();
		float snakeSpawnX = 640f - snakeAttack.breakLinePosition;
		float num3 = next.transform.position.y + num;
		for (int i = 0; i < 2; i++)
		{
			float num4 = ((i != 0) ? (0f - num2) : num2);
			float num5 = num3 + num4;
			float finalYPosition = ((!(num5 > 0f)) ? ((!(num5 > -360f)) ? (-340f) : num5) : ((!(num5 < 360f)) ? 340f : num5));
			Vector3 position = ((!onBottom) ? snakeTopRoot[i].position : snakeBottomRoot[i].position);
			snakeOilMuzzleFXPrefab.Create(position);
			oilBlobPrefab.Create(position, finalYPosition, snakeSpawnX, snakeAttack, i == 0);
		}
	}

	private IEnumerator beamAttack_cr()
	{
		LevelProperties.FlyingCowboy.BeamAttack p = base.properties.CurrentState.beamAttack;
		state = State.BeamAttack;
		cactus.SetActive(value: true);
		string prefix = ((!onBottom) ? "Cactus_High." : "Cactus_Low.");
		base.animator.SetTrigger("OnCactus");
		yield return base.animator.WaitForAnimationToEnd(this, prefix + "Intro");
		yield return CupheadTime.WaitForSeconds(this, p.beamWarningTime);
		base.animator.SetTrigger("EndLasso");
		SFX_COWGIRL_COWGIRL_LassoSpinLoopStop();
		yield return base.animator.WaitForAnimationToStart(this, prefix + "Hold");
		yield return CupheadTime.WaitForSeconds(this, p.beamDuration);
		base.animator.SetTrigger("EndCactusHold");
		yield return base.animator.WaitForAnimationToEnd(this, prefix + "End");
		cactus.SetActive(value: false);
		yield return StartCoroutine(breakableRecoveryPhase1_cr(p.attackRecovery));
		state = State.Idle;
	}

	private IEnumerator spawnBackshotEnemy_cr()
	{
		LevelProperties.FlyingCowboy.BackshotEnemy p = base.properties.CurrentState.backshotEnemy;
		yield return CupheadTime.WaitForSeconds(this, backshotSpawnDelay.PopFloat());
		float positionY = ((!onBottom) ? backshotLowSpawnPosition.PopFloat() : backshotHighSpawnPosition.PopFloat());
		Vector3 position = new Vector3(740f, positionY);
		backshotPrefab.Create(position, 180f, p.enemySpeed, p.bulletSpeed, p.enemyHealth, backshotAnticipationStartDistancePattern.PopFloat(), backshotBulletParryable.PopLetter() == 'P');
	}

	private IEnumerator spawnBirdEnemies_cr()
	{
		LevelProperties.FlyingCowboy.Bird p = base.properties.CurrentState.bird;
		PatternString bulletLandingPositionPattern = new PatternString(p.bulletLandingPosition);
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, p.spawnDelayRange.RandomFloat());
			bool canSpawn = false;
			float safetyTimer = 0f;
			while (!canSpawn)
			{
				bool found = false;
				foreach (AbstractPlayerController allPlayer in PlayerManager.GetAllPlayers())
				{
					if (allPlayer != null && birdSafetyZone.Contains(allPlayer.center))
					{
						found = true;
						safetyTimer += (float)CupheadTime.Delta;
						break;
					}
				}
				if (found && safetyTimer < p.safetyZoneMaxDuration)
				{
					yield return null;
				}
				else
				{
					canSpawn = true;
				}
			}
			FlyingCowboyLevelBird bird = birdPrefab.Spawn(birdStartPosition.position);
			bird.Initialize(birdStartPosition.position, birdEndPosition.position, bulletLandingPositionPattern.PopFloat(), p, this);
			while (bird != null)
			{
				yield return null;
			}
		}
	}

	private void SpawnUFOs()
	{
		LevelProperties.FlyingCowboy.UFOEnemy uFOEnemy = base.properties.CurrentState.uFOEnemy;
		Vector3 pos = new Vector3(740f, uFOEnemy.topUFOVerticalPosition);
		ufo = ufoPrefab.Spawn();
		ufo.Init(pos, base.properties.CurrentState.uFOEnemy, uFOEnemy.UFOHealth);
	}

	public void OnPhase2(LevelProperties.FlyingCowboy.Pattern postTransitionPattern)
	{
		phase2Trigger = true;
		base.animator.SetBool("OnPhase2", value: true);
		StartCoroutine(phase2TransStart_cr(postTransitionPattern));
	}

	private IEnumerator phase2TransStart_cr(LevelProperties.FlyingCowboy.Pattern postTransitionPattern)
	{
		int hash2 = Animator.StringToHash("HideToLowStart");
		int hash3 = Animator.StringToHash("HideToHighStart");
		while (true)
		{
			int hash = base.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
			if (hash == hash2 || hash == hash3)
			{
				break;
			}
			yield return null;
		}
		float previousT = float.MaxValue;
		WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
		while (true)
		{
			yield return waitForEndOfFrame;
			float t = MathUtilities.DecimalPart(base.animator.GetCurrentAnimatorStateInfo(SaloonAnimatorLayer).normalizedTime);
			if (previousT < 1f / 24f && t > 1f / 24f)
			{
				lanternARenderer.enabled = false;
				lanternBRenderer.enabled = true;
				break;
			}
			if (previousT < 13f / 24f && t > 13f / 24f)
			{
				lanternARenderer.enabled = true;
				lanternBRenderer.enabled = false;
				break;
			}
			previousT = t;
		}
		base.animator.Play("Ph1_To_Ph2", 0);
		StopAllCoroutines();
		if (ufo != null)
		{
			ufo.Dead();
		}
		state = State.PhaseTrans;
		StartCoroutine(phase2_trans_cr(postTransitionPattern));
	}

	private IEnumerator phase2_trans_cr(LevelProperties.FlyingCowboy.Pattern postTransitionPattern)
	{
		yield return null;
		yield return base.animator.WaitForNormalizedTime(this, 13f / 15f, "Ph1_To_Ph2");
		SFX_COWGIRL_COWGIRL_WheelConstantLoopStop();
		Vacuum(initial: false, postTransitionPattern);
		yield return base.animator.WaitForNormalizedTime(this, 1f, "Ph1_To_Ph2");
		base.animator.Play("Vacuum", 0);
		base.animator.Play("TransitionSmoke", TransitionSmokeLayer);
		yield return base.animator.WaitForAnimationToEnd(this, "TransitionSmoke", TransitionSmokeLayer);
		endTransitionTrigger = true;
		while (transitionVacuumAttackCoroutine != null)
		{
			yield return null;
		}
		if (postTransitionPattern != LevelProperties.FlyingCowboy.Pattern.Vacuum || phase3Trigger)
		{
			endVacuumPullPlayer();
		}
		endTransitionTrigger = false;
		yield return null;
		yield return null;
		state = State.Idle;
	}

	private IEnumerator moveDown_cr()
	{
		phase2BasePosition = initialSaloonPosition;
		phase2BasePosition.y = -183f;
		Vector3 initialPosition = base.transform.position;
		Vector3 targetPosition = phase2BasePosition;
		float elapsedTime = 0f;
		while (elapsedTime < 2f)
		{
			yield return null;
			elapsedTime += (float)CupheadTime.Delta;
			base.transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / 2f);
		}
	}

	public void Vacuum(bool initial, LevelProperties.FlyingCowboy.Pattern postTransitionPattern = LevelProperties.FlyingCowboy.Pattern.Default)
	{
		base.animator.SetBool("OnRicochet", value: false);
		if (postTransitionPattern != 0)
		{
			transitionVacuumAttackCoroutine = StartCoroutine(vacuum_cr(initial, postTransitionPattern));
		}
		else
		{
			StartCoroutine(vacuum_cr(initial, postTransitionPattern));
		}
	}

	private IEnumerator vacuumCurveShots_cr(bool transition)
	{
		LevelProperties.FlyingCowboy.Debris p = base.properties.CurrentState.debris;
		string[] debrisCurveShotString;
		if (transition)
		{
			PatternString patternString = new PatternString(p.transitionCurveShotString);
			debrisCurveShotString = patternString.PopString().Split(',');
		}
		else
		{
			debrisCurveShotString = debrisCurveString.PopString().Split(',');
		}
		Vector3[] positions = topPositions;
		int spawnIndex = 0;
		float angle = 0f;
		Vector3 root = vacuumDebrisAimTransform.position;
		for (int i = 0; i < debrisCurveShotString.Length; i++)
		{
			string[] spawn = debrisCurveShotString[i].Split(':');
			string[] array = spawn;
			foreach (string text in array)
			{
				if (text == "B")
				{
					positions = bottomCurvePositions;
				}
				else if (text == "T")
				{
					positions = topCurvePositions;
				}
				else
				{
					Parser.IntTryParse(text, out spawnIndex);
				}
			}
			float apexHeight = Mathf.Abs(vacuumDebrisAimTransform.position.x - positions[spawnIndex].x) + 300f;
			float timeToApex = p.debrisCurveApexTime;
			float height = 0f - apexHeight;
			float apexTime2 = timeToApex * timeToApex;
			float g = -2f * height / apexTime2;
			float viX = 2f * height / timeToApex;
			float viY2 = viX * viX;
			float x = root.x - positions[spawnIndex].x;
			float y = root.y - positions[spawnIndex].y;
			float sqrtRooted = viY2 + 2f * g * x;
			float tEnd2 = (0f - viX + Mathf.Sqrt(sqrtRooted)) / g;
			float tEnd3 = (0f - viX - Mathf.Sqrt(sqrtRooted)) / g;
			float tEnd = Mathf.Max(tEnd2, tEnd3);
			float velocityY = y / tEnd;
			FlyingCowboyLevelDebris debris = largeVacuumDebrisPrefabs.GetRandom().Create(positions[spawnIndex], angle * 57.29578f, p.debrisOneSpeedStartEnd.min) as FlyingCowboyLevelDebris;
			debris.GetComponent<SpriteRenderer>().sortingOrder = i;
			bool parryable = debrisParryString.PopLetter() == 'P';
			debris.SetParryable(parryable);
			Vector3 velocity = new Vector3(viX, velocityY);
			debris.ToCurve(velocity, g);
			debris.SetupVacuum(vacuumDebrisAimTransform, vacuumDebrisDisappearTransform);
			allDebris.Add(debris);
			yield return CupheadTime.WaitForSeconds(this, p.debrisDelay);
			if (phase3Trigger || endTransitionTrigger)
			{
				break;
			}
		}
	}

	private IEnumerator vacuum_cr(bool initial, LevelProperties.FlyingCowboy.Pattern postTransitionPattern)
	{
		bool transition = postTransitionPattern != LevelProperties.FlyingCowboy.Pattern.Default;
		if (!initial)
		{
			SFX_COWGIRL_COWGIRL_P2_VacuumSuckLoop();
		}
		if (!transition)
		{
			state = State.Vacuum;
		}
		LevelProperties.FlyingCowboy.Debris p = base.properties.CurrentState.debris;
		if (!initial && !transition)
		{
			startVacuumPullPlayer(immediateFullStrength: false);
		}
		if (!initial && !transition)
		{
			yield return CupheadTime.WaitForSeconds(this, p.warningDelayRange.RandomFloat());
		}
		PatternString debrisTypePattern = new PatternString(p.debrisTypeString);
		StartCoroutine(vacuumCurveShots_cr(transition));
		string[] debrisTop;
		string[] debrisBottom;
		string[] debrisSide;
		if (transition)
		{
			int num = Random.Range(0, base.properties.CurrentState.debris.transitionTopSpawn.Length);
			int num2 = Random.Range(0, base.properties.CurrentState.debris.transitionBottomSpawn.Length);
			int num3 = Random.Range(0, base.properties.CurrentState.debris.transitionSideSpawn.Length);
			debrisTop = p.transitionTopSpawn[num].Split(',');
			debrisBottom = p.transitionBottomSpawn[num2].Split(',');
			debrisSide = p.transitionSideSpawn[num3].Split(',');
		}
		else
		{
			debrisTop = p.debrisTopSpawn[debrisTopMainIndex].Split(',');
			debrisBottom = p.debrisBottomSpawn[debrisBottomMainIndex].Split(',');
			debrisSide = p.debrisSideSpawn[debrisSideMainIndex].Split(',');
		}
		int debrisTopCount = 0;
		int debrisBottomCount = 0;
		int debrisSideCount = 0;
		int maxLength = Mathf.Max(debrisTop.Length, debrisBottom.Length, debrisSide.Length);
		if (transition && postTransitionPattern == LevelProperties.FlyingCowboy.Pattern.Ricochet)
		{
			vacuumSizeCoroutine = StartCoroutine(growVacuum_cr());
		}
		for (int i = 0; i < maxLength; i++)
		{
			int posIndex;
			if (i < debrisTop.Length)
			{
				Parser.IntTryParse(debrisTop[debrisTopCount], out posIndex);
				createLinearDebris(topPositions[posIndex], debrisTypePattern.PopInt(), i);
				debrisTopCount++;
			}
			if (i < debrisBottom.Length)
			{
				Parser.IntTryParse(debrisBottom[debrisBottomCount], out posIndex);
				createLinearDebris(bottomPositions[posIndex], debrisTypePattern.PopInt(), i);
				debrisBottomCount++;
			}
			if (i < debrisSide.Length)
			{
				Parser.IntTryParse(debrisSide[debrisSideCount], out posIndex);
				createLinearDebris(sidePositions[posIndex], debrisTypePattern.PopInt(), i);
				debrisSideCount++;
			}
			yield return CupheadTime.WaitForSeconds(this, p.debrisDelay);
			if (phase3Trigger || endTransitionTrigger)
			{
				break;
			}
		}
		if (transition && postTransitionPattern == LevelProperties.FlyingCowboy.Pattern.Vacuum && !phase3Trigger)
		{
			allDebris.Clear();
		}
		else
		{
			if (!transition)
			{
				vacuumSizeCoroutine = StartCoroutine(growVacuum_cr());
			}
			bool allDebrisGone = false;
			while (!allDebrisGone)
			{
				allDebrisGone = true;
				for (int j = 0; j < allDebris.Count; j++)
				{
					if (allDebris[j] != null && !allDebris[j].dead)
					{
						allDebrisGone = false;
					}
				}
				yield return null;
			}
			allDebris.Clear();
			while (vacuumSizeCoroutine != null)
			{
				yield return null;
			}
		}
		if (!transition || (transition && (postTransitionPattern == LevelProperties.FlyingCowboy.Pattern.Ricochet || phase3Trigger)))
		{
			endVacuumPullPlayer();
			SFX_COWGIRL_COWGIRL_P2_VacuumSuckLoopStop();
		}
		if (transition)
		{
			transitionVacuumAttackCoroutine = null;
			if (phase3Trigger)
			{
				SFX_COWGIRL_COWGIRL_P2_VacuumSuckLoopStop();
				base.animator.SetBool("OnPhase3", value: true);
			}
			yield break;
		}
		SFX_COWGIRL_COWGIRL_P2_VacuumSuckLoopStop();
		debrisTopMainIndex = (debrisTopMainIndex + 1) % p.debrisTopSpawn.Length;
		debrisBottomMainIndex = (debrisBottomMainIndex + 1) % p.debrisBottomSpawn.Length;
		debrisSideMainIndex = (debrisSideMainIndex + 1) % p.debrisSideSpawn.Length;
		bool manualDeathHandling = true;
		if (phase3Trigger)
		{
			manualDeathHandling = false;
			base.animator.SetBool("OnPhase3", value: true);
		}
		else
		{
			base.animator.SetBool("OnRicochet", value: true);
			yield return CupheadTime.WaitForSeconds(this, p.hesitate);
		}
		state = State.Idle;
		if (phase3Trigger && manualDeathHandling)
		{
			Ricochet();
		}
	}

	private void createLinearDebris(Vector3 rootPosition, int type, int sortingIndex)
	{
		LevelProperties.FlyingCowboy.Debris debris = base.properties.CurrentState.debris;
		MinMax minMax;
		FlyingCowboyLevelDebris random;
		switch (type)
		{
		case 1:
			minMax = debris.debrisOneSpeedStartEnd;
			random = largeVacuumDebrisPrefabs.GetRandom();
			break;
		case 2:
			minMax = debris.debrisTwoSpeedStartEnd;
			random = mediumVacuumDebrisPrefabs.GetRandom();
			break;
		default:
			minMax = debris.debrisThreeSpeedStartEnd;
			random = smallVacuumDebrisPrefabs.GetRandom();
			break;
		}
		Vector3 vector = vacuumDebrisAimTransform.position - rootPosition;
		FlyingCowboyLevelDebris flyingCowboyLevelDebris = random.Create(rootPosition, MathUtils.DirectionToAngle(vector), minMax.min) as FlyingCowboyLevelDebris;
		flyingCowboyLevelDebris.GetComponent<SpriteRenderer>().sortingOrder = 50 * type + sortingIndex;
		bool parryable = debrisParryString.PopLetter() == 'P';
		flyingCowboyLevelDebris.SetParryable(parryable);
		flyingCowboyLevelDebris.SetupLinearSpeed(minMax, debris.debrisSpeedUpDistance, vacuumDebrisAimTransform);
		flyingCowboyLevelDebris.SetupVacuum(vacuumDebrisAimTransform, vacuumDebrisDisappearTransform);
		allDebris.Add(flyingCowboyLevelDebris);
	}

	private void startVacuumPullPlayer(bool immediateFullStrength)
	{
		endVacuumPullPlayer();
		LevelProperties.FlyingCowboy.Debris debris = base.properties.CurrentState.debris;
		foreach (PlanePlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (!(allPlayer == null))
			{
				VacuumForce force = new VacuumForce(allPlayer, vacuumDebrisDisappearTransform, debris.vacuumWindStrength * 0.5f, (!immediateFullStrength) ? debris.vacuumTimeToFullStrength : 0f);
				allPlayer.motor.AddForce(force);
				if (allPlayer.id == PlayerId.PlayerOne)
				{
					forcePlayer1 = force;
				}
				else if (allPlayer.id == PlayerId.PlayerTwo)
				{
					forcePlayer2 = force;
				}
			}
		}
	}

	private void endVacuumPullPlayer()
	{
		foreach (PlanePlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (!(allPlayer == null) && !(allPlayer.motor == null))
			{
				allPlayer.motor.RemoveForce(forcePlayer1);
				allPlayer.motor.RemoveForce(forcePlayer2);
			}
		}
		forcePlayer1 = null;
		forcePlayer2 = null;
	}

	private IEnumerator growVacuum_cr()
	{
		int hash = Animator.StringToHash("Vacuum");
		float previousTime2 = float.MaxValue;
		while (true)
		{
			float normalizedTime2 = MathUtilities.DecimalPart(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
			if (previousTime2 < 5f / 12f && normalizedTime2 >= 5f / 12f)
			{
				break;
			}
			previousTime2 = normalizedTime2;
			yield return null;
		}
		setVacuumTransition();
		previousTime2 = float.MaxValue;
		while (true)
		{
			float normalizedTime = MathUtilities.DecimalPart(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
			if (previousTime2 < 7f / 24f && normalizedTime >= 7f / 24f)
			{
				break;
			}
			previousTime2 = normalizedTime;
			yield return null;
		}
		setVacuumBig();
		vacuumSizeCoroutine = null;
	}

	public void Ricochet()
	{
		base.animator.SetBool("OnRicochet", value: true);
		StartCoroutine(ricochet_cr());
	}

	private IEnumerator ricochet_cr()
	{
		nextSafeShoot = 1;
		SFX_COWGIRL_COWGIRL_P2_StirrupWheelsLoopStart();
		state = State.Ricochet;
		LevelProperties.FlyingCowboy.Ricochet p = base.properties.CurrentState.ricochet;
		setVacuumBig();
		yield return base.animator.WaitForAnimationToStart(this, "Ricochet");
		vacuumSizeCoroutine = StartCoroutine(shrinkVacuum_cr());
		base.transform.position = phase2BasePosition;
		float elapsedTime = 0f;
		PatternString delayPattern = new PatternString(p.rainDelayString);
		PatternString bulletTypePattern = new PatternString(p.rainTypeString);
		PatternString xPositionPattern = new PatternString(p.rainSpawnString);
		PatternString speedPattern = new PatternString(p.rainSpeedString);
		float delayTime;
		for (; elapsedTime < p.rainDuration && (!phase3Trigger || !(elapsedTime >= 2f)); elapsedTime += delayTime)
		{
			delayTime = delayPattern.PopFloat();
			yield return CupheadTime.WaitForSeconds(this, delayTime);
			FlyingCowboyLevelRicochetDebris.BulletType bulletType = FlyingCowboyLevelRicochetDebris.BulletType.Nothing;
			if (bulletTypePattern.PopLetter() == 'R')
			{
				bulletType = FlyingCowboyLevelRicochetDebris.BulletType.Ricochet;
			}
			float xPosition = xPositionPattern.PopFloat();
			float speed = speedPattern.PopFloat();
			ricochetPrefab.Create(new Vector3(0f - xPosition, 430f), speed, p.splitBulletSpeed, bulletType, bulletType != 0 && ricochetParryString.PopLetter() == 'P');
		}
		base.animator.SetBool("OnRicochet", value: false);
		SFX_COWGIRL_COWGIRL_P2_StirrupWheelsLoopStop();
		if (phase3Trigger)
		{
			if (vacuumSizeCoroutine != null)
			{
				StopCoroutine(vacuumSizeCoroutine);
				vacuumSizeCoroutine = null;
				setVacuumRegular();
			}
			base.animator.SetBool("OnPhase3", value: true);
		}
		else
		{
			yield return CupheadTime.WaitForSeconds(this, p.rainRecoveryTime);
		}
		if (phase3Trigger)
		{
			base.animator.SetBool("OnPhase3", value: true);
		}
		state = State.Idle;
	}

	private void animationEvent_SafeShoot(int eventType)
	{
		switch (eventType)
		{
		case 0:
			if (nextSafeShoot == 0)
			{
				AbstractProjectile abstractProjectile2 = ricochetUpPrefab.Create(ricochetUpSpawnPoint.position);
				abstractProjectile2.animator.Play("A");
				abstractProjectile2.animator.Update(0f);
				nextSafeShoot = 1;
			}
			else if (nextSafeShoot == 2)
			{
				AbstractProjectile abstractProjectile3 = ricochetUpPrefab.Create(ricochetUpSpawnPoint.position);
				abstractProjectile3.animator.Play("C");
				abstractProjectile3.animator.Update(0f);
				nextSafeShoot = 1;
			}
			break;
		case 1:
			if (nextSafeShoot == 1)
			{
				AbstractProjectile abstractProjectile = ricochetUpPrefab.Create(ricochetUpSpawnPoint.position);
				abstractProjectile.animator.Play("B");
				abstractProjectile.animator.Update(0f);
				nextSafeShoot = ((!Rand.Bool()) ? 2 : 0);
				shootCoins();
			}
			break;
		}
	}

	private void shootCoins()
	{
		LevelProperties.FlyingCowboy.Ricochet ricochet = base.properties.CurrentState.ricochet;
		int num = ricochet.coinCountRange.RandomInt();
		for (int i = 0; i < num; i++)
		{
			Vector2 vector = new Vector2(ricochet.coinSpeedXRange.RandomFloat(), KinematicUtilities.CalculateInitialSpeedToReachApex(ricochet.coinHeightRange.RandomFloat(), ricochet.coinGravity));
			float rotation = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			BasicProjectile basicProjectile = coinProjectile.Create(ricochetUpSpawnPoint.transform.position, rotation, vector.magnitude);
			basicProjectile.Gravity = ricochet.coinGravity;
			basicProjectile.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;
		}
	}

	private IEnumerator shrinkVacuum_cr()
	{
		yield return base.animator.WaitForNormalizedTime(this, 2f, "Ricochet");
		setVacuumTransition();
		yield return base.animator.WaitForNormalizedTime(this, 2.9375f, "Ricochet");
		setVacuumRegular();
		vacuumSizeCoroutine = null;
	}

	private void setVacuumRegular()
	{
		SpriteRenderer spriteRenderer = regularVacuumRenderer;
		bool flag = true;
		regularHoseRenderer.enabled = flag;
		spriteRenderer.enabled = flag;
		SpriteRenderer spriteRenderer2 = transitionVacuumRenderer;
		flag = false;
		bigHoseRenderer.enabled = flag;
		flag = flag;
		bigVacuumRenderer.enabled = flag;
		flag = flag;
		transitionHoseRenderer.enabled = flag;
		spriteRenderer2.enabled = flag;
	}

	private void setVacuumTransition()
	{
		SpriteRenderer spriteRenderer = transitionVacuumRenderer;
		bool flag = true;
		transitionHoseRenderer.enabled = flag;
		spriteRenderer.enabled = flag;
		SpriteRenderer spriteRenderer2 = regularVacuumRenderer;
		flag = false;
		bigHoseRenderer.enabled = flag;
		flag = flag;
		bigVacuumRenderer.enabled = flag;
		flag = flag;
		regularHoseRenderer.enabled = flag;
		spriteRenderer2.enabled = flag;
	}

	private void setVacuumBig()
	{
		SpriteRenderer spriteRenderer = bigVacuumRenderer;
		bool flag = true;
		bigHoseRenderer.enabled = flag;
		spriteRenderer.enabled = flag;
		SpriteRenderer spriteRenderer2 = regularVacuumRenderer;
		flag = false;
		transitionHoseRenderer.enabled = flag;
		flag = flag;
		transitionVacuumRenderer.enabled = flag;
		flag = flag;
		regularHoseRenderer.enabled = flag;
		spriteRenderer2.enabled = flag;
	}

	public void Death()
	{
		StartCoroutine(phase3_cr());
	}

	private IEnumerator phase3_cr()
	{
		phase3Trigger = true;
		yield return base.animator.WaitForAnimationToEnd(this, "Ph2_To_Ph3");
		StopAllCoroutines();
		Object.Destroy(base.gameObject);
	}

	private void aniEvent_SpawnMeat()
	{
		IsDead = true;
	}

	private void animationEvent_PosterFlyAway()
	{
		if (!posterFlyAwayTriggered)
		{
			base.animator.Play("FlyAway", PosterAnimatorLayer);
			posterRenderer.sortingLayerName = "Effects";
		}
		posterFlyAwayTriggered = true;
	}

	private void animationEvent_DisablePhase1Saloon()
	{
		SpriteRenderer[] array = saloonTransitionDisableRenderers;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.enabled = false;
		}
	}

	private void animationEvent_DisableSaloonCollider()
	{
		saloonCollidersParent.SetActive(value: false);
	}

	private void animationEvent_EnablePlayerVacuumForce()
	{
		startVacuumPullPlayer(immediateFullStrength: false);
	}

	private void animationEvent_DisableFrontSaloonWheel()
	{
		frontWheelRenderer.enabled = false;
	}

	private void animationEvent_DisableBackSaloonWheel()
	{
		backWheelRenderer.enabled = false;
	}

	private void animationEvent_TurnOffPhase1Animators()
	{
		base.animator.Play("Off", SaloonAnimatorLayer);
		base.animator.Play("Off", PosterAnimatorLayer);
		base.animator.Play("Off", DoorsAnimatorLayer);
		base.animator.Play("Off", WheelSmokeAnimatorLayer);
	}

	private void animationEvent_MoveCowgirlDown()
	{
		StartCoroutine(moveDown_cr());
	}

	private void animationEvent_SwapPhase2Puffs()
	{
		phase2PuffARenderer.enabled = phase2PuffBRenderer.enabled;
		phase2PuffBRenderer.enabled = !phase2PuffARenderer.enabled;
	}

	private void onFadeOutStartEvent(float time)
	{
		StartCoroutine(introBird_cr(0.25f));
	}

	private void onPlayerJoinedEvent(PlayerId playerId)
	{
		if (forcePlayer1 != null || forcePlayer2 != null)
		{
			startVacuumPullPlayer(immediateFullStrength: true);
		}
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.color = Color.green;
		float num = (vacuumSpawnTop.position.y - vacuumSpawnBottom.position.y) / 5f;
		for (int i = 0; i < 6; i++)
		{
			float x = vacuumSpawnBottom.position.x;
			float y = ((i != 5) ? (vacuumSpawnBottom.position.y + num * (float)i) : vacuumSpawnTop.position.y);
			Gizmos.DrawWireSphere(new Vector3(x, y), 10f);
		}
		Gizmos.color = Color.yellow;
		float num2 = ((float)debrisSpawnHorizontalSpacing - vacuumSpawnTop.position.x) / 6f;
		for (int j = 0; j < 6; j++)
		{
			float x2 = vacuumSpawnTop.position.x + num2 + num2 * (float)j;
			float y2 = vacuumSpawnTop.position.y;
			Gizmos.DrawWireSphere(new Vector3(x2, y2), 10f);
		}
		Gizmos.color = Color.yellow;
		num2 = ((float)debrisSpawnHorizontalSpacing - vacuumSpawnBottom.position.x) / 6f;
		for (int k = 0; k < 6; k++)
		{
			float x3 = vacuumSpawnBottom.position.x + num2 + num2 * (float)k;
			float y3 = vacuumSpawnBottom.position.y;
			Gizmos.DrawWireSphere(new Vector3(x3, y3), 10f);
		}
		Gizmos.color = Color.red;
		for (int l = 0; l < 4; l++)
		{
			float x4 = vacuumSpawnTop.position.x + num2 + num2 * (float)(6 + l);
			float y4 = vacuumSpawnTop.position.y;
			Gizmos.DrawWireSphere(new Vector3(x4, y4), 10f);
		}
		Gizmos.color = Color.red;
		for (int m = 0; m < 4; m++)
		{
			float x5 = vacuumSpawnBottom.position.x + num2 + num2 * (float)(6 + m);
			float y5 = vacuumSpawnBottom.position.y;
			Gizmos.DrawWireSphere(new Vector3(x5, y5), 10f);
		}
	}

	private void AnimationEvent_SFX_COWGIRL_Vocal_Laugh()
	{
		AudioManager.Play("sfx_dlc_cowgirl_vocal_laugh");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_vocal_laugh");
	}

	private void AnimationEvent_SFX_COWGIRL_Vocal_MooHa()
	{
		AudioManager.Play("sfx_dlc_cowgirl_vocal_mooha");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_vocal_mooha");
	}

	private void AnimationEvent_SFX_COWGIRL_Vocal_Surprised()
	{
		AudioManager.Play("sfx_dlc_cowgirl_vocal_surprised");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_vocal_surprised");
	}

	private void AnimationEvent_SFX_COWGIRL_Vocal_YeeHaw()
	{
		AudioManager.Play("sfx_dlc_cowgirl_vocal_yeehaw");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_vocal_yeehaw");
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_JugGunRaise()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p1_snakeoilattack_juggun_raise");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_snowmonster_death_stompoffscreen");
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_JugGunHolster()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p1_snakeoilattack_juggun_holster");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p1_snakeoilattack_juggun_holster");
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_JugGunBlow()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p1_snakeoilattack_juggun_blow");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p1_snakeoilattack_juggun_blow");
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_JugGunBlowAndHolster()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p1_snakeoilattack_juggun_blowandholster");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p1_snakeoilattack_juggun_blowandholster");
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_JugGunBlast()
	{
		AudioManager.Stop("sfx_dlc_cowgirl_p1_snakeoilattack_juggun_spin_loop");
		AudioManager.Play("sfx_dlc_cowgirl_p1_snakeoilattack_juggunblast");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p1_snakeoilattack_juggunblast");
	}

	private void SFX_COWGIRL_COWGIRL_JugGunSpinLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_cowgirl_p1_snakeoilattack_juggun_spin_loop");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p1_snakeoilattack_juggun_spin_loop");
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_P1toP2VacuumSuckup()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p1_death_saloon_vacuumsuckup");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p1_death_saloon_vacuumsuckup");
	}

	private void SFX_COWGIRL_COWGIRL_LassoSpinLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_cowgirl_p1_lasso_spin_loop");
		AudioManager.FadeSFXVolume("sfx_dlc_cowgirl_p1_lasso_spin_loop", 0.7f, 0.01f);
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p1_lasso_spin_loop");
	}

	private void SFX_COWGIRL_COWGIRL_LassoSpinLoopStop()
	{
		AudioManager.FadeSFXVolume("sfx_dlc_cowgirl_p1_lasso_spin_loop", 0f, 0.2f);
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_LassoThrowCatchRelease()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p1_lasso_throw_catch_release");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p1_lasso_throw_catch_release");
	}

	private void SFX_COWGIRL_COWGIRL_WheelConstantLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_cowgirl_p1_saloon_wheelsconstant_loop");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p1_saloon_wheelsconstant_loop");
	}

	private void SFX_COWGIRL_COWGIRL_WheelConstantLoopStop()
	{
		AudioManager.Stop("sfx_dlc_cowgirl_p1_saloon_wheelsconstant_loop");
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_PositionLowtoHigh()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p1_saloon_positionchange_lowtohigh");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p1_saloon_positionchange_lowtohigh");
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_PositionHightoLow()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p1_saloon_positionchange_hightolow");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p1_saloon_positionchange_hightolow");
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_P2_Stirrups()
	{
		AudioManager.Play("sfx_DLC_Cowgirl_Stirrups");
		emitAudioFromObject.Add("sfx_DLC_Cowgirl_Stirrups");
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_P2_VacuumBlowback()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p2_vacuum_blowback");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p2_vacuum_blowback");
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_P2_VacuumCrouchPosition()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p2_vacuum_blowback_crouchposition");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p2_vacuum_blowback_crouchposition");
	}

	private void SFX_COWGIRL_COWGIRL_P2_VacuumSuckLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_cowgirl_p2_vacuum_constantsuck_loop");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p2_vacuum_constantsuck_loop");
		AudioManager.FadeSFXVolume("sfx_dlc_cowgirl_p2_vacuum_constantsuck_loop", 0f, 1f, 1f);
	}

	private void SFX_COWGIRL_COWGIRL_P2_VacuumSuckLoopStop()
	{
		AudioManager.FadeSFXVolume("sfx_dlc_cowgirl_p2_vacuum_constantsuck_loop", 0f, 1f);
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_P2_VacuumSuckIn()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p2_vacuum_suckin");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p2_vacuum_suckin");
	}

	private void AnimationEvent_SFX_COWGIRL_COWGIRL_P2_VacuumeExplosionDeath()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p2_death_vacuumexplosion_transition");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p2_death_vacuumexplosion_transition");
	}

	private void SFX_COWGIRL_COWGIRL_P2_StirrupWheelsLoopStart()
	{
		AudioManager.PlayLoop("sfx_dlc_cowgirl_stirrupswheels_loop");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_stirrupswheels_loop");
	}

	private void SFX_COWGIRL_COWGIRL_P2_StirrupWheelsLoopStop()
	{
		AudioManager.Stop("sfx_dlc_cowgirl_stirrupswheels_loop");
	}
}
