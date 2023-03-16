using System;
using System.Collections;
using UnityEngine;

public class AirplaneLevelBulldogPlane : LevelProperties.Airplane.Entity
{
	public enum State
	{
		Intro,
		Main,
		Parachute,
		TripleAttack,
		CatAttack
	}

	private const float MOVE_POS_X = 245f;

	private const float PARACHUTE_POS_X = 575f;

	private const float PARACHUTE_APPEAR_DELAY = 0.35f;

	private const float PARACHUTE_EXIT_BOUNCE_HEIGHT = 40f;

	private const float PARACHUTE_EXIT_BOUNCE_SPEED = 1.6f;

	private const float PARACHUTE_EXIT_BOUNCE_FRAME_DELAY = 16f;

	private const float PARACHUTE_RETURN_BOUNCE_HEIGHT = 60f;

	private const float PARACHUTE_RETURN_BOUNCE_SPEED = 1.7f;

	private const float PARACHUTE_RETURN_BOUNCE_FRAME_DELAY = 3f;

	private const float BUMP_RETURN_BOUNCE_HEIGHT = 30f;

	private const float BUMP_RETURN_BOUNCE_SPEED = 3f;

	private const float CAT_ATTACK_POS_X = 600f;

	private const float CAT_ATTACK_APPEAR_DELAY = 0.35f;

	private const float MOVE_DOWN_POS = 256f;

	private const float MOVE_TIME = 0.3f;

	private const float MOVE_LENGTH = 4f;

	public State state;

	[SerializeField]
	private Animator leaderIntroBG;

	[SerializeField]
	private Animator hydrantAttackBG;

	[Header("Roots")]
	[SerializeField]
	private AirplaneLevelTurretDog[] turretSpawnPoints;

	[SerializeField]
	private Transform rocketSpawnLeft;

	[SerializeField]
	private Transform rocketSpawnRight;

	[Header("Prefabs")]
	[SerializeField]
	private AirplaneLevelRocket rocketPrefab;

	[Header("Bulldog")]
	[SerializeField]
	private Animator bullDogPlane;

	[SerializeField]
	private AirplaneLevelBulldogParachute bulldogParachute;

	[SerializeField]
	private AirplaneLevelBulldogCatAttack bulldogCatAttack;

	[SerializeField]
	private GameObject canteenPlane;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private float moveTime;

	private float startPosY;

	private float bounceY;

	private float bounceYTimer;

	private float bounceX;

	private float bounceXTimer;

	private float bounceXDir = 1f;

	private bool exitBounce;

	private float baseX;

	private float wobbleTimer;

	[SerializeField]
	private float wobbleX = 10f;

	[SerializeField]
	private float wobbleY = 10f;

	[SerializeField]
	private float wobbleSpeed = 1f;

	private bool movingRight;

	private bool dontDamage;

	public bool endPhaseOne;

	private bool firstAttack = true;

	private PatternString sideString;

	[SerializeField]
	private Animator[] smokePuff;

	private int smokePuffLCounter;

	private int smokePuffRCounter = 2;

	private float smokePuffLTimer;

	private float smokePuffRTimer;

	public bool isDead;

	public bool startPhaseTwo;

