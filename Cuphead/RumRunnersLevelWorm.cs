using System.Collections;
using UnityEngine;

public class RumRunnersLevelWorm : LevelProperties.RumRunners.Entity
{
	private static readonly float PositionYOffset = 20f;

	private static readonly float PositionYRayLength = 250f;

	[SerializeField]
	private Sprite[] dropShadowSprites;

	[SerializeField]
	private SpriteRenderer fakePhonographShadowRenderer;

	[SerializeField]
	private SpriteRenderer realPhonographShadowRenderer;

	[SerializeField]
	private Effect dropDustEffect;

	[SerializeField]
	private RumRunnersLevelLaser laserGroup1;

	[SerializeField]
	private RumRunnersLevelLaser laserGroup2;

	[SerializeField]
	private RumRunnersLevelDiamond diamond;

	[SerializeField]
	private RumRunnersLevelBarrel barrelPrefab;

	[SerializeField]
	private Transform runnerSpawnPointTop;

	[SerializeField]
	private Transform runnerSpawnPointBottom;

	[SerializeField]
	private AudioWarble audioWarble;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private bool canTakeDamage;

	private Vector3 phonographPos;

	private Vector3 offscreenPos;

	private float speed;

	private float groupSpeed1;

	private float groupSpeed2;

	private float bossMaxHealth;

	private bool topBarrelCop;

	private bool bottomBarrelCop;

	private int direction;

	private Coroutine runnersCoroutine;

	private Coroutine lasersChangeCoroutine;

	private Coroutine lasersTurnOnCoroutine;

	private BoxCollider2D boxCollider;

	public bool introDrop { get; set; }

