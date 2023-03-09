using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingGenieLevelGenieTransform : LevelProperties.FlyingGenie.Entity
{
	public enum State
	{
		Intro,
		Idle,
		Marionette,
		Giant,
		Dead
	}

	private const float FRAME_TIME = 1f / 24f;

	[SerializeField]
	private Effect deathPuffEffect;

	[SerializeField]
	private SpriteRenderer bottomLayer;

	[Space(10f)]
	[SerializeField]
	private FlyingGenieLevelSpawner spawnerPrefab;

	[SerializeField]
	private Transform marionetteShootRoot;

	[SerializeField]
	private BasicProjectile shotBullet;

	[SerializeField]
	private BasicProjectile pinkBullet;

	[SerializeField]
	private BasicProjectile shootBullet;

	[SerializeField]
	private BasicProjectile spreadProjectile;

	[SerializeField]
	private FlyingGenieLevelRing ringPrefab;

	[SerializeField]
	private FlyingGenieLevelRing pinkRingPrefab;

	[SerializeField]
	private FlyingGenieLevelPyramid pyramidPrefab;

	[SerializeField]
	private FlyingGenieLevelTinyMarionette tinyMarionette;

	[Space(10f)]
	[SerializeField]
	private Transform pyramidPivotPoint;

	[SerializeField]
	private Transform gemStone;

	[SerializeField]
	private Transform pipe;

	[SerializeField]
	private Transform giantRoot;

	[SerializeField]
	private Transform handFront;

	[SerializeField]
	private Transform handBack;

	[SerializeField]
	private Transform deathPuffRoot;

	[SerializeField]
	private Transform morphRoot;

	[SerializeField]
	private Transform marionetteRoot;

	[SerializeField]
	private GameObject spark;

	private FlyingGenieLevelMeditateFX meditateP1;

	private FlyingGenieLevelMeditateFX meditateP2;

	private FlyingGenieLevelSpawner spawner;

	private List<FlyingGenieLevelBomb> bombs;

	private List<FlyingGenieLevelPyramid> pyramids;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private Vector3 startPos;

	private bool pyramidsGoingClockwise;

	private bool isShooting;

	private float transitionHP;

	private int pinkIndex;

	private string[] pinkString;

	public State state { get; private set; }

	public bool skipMarionette { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		skipMarionette = false;
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		GetComponent<Collider2D>().enabled = false;
	}

	public override void LevelInit(LevelProperties.FlyingGenie properties)
	{
		base.LevelInit(properties);
		state = State.Intro;
		pyramids = new List<FlyingGenieLevelPyramid>();
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
		if (skipMarionette && state == State.Giant && transitionHP > 0f)
		{
			float num = transitionHP;
			transitionHP -= info.damage;
			Level.Current.timeline.DealDamage(Mathf.Clamp(num - transitionHP, 0f, num));
		}
		else if (base.properties.CurrentHealth <= 0f && state != State.Dead)
		{
			state = State.Dead;
			if (Level.Current.mode == Level.Mode.Easy)
			{
				MarionetteDead();
			}
			else
			{
				StartDeath();
			}
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

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	public void StartMarionette(Vector3 spawnPos, FlyingGenieLevelMeditateFX meditateP1, FlyingGenieLevelMeditateFX meditateP2)
	{
		GetComponent<Collider2D>().enabled = true;
		base.transform.position = spawnPos;
		this.meditateP1 = meditateP1;
		this.meditateP2 = meditateP2;
		StartCoroutine(phase2_intro_cr());
	}

	private IEnumerator phase2_intro_cr()
	{
		AudioManager.Play("genie_return");
		emitAudioFromObject.Add("genie_return");
		LevelProperties.FlyingGenie.Scan p = base.properties.CurrentState.scan;
		float timer = 0f;
		float P1ShrinkTimer = 0f;
		float P2ShrinkTimer = 0f;
		PlanePlayerController player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne) as PlanePlayerController;
		PlanePlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo) as PlanePlayerController;
		bool player2In = player2 != null;
		while (timer < p.scanDuration)
		{
			timer += (float)CupheadTime.Delta;
			if (Level.Current.mode != 0)
			{
				if (player1.Shrunk)
				{
					P1ShrinkTimer += (float)CupheadTime.Delta;
				}
				if (player2In && player2.Shrunk)
				{
					P2ShrinkTimer += (float)CupheadTime.Delta;
				}
			}
			yield return null;
		}
		if (P1ShrinkTimer >= p.miniDuration)
		{
			if (player2In)
			{
				if (P2ShrinkTimer >= p.miniDuration)
				{
					skipMarionette = true;
				}
				else
				{
					skipMarionette = false;
				}
			}
			else
			{
				skipMarionette = true;
			}
		}
		base.animator.SetTrigger("Continue");
		pyramidsGoingClockwise = Rand.Bool();
		if (skipMarionette)
		{
			transitionHP = p.transitionDamage;
			base.animator.SetBool("IsPuppet", value: true);
			state = State.Giant;
			StartCoroutine(move_up_puppet_cr());
			base.properties.DealDamageToNextNamedState();
		}
		else
		{
			base.animator.SetBool("IsPuppet", value: false);
			state = State.Marionette;
			yield return base.animator.WaitForAnimationToEnd(this, "Marionette_Intro");
			startPos = base.transform.position;
			StartCoroutine(move_cr());
			StartCoroutine(shoot_cr());
		}
		yield return null;
	}

	private void EndFX()
	{
		if (meditateP1 != null)
		{
			meditateP1.EndEffect();
		}
		if (meditateP2 != null)
		{
			meditateP2.EndEffect();
		}
	}

	private void SnapPosition()
	{
		HandSFX();
		base.transform.position = morphRoot.position;
		bottomLayer.transform.localPosition = new Vector3(-160f, bottomLayer.transform.localPosition.y);
		StartCoroutine(handle_carpet_fadeout_cr());
	}

	private IEnumerator move_up_puppet_cr()
	{
		float t = 0f;
		float timer = 0.4f;
		float slowTimer = 0.8f;
		float midTimer = 0.5f;
		float midEnd = 300f;
		float slowEnd = 410f;
		float end = 1071f;
		float start = base.transform.position.y;
		yield return base.animator.WaitForAnimationToEnd(this, "Genie_Morph_Puppet");
		tinyMarionette.gameObject.SetActive(value: true);
		while (t < slowTimer)
		{
			TransformExtensions.SetPosition(y: Mathf.Lerp(start, slowEnd, EaseUtils.Ease(EaseUtils.EaseType.easeOutSine, 0f, 1f, t / slowTimer)), transform: base.transform);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		t = 0f;
		start = base.transform.position.y;
		bool startIntro = false;
		while (t < midTimer)
		{
			TransformExtensions.SetPosition(y: Mathf.Lerp(start, midEnd, EaseUtils.Ease(EaseUtils.EaseType.easeInSine, 0f, 1f, t / midTimer)), transform: base.transform);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		t = 0f;
		start = base.transform.position.y;
		while (t < timer)
		{
			TransformExtensions.SetPosition(y: Mathf.Lerp(start, end, EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, 0f, 1f, t / timer)), transform: base.transform);
			if (t / timer > 0.95f && !startIntro)
			{
				tinyMarionette.animator.SetTrigger("OnIntro");
				startIntro = true;
			}
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		tinyMarionette.transform.parent = null;
		base.properties.DealDamage(base.properties.CurrentState.scan.transitionDamage);
		Vector3 endPos = new Vector3(pyramidPivotPoint.position.x, pyramidPivotPoint.position.y + 145f);
		tinyMarionette.Activate(endPos, base.properties.CurrentState.scan, !pyramidsGoingClockwise);
		EndMarionette();
		yield return null;
	}

	private IEnumerator handle_carpet_fadeout_cr()
	{
		bottomLayer.color = new Color(1f, 1f, 1f, 1f);
		float t = 0f;
		float time = 2f;
		while (t < time)
		{
			bottomLayer.color = new Color(1f, 1f, 1f, 1f - t / time);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		base.animator.Play("Hands_Off");
		bottomLayer.color = new Color(1f, 1f, 1f, 1f);
		bottomLayer.transform.localPosition = Vector3.zero;
		yield return null;
	}

	private void SpawnTurban()
	{
		if (!skipMarionette)
		{
			spawner = spawnerPrefab.Create(new Vector3(base.transform.position.x, (float)Level.Current.Height + 100f), PlayerManager.GetNext(), base.properties.CurrentState.bullets);
			spawner.isDead = false;
		}
	}

	private IEnumerator shoot_cr()
	{
		LevelProperties.FlyingGenie.Bullets p = base.properties.CurrentState.bullets;
		int mainShotIndex = UnityEngine.Random.Range(0, p.shotCount.Length);
		string[] shotCount = p.shotCount[mainShotIndex].Split(',');
		int shotIndex = 0;
		string[] pinkCount = p.pinkString.Split(',');
		int pinkIndex = 0;
		while (state == State.Marionette)
		{
			isShooting = false;
			shotCount = p.shotCount[mainShotIndex].Split(',');
			yield return CupheadTime.WaitForSeconds(this, p.hesitateRange.RandomFloat());
			isShooting = true;
			base.animator.SetBool("IsAttacking", value: true);
			yield return base.animator.WaitForAnimationToEnd(this, "Marionette_Attack_Start");
			AudioManager.Play("genie_voice_laugh_reverb");
			AbstractPlayerController player = PlayerManager.GetNext();
			base.animator.Play("Marionette_Spark");
			for (int i = 0; i < shotCount.Length; i++)
			{
				for (int j = 0; j < Parser.IntParse(shotCount[shotIndex]); j++)
				{
					if (player == null || player.IsDead)
					{
						player = PlayerManager.GetNext();
					}
					Vector3 dir = player.transform.position - marionetteShootRoot.transform.position;
					if (dir.x > 0f)
					{
						dir.x = 0f;
					}
					if (pinkCount[pinkIndex][0] == 'P')
					{
						pinkBullet.Create(marionetteShootRoot.transform.position, MathUtils.DirectionToAngle(dir), p.shotSpeed);
						AudioManager.Play("genie_puppet_shoot");
						emitAudioFromObject.Add("genie_puppet_shoot");
					}
					else if (pinkCount[pinkIndex][0] == 'R')
					{
						shotBullet.Create(marionetteShootRoot.transform.position, MathUtils.DirectionToAngle(dir), p.shotSpeed);
						AudioManager.Play("genie_puppet_shoot");
						emitAudioFromObject.Add("genie_puppet_shoot");
					}
					yield return WaitWhileShooting(p.shotDelay, p.shotSpeed);
					pinkIndex = (pinkIndex + 1) % pinkCount.Length;
				}
				if (player == null || player.IsDead)
				{
					player = PlayerManager.GetNext();
				}
				yield return WaitWhileShooting(p.shotDelay, p.shotSpeed);
				if (shotIndex < shotCount.Length - 1)
				{
					shotIndex++;
				}
				else
				{
					mainShotIndex = (mainShotIndex + 1) % p.shotCount.Length;
					shotIndex = 0;
				}
				yield return null;
			}
			yield return null;
			base.animator.SetBool("IsAttacking", value: false);
		}
		yield return null;
	}

	private IEnumerator WaitWhileShooting(float time, float shootSpeed)
	{
		bool pointingUp = false;
		float timeEsalpsed = 0f;
		float timeSinceSubShot = 0f;
		while (timeEsalpsed <= time)
		{
			if (timeSinceSubShot >= 0.12f)
			{
				shootBullet.Create(marionetteShootRoot.transform.position, (!pointingUp) ? (-100) : 100, shootSpeed);
				pointingUp = !pointingUp;
				timeSinceSubShot = 0f;
			}
			timeEsalpsed += (float)CupheadTime.Delta;
			timeSinceSubShot += (float)CupheadTime.Delta;
			yield return null;
		}
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		while (state == State.Marionette)
		{
			if (!isShooting)
			{
				if (base.transform.position.x > 0f - startPos.x)
				{
					base.transform.AddPosition((0f - base.properties.CurrentState.bullets.marionetteMoveSpeed) * CupheadTime.FixedDelta);
				}
			}
			else if (base.transform.position.x < startPos.x)
			{
				base.transform.AddPosition(base.properties.CurrentState.bullets.marionetteReturnSpeed * CupheadTime.FixedDelta);
			}
			yield return wait;
		}
	}

	public void EndMarionette()
	{
		if (!skipMarionette)
		{
			AudioManager.Play("genie_puppet_exit");
			emitAudioFromObject.Add("genie_puppet_exit");
		}
		if (spawner != null)
		{
			spawner.isDead = true;
		}
		spark.SetActive(value: false);
		StopAllCoroutines();
		state = State.Giant;
		StartCoroutine(genie_intro_cr());
	}

	private void MarionetteDead()
	{
		GetComponent<Collider2D>().enabled = false;
		base.animator.SetTrigger("MarionetteDeath");
	}

	private IEnumerator genie_intro_cr()
	{
		float pullSpeed = 700f;
		float size = GetComponent<SpriteRenderer>().bounds.size.x;
		float angle = 120f;
		int number = 1;
		if (!skipMarionette)
		{
			base.animator.SetTrigger("MarionetteDeath");
			GetComponent<LevelBossDeathExploder>().StartExplosion();
		}
		yield return CupheadTime.WaitForSeconds(this, 1f);
		while (base.transform.position.y < 960f)
		{
			base.transform.AddPosition(0f, pullSpeed * (float)CupheadTime.Delta);
			yield return null;
		}
		if (!skipMarionette)
		{
			GetComponent<LevelBossDeathExploder>().StopExplosions();
		}
		yield return CupheadTime.WaitForSeconds(this, 0.7f);
		base.animator.Play("Giant_Intro");
		base.transform.position = new Vector3(640f + size / 3f, 0f);
		Vector3 startPos = base.transform.position;
		float t = 0f;
		float time = 1f;
		while (t < time)
		{
			float val = EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t / time);
			base.transform.position = Vector2.Lerp(startPos, giantRoot.position, val);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		base.transform.position = giantRoot.position;
		for (int i = 0; i < 3; i++)
		{
			SpawnPyramids(angle * ((float)Math.PI / 180f) * (float)i, number);
			number++;
		}
		StartCoroutine(attack_cr());
		yield return null;
	}

	private void IntroHands()
	{
		StartCoroutine(intro_hands_cr());
	}

	private IEnumerator intro_hands_cr()
	{
		Vector3 end = handFront.transform.position;
		Vector3 start = handFront.transform.position;
		start.y = handFront.transform.position.y - 500f;
		handFront.transform.position = start;
		handBack.transform.position = start;
		base.animator.Play("Giant_Hands");
		float t2 = 0f;
		float time = 1.25f;
		while (t2 < time)
		{
			float val = EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t2 / time);
			handFront.transform.position = Vector2.Lerp(start, end, val);
			handBack.transform.position = Vector2.Lerp(start, end, val);
			t2 += (float)CupheadTime.Delta;
			yield return null;
		}
		yield return CupheadTime.WaitForSeconds(this, 0.8f);
		t2 = 0f;
		while (t2 < time)
		{
			float val2 = EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t2 / time);
			handFront.transform.position = Vector2.Lerp(end, start, val2);
			handBack.transform.position = Vector2.Lerp(end, start, val2);
			t2 += (float)CupheadTime.Delta;
			yield return null;
		}
		base.animator.Play("Hands_Off");
		StartCoroutine(gem_stone_cr());
		yield return null;
	}

	private void SpawnPyramids(float startingAngle, int number)
	{
		LevelProperties.FlyingGenie.Pyramids pyramids = base.properties.CurrentState.pyramids;
		FlyingGenieLevelPyramid flyingGenieLevelPyramid = UnityEngine.Object.Instantiate(pyramidPrefab);
		flyingGenieLevelPyramid.Init(pyramids, base.transform.position, startingAngle, pyramids.speedRotation, pyramidPivotPoint, number, pyramidsGoingClockwise);
		flyingGenieLevelPyramid.GetComponent<Collider2D>().enabled = false;
		this.pyramids.Add(flyingGenieLevelPyramid);
	}

	private IEnumerator attack_cr()
	{
		LevelProperties.FlyingGenie.Pyramids p = base.properties.CurrentState.pyramids;
		string[] delayString = p.attackDelayString.GetRandom().Split(',');
		string[] attackString = p.pyramidAttackString.GetRandom().Split(',');
		int delayIndex = UnityEngine.Random.Range(0, delayString.Length);
		int attackIndex = UnityEngine.Random.Range(0, attackString.Length);
		float delay = 0f;
		int numberReceived = 0;
		float t = 0f;
		float time = 2.5f;
		foreach (FlyingGenieLevelPyramid pyramid in pyramids)
		{
			pyramid.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
		}
		while (t < time)
		{
			t += (float)CupheadTime.Delta;
			foreach (FlyingGenieLevelPyramid pyramid2 in pyramids)
			{
				pyramid2.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, t / time);
			}
			yield return null;
		}
		foreach (FlyingGenieLevelPyramid pyramid3 in pyramids)
		{
			pyramid3.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
			pyramid3.GetComponent<Collider2D>().enabled = true;
		}
		while (true)
		{
			int k;
			for (k = attackIndex; k < attackString.Length; k++)
			{
				Parser.FloatTryParse(delayString[delayIndex], out delay);
				yield return CupheadTime.WaitForSeconds(this, delay);
				string[] attackOrder = attackString[k].Split('-');
				string[] array = attackOrder;
				foreach (string s in array)
				{
					Parser.IntTryParse(s, out numberReceived);
					for (int m = 0; m < pyramids.Count; m++)
					{
						if (pyramids[m].number == numberReceived)
						{
							StartCoroutine(pyramids[m].beam_cr());
						}
					}
				}
				for (int i = 0; i < pyramids.Count; i++)
				{
					if (pyramids[i].number == numberReceived)
					{
						while (!pyramids[i].finishedATK)
						{
							yield return null;
						}
					}
				}
				attackIndex = 0;
				k %= attackString.Length;
				delayIndex = (delayIndex + 1) % delayString.Length;
			}
			yield return null;
		}
	}

	private IEnumerator gem_stone_cr()
	{
		LevelProperties.FlyingGenie.GemStone p = base.properties.CurrentState.gemStone;
		string[] attackDelayPattern = p.attackDelayString.GetRandom().Split(',');
		int delayIndex = UnityEngine.Random.Range(0, attackDelayPattern.Length);
		pinkString = p.pinkString.Split(',');
		pinkIndex = UnityEngine.Random.Range(0, pinkString.Length);
		float delay = 0f;
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, p.warningDuration);
			Parser.FloatTryParse(attackDelayPattern[delayIndex], out delay);
			base.animator.SetTrigger("OnGiantAttack");
			yield return base.animator.WaitForAnimationToEnd(this, "Giant_Attack");
			yield return CupheadTime.WaitForSeconds(this, delay);
			delayIndex = (delayIndex + 1) % attackDelayPattern.Length;
			yield return null;
		}
	}

	private void OnRing()
	{
		LevelProperties.FlyingGenie.GemStone gemStone = base.properties.CurrentState.gemStone;
		bool flag = false;
		FlyingGenieLevelRing flyingGenieLevelRing = null;
		this.gemStone.LookAt2D(PlayerManager.GetNext().center);
		if (pinkString[pinkIndex][0] == 'P')
		{
			flag = true;
			flyingGenieLevelRing = pinkRingPrefab.Create(this.gemStone.position, this.gemStone.eulerAngles.z, gemStone.bulletSpeed) as FlyingGenieLevelRing;
		}
		else
		{
			flag = false;
			flyingGenieLevelRing = ringPrefab.Create(this.gemStone.position, this.gemStone.eulerAngles.z, gemStone.bulletSpeed) as FlyingGenieLevelRing;
		}
		StartCoroutine(ring_cr(flyingGenieLevelRing, flag));
		pinkIndex = (pinkIndex + 1) % pinkString.Length;
	}

	private IEnumerator ring_cr(FlyingGenieLevelRing ring, bool isPink)
	{
		ring.isMain = true;
		int frameCount = 0;
		float frameTime = 0f;
		FlyingGenieLevelRing trailRing = ((!isPink) ? ringPrefab : pinkRingPrefab);
		FlyingGenieLevelRing lastRing = null;
		while (ring != null)
		{
			frameTime += (float)CupheadTime.Delta;
			if (frameTime > 1f / 24f)
			{
				if (frameCount < 3)
				{
					frameCount++;
				}
				else
				{
					frameCount = 0;
					if (lastRing != null)
					{
						lastRing.DisableCollision();
					}
					lastRing = trailRing.Create(ring.transform.position, gemStone.eulerAngles.z, 0.1f) as FlyingGenieLevelRing;
				}
				frameTime -= 1f / 24f;
				yield return null;
			}
			yield return null;
		}
		yield return null;
	}

	private void StartDeath()
	{
		if (skipMarionette && tinyMarionette != null)
		{
			tinyMarionette.Die();
		}
		base.animator.SetTrigger("Death");
	}

	private void SpawnPuff()
	{
		deathPuffEffect.Create(deathPuffRoot.transform.position);
	}

	private void HandSFX()
	{
		AudioManager.Play("genie_puppet_hand_enter");
		emitAudioFromObject.Add("genie_puppet_hand_enter");
	}

	private void SoundGenieVoiceMorph()
	{
		AudioManager.Play("genie_voice_excited");
		emitAudioFromObject.Add("genie_voice_excited");
	}

	private void SoundPuppetRun()
	{
		AudioManager.Play("genie_puppet_run");
		emitAudioFromObject.Add("genie_puppet_run");
	}

	private void SoundGenieVoicePhase3Intro()
	{
		AudioManager.Play("genie_voice_phase3_intro");
		emitAudioFromObject.Add("genie_voice_phase3_intro");
	}

	private void SoundGenieMindShoot()
	{
		AudioManager.Play("genie_phase3_mind_shoot");
		emitAudioFromObject.Add("genie_phase3_mind_shoot");
	}

	private void SoundPuppetSmallEnterMobile()
	{
		AudioManager.Play("genie_puppetsmall_enter_mobile");
		emitAudioFromObject.Add("genie_puppetsmall_enter_mobile");
	}
}