	private void Start()
	{
		state = State.Intro;
		startPosY = 256f;
		baseX = base.transform.position.x;
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = bullDogPlane.GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		bulldogParachute.GetComponent<DamageReceiver>().OnDamageTaken += OnDamageTaken;
		bulldogCatAttack.GetComponent<DamageReceiver>().OnDamageTaken += OnDamageTaken;
		bulldogParachute.gameObject.SetActive(value: false);
		bulldogCatAttack.gameObject.SetActive(value: false);
		StartCoroutine(idle_timer_cr());
		StartCoroutine(rotate_cr());
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void FixedUpdate()
	{
		if (state == State.Intro)
		{
			return;
		}
		moveTime += Time.fixedDeltaTime;
		Vector2 vector = base.transform.position;
		vector.y = Mathf.Sin(moveTime / 0.3f) * 4f;
		if (bounceXTimer > 0f)
		{
			bounceXTimer -= CupheadTime.FixedDelta * 3f * ((!(bounceXTimer > 0.5f)) ? 0.25f : 1f);
			bounceX = ((!(bounceXTimer > 0.5f)) ? (bounceX = EaseUtils.EaseInOutSine(30f, 0f, 1f - bounceXTimer * 2f)) : (Mathf.Sin(bounceXTimer * (float)Math.PI) * 30f));
			bounceX *= bounceXDir;
		}
		else
		{
			bounceXTimer = 0f;
			bounceX = 0f;
		}
		if (bounceYTimer > 0f)
		{
			bounceYTimer -= CupheadTime.FixedDelta * ((!exitBounce) ? 1.7f : 1.6f);
			bounceY = ((!(bounceYTimer > 0.5f)) ? (bounceY = EaseUtils.EaseInOutSine((!exitBounce) ? 60f : 40f, 0f, 1f - bounceYTimer * 2f)) : (Mathf.Sin(bounceYTimer * (float)Math.PI) * ((!exitBounce) ? 60f : 40f)));
		}
		else
		{
			bounceYTimer = 0f;
			bounceY = 0f;
		}
		base.transform.SetPosition(baseX + Mathf.Sin(wobbleTimer * 3f) * wobbleX + bounceX, startPosY + vector.y - bounceY + Mathf.Sin(wobbleTimer * 2f) * wobbleY);
		wobbleTimer += CupheadTime.FixedDelta * wobbleSpeed;
		if (!isDead)
		{
			smokePuffLTimer -= CupheadTime.FixedDelta;
			smokePuffRTimer -= CupheadTime.FixedDelta;
			if (smokePuffLTimer <= 0f)
			{
				smokePuff[smokePuffLCounter % 3].Play((smokePuffLCounter % 4).ToString(), 0, 0f);
				smokePuff[smokePuffLCounter % 3].transform.localPosition = Vector3.left * 300f + Vector3.up * 50f;
				smokePuffLTimer += 0.25f;
				smokePuffLCounter++;
			}
			if (smokePuffRTimer <= 0f)
			{
				smokePuff[smokePuffRCounter % 3 + 3].Play((smokePuffRCounter % 4).ToString(), 0, 0f);
				smokePuff[smokePuffRCounter % 3 + 3].transform.localPosition = Vector3.right * 300f + Vector3.up * 50f;
				smokePuffRTimer += 0.27f;
				smokePuffRCounter++;
			}
		}
		for (int i = 0; i < smokePuff.Length; i++)
		{
			smokePuff[i].transform.localPosition += new Vector3((i >= 3) ? (-3) : 3, 2f - 4f * smokePuff[i].GetCurrentAnimatorStateInfo(0).normalizedTime) * CupheadTime.FixedDelta * 100f;
		}
	}

	private IEnumerator rotate_cr()
	{
		float t = 0f;
		float time = 4f;
		float maxAngle = 1f;
		base.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f - maxAngle));
		while (true)
		{
			if (t < time)
			{
				t += (float)CupheadTime.Delta;
				base.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, Mathf.Lerp(0f - maxAngle, maxAngle, EaseUtils.EaseInOutSine(0f, 1f, t / time))));
				yield return null;
			}
			else
			{
				t = 0f;
				maxAngle = 0f - maxAngle;
				yield return null;
			}
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (!dontDamage)
		{
			base.properties.DealDamage(info.damage);
		}
	}

	public override void LevelInit(LevelProperties.Airplane properties)
	{
		base.LevelInit(properties);
		sideString = new PatternString(properties.CurrentState.parachute.sideString);
		StartCoroutine(intro_cr());
	}

	private IEnumerator intro_cr()
	{
		leaderIntroBG.SetTrigger("Continue");
		YieldInstruction wait = new WaitForFixedUpdate();
		yield return bullDogPlane.WaitForAnimationToStart(this, "Intro");
		int target = Animator.StringToHash(bullDogPlane.GetLayerName(0) + ".Intro");
		while (bullDogPlane.GetCurrentAnimatorStateInfo(0).fullPathHash == target)
		{
			float s = bullDogPlane.GetCurrentAnimatorStateInfo(0).normalizedTime;
			if (s > 0.7f && s < 0.95f)
			{
				((AirplaneLevel)Level.Current).UpdateShadow(1f - Mathf.Sin(Mathf.InverseLerp(0.7f, 0.95f, s) * (float)Math.PI) * 0.2f);
			}
			else
			{
				((AirplaneLevel)Level.Current).UpdateShadow(1f);
			}
			yield return wait;
		}
		((AirplaneLevel)Level.Current).UpdateShadow(1f);
		yield return CupheadTime.WaitForSeconds(this, 0.35f);
		SFX_DOGFIGHT_BulldogPlane_Loop();
		SFX_DOGFIGHT_Intro_BulldogPlaneDecend();
		StartCoroutine(turret_cr());
		StartCoroutine(mainattack_cr());
		float t = 0f;
		float time = 0.8f;
		float endTime = 0.4f;
		float start = base.transform.position.y;
		StartCoroutine(scale_in_cr());
		while (t < time)
		{
			t += CupheadTime.FixedDelta;
			TransformExtensions.SetPosition(y: Mathf.Lerp(start, 156f, EaseUtils.Ease(EaseUtils.EaseType.easeOutSine, 0f, 1f, t / time)), transform: base.transform);
			yield return wait;
		}
		t = 0f;
		start = base.transform.position.y;
		while (t < endTime)
		{
			t += CupheadTime.FixedDelta;
			TransformExtensions.SetPosition(y: Mathf.Lerp(start, 256f, EaseUtils.Ease(EaseUtils.EaseType.easeOutSine, 0f, 1f, t / endTime)), transform: base.transform);
			yield return wait;
		}
		base.transform.SetPosition(null, 256f);
		if (state == State.Intro)
		{
			state = State.Main;
		}
		StartCoroutine(move_cr());
		yield return null;
	}

	private IEnumerator scale_in_cr()
	{
		base.transform.localScale = new Vector3(0.8f, 0.8f);
		YieldInstruction wait = new WaitForFixedUpdate();
		float t = 0f;
		float frameTime = 0f;
		while (t < 1.2f)
		{
			while (frameTime < 1f / 24f)
			{
				frameTime += CupheadTime.FixedDelta;
				yield return wait;
			}
			t += frameTime;
			frameTime -= 1f / 24f;
			base.transform.localScale = Vector3.Lerp(new Vector3(0.8f, 0.8f), new Vector3(1f, 1f), EaseUtils.EaseOutSine(0f, 1f, Mathf.InverseLerp(0f, 1.2f, t)));
		}
		base.transform.localScale = new Vector3(1f, 1f);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		WORKAROUND_NullifyFields();
	}

	private IEnumerator mainattack_cr()
	{
		LevelProperties.Airplane.Main p = base.properties.CurrentState.main;
		PatternString attackType = new PatternString(p.attackType);
		while (true)
		{
			if (firstAttack)
			{
				yield return CupheadTime.WaitForSeconds(this, 0.6f);
				yield return CupheadTime.WaitForSeconds(this, p.firstAttackDelay);
				firstAttack = false;
			}
			else
			{
				yield return CupheadTime.WaitForSeconds(this, p.attackDelayRange.RandomFloat());
			}
			switch (attackType.PopLetter())
			{
			case 'P':
				yield return StartCoroutine(parachute_cr());
				break;
			case 'T':
				yield return StartCoroutine(catattack_cr());
				break;
			}
			state = State.Main;
			yield return null;
		}
	}

	private IEnumerator idle_timer_cr()
	{
		bool pickSide = Rand.Bool();
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, UnityEngine.Random.Range(2f, 3f));
			if (state == State.Main)
			{
				pickSide = ((!Rand.Bool()) ? (base.transform.position.x > canteenPlane.transform.position.x) : Rand.Bool());
				string side = ((!pickSide) ? "Right" : "Left");
				bullDogPlane.SetTrigger("OnIdle" + side);
				bullDogPlane.SetInteger("IdleLoopCount", (pickSide != base.transform.position.x > canteenPlane.transform.position.x) ? 2 : 0);
				yield return bullDogPlane.WaitForAnimationToStart(this, "Idle");
			}
			while (state != State.Main)
			{
				yield return null;
			}
			yield return null;
		}
	}

	private IEnumerator move_cr()
	{
		movingRight = Rand.Bool();
		float t = 0f;
		float time = base.properties.CurrentState.main.moveTime;
		float start = 0f;
		float end = 0f;
		float speedModifier = 1f;
		while (true)
		{
			t = 0f;
			start = base.transform.position.x;
			end = ((!movingRight) ? (-245f) : 245f);
			while (t < time)
			{
				t += CupheadTime.FixedDelta * speedModifier;
				speedModifier = Mathf.Clamp(speedModifier + ((state != State.Main) ? (-0.01f) : 0.01f), 0f, 1f);
				float val = t / time;
				baseX = EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, start, end, val);
				yield return new WaitForFixedUpdate();
			}
			base.transform.SetPosition(end);
			movingRight = !movingRight;
			yield return null;
		}
	}

	private IEnumerator turret_cr()
	{
		LevelProperties.Airplane.Turrets p = base.properties.CurrentState.turrets;
		PatternString positionString = new PatternString(p.positionString);
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, p.attackDelayRange.RandomFloat());
			turretSpawnPoints[positionString.PopInt()].StartAttack(p.velocityX, p.velocityY, p.gravity);
		}
	}

	public void StartRocket()
	{
		StartCoroutine(rocket_cr());
	}

	private IEnumerator rocket_cr()
	{
		LevelProperties.Airplane.Rocket p = base.properties.CurrentState.rocket;
		PatternString delayString = new PatternString(p.attackDelayString);
		PatternString dirString = new PatternString(p.attackOrderString);
		hydrantAttackBG.Play("Fly");
		yield return hydrantAttackBG.WaitForAnimationToEnd(this, "Fly");
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, delayString.PopInt());
			Vector3 position = ((dirString.PopLetter() != 'R') ? rocketSpawnLeft.position : rocketSpawnRight.position);
			rocketPrefab.Create(PlayerManager.GetNext(), position, p.homingSpeed, p.homingRotation, p.homingHP, p.homingTime);
		}
	}

	private IEnumerator parachute_cr()
	{
		bool onLeft = sideString.PopLetter() == 'L';
		bullDogPlane.SetInteger("IdleLoopCount", 3);
		bullDogPlane.SetBool("InParachuteATK", value: true);
		yield return bullDogPlane.WaitForAnimationToStart(this, "Parachute_Start");
		state = State.Parachute;
		yield return CupheadTime.WaitForSeconds(this, 2f / 3f);
		exitBounce = true;
		bounceYTimer = 1f;
		bullDogPlane.GetComponent<Collider2D>().enabled = false;
		yield return bullDogPlane.WaitForAnimationToEnd(this, "Parachute_Start", waitForEndOfFrame: false, waitForStart: false);
		SFX_DOGFIGHT_Bulldog_ParachuteDown();
		float posX = ((!onLeft) ? 575f : (-575f));
		Vector3 pos = new Vector3(posX, 0f);
		float scale = (onLeft ? 1 : (-1));
		yield return CupheadTime.WaitForSeconds(this, 0.35f);
		bulldogParachute.gameObject.SetActive(value: true);
		bulldogParachute.StartDescent(pos, scale);
		while (bulldogParachute.isMoving)
		{
			yield return null;
		}
		bulldogParachute.gameObject.SetActive(value: false);
		bullDogPlane.SetBool("InParachuteATK", value: false);
		yield return CupheadTime.WaitForSeconds(this, 0.125f);
		exitBounce = false;
		bounceYTimer = 1f;
		SFX_DOGFIGHT_BulldogPlane_ParachuteDownStop();
		yield return bullDogPlane.WaitForAnimationToEnd(this, "Parachute_End", waitForEndOfFrame: false, waitForStart: false);
		bullDogPlane.GetComponent<Collider2D>().enabled = true;
	}

	private IEnumerator catattack_cr()
	{
		LevelProperties.Airplane.Triple p = base.properties.CurrentState.triple;
		bool onLeft = sideString.PopLetter() == 'L';
		bullDogPlane.SetInteger("IdleLoopCount", 3);
		bullDogPlane.SetBool("InParachuteATK", value: true);
		bullDogPlane.SetBool("OnLeft", onLeft);
		yield return bullDogPlane.WaitForAnimationToStart(this, "Parachute_Start");
		state = State.CatAttack;
		yield return CupheadTime.WaitForSeconds(this, 2f / 3f);
		exitBounce = true;
		bounceYTimer = 1f;
		bullDogPlane.GetComponent<Collider2D>().enabled = false;
		yield return bullDogPlane.WaitForAnimationToEnd(this, "Parachute_Start", waitForEndOfFrame: false, waitForStart: false);
		yield return CupheadTime.WaitForSeconds(this, 0.35f);
		float posX = ((!onLeft) ? 600f : (-600f));
		Vector3 startPos = new Vector3(posX, p.yHeight);
		bulldogCatAttack.gameObject.SetActive(value: true);
		yield return null;
		bulldogCatAttack.StartCat(startPos);
		while (bulldogCatAttack.isAttacking)
		{
			yield return null;
		}
		bulldogCatAttack.gameObject.SetActive(value: false);
		bullDogPlane.SetBool("InParachuteATK", value: false);
		yield return CupheadTime.WaitForSeconds(this, 0.125f);
		exitBounce = false;
		bounceYTimer = 1f;
		SFX_DOGFIGHT_BulldogPlane_ParachuteDownStop();
		yield return bullDogPlane.WaitForAnimationToEnd(this, "Parachute_End", waitForEndOfFrame: false, waitForStart: false);
		bullDogPlane.GetComponent<Collider2D>().enabled = true;
	}

	public void OnStageChange()
	{
		endPhaseOne = true;
		dontDamage = true;
		if (bulldogCatAttack.isAttacking)
		{
			bulldogCatAttack.EarlyExit();
		}
		if (bulldogParachute.isMoving)
		{
			bulldogParachute.EarlyExit();
		}
	}

	public void BulldogDeath()
	{
		isDead = true;
		StopAllCoroutines();
		StartCoroutine(death_cr());
	}

	private IEnumerator death_cr()
	{
		bullDogPlane.SetBool("Dead", value: true);
		bullDogPlane.SetBool("InTripleATK", value: false);
		if (!bulldogParachute.gameObject.activeInHierarchy)
		{
			bullDogPlane.SetBool("InParachuteATK", value: false);
		}
		yield return bullDogPlane.WaitForAnimationToStart(this, "Death");
		startPhaseTwo = true;
		SFX_DOGFIGHT_BulldogPlane_StopLoop();
		bullDogPlane.GetComponent<Collider2D>().enabled = false;
		bullDogPlane.SetLayerWeight(1, 0f);
		bullDogPlane.SetLayerWeight(2, 0f);
		bullDogPlane.SetLayerWeight(3, 0f);
		yield return bullDogPlane.WaitForAnimationToEnd(this, "Death");
		UnityEngine.Object.Destroy(base.gameObject);
		yield return null;
	}

	private void SFX_DOGFIGHT_Intro_BulldogPlaneDecend()
	{
		AudioManager.Play("sfx_dlc_dogfight_bulldogplane_introdecend");
	}

	private void SFX_DOGFIGHT_BulldogPlane_Loop()
	{
		AudioManager.PlayLoop("sfx_dlc_dogfight_bulldogplane_loop");
		AudioManager.FadeSFXVolumeLinear("sfx_dlc_dogfight_bulldogplane_loop", 0.25f, 3f);
	}

	private void SFX_DOGFIGHT_BulldogPlane_StopLoop()
	{
		AudioManager.Stop("sfx_dlc_dogfight_bulldogplane_loop");
	}

	private void SFX_DOGFIGHT_Bulldog_ParachuteDown()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_bulldog_parachutedown");
		emitAudioFromObject.Add("sfx_dlc_dogfight_p1_bulldog_parachutedown");
		AudioManager.Play("sfx_DLC_Dogfight_P1_Bulldog_ParachuteFlump");
	}

	private void SFX_DOGFIGHT_BulldogPlane_ParachuteDownStop()
	{
		AudioManager.Stop("sfx_dlc_dogfight_p1_bulldog_parachutedown");
	}

	private void WORKAROUND_NullifyFields()
	{
		leaderIntroBG = null;
		hydrantAttackBG = null;
		turretSpawnPoints = null;
		rocketSpawnLeft = null;
		rocketSpawnRight = null;
		rocketPrefab = null;
		bullDogPlane = null;
		bulldogParachute = null;
		bulldogCatAttack = null;
		canteenPlane = null;
		damageDealer = null;
		sideString = null;
		smokePuff = null;
	}
}
