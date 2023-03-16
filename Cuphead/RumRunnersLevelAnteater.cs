using System;
using System.Collections;
using UnityEngine;

public class RumRunnersLevelAnteater : LevelProperties.RumRunners.Entity
{
	private enum AttackIntro
	{
		Initial = -1,
		A,
		B,
		Final
	}

	private enum AttackIntroLength
	{
		Standard,
		Long
	}

	private static readonly float SNOUT_SPAWN_X = 0f;

	private static readonly int EyesAnimationLayer = 1;

	private static readonly int HandsAnimatorLayer = 2;

	private static readonly int DeathDustAnimatorLayer = 3;

	private static readonly int HandsUpAnimatorHash = Animator.StringToHash("HandsUp");

	private static readonly int HandsDownAnimatorHash = Animator.StringToHash("HandsDown");

	private Vector2 eyePositionAttack = new Vector2(348f, 145f);

	[SerializeField]
	private Transform[] spawnPoints;

	[SerializeField]
	private Vector2[] snoutShadowPositions;

	[SerializeField]
	private RumRunnersLevelSnout snout;

	[SerializeField]
	private RumRunnersLevelMobBoss mobBoss;

	[SerializeField]
	private Transform mobBossHelperTransform;

	[SerializeField]
	private CollisionChild[] collChildren;

	[SerializeField]
	private RumRunnersLevelSnoutTongue tongue;

	[SerializeField]
	private SpriteRenderer[] flipRenderers;

	[SerializeField]
	private float blinkProbability;

	[SerializeField]
	private GameObject eyes;

	private DamageDealer damageDealer;

	private PatternString snoutAttackPattern;

	private PatternString snoutPositionPattern;

	private bool onLeft = true;

	private bool firstAttack = true;

	private RumRunnersLevelSnout.AttackType queuedAttack;

	private void OnEnable()
	{
		GetComponent<DamageReceiver>().OnDamageTaken += onDamageTakenEventHandler;
	}

	private void OnDisable()
	{
		GetComponent<DamageReceiver>().OnDamageTaken -= onDamageTakenEventHandler;
	}