	public bool isDead { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		boxCollider = GetComponent<BoxCollider2D>();
		boxCollider.enabled = false;
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	public override void LevelInit(LevelProperties.RumRunners properties)
	{
		base.LevelInit(properties);
	}

	public void Setup()
	{
		base.gameObject.SetActive(value: true);
		phonographPos = base.transform.position;
		Vector3 position = laserGroup1.transform.parent.position;
		Vector3 position2 = new Vector3(base.transform.position.x, 720f);
		base.transform.position = position2;
		diamond.transform.position = phonographPos;
		laserGroup1.transform.parent.position = position;
	}

	public void StartWorm(float introDamage)
	{
		bossMaxHealth = base.properties.CurrentHealth;
		if (introDamage > 0f)
		{
			base.properties.DealDamage(introDamage * base.properties.CurrentState.worm.introDamageMultiplier);
			GetNewSpeed();
		}
		diamond.transform.parent = null;
		laserGroup1.transform.parent.parent = null;
		speed = base.properties.CurrentState.worm.rotationSpeedRange.min;
		StartCoroutine(bugIntro_cr());
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (canTakeDamage)
		{
			base.properties.DealDamage(info.damage);
			GetNewSpeed();
			if (Level.Current.mode == Level.Mode.Easy && !isDead && base.properties.CurrentHealth <= 0f)
			{
				StartDeath();
			}
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		damageDealer.DealDamage(hit);
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	public void StartBarrels()
	{
		runnersCoroutine = StartCoroutine(spawnRunners_cr());
	}

	private void AniEvent_StartBug()
	{
		StartCoroutine(revealLaser_cr());
		lasersChangeCoroutine = StartCoroutine(lasersChangeDir_cr());
		lasersTurnOnCoroutine = StartCoroutine(lasersTurnOn_cr());
		StartCoroutine(move_cr());
	}

	private IEnumerator bugIntro_cr()
	{
		boxCollider.enabled = true;
		canTakeDamage = true;
		YieldInstruction wait = new WaitForFixedUpdate();
		Vector3 startPos = new Vector3(base.transform.position.x, 720f);
		Vector3 endPos = phonographPos;
		offscreenPos = startPos;
		base.transform.position = startPos;
		base.animator.Play(0, 0, 0.2f);
		float elapsedTime2 = 0f;
		bool canDrop = false;
		while (!canDrop)
		{
			if (introDrop)
			{
				float normalizedTime = base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
				canDrop = normalizedTime >= 0.9f || normalizedTime <= 0.1f;
			}
			elapsedTime2 += CupheadTime.FixedDelta;
			base.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime2 / 1.65f);
			yield return wait;
		}
		base.animator.SetTrigger("Continue");
		elapsedTime2 = 0f;
		bool dustSpawned = false;
		float dropPosition = base.transform.position.y;
		while (elapsedTime2 < 0.6f)
		{
			elapsedTime2 += CupheadTime.FixedDelta;
			float t = elapsedTime2 / 0.6f;
			Vector3 position = base.transform.position;
			float startPosition = dropPosition;
			if (t >= 0.36363637f)
			{
				startPosition = (dropPosition - endPos.y) * 0.6f + endPos.y;
			}
			position.y = EaseUtils.EaseOutBounce(startPosition, endPos.y, t);
			base.transform.position = position;
			int shadowIndex = Mathf.Clamp(Mathf.RoundToInt(t * 10f), 0, dropShadowSprites.Length - 1);
			fakePhonographShadowRenderer.sprite = dropShadowSprites[shadowIndex];
			fakePhonographShadowRenderer.transform.position = endPos;
			if (!dustSpawned && t >= 0.36363637f)
			{
				dropDustEffect.Create(endPos);
				dustSpawned = true;
				CupheadLevelCamera.Current.Shake(10f, 0.3f);
				diamond.GetComponent<Collider2D>().enabled = true;
			}
			yield return wait;
		}
		base.animator.SetTrigger("Continue");
		yield return base.animator.WaitForNormalizedTime(this, 1f, "IntroEnd1", 0, allowEqualTime: true);
		fakePhonographShadowRenderer.sprite = null;
		base.animator.Play("IntroEnd2");
		diamond.animator.Play("Slack", 0);
		yield return diamond.animator.WaitForNormalizedTime(this, 1f, "Slack", 0, allowEqualTime: true);
		diamond.animator.Play("Loop", 0);
		diamond.animator.Play("Idle", 1);
	}

	private IEnumerator revealLaser_cr()
	{
		diamond.StartSparkle();
		laserGroup1.Begin();
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		laserGroup2.Begin();
	}

	private IEnumerator lasersChangeDir_cr()
	{
		LevelProperties.RumRunners.Worm p = base.properties.CurrentState.worm;
		string[] directionPattern = p.directionAttackString.GetRandom().Split(',');
		int directionIndex = Random.Range(0, directionPattern.Length);
		groupSpeed1 = speed;
		groupSpeed2 = 0f - speed;
		StartCoroutine(lasersRotate_cr());
		while (!isDead)
		{
			Parser.IntTryParse(directionPattern[directionIndex], out direction);
			if (direction == 1)
			{
				groupSpeed1 = speed;
				groupSpeed2 = 0f - speed;
			}
			else
			{
				groupSpeed1 = 0f - speed;
				groupSpeed2 = speed;
			}
			yield return CupheadTime.WaitForSeconds(this, p.directionTime);
			directionIndex = (directionIndex + 1) % directionPattern.Length;
		}
	}

	private IEnumerator lasersTurnOn_cr()
	{
		LevelProperties.RumRunners.Worm p = base.properties.CurrentState.worm;
		MusicSnapshot_StartGreenBeam();
		while (!isDead)
		{
			RumRunnersLevelLaser currentLaser = ((!Rand.Bool()) ? laserGroup2 : laserGroup1);
			yield return CupheadTime.WaitForSeconds(this, p.attackOffDurationRange.RandomFloat());
			yield return null;
			if (currentLaser != null)
			{
				currentLaser.Warning();
				MusicSnapshot_StartYellowBeam();
			}
			yield return CupheadTime.WaitForSeconds(this, p.warningDuration);
			yield return null;
			if (currentLaser != null)
			{
				lasersAttack(currentLaser);
				audioWarble.HandleWarble();
				MusicSnapshot_StartRedBeam();
			}
			yield return CupheadTime.WaitForSeconds(this, p.attackOnDurationRange.RandomFloat());
			yield return null;
			if (currentLaser != null)
			{
				lasersEndAttack(currentLaser);
				MusicSnapshot_StartGreenBeam();
			}
			yield return null;
		}
	}

	private IEnumerator lasersRotate_cr()
	{
		while (true)
		{
			if (laserGroup1 != null)
			{
				laserGroup1.transform.Rotate(Vector3.forward * groupSpeed1 * CupheadTime.Delta);
			}
			if (laserGroup2 != null)
			{
				laserGroup2.transform.Rotate(Vector3.forward * groupSpeed2 * CupheadTime.Delta);
			}
			yield return null;
		}
	}

	public void GetNewSpeed()
	{
		MinMax rotationSpeedRange = base.properties.CurrentState.worm.rotationSpeedRange;
		float num = base.properties.CurrentHealth / bossMaxHealth;
		float num2 = 1f - num;
		speed = rotationSpeedRange.min + rotationSpeedRange.max * num2;
		if (direction == 1)
		{
			groupSpeed1 = speed;
			groupSpeed2 = 0f - speed;
		}
		else
		{
			groupSpeed1 = 0f - speed;
			groupSpeed2 = speed;
		}
	}

	private void endLasers()
	{
		StopCoroutine(lasersTurnOnCoroutine);
		StopCoroutine(lasersChangeCoroutine);
		laserGroup1.CancelWarning();
		laserGroup2.CancelWarning();
		lasersEndAttack(laserGroup1);
		lasersEndAttack(laserGroup2);
		laserGroup1.End();
		laserGroup2.End();
		diamond.EndSparkle();
	}

	private void lasersAttack(RumRunnersLevelLaser laserGroup)
	{
		laserGroup.Attack();
		diamond.SetAttack(attack: true);
	}

	private void lasersEndAttack(RumRunnersLevelLaser laserGroup)
	{
		laserGroup.EndAttack();
		diamond.SetAttack(attack: false);
	}

	private IEnumerator spawnRunners_cr()
	{
		LevelProperties.RumRunners.Barrels p = base.properties.CurrentState.barrels;
		float topDirection = Rand.PosOrNeg();
		bool bottom = false;
		AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		if (player == null)
		{
			player = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		}
		if (player != null)
		{
			Vector3 position = player.transform.position;
			if ((position.x > 0f && topDirection < 0f) || (position.x < 0f && topDirection > 0f))
			{
				bottom = true;
			}
		}
		PatternString barrelDelayPattern = new PatternString(p.barrelDelayString);
		PatternString parryString = new PatternString(p.barrelParryString);
		while (base.properties.CurrentState.stateName != LevelProperties.RumRunners.States.Anteater)
		{
			bool isCop = ((!bottom) ? topBarrelCop : bottomBarrelCop);
			bottomBarrelCop = ((!bottom) ? bottomBarrelCop : (!bottomBarrelCop));
			topBarrelCop = ((!bottom) ? (!topBarrelCop) : topBarrelCop);
			RumRunnersLevelBarrel r = barrelPrefab.InstantiatePrefab<RumRunnersLevelBarrel>();
			bool parryable = !isCop && parryString.PopLetter() == 'P';
			Vector3 spawnPos = ((!bottom) ? runnerSpawnPointTop.position : runnerSpawnPointBottom.position);
			float direction = topDirection * (float)((!bottom) ? 1 : (-1));
			spawnPos.x *= direction;
			r.LevelInit(base.properties);
			r.Initialize(direction, spawnPos, this, parryable, isCop);
			bottom = !bottom;
			float delayTime = barrelDelayPattern.PopFloat();
			yield return CupheadTime.WaitForSeconds(this, delayTime);
		}
	}

	private IEnumerator move_cr()
	{
		bool movingOut = true;
		float time = base.properties.CurrentState.worm.moveTime;
		Vector3 startPos = new Vector3((0f - base.properties.CurrentState.worm.moveDistance) / 2f, base.transform.position.y);
		Vector3 endPos = new Vector3(base.properties.CurrentState.worm.moveDistance / 2f, base.transform.position.y);
		float t = time / 2f;
		bool kick = false;
		bool endMove = false;
		AnimationEvent_SFX_RUMRUN_BugGirl_Tapdance();
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		int initialSortingOrder = spriteRenderer.sortingOrder;
		spriteRenderer.sortingOrder = 10;
		YieldInstruction waitInstruction = new WaitForFixedUpdate();
		bool initialLoop = true;
		while (!endMove)
		{
			float start = ((!movingOut) ? endPos.x : startPos.x);
			float end = ((!movingOut) ? startPos.x : endPos.x);
			if (initialLoop)
			{
				start = base.transform.position.x;
				t = 0f;
				time /= 2f;
			}
			while (t < time && !isDead)
			{
				t += CupheadTime.FixedDelta;
				float val = t / time;
				Vector3 position = base.transform.position;
				position.x = EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, start, end, val);
				position.y = RumRunnersLevel.GroundWalkingPosY(position, boxCollider, PositionYOffset, PositionYRayLength);
				base.transform.position = position;
				yield return waitInstruction;
				if (!kick && val > 0.7f)
				{
					kick = true;
					string trigger = ((!(end > 0f)) ? "KickLeft" : "KickRight");
					base.animator.SetTrigger(trigger);
				}
			}
			if (isDead)
			{
				endMove = true;
				StartCoroutine(defeat_cr());
			}
			else
			{
				movingOut = !movingOut;
				kick = false;
				t = 0f;
				base.transform.SetPosition(end);
				if (initialLoop)
				{
					time *= 2f;
					initialLoop = false;
				}
			}
			yield return null;
		}
		StartCoroutine(deathMove_cr(initialSortingOrder));
	}

	private IEnumerator defeat_cr()
	{
		base.animator.SetBool("EasyMode", Level.Current.mode == Level.Mode.Easy);
		base.animator.Play("Defeat");
		base.transform.localScale = new Vector3(Mathf.Sign(base.transform.position.x), 1f);
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		diamond.animator.Play("Defeat");
	}

	private IEnumerator deathMove_cr(int initialSortingOrder)
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		float FALL_SPEED = ((Level.Current.mode != 0) ? 400f : 200f);
		bool flipped = base.transform.position.x < 0f;
		float POS_X = 612f * Mathf.Sign(base.transform.position.x);
		float fallTime = (Mathf.Abs(POS_X) - Mathf.Abs(base.transform.position.x)) / FALL_SPEED;
		float startPos = base.transform.position.x;
		float endpos = POS_X;
		float elapsedTime = 0f;
		while (elapsedTime < fallTime)
		{
			elapsedTime += CupheadTime.FixedDelta;
			Vector3 position = base.transform.position;
			position.x = Mathf.Lerp(startPos, endpos, elapsedTime / fallTime);
			position.y = RumRunnersLevel.GroundWalkingPosY(position, boxCollider, PositionYOffset, PositionYRayLength);
			base.transform.position = position;
			yield return wait;
		}
		base.animator.SetTrigger("Continue");
		if (Level.Current.mode == Level.Mode.Easy)
		{
			StopAllCoroutines();
			yield break;
		}
		yield return base.animator.WaitForAnimationToEnd(this, "Fall");
		canTakeDamage = false;
		GetComponent<HitFlash>().disabled = true;
		GetComponent<SpriteRenderer>().sortingOrder = initialSortingOrder;
		elapsedTime = 0f;
		startPos = base.transform.position.x;
		float targetPositionX = ((!flipped) ? phonographPos.x : (-105f));
		base.animator.enabled = false;
		while (elapsedTime < 2f)
		{
			Vector2 position2 = base.transform.position;
			position2.x = Mathf.Lerp(startPos, targetPositionX, elapsedTime / 2f);
			position2.y = RumRunnersLevel.GroundWalkingPosY(position2, boxCollider, PositionYOffset, PositionYRayLength);
			base.transform.position = position2;
			yield return null;
			base.animator.Update(CupheadTime.Delta);
			elapsedTime = base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}
		base.transform.SetPosition(targetPositionX);
		base.animator.enabled = true;
		base.animator.SetBool("Flipped", flipped);
		base.animator.SetTrigger("End");
		string animationName = ((!flipped) ? "Jump" : "JumpFlipped");
		yield return base.animator.WaitForNormalizedTime(this, 1f, animationName, 0, allowEqualTime: true);
		animationName = ((!flipped) ? "JumpSquish" : "JumpSquishFlipped");
		base.animator.Play(animationName);
		diamond.animator.Play((!flipped) ? "DefeatSquish" : "DefeatSquishFlipped");
		yield return base.animator.WaitForNormalizedTime(this, 1f, animationName, 0, allowEqualTime: true);
		base.transform.SetPosition(phonographPos.x * Mathf.Sign(base.transform.position.x) + ((!flipped) ? 0f : (-6f)));
		base.animator.Play("Wave");
		diamond.GetComponent<Collider2D>().enabled = false;
		elapsedTime = 0f;
		Vector3 start = base.transform.position;
		Vector3 targetPosition = new Vector3(base.transform.position.x, offscreenPos.y);
		diamond.transform.parent = base.transform;
		StartCoroutine(exitShadow_cr());
		while (elapsedTime < 0.866f)
		{
			elapsedTime += CupheadTime.FixedDelta;
			base.transform.position = Vector3.Lerp(start, targetPosition, elapsedTime / 0.866f);
			yield return wait;
		}
		StopAllCoroutines();
		diamond.Die();
		Object.Destroy(base.gameObject);
		yield return null;
	}

