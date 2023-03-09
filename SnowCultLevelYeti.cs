using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowCultLevelYeti : LevelProperties.SnowCult.Entity
{
	public enum States
	{
		Intro,
		Idle,
		Move,
		IcePillar,
		Sled,
		Snowball
	}

	public States state;

	private States previousState;

	private const float BURST_SPAWN_X = 290f;

	private const float Y_TO_SPAWN = 95f;

	private const float Y_ICE_PILLAR_SPAWN = -142f;

	private const float POS_OFFSET_X = 147f;

	private const float JUMP_LANDING_OFFSET_X = 247f;

	private const float REFORM_TIME = 23f / 24f;

	private const float BALL_RADIUS = 180f;

	private Vector3 BALL_JUMP_OFFSET = new Vector3(50f, -100f);

	private Vector3 BALL_DASH_OFFSET = new Vector3(50f, -180f);

	[SerializeField]
	private SnowCultHandleBackground snowCultBGHandler;

	[SerializeField]
	private Transform yetiMidPoint;

	[SerializeField]
	private Transform yetiSpawnPoint;

	[SerializeField]
	private SnowCultLevelIcePillar icePillarPrefab;

	[SerializeField]
	private SnowCultLevelBat batPrefab;

	[SerializeField]
	private Effect batSpawnEffectPrefab;

	private bool batSoundLong;

	[SerializeField]
	private SnowCultLevelBurstEffect snowBurstA;

	[SerializeField]
	private SnowCultLevelBurstEffect snowFallA;

	[Header("Snowballs")]
	[SerializeField]
	private SnowCultLevelSnowball smallSnowballPrefab;

	[SerializeField]
	private SnowCultLevelSnowball mediumSnowballPrefab;

	[SerializeField]
	private SnowCultLevelSnowball largeSnowballPrefab;

	[SerializeField]
	private GameObject cubeLaunchPosition;

	[SerializeField]
	private GameObject ball;

	[SerializeField]
	private Animator[] meltFXAnimator;

	[SerializeField]
	private GameObject dashGroundFX;

	[SerializeField]
	private GameObject groundMask;

	[SerializeField]
	private SpriteRenderer ballShadow;

	[SerializeField]
	private GameObject introShadow;

	[SerializeField]
	private Sprite[] shadowSprites;

	private int offsetCoordIndex;

	private int snowballMainIndex;

	private string[] patternString;

	private int patternStringIndex;

	private PatternString batAttackPositionString;

	private PatternString batAttackHeightString;

	private PatternString batAttackWidthString;

	private PatternString batAttackSideString;

	private PatternString batAttackInterDelayString;

	private PatternString batArcModifierString;

	private PatternString batParryableString;

	[SerializeField]
	private Transform[] batAttackPositions;

	private float xScale;

	private bool onLeft;

	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private Collider2D coll;

	[SerializeField]
	private GameObject legs;

	[SerializeField]
	private GameObject bucket;

	private SnowCultLevelBat[] bats;

	private List<SnowCultLevelBat> batCirclingList = new List<SnowCultLevelBat>();

	private float batLaunchTimer;

	private bool batColor;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private DamageReceiver ballDamageReceiver;

	private bool fridgeCanShoot;

	public bool introRibcageClosed;

	private bool forceOutroToStart;

	private int iceCubeStartFrame;

	private int iceCubeExplosionCounterMedium;

	private int iceCubeExplosionCounterSmall;

	private int idleAnimFullPathHash;

	public bool inBallForm { get; private set; }

	public event Action OnDeathEvent;

	protected override void Awake()
	{
		base.Awake();
		xScale = 1f;
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		ballDamageReceiver = ball.GetComponent<DamageReceiver>();
		ballDamageReceiver.OnDamageTaken += OnDamageTaken;
		idleAnimFullPathHash = Animator.StringToHash(base.animator.GetLayerName(0) + ".Idle");
		if (Level.Current.mode != 0)
		{
			InitBats();
		}
		else
		{
			base.properties.OnBossDeath += OnBossDeath;
		}
		ball.GetComponent<CollisionChild>().OnPlayerCollision += OnCollisionPlayer;
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private bool InIdleAnim()
	{
		return base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == idleAnimFullPathHash;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (Level.Current.mode != 0 || base.properties.CurrentState.stateName != LevelProperties.SnowCult.States.EasyYeti || InIdleAnim() || !(info.damage >= base.properties.CurrentHealth))
		{
			base.properties.DealDamage(info.damage);
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public override void LevelInit(LevelProperties.SnowCult properties)
	{
		base.LevelInit(properties);
		offsetCoordIndex = UnityEngine.Random.Range(0, properties.CurrentState.icePillar.offsetCoordString.Split(',').Length);
		snowballMainIndex = UnityEngine.Random.Range(0, properties.CurrentState.snowball.snowballTypeString.Length);
	}

	public void StartOnLeft(Vector3 reflectionPoint)
	{
		yetiSpawnPoint.position = new Vector3(reflectionPoint.x + (reflectionPoint.x - yetiSpawnPoint.position.x), yetiSpawnPoint.position.y);
		base.transform.localScale = new Vector3(-1f, 1f);
		xScale = base.transform.localScale.x;
		onLeft = true;
	}

	public void StartYeti()
	{
		StartCoroutine(intro_cr());
		if (Level.Current.mode != 0)
		{
			StartCoroutine(bats_attack_cr());
		}
	}

	private void SetState(States s)
	{
		previousState = state;
		state = s;
	}

	private IEnumerator intro_cr()
	{
		state = States.Intro;
		introRibcageClosed = false;
		base.transform.position = Vector3.zero;
		base.transform.position = yetiSpawnPoint.position;
		base.transform.position += Vector3.up * 300f;
		float t = 0f;
		sprite = GetComponent<SpriteRenderer>();
		base.animator.Play("Intro", 0, 0f);
		YieldInstruction wait = new WaitForFixedUpdate();
		while (base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash(base.animator.GetLayerName(0) + ".Intro"))
		{
			if (t < 0.34f)
			{
				base.transform.position = new Vector3(base.transform.position.x, EaseUtils.EaseOutBack(yetiSpawnPoint.position.y + 300f, yetiSpawnPoint.position.y, t * 3f));
				t += CupheadTime.FixedDelta;
			}
			introShadow.transform.position = new Vector3(base.transform.position.x, -25f);
			yield return wait;
		}
		SetState(States.Idle);
		StartCoroutine(do_patterns_cr());
		yield return null;
	}

	private void ShakeScreenInIntro()
	{
		CupheadLevelCamera.Current.Shake(30f, 0.7f);
		((SnowCultLevel)Level.Current).YetiHitGround();
	}

	public void RibcageClosedAroundWizard()
	{
		introRibcageClosed = true;
	}

	private void FlipSprite()
	{
		base.transform.SetScale((!onLeft) ? xScale : (0f - xScale));
	}

	private IEnumerator do_patterns_cr()
	{
		LevelProperties.SnowCult.Yeti p = base.properties.CurrentState.yeti;
		patternString = p.yetiPatternString.Split(',');
		patternStringIndex = UnityEngine.Random.Range(0, patternString.Length);
		while (!forceOutroToStart)
		{
			switch (patternString[patternStringIndex])
			{
			case "S":
				StartCoroutine(start_dash_cr());
				break;
			case "J":
				StartCoroutine(start_jump_cr());
				break;
			case "L":
				Snowball();
				break;
			case "P":
				StartIcePillar();
				break;
			default:
				Snowball();
				break;
			}
			while (state != States.Idle)
			{
				yield return null;
			}
			patternStringIndex = (patternStringIndex + 1) % patternString.Length;
			yield return null;
		}
	}

	private string PeekNextPattern()
	{
		if (forceOutroToStart || Level.Current.mode == Level.Mode.Easy)
		{
			return "I";
		}
		switch (patternString[(patternStringIndex + 1) % patternString.Length])
		{
		case "S":
		case "J":
			return patternString[(patternStringIndex + 1) % patternString.Length];
		case "L":
		case "P":
			return "I";
		default:
			return null;
		}
	}

	private IEnumerator cue_reform_effect_cr(float delayTime, float position, string clipName)
	{
		yield return CupheadTime.WaitForSeconds(this, delayTime - 23f / 24f);
		meltFXAnimator[1].gameObject.transform.SetPosition(position);
		meltFXAnimator[1].gameObject.transform.localScale = new Vector3(base.transform.localScale.x * -1f, 1f);
		meltFXAnimator[1].gameObject.SetActive(value: true);
		meltFXAnimator[1].Play(clipName);
	}

	private IEnumerator start_dash_cr()
	{
		inBallForm = true;
		float PRE_DASH_TIME = 0.25f;
		float DASH_TIME = 0.375f;
		LevelProperties.SnowCult.Yeti p = base.properties.CurrentState.yeti;
		float start = ((!onLeft) ? 493f : (-493f));
		float end = ((!onLeft) ? (-493f) : 493f);
		start += ball.transform.localPosition.x * (0f - base.transform.localScale.x) * 2f;
		float t = 0f;
		float time = p.slideTime;
		if (previousState != States.Move || Level.Current.mode == Level.Mode.Easy)
		{
			base.animator.Play("IdleToDash");
		}
		SetState(States.Move);
		YieldInstruction wait = new WaitForFixedUpdate();
		yield return base.animator.WaitForAnimationToStart(this, "PreDash");
		StartCoroutine(cue_reform_effect_cr(PRE_DASH_TIME + p.slideWarning + DASH_TIME + time, end, "DashReformEffect"));
		yield return base.animator.WaitForAnimationToEnd(this, "PreDash");
		yield return CupheadTime.WaitForSeconds(this, p.slideWarning);
		base.animator.Play("Dash");
		yield return base.animator.WaitForAnimationToEnd(this, "Dash");
		meltFXAnimator[0].transform.position = base.transform.position;
		meltFXAnimator[0].transform.localScale = base.transform.localScale;
		meltFXAnimator[0].gameObject.SetActive(value: true);
		meltFXAnimator[0].Play("DashMeltEffect");
		meltFXAnimator[0].transform.parent = null;
		yield return null;
		base.transform.SetPosition(start);
		ball.transform.localPosition = BALL_DASH_OFFSET;
		ball.SetActive(value: true);
		dashGroundFX.SetActive(value: true);
		groundMask.SetActive(value: true);
		sprite.enabled = false;
		coll.enabled = false;
		base.animator.Play("DashBall", 1, 0f);
		while (t < time)
		{
			if (t < time - 23f / 24f && t + CupheadTime.FixedDelta >= time - 23f / 24f)
			{
				meltFXAnimator[1].gameObject.transform.SetPosition(end);
				meltFXAnimator[1].gameObject.transform.localScale = new Vector3(base.transform.localScale.x * -1f, 1f);
				meltFXAnimator[1].gameObject.SetActive(value: true);
				meltFXAnimator[1].Play("DashReformEffect");
			}
			t += CupheadTime.FixedDelta;
			base.transform.SetPosition(Mathf.Lerp(start, end, t / time));
			yield return wait;
		}
		onLeft = !onLeft;
		FlipSprite();
		sprite.enabled = true;
		coll.enabled = true;
		ball.SetActive(value: false);
		dashGroundFX.SetActive(value: false);
		groundMask.SetActive(value: false);
		meltFXAnimator[1].gameObject.SetActive(value: false);
		switch (PeekNextPattern())
		{
		case "I":
			base.animator.Play("DashToIdle");
			yield return base.animator.WaitForAnimationToEnd(this, "DashToIdle");
			break;
		case "S":
			base.animator.Play("DashToDash");
			yield return base.animator.WaitForAnimationToEnd(this, "DashToDash");
			break;
		case "J":
			base.animator.Play("DashToJump");
			yield return base.animator.WaitForAnimationToEnd(this, "DashToJump");
			break;
		}
		if (PeekNextPattern() == "I")
		{
			yield return CupheadTime.WaitForSeconds(this, (!forceOutroToStart) ? p.hesitate : 0f);
		}
		SetState(States.Idle);
		inBallForm = false;
	}

	private IEnumerator start_jump_cr()
	{
		inBallForm = true;
		LevelProperties.SnowCult.Yeti p = base.properties.CurrentState.yeti;
		float PRE_JUMP_TIME = 5f / 24f;
		float JUMP_TIME = 0.25f;
		float endArcPosX = ((!onLeft) ? (-393f) : 393f);
		float reformPosX = ((!onLeft) ? (-493f) : 493f);
		float xDistance = endArcPosX - base.transform.position.x;
		float ground = base.transform.position.y;
		float timeToApex = p.jumpApexTime;
		float height = p.jumpApexHeight;
		float apexTime2 = timeToApex * timeToApex;
		float g = -2f * height / apexTime2;
		float viY = 2f * height / timeToApex;
		float viX2 = viY * viY;
		float sqrtRooted = viX2 + 2f * g * ground;
		float tEnd2 = (0f - viY + Mathf.Sqrt(sqrtRooted)) / g;
		float tEnd3 = (0f - viY - Mathf.Sqrt(sqrtRooted)) / g;
		float tEnd = Mathf.Max(tEnd2, tEnd3);
		float velocityX = xDistance / tEnd;
		Vector3 speed = new Vector3(velocityX, viY);
		float t = 0f;
		if (previousState != States.Move || Level.Current.mode == Level.Mode.Easy)
		{
			base.animator.Play("IdleToJump");
		}
		SetState(States.Move);
		yield return base.animator.WaitForAnimationToStart(this, "PreJump");
		StartCoroutine(cue_reform_effect_cr(PRE_JUMP_TIME + p.jumpWarning + JUMP_TIME + tEnd, reformPosX, "JumpReformEffect"));
		yield return base.animator.WaitForAnimationToEnd(this, "PreJump");
		yield return CupheadTime.WaitForSeconds(this, p.jumpWarning);
		base.animator.Play("Jump");
		ball.transform.localPosition = BALL_JUMP_OFFSET;
		yield return base.animator.WaitForAnimationToEnd(this, "Jump");
		meltFXAnimator[0].transform.position = base.transform.position;
		meltFXAnimator[0].transform.localScale = base.transform.localScale;
		meltFXAnimator[0].gameObject.SetActive(value: true);
		meltFXAnimator[0].Play("JumpMeltEffect");
		meltFXAnimator[0].transform.parent = null;
		base.transform.position += Vector3.right * (ball.transform.localPosition.x * (0f - base.transform.localScale.x) * 2f);
		ball.SetActive(value: true);
		ballShadow.sprite = shadowSprites[0];
		ballShadow.enabled = true;
		sprite.enabled = false;
		coll.enabled = false;
		base.animator.Play("JumpBall", 1, 0f);
		bool stillMoving = true;
		while (stillMoving)
		{
			speed += new Vector3(0f, g * CupheadTime.FixedDelta);
			base.transform.Translate(speed * CupheadTime.FixedDelta);
			yield return new WaitForFixedUpdate();
			ballShadow.transform.SetPosition(ball.transform.position.x, ground - 145f);
			ballShadow.sprite = shadowSprites[Mathf.Clamp((int)((base.transform.position.y - ground) / height * (float)shadowSprites.Length), 0, shadowSprites.Length - 1)];
			t += CupheadTime.FixedDelta;
			if (t > timeToApex && base.transform.position.y <= ground)
			{
				stillMoving = false;
			}
		}
		base.transform.SetPosition(reformPosX, ground);
		base.transform.SetEulerAngles(null, null, 0f);
		onLeft = !onLeft;
		FlipSprite();
		sprite.enabled = true;
		coll.enabled = true;
		ballShadow.enabled = false;
		ball.SetActive(value: false);
		meltFXAnimator[1].gameObject.SetActive(value: false);
		switch (PeekNextPattern())
		{
		case "I":
			base.animator.Play("JumpToIdle");
			yield return base.animator.WaitForAnimationToEnd(this, "JumpToIdle");
			break;
		case "S":
			base.animator.Play("JumpToDash");
			yield return base.animator.WaitForAnimationToEnd(this, "JumpToDash");
			break;
		case "J":
			base.animator.Play("JumpToJump");
			yield return base.animator.WaitForAnimationToEnd(this, "JumpToJump");
			break;
		}
		if (PeekNextPattern() == "I")
		{
			yield return CupheadTime.WaitForSeconds(this, (!forceOutroToStart) ? p.hesitate : 0f);
		}
		SetState(States.Idle);
		inBallForm = false;
	}

	public void StartIcePillar()
	{
		SetState(States.IcePillar);
		base.animator.SetTrigger("OnSmash");
	}

	private void SpawnIcePillars()
	{
		CupheadLevelCamera.Current.Shake(30f, 0.7f);
		snowCultBGHandler.CandleGust();
		Vector3 pos = new Vector3(base.transform.position.x + 290f * (float)(onLeft ? 1 : (-1)), 95f);
		snowBurstA.Create(pos, onLeft ? 1 : (-1));
		StartCoroutine(spawn_snowfall_cr());
		StartCoroutine(ice_pillar_cr());
	}

	private IEnumerator ice_pillar_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.25f);
		LevelProperties.SnowCult.IcePillar p = base.properties.CurrentState.icePillar;
		float offset = 0f;
		int dir = (onLeft ? 1 : (-1));
		bool type = Rand.Bool();
		Parser.FloatTryParse(p.offsetCoordString.Split(',')[offsetCoordIndex], out offset);
		for (int i = 0; i < p.icePillarCount; i++)
		{
			Vector3 pos = new Vector3(yetiMidPoint.position.x + offset * (float)dir + p.icePillarSpacing * (float)i * (float)dir, -142f);
			SnowCultLevelIcePillar icePillar = icePillarPrefab.Spawn();
			icePillar.Init(pos, p, type, p.appearDelay * (float)(i + 1));
			type = !type;
			yield return CupheadTime.WaitForSeconds(this, p.appearDelay);
		}
		offsetCoordIndex = (offsetCoordIndex + 1) % p.offsetCoordString.Split(',').Length;
		yield return CupheadTime.WaitForSeconds(this, (!forceOutroToStart) ? p.hesitate : 0f);
		SetState(States.Idle);
		yield return null;
	}

	private IEnumerator spawn_snowfall_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		Vector3 pos = new Vector3(base.transform.position.x + 290f * (float)(onLeft ? 1 : (-1)), 510f);
		snowFallA.Create(pos, onLeft ? 1 : (-1));
		yield return null;
	}

	private void InitBats()
	{
		batAttackPositionString = new PatternString(base.properties.CurrentState.snowball.batAttackPosition);
		batAttackHeightString = new PatternString(base.properties.CurrentState.snowball.batAttackHeight);
		batAttackWidthString = new PatternString(base.properties.CurrentState.snowball.batAttackWidth);
		batAttackSideString = new PatternString(base.properties.CurrentState.snowball.batAttackSide);
		batAttackInterDelayString = new PatternString(base.properties.CurrentState.snowball.batAttackInterDelay);
		batArcModifierString = new PatternString(base.properties.CurrentState.snowball.batArcModifier);
		batParryableString = new PatternString(base.properties.CurrentState.snowball.batParryableString);
	}

	private IEnumerator bats_attack_cr()
	{
		LevelProperties.SnowCult.Snowball p = base.properties.CurrentState.snowball;
		batLaunchTimer = batAttackInterDelayString.PopFloat();
		AbstractPlayerController player2 = PlayerManager.GetNext();
		while (true)
		{
			if (batCirclingList.Count > 0)
			{
				int which = UnityEngine.Random.Range(0, batCirclingList.Count);
				if (batCirclingList[which] != null && batCirclingList[which].reachedCircle)
				{
					while (batLaunchTimer > 0f)
					{
						batLaunchTimer -= CupheadTime.Delta;
						yield return null;
					}
					if (batCirclingList.Count > which && batCirclingList[which] != null)
					{
						batLaunchTimer = batAttackInterDelayString.PopFloat();
						float height = batAttackHeightString.PopFloat();
						float num = batAttackWidthString.PopFloat();
						Vector3 position = batAttackPositions[batAttackPositionString.PopInt()].position;
						position.x *= (onLeft ? 1 : (-1));
						num *= (float)((!onLeft) ? 1 : (-1));
						bool flag = batAttackSideString.PopLetter() == 'S';
						position.x *= ((!flag) ? 1 : (-1));
						num *= (float)((!flag) ? 1 : (-1));
						batCirclingList[which].AttackPlayer(position, height, num, batArcModifierString.PopFloat());
						batCirclingList.RemoveAt(which);
					}
				}
				else if (batCirclingList[which] == null)
				{
					batCirclingList.RemoveAt(which);
				}
				yield return null;
				player2 = PlayerManager.GetNext();
			}
			else
			{
				yield return null;
			}
		}
	}

	public void ReturnBatToList(SnowCultLevelBat bat)
	{
		batCirclingList.Add(bat);
	}

	private IEnumerator spawn_bats_cr()
	{
		batSpawnEffectPrefab.Create(base.transform.position + Vector3.up * 180f + Vector3.right * ((!onLeft) ? (-20) : 20));
		if (bats == null)
		{
			bats = new SnowCultLevelBat[base.properties.CurrentState.snowball.batCount];
		}
		for (int j = 0; j < batCirclingList.Count; j++)
		{
			UnityEngine.Object.Destroy(batCirclingList[j].gameObject);
		}
		yield return null;
		batCirclingList.RemoveAll((SnowCultLevelBat b) => b == null);
		SFX_SNOWCULT_YetiFreezerScream();
		for (int i = 0; i < bats.Length; i++)
		{
			if (bats[i] == null || bats[i].gameObject == null || !bats[i].gameObject.activeInHierarchy)
			{
				Vector3 launchVelocity2 = new Vector3(onLeft ? 1 : (-1), 0.25f);
				launchVelocity2 *= (float)UnityEngine.Random.Range(500, 800);
				launchVelocity2 = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-30, 30)) * launchVelocity2;
				bats[i] = batPrefab.Spawn();
				bool parryable = batParryableString.PopLetter() == 'P';
				bats[i].Init(base.transform.position + Vector3.up * 180f, launchVelocity2, base.properties.CurrentState.snowball, this, parryable, parryable ? "Pink" : ((!batColor) ? "Yellow" : string.Empty));
				if (!parryable)
				{
					batColor = !batColor;
				}
				batCirclingList.Add(bats[i]);
				yield return CupheadTime.WaitForSeconds(this, 0.125f);
			}
		}
		batLaunchTimer = base.properties.CurrentState.snowball.batInitialDelay;
	}

	private void RemoveBats()
	{
		if (bats == null)
		{
			return;
		}
		for (int i = 0; i < bats.Length; i++)
		{
			if (bats[i] != null)
			{
				bats[i].Dead();
			}
		}
	}

	public void Snowball()
	{
		StartCoroutine(snowball_cr());
	}

	private bool BreakOutOfFridge()
	{
		return forceOutroToStart || base.properties.CurrentState.stateName == LevelProperties.SnowCult.States.EasyYeti;
	}

	public void FridgeCanShoot()
	{
		fridgeCanShoot = true;
	}

	public float GetIceCubeStartFrame()
	{
		iceCubeStartFrame = (iceCubeStartFrame + 1) % 3;
		return iceCubeStartFrame switch
		{
			1 => 2f, 
			2 => 5f, 
			_ => 0f, 
		};
	}

	public int GetMediumExplosion()
	{
		iceCubeExplosionCounterMedium = (iceCubeExplosionCounterMedium + 1) % 2;
		return iceCubeExplosionCounterMedium;
	}

	public int GetSmallExplosion()
	{
		iceCubeExplosionCounterSmall = (iceCubeExplosionCounterSmall + 1) % 3;
		return iceCubeExplosionCounterSmall;
	}

	private IEnumerator snowball_cr()
	{
		SetState(States.Snowball);
		fridgeCanShoot = false;
		base.animator.SetTrigger("OnFridgeMorph");
		LevelProperties.SnowCult.Snowball p = base.properties.CurrentState.snowball;
		string[] snowballType = p.snowballTypeString[snowballMainIndex].Split(',');
		int target = Animator.StringToHash(base.animator.GetLayerName(0) + ".Idle");
		while (base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == target)
		{
			if (BreakOutOfFridge())
			{
				base.animator.ResetTrigger("OnFridgeMorph");
				SetState(States.Idle);
				yield break;
			}
			yield return null;
		}
		int count = snowballType.Length;
		float t;
		for (int i = 0; i < count; i++)
		{
			if (BreakOutOfFridge())
			{
				break;
			}
			while (!fridgeCanShoot && !forceOutroToStart)
			{
				yield return null;
			}
			fridgeCanShoot = false;
			if (!BreakOutOfFridge())
			{
				base.animator.Play("FridgeShoot");
				SFX_SNOWCULT_YetiFreezerIceCubeLaunch();
				AbstractPlayerController next = PlayerManager.GetNext();
				float num = p.shotMaxAngle;
				float speed = p.shotMaxSpeed;
				float num2 = float.MaxValue;
				Vector2 vector = new Vector2(next.transform.position.x, Level.Current.Ground) - (Vector2)cubeLaunchPosition.transform.position;
				vector.x = Mathf.Abs(vector.x);
				MinMax minMax = new MinMax(p.shotMinAngle, p.shotMaxAngle);
				MinMax minMax2 = new MinMax(p.shotMinSpeed, p.shotMaxSpeed);
				if (vector.y > 0f)
				{
					float num3 = minMax2.max / p.shotGravity;
					float num4 = minMax2.max * num3 - 0.5f * p.shotGravity * num3 * num3;
					float num5 = num4 + vector.y * 0f;
					float num6 = Mathf.Sqrt(2f * num5 / p.shotGravity);
					minMax2.max = num6 * p.shotGravity;
					minMax2.min *= minMax2.max / p.shotMaxSpeed;
				}
				for (float num7 = 0f; num7 < 1f; num7 += 0.01f)
				{
					float floatAt = minMax.GetFloatAt(num7);
					float floatAt2 = minMax2.GetFloatAt(num7);
					Vector2 vector2 = MathUtils.AngleToDirection(floatAt) * floatAt2;
					t = vector.x / vector2.x;
					float num8 = vector2.y * t - 0.5f * p.shotGravity * t * t;
					float num9 = Mathf.Abs(vector.y - num8);
					if (p.shotGravity > 0.01f)
					{
						float num10 = vector2.y - p.shotGravity * t;
						if (num10 > 0f)
						{
							continue;
						}
					}
					if (num9 < num2)
					{
						num2 = num9;
						num = floatAt;
						speed = floatAt2;
					}
				}
				if (next.transform.position.x < base.transform.position.x)
				{
					num = 180f - num;
				}
				SnowCultLevelSnowball snowCultLevelSnowball = null;
				if (snowballType[i][0] == 'S')
				{
					snowCultLevelSnowball = smallSnowballPrefab.Spawn();
				}
				else if (snowballType[i][0] == 'M')
				{
					snowCultLevelSnowball = mediumSnowballPrefab.Spawn();
				}
				else if (snowballType[i][0] == 'L')
				{
					snowCultLevelSnowball = largeSnowballPrefab.Spawn();
				}
				snowCultLevelSnowball.InitOriginal(cubeLaunchPosition.transform.position, p.shotGravity, speed, num, p, this);
				if (i == snowballType.Length - 1 && Level.Current.mode == Level.Mode.Easy && !BreakOutOfFridge())
				{
					i = -1;
					snowballMainIndex = (snowballMainIndex + 1) % p.snowballTypeString.Length;
					snowballType = p.snowballTypeString[snowballMainIndex].Split(',');
					count = snowballType.Length;
				}
			}
			if (!BreakOutOfFridge() && i < count - 1)
			{
				yield return CupheadTime.WaitForSeconds(this, p.snowballThrowDelay);
			}
		}
		t = 0f;
		while (t < p.batLaunchDelay && !BreakOutOfFridge())
		{
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		if (!BreakOutOfFridge())
		{
			base.animator.SetTrigger("OnFridgeOutro");
			yield return base.animator.WaitForAnimationToStart(this, "FridgeOutroLoop");
		}
		else
		{
			yield return base.animator.WaitForAnimationToEnd(this, "FridgeShoot", waitForEndOfFrame: false, waitForStart: false);
			if (base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash(base.animator.GetLayerName(0) + ".FridgeIdle"))
			{
				while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 5f / 18f && base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 5f / 6f)
				{
					yield return null;
				}
				base.animator.Play("FridgeOutroOpen");
			}
		}
		if (!forceOutroToStart)
		{
			yield return StartCoroutine(spawn_bats_cr());
			yield return CupheadTime.WaitForSeconds(this, 0.1f);
			snowballMainIndex = (snowballMainIndex + 1) % p.snowballTypeString.Length;
			base.animator.Play("FridgeOutroMorph");
		}
		yield return CupheadTime.WaitForSeconds(this, (!BreakOutOfFridge()) ? p.hesitate : 0f);
		yield return base.animator.WaitForAnimationToStart(this, "Idle");
		SetState(States.Idle);
		yield return null;
	}

	public void ToEasyPhaseThree()
	{
		LevelProperties.SnowCult.Yeti yeti = base.properties.CurrentState.yeti;
		patternString = yeti.yetiPatternString.Split(',');
		patternStringIndex = UnityEngine.Random.Range(0, patternString.Length);
		InitBats();
		StartCoroutine(bats_attack_cr());
	}

	public void ForceOutroToStart()
	{
		base.animator.SetBool("ForceOutro", value: true);
		forceOutroToStart = true;
	}

	private void OnBossDeath()
	{
		StopAllCoroutines();
		base.animator.Play("DeathEasy");
	}

	public void OnDeath()
	{
		base.animator.SetBool("Dead", value: true);
		StopAllCoroutines();
		RemoveBats();
		if (this.OnDeathEvent != null)
		{
			this.OnDeathEvent();
		}
	}

	public void ActivateLegs()
	{
		bucket.transform.parent = null;
		legs.transform.parent = null;
		legs.SetActive(value: true);
	}

	public void DeathAnimationEnded()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void AnimationEvent_SFX_SNOWCULT_YetiIntro02DroptoGround()
	{
		AudioManager.Play("sfx_dlc_snowcult_p2_snowmonster_intro_02_droptoground");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_snowmonster_intro_02_droptoground");
	}

	private void AnimationEvent_SFX_SNOWCULT_YetiFridgeToSnowmonster()
	{
		AudioManager.Play("sfx_dlc_snowcult_p2_transform_from_fridge_to_snowmonster");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_transform_from_fridge_to_snowmonster");
	}

	private void AnimationEvent_SFX_SNOWCULT_YetiSnowmonsterToFridge()
	{
		AudioManager.Play("sfx_dlc_snowcult_p2_transform_from_snowmonster_to_fridge");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_transform_from_snowmonster_to_fridge");
	}

	private void AnimationEvent_SFX_SNOWCULT_GroundSmash()
	{
		AudioManager.Play("sfx_DLC_SnowCult_P2_SnowMonster_GroundSmash_withHands");
		emitAudioFromObject.Add("sfx_DLC_SnowCult_P2_SnowMonster_GroundSmash_withHands");
	}

	private void AnimationEvent_SFX_SNOWCULT_BodyRollPre()
	{
		AudioManager.Play("sfx_dlc_snowcult_p2_snowmonster_bodyrollpre");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_snowmonster_bodyrollpre");
	}

	private void AnimationEvent_SFX_SNOWCULT_BodyRoll()
	{
		AudioManager.Play("sfx_dlc_snowcult_p2_snowmonster_bodyroll");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_snowmonster_bodyroll");
	}

	private void AnimationEvent_SFX_SNOWCULT_BodyTossPre()
	{
		AudioManager.Play("sfx_dlc_snowcult_p2_snowmonster_bodytosspre");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_snowmonster_bodytosspre");
	}

	private void AnimationEvent_SFX_SNOWCULT_BodyToss()
	{
		AudioManager.Play("sfx_dlc_snowcult_p2_snowmonster_bodytoss");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_snowmonster_bodytoss");
	}

	private void AnimationEvent_SFX_SNOWCULT_YetiDie()
	{
		AudioManager.Play("sfx_dlc_snowcult_p2_snowmonster_death_explode");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_snowmonster_death_explode");
	}

	private void SFX_SNOWCULT_YetiFreezerScream()
	{
		batSoundLong = !batSoundLong;
		AudioManager.Play((!batSoundLong) ? "sfx_dlc_snowcult_p2_snowmonster_fridge_freezerscream_short" : "sfx_dlc_snowcult_p2_snowmonster_fridge_freezerscream_long");
		emitAudioFromObject.Add((!batSoundLong) ? "sfx_dlc_snowcult_p2_snowmonster_fridge_freezerscream_short" : "sfx_dlc_snowcult_p2_snowmonster_fridge_freezerscream_long");
	}

	private void SFX_SNOWCULT_YetiFreezerIceCubeLaunch()
	{
		AudioManager.Play("sfx_dlc_snowcult_p2_snowmonster_fridge_icecube_launch");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_snowmonster_fridge_icecube_launch");
	}
}