	private void Start()
	{
		damageDealer = DamageDealer.NewEnemy();
		CollisionChild[] array = collChildren;
		foreach (CollisionChild collisionChild in array)
		{
			collisionChild.OnPlayerCollision += OnCollisionPlayer;
		}
		tongue.OnPlayerCollision += OnCollisionPlayer;
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	public void DoDamage(float damage)
	{
		base.properties.DealDamage(damage);
	}

	private void onDamageTakenEventHandler(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (damageDealer != null && phase == CollisionPhase.Enter)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public override void LevelInit(LevelProperties.RumRunners properties)
	{
		base.LevelInit(properties);
		snoutAttackPattern = new PatternString(properties.CurrentState.anteaterSnout.snoutActionArray, randomizeMain: true, randomizeSub: false);
		snoutPositionPattern = new PatternString(properties.CurrentState.anteaterSnout.snoutPosString);
		snout.Setup(properties);
	}

	public void StartAnteater()
	{
		StartCoroutine(snout_cr());
	}

	private IEnumerator snout_cr()
	{
		LevelProperties.RumRunners.AnteaterSnout p = base.properties.CurrentState.anteaterSnout;
		while (true)
		{
			AttackIntro attackIntro = ((!Rand.Bool()) ? AttackIntro.B : AttackIntro.A);
			base.animator.SetInteger("AttackIntro", (int)attackIntro);
			int count = snoutAttackPattern.SubStringLength();
			for (int i = 0; i < count; i++)
			{
				parseAttackString(snoutAttackPattern.GetString(), out var attackType, out var _);
				snoutAttackPattern.IncrementString();
				parseAttackString(snoutAttackPattern.GetString(), out var nextAttackType, out var nextIntroLength);
				queuedAttack = attackType;
				if (p.anticipationBoilDelay > 0f)
				{
					yield return CupheadTime.WaitForSeconds(this, p.anticipationBoilDelay);
				}
				base.animator.SetTrigger("AnticipationComplete");
				while (!snout.isAttacking)
				{
					yield return null;
				}
				string endAnimationName;
				if (i == 0)
				{
					endAnimationName = "AttackInitialEnd";
				}
				else
				{
					string text = ((attackIntro != 0) ? "AttackB." : "AttackA.");
					endAnimationName = text + "AttackEnd";
				}
				bool nextAttackIsFinal = nextAttackType == RumRunnersLevelSnout.AttackType.Tongue || i == count - 1;
				if (!nextAttackIsFinal)
				{
					attackIntro = ((attackIntro == AttackIntro.A) ? AttackIntro.B : AttackIntro.A);
				}
				else
				{
					if (i < count - 1 && nextAttackType == RumRunnersLevelSnout.AttackType.Tongue)
					{
						snoutAttackPattern.IncrementString();
					}
					attackIntro = AttackIntro.Final;
				}
				base.animator.SetInteger("AttackIntro", (int)attackIntro);
				base.animator.SetBool("LongIntro", nextIntroLength == AttackIntroLength.Long);
				while (snout.isAttacking)
				{
					yield return null;
				}
				base.animator.SetTrigger("EndAttack");
				yield return base.animator.WaitForAnimationToEnd(this, endAnimationName);
				if (nextAttackIsFinal)
				{
					break;
				}
			}
			queuedAttack = RumRunnersLevelSnout.AttackType.Tongue;
			if (p.anticipationBoilDelay > 0f)
			{
				yield return CupheadTime.WaitForSeconds(this, p.anticipationBoilDelay);
			}
			base.animator.SetTrigger("AnticipationComplete");
			StartCoroutine(finalAttackEyes_cr());
			while (!snout.isAttacking)
			{
				yield return null;
			}
			while (snout.isAttacking)
			{
				yield return null;
			}
			base.animator.SetTrigger("EndAttack");
			base.animator.Play("Off", EyesAnimationLayer, 0f);
			base.animator.Update(0f);
			yield return base.animator.WaitForAnimationToStart(this, "AttackFinal.AttackEndHold");
			yield return CupheadTime.WaitForSeconds(this, p.finalAttackTauntDuration);
			base.animator.SetTrigger("EndTaunt");
			yield return base.animator.WaitForAnimationToEnd(this, "AttackFinal.AttackEnd");
		}
	}

	private IEnumerator finalAttackEyes_cr()
	{
		yield return base.animator.WaitForAnimationToEnd(this, "AttackFinal.AttackStart");
		base.animator.Play("Hold", EyesAnimationLayer, 0f);
		base.animator.Update(0f);
	}

	public void FakeDeathStart()
	{
		StopAllCoroutines();
		LevelProperties.RumRunners.Boss boss = base.properties.CurrentState.boss;
		mobBoss.Setup(base.properties, this, mobBossHelperTransform);
		SpriteRenderer[] array = flipRenderers;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.flipX = false;
		}
		snout.Death();
		base.animator.Play("Off", EyesAnimationLayer);
		base.animator.Play("Off", HandsAnimatorLayer);
		base.animator.Play("Death");
		base.animator.Update(0f);
		GetComponent<AnimationHelper>().Speed = 0f;
		eyes.transform.localPosition = new Vector3(2f, -62f);
		GetComponent<LevelBossDeathExploder>().StartExplosion(bypassCameraShakeEvent: true);
	}

	public void FakeDeathContinue()
	{
		GetComponent<AnimationHelper>().Speed = 1f;
	}

	private IEnumerator fakeDeathEyes_cr()
	{
		PatternString eyesPattern = new PatternString(base.properties.CurrentState.boss.anteaterEyeClosedOpenString);
		while (true)
		{
			string[] currentPattern = eyesPattern.PopString().Split(':');
			if (currentPattern.Length != 2)
			{
				break;
			}
			Parser.IntTryParse(currentPattern[0], out var closedCount);
			Parser.IntTryParse(currentPattern[1], out var openCount);
			yield return StartCoroutine(eyeHandler_cr(closedCount, "DeathLoop"));
			yield return base.animator.WaitForAnimationToStart(this, "DeathLoopEyeOpen");
			yield return StartCoroutine(eyeHandler_cr(openCount, "DeathLoopOpen"));
			yield return base.animator.WaitForAnimationToStart(this, "DeathLoopEyeClose");
		}
		throw new Exception("Invalid anteater eye pattern");
	}

	private IEnumerator eyeHandler_cr(int count, string loopAnimationName)
	{
		if (count == 0)
		{
			base.animator.SetTrigger("DeathLoopEyeChange");
			yield break;
		}
		yield return base.animator.WaitForAnimationToStart(this, loopAnimationName);
		while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < (float)count - 0.5f)
		{
			yield return null;
		}
		base.animator.SetTrigger("DeathLoopEyeChange");
	}