	private IEnumerator exitShadow_cr()
	{
		realPhonographShadowRenderer.enabled = false;
		Vector3 position = realPhonographShadowRenderer.transform.position;
		float accumulator = 0f;
		int index = 4;
		while (index >= 0)
		{
			fakePhonographShadowRenderer.sprite = dropShadowSprites[index];
			fakePhonographShadowRenderer.transform.position = position;
			yield return null;
			accumulator += (float)CupheadTime.Delta;
			if (accumulator > 1f / 24f)
			{
				accumulator -= 1f / 24f;
				index--;
			}
		}
		fakePhonographShadowRenderer.sprite = null;
	}

	public void StartDeath()
	{
		if (!isDead)
		{
			AnimationEvent_SFX_RUMRUN_BugGirl_Tapdance_Stop();
			SFX_RUMRUN_BugGirl_DieFalltoGround();
			MusicSnapshot_RevertToDefault();
			isDead = true;
			endLasers();
			StopCoroutine(runnersCoroutine);
			if (Level.Current.mode == Level.Mode.Easy)
			{
				GetComponent<LevelBossDeathExploder>().StartExplosion();
			}
		}
	}

	protected virtual void MusicSnapshot_StartGreenBeam()
	{
		AudioManager.SnapshotTransition(new string[3] { "RumRunners_GreenBeam", "Unpaused", "Unpaused_1920s" }, new float[3] { 1f, 0f, 0f }, 0.5f);
	}

	protected virtual void MusicSnapshot_StartYellowBeam()
	{
		AudioManager.SnapshotTransition(new string[3] { "RumRunners_YellowBeam", "Unpaused", "Unpaused_1920s" }, new float[3] { 1f, 0f, 0f }, 0.5f);
	}

	protected virtual void MusicSnapshot_StartRedBeam()
	{
		AudioManager.SnapshotTransition(new string[3] { "RumRunners_RedBeam", "RumRunners_GreenBeam", "Unpaused_1920s" }, new float[3] { 1f, 0f, 0f }, 0.16f);
	}

	protected virtual void MusicSnapshot_RevertToDefault()
	{
		string[] array = new string[2] { "RumRunners_RedBeam", null };
		if (SettingsData.Data.vintageAudioEnabled)
		{
			array[1] = "Unpaused_1920s";
		}
		else
		{
			array[1] = "Unpaused";
		}
		AudioManager.SnapshotTransition(array, new float[2] { 0f, 1f }, 3f);
	}

	private IEnumerator StartRedBeamMusicSnapshotWait_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 3f);
	}

	private void SFX_RUMRUN_BugGirl_DieFalltoGround()
	{
		AudioManager.Play("sfx_DLC_RUMRUN_P2_BugGirl_DieFalltoGround");
		emitAudioFromObject.Add("sfx_DLC_RUMRUN_P2_BugGirl_DieFalltoGround");
	}

	private void AnimationEvent_SFX_RUMRUN_BugGirl_DismountJumpLand()
	{
		AudioManager.Play("sfx_DLC_RUMRUN_P2_BugGirl_DismountJumpLand");
		emitAudioFromObject.Add("sfx_DLC_RUMRUN_P2_BugGirl_DismountJumpLand");
	}

	private void AnimationEvent_SFX_RUMRUN_BugGirl_ExitWinch()
	{
		AudioManager.Play("sfx_DLC_RUMRUN_P2_BugGirl_ExitWinch");
		emitAudioFromObject.Add("sfx_DLC_RUMRUN_P2_BugGirl_ExitWinch");
	}

	private void AnimationEvent_SFX_RUMRUN_BugGirl_Tapdance()
	{
		AudioManager.PlayLoop("sfx_dlc_rumrun_p2_buggirl_tapdance");
		emitAudioFromObject.Add("sfx_dlc_rumrun_p2_buggirl_tapdance");
	}

	private void AnimationEvent_SFX_RUMRUN_BugGirl_Tapdance_Stop()
	{
		AudioManager.Stop("sfx_dlc_rumrun_p2_buggirl_tapdance");
	}

	private void AnimationEvent_SFX_RUMRUN_BugGirl_VocalDismountLaugh()
	{
		AudioManager.Play("sfx_DLC_RUMRUN_P2_BugGirl_VocalDismountLaugh");
		emitAudioFromObject.Add("sfx_DLC_RUMRUN_P2_BugGirl_VocalDismountLaugh");
	}

	private void AnimationEvent_SFX_RUMRUN_BugGirl_VocalExcited()
	{
		AudioManager.Play("sfx_DLC_RUMRUN_P2_BugGirl_VocalExcited");
		emitAudioFromObject.Add("sfx_DLC_RUMRUN_P2_BugGirl_VocalExcited");
	}
}