	public void RealDeath()
	{
		StopAllCoroutines();
		base.animator.Play("ActualDeath", 0, 0.2631579f);
	}

	private void animationEvent_LeftDirt()
	{
		CupheadLevelCamera.Current.Shake(20f, 0.3f, bypassEvent: true);
		((RumRunnersLevel)Level.Current).FullscreenDirt(1, -640f + UnityEngine.Random.Range(100f, 200f));
	}

	private void animationEvent_RightDirt()
	{
		CupheadLevelCamera.Current.Shake(20f, 0.3f, bypassEvent: true);
		((RumRunnersLevel)Level.Current).FullscreenDirt(1, 640f - UnityEngine.Random.Range(100f, 200f));
	}

	private void animationEvent_MiddleBridgeDestroy()
	{
		((RumRunnersLevel)Level.Current).DestroyMiddleBridge();
	}

	private void animationEvent_UpperBridgeDestroy()
	{
		((RumRunnersLevel)Level.Current).DestroyUpperBridge();
	}

	private void animationEvent_BridgeShatter()
	{
		((RumRunnersLevel)Level.Current).ShatterBridges();
	}

	private void animationEvent_InitialAttackStarted()
	{
		if (firstAttack)
		{
			firstAttack = false;
			return;
		}
		onLeft = !onLeft;
		SpriteRenderer[] array = flipRenderers;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.flipX = !onLeft;
		}
	}

	private void animationEvent_SnoutAttack()
	{
		int num = snoutPositionPattern.PopInt();
		Vector2 vector = default(Vector2);
		vector.x = ((!onLeft) ? SNOUT_SPAWN_X : (0f - SNOUT_SPAWN_X));
		vector.y = spawnPoints[num].position.y;
		snout.Attack(vector, snoutShadowPositions[num], onLeft, queuedAttack);
	}

	private void animationEvent_HandsUp()
	{
		int num = ((!onLeft) ? HandsDownAnimatorHash : HandsUpAnimatorHash);
		AnimatorStateInfo currentAnimatorStateInfo = base.animator.GetCurrentAnimatorStateInfo(HandsAnimatorLayer);
		if (currentAnimatorStateInfo.shortNameHash != num || currentAnimatorStateInfo.normalizedTime >= 1f)
		{
			base.animator.Play(num, HandsAnimatorLayer, 0f);
		}
		base.animator.Update(0f);
	}

	private void animationEvent_HandsDown()
	{
		int num = ((!onLeft) ? HandsUpAnimatorHash : HandsDownAnimatorHash);
		AnimatorStateInfo currentAnimatorStateInfo = base.animator.GetCurrentAnimatorStateInfo(HandsAnimatorLayer);
		if (currentAnimatorStateInfo.shortNameHash != num || currentAnimatorStateInfo.normalizedTime >= 1f)
		{
			base.animator.Play(num, HandsAnimatorLayer, 0f);
		}
		base.animator.Update(0f);
	}

	private void animationEvent_HandsUpHalfway()
	{
		int stateNameHash = ((!onLeft) ? HandsDownAnimatorHash : HandsUpAnimatorHash);
		AnimatorStateInfo currentAnimatorStateInfo = base.animator.GetCurrentAnimatorStateInfo(HandsAnimatorLayer);
		base.animator.Play(stateNameHash, HandsAnimatorLayer, 0.5f);
		base.animator.Update(0f);
	}

	private void animationEvent_HandsStartTaunt()
	{
		if (onLeft)
		{
			base.animator.Play("HandsTaunt", 2, 0f);
		}
		else
		{
			base.animator.Play("HandsTauntRight", 2, 0f);
		}
		base.animator.Update(0f);
	}

	private void animationEvent_HandsEndTaunt()
	{
		base.animator.SetTrigger("HandsEndTaunt");
	}

	private void animationEvent_FalseDeathDust()
	{
		base.animator.Play("DeathDust", DeathDustAnimatorLayer);
		CupheadLevelCamera.Current.Shake(35f, 0.5f);
	}

	private void animationEvent_FalseDeathEnded()
	{
		mobBoss.Begin();
		GetComponent<LevelBossDeathExploder>().StopExplosions();
		StartCoroutine(fakeDeathEyes_cr());
	}

	private void parseAttackString(string attackString, out RumRunnersLevelSnout.AttackType attackType, out AttackIntroLength introLength)
	{
		char c;
		char c2;
		if (attackString.Length == 2)
		{
			c = attackString[1];
			c2 = attackString[0];
		}
		else
		{
			c = attackString[0];
			c2 = '0';
		}
		switch (c)
		{
		case 'Q':
			attackType = RumRunnersLevelSnout.AttackType.Quick;
			break;
		case 'F':
			attackType = RumRunnersLevelSnout.AttackType.Fake;
			break;
		case 'T':
			attackType = RumRunnersLevelSnout.AttackType.Tongue;
			break;
		default:
			throw new Exception("Invalid attack string: " + attackString);
		}
		if (c2 == 'L')
		{
			introLength = AttackIntroLength.Long;
		}
		else
		{
			introLength = AttackIntroLength.Standard;
		}
	}

	public void TriggerEyesTurnaround()
	{
		base.animator.SetTrigger((!(snout.transform.position.y < 150f)) ? "EyesLookUp" : "EyesLookDown");
	}

	public void SetEyeSide(bool onLeft)
	{
		eyes.transform.localPosition = new Vector3((!onLeft) ? eyePositionAttack.x : (0f - eyePositionAttack.x), eyePositionAttack.y);
	}

	private void AnimationEvent_SFX_RUMRUN_P3_Anteater_Intro()
	{
		AudioManager.Play("sfx_dlc_rumrun_p3_anteater_intro");
		emitAudioFromObject.Add("sfx_dlc_rumrun_p3_anteater_intro");
	}

	private void AnimationEvent_SFX_RUMRUN_P3_Anteater_HandSlamFirst()
	{
		AudioManager.Play("sfx_dlc_rumrun_p3_anteater_handslamfirst");
		emitAudioFromObject.Add("sfx_dlc_rumrun_p3_anteater_handslamfirst");
	}

	private void AnimationEvent_SFX_RUMRUN_P3_Anteater_HandSlamSecond()
	{
		AudioManager.Play("sfx_dlc_rumrun_p3_anteater_handslamsecond");
		emitAudioFromObject.Add("sfx_dlc_rumrun_p3_anteater_handslamsecond");
	}

	private void AnimationEvent_SFX_RUMRUN_P3_Anteater_Intro_Hat_Off()
	{
		AudioManager.Play("sfx_dlc_rumrun_p3_anteater_intro_hat_off");
		emitAudioFromObject.Add("sfx_dlc_rumrun_p3_anteater_intro_hat_off");
	}

	private void AnimationEvent_SFX_RUMRUN_P3_Anteater_Intro_Hatton()
	{
		AudioManager.Play("sfx_dlc_rumrun_p3_anteater_intro_hatton");
		emitAudioFromObject.Add("sfx_dlc_rumrun_p3_anteater_intro_hatton");
	}

	private void AnimationEvent_SFX_RUMRUN_P3_AntEater_Attack_Start()
	{
		AudioManager.Play("sfx_dlc_rumrun_p3_anteater_attack_initial_start");
		emitAudioFromObject.Add("sfx_dlc_rumrun_p3_anteater_attack_initial_start");
	}

	private void AnimationEvent_SFX_RUMRUN_P4_IntroSnailLaugh()
	{
		AudioManager.Play("sfx_dlc_rumrun_vx_fakeannouncer_laughing");
		emitAudioFromObject.Add("sfx_dlc_rumrun_vx_fakeannouncer_laughing");
	}
}
