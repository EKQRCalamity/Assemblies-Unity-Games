using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessQueenLevelQueen : LevelProperties.ChessQueen.Entity
{
	public enum States
	{
		Idle,
		Lightning,
		Egg
	}

	[SerializeField]
	private SpriteRenderer[] dressRenderers;

	[SerializeField]
	private float easeCoefficient;

	[SerializeField]
	private float accel = 2f;

	[SerializeField]
	private float attackDecel = 2f;

	[SerializeField]
	private bool useSineEasing;

	[SerializeField]
	private float minMoveDistanceAfterLightning = 100f;

	private bool flipPositionString;

	[SerializeField]
	private HitFlash hitFlash;

	[SerializeField]
	private ChessQueenLevelCannon cannonLeft;

	[SerializeField]
	private ChessQueenLevelCannon cannonMiddle;

	[SerializeField]
	private ChessQueenLevelCannon cannonRight;

	[SerializeField]
	private Transform head;

	[SerializeField]
	private ChessQueenLevelLooseMouse mouse;

	private float headWobbleCurrentAmplitude;

	private float headWobbleTimer;

	[SerializeField]
	private float headWobbleSpeed = 50f;

	[SerializeField]
	private float headWobbleAmplitude = 25f;

	[SerializeField]
	private float headWobbleDecay = 50f;

	[Header("Egg")]
	[SerializeField]
	private ChessQueenLevelEgg eggPrefab;

	[SerializeField]
	private Transform eggRootRight;

	[SerializeField]
	private Transform eggRootLeft;

	[SerializeField]
	private float maxTimeToHoldForTwoEggAttacks;

	[SerializeField]
	private float maxTimeToStayOpenForTwoEggAttacks = 0.7f;

	[Header("Lightning")]
	[SerializeField]
	private ChessQueenLevelLightning lightningPrefab;

	[SerializeField]
	public float lightningDisableRange = 150f;

	private float lastXPos;

	private int cannonCycleDirection;

	private List<ChessQueenLevelCannon> cannons;

	private int activeCannonIndex;

	private PatternString delayPattern;

	private PatternString lightningPositionPattern;

	private PatternString positionPattern;

	private bool movingLeft;

	private bool isMoving;

	private bool dead;

	public float speedThresholdForFastAnimation;

	public States state { get; private set; }

	public ChessQueenLevelLightning activeLightning { get; private set; }

	public override void LevelInit(LevelProperties.ChessQueen properties)
	{
		base.LevelInit(properties);
		Level.Current.OnIntroEvent += onIntroEventHandler;
		LevelProperties.ChessQueen.Turret turret = properties.CurrentState.turret;
		cannons = new List<ChessQueenLevelCannon>();
		cannonLeft.SetProperties(turret.leftTurretRange.min, turret.leftTurretRange.max, turret.leftTurretRotationTime, ChessQueenLevelCannon.CannonPosition.Side, turret, this);
		cannons.Add(cannonLeft);
		cannonMiddle.SetProperties(turret.middleTurretRange.min, turret.middleTurretRange.max, turret.middleTurretRotationTime, ChessQueenLevelCannon.CannonPosition.Center, turret, this);
		cannons.Add(cannonMiddle);
		cannonRight.SetProperties(turret.rightTurretRange.min, turret.rightTurretRange.max, turret.rightTurretRotationTime, ChessQueenLevelCannon.CannonPosition.Side, turret, this);
		cannons.Add(cannonRight);
		cannonCycleDirection = MathUtils.PlusOrMinus();
		activeCannonIndex = ((cannonCycleDirection == -1) ? 2 : 0);
		cannons[activeCannonIndex].IsActive = true;
		StartCoroutine(check_cannons_cr());
		delayPattern = new PatternString(properties.CurrentState.queen.queenAttackDelayString);
		lightningPositionPattern = new PatternString(properties.CurrentState.lightning.lightningPositionString);
		flipPositionString = Rand.Bool();
		positionPattern = new PatternString(properties.CurrentState.movement.queenPositionString);
		SFX_KOG_QUEEN_IntroTypeWriter();
	}

	protected override void OnCollisionEnemyProjectile(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionEnemyProjectile(hit, phase);
		if (phase == CollisionPhase.Enter)
		{
			ChessQueenLevelCannonball component = hit.GetComponent<ChessQueenLevelCannonball>();
			if ((bool)component)
			{
				receiveDamage();
				SFX_KOG_QUEEN_CannonHitQueenDing();
				component.HitQueen();
			}
		}
	}

	private void receiveDamage()
	{
		base.properties.DealDamage((!PlayerManager.BothPlayersActive()) ? 10f : ChessKingLevelKing.multiplayerDamageNerf);
		hitFlash.Flash(0.7f);
		if (base.properties.CurrentHealth <= 0f)
		{
			die();
		}
		else
		{
			mouse.HitQueen();
		}
		if (!base.animator.GetBool("OnLightning"))
		{
			headWobbleCurrentAmplitude = headWobbleAmplitude;
			headWobbleTimer = 0f;
		}
	}

	public void StateChanged()
	{
		delayPattern = new PatternString(base.properties.CurrentState.queen.queenAttackDelayString);
		lightningPositionPattern = new PatternString(base.properties.CurrentState.lightning.lightningPositionString);
		positionPattern = new PatternString(base.properties.CurrentState.movement.queenPositionString);
	}

	private void LateUpdate()
	{
		SpriteRenderer[] array = dressRenderers;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.enabled = base.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || base.animator.GetCurrentAnimatorStateInfo(0).IsTag("Egg");
		}
		head.localPosition = new Vector3(Mathf.Sin(headWobbleTimer) * headWobbleCurrentAmplitude, 0f);
		headWobbleTimer += (float)CupheadTime.Delta * headWobbleSpeed;
		if (headWobbleCurrentAmplitude > 0f)
		{
			headWobbleCurrentAmplitude -= (float)CupheadTime.Delta * headWobbleDecay;
			if (headWobbleCurrentAmplitude < 0f)
			{
				headWobbleCurrentAmplitude = 0f;
			}
		}
	}

	private void onIntroEventHandler()
	{
		StartCoroutine(intro_cr());
	}

	private IEnumerator intro_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.4f);
		base.animator.SetTrigger("Intro");
		yield return base.animator.WaitForAnimationToEnd(this, "Intro.Start");
		StartCoroutine(moving_cr());
	}

	private IEnumerator check_cannons_cr()
	{
		while (true)
		{
			if (cannons[activeCannonIndex].IsActive)
			{
				yield return null;
				continue;
			}
			activeCannonIndex = ((cannonCycleDirection <= 0) ? MathUtilities.PreviousIndex(activeCannonIndex, cannons.Count) : MathUtilities.NextIndex(activeCannonIndex, cannons.Count));
			cannons[activeCannonIndex].SetActive(setActive: true);
		}
	}

	private IEnumerator moving_cr()
	{
		LevelProperties.ChessQueen.Queen p = base.properties.CurrentState.queen;
		float moveSpeed = 0f;
		lastXPos = base.transform.position.x;
		base.animator.Play("MoveSlow", 1, 0f);
		base.animator.Update(0f);
		isMoving = true;
		bool firstMove = true;
		bool inLightning = false;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			if (isMoving)
			{
				float elapsed = 0f;
				float startX = base.transform.position.x;
				float endX = Mathf.Lerp((float)Level.Current.Left + 150f, (float)Level.Current.Right - 150f, (positionPattern.PopFloat() * (float)((!flipPositionString) ? 1 : (-1)) + 1f) / 2f);
				if (firstMove && endX > 0f)
				{
					endX = 0f - endX;
				}
				firstMove = false;
				movingLeft = endX < startX;
				float distance = Mathf.Abs(endX - startX);
				float time = distance / p.queenMovementSpeed;
				while (elapsed <= time)
				{
					if (!isMoving)
					{
						inLightning = true;
					}
					if (inLightning && isMoving)
					{
						inLightning = false;
						startX = base.transform.position.x;
						elapsed = 0f;
						distance = Mathf.Abs(endX - startX);
						int num = 0;
						while (distance < minMoveDistanceAfterLightning && num < positionPattern.SubStringLength())
						{
							endX = Mathf.Lerp((float)Level.Current.Left + 150f, (float)Level.Current.Right - 150f, (positionPattern.PopFloat() * (float)((!flipPositionString) ? 1 : (-1)) + 1f) / 2f);
							distance = Mathf.Abs(endX - startX);
							num++;
						}
						movingLeft = endX < startX;
						time = distance / p.queenMovementSpeed;
						moveSpeed = 0f;
					}
					if ((base.animator.GetCurrentAnimatorStateInfo(1).IsName("MoveSlow") && base.animator.GetCurrentAnimatorStateInfo(1).normalizedTime % 1f < 1f / 6f) || base.animator.GetCurrentAnimatorStateInfo(1).IsName("MoveEaseOut"))
					{
						SpriteRenderer[] array = dressRenderers;
						foreach (SpriteRenderer spriteRenderer in array)
						{
							spriteRenderer.flipX = !movingLeft;
						}
					}
					base.animator.SetBool("Fast", isMoving && Mathf.Abs(lastXPos - base.transform.position.x) > speedThresholdForFastAnimation && dressRenderers[0].flipX != lastXPos - base.transform.position.x > 0f);
					moveSpeed = Mathf.Clamp(moveSpeed + CupheadTime.FixedDelta * ((!isMoving) ? (0f - attackDecel) : accel), 0f, 1f);
					float t = elapsed / time;
					float val = ((!useSineEasing) ? EaseUtils.EaseInOutArbitraryCoefficient(startX, endX, t, easeCoefficient) : EaseUtils.EaseInOutSine(startX, endX, t));
					lastXPos = base.transform.position.x;
					base.transform.SetPosition(val);
					elapsed += CupheadTime.FixedDelta * moveSpeed;
					yield return wait;
				}
			}
			else
			{
				yield return null;
			}
		}
	}

	public void StartLightning()
	{
		state = States.Lightning;
		StartCoroutine(SFX_KOG_QUEEN_VocalAttack_cr());
		StartCoroutine(lightning_cr());
	}

	private IEnumerator lightning_cr()
	{
		LevelProperties.ChessQueen.Lightning p = base.properties.CurrentState.lightning;
		isMoving = false;
		base.animator.SetBool("Fast", value: false);
		base.animator.SetBool("OnLightning", value: true);
		headWobbleDecay *= 2f;
		yield return CupheadTime.WaitForSeconds(this, p.lightningAnticipationTime);
		base.animator.SetTrigger("OnAttack");
		while (activeLightning != null && !activeLightning.isGone)
		{
			yield return null;
		}
		base.animator.SetBool("OnLightning", value: false);
		base.animator.Play("MoveEaseOutHold", 1, 0f);
		base.animator.Update(0f);
		yield return base.animator.WaitForAnimationToEnd(this, "Lightning.Exit");
		base.animator.Play("MoveSlow", 1, 0f);
		base.animator.Update(0f);
		isMoving = true;
		headWobbleDecay *= 0.5f;
		yield return CupheadTime.WaitForSeconds(this, delayPattern.PopFloat());
		state = States.Idle;
	}

	private void AniEvent_CreateLightning()
	{
		AbstractPlayerController next = PlayerManager.GetNext();
		lightningPositionPattern.IncrementString();
		activeLightning = lightningPrefab.Spawn();
		activeLightning.Create((lightningPositionPattern.GetString()[0] != 'P') ? lightningPositionPattern.GetFloat() : next.transform.position.x, base.properties.CurrentState.lightning);
	}

	public void StartEgg()
	{
		state = States.Egg;
		base.animator.SetBool("Egg", value: true);
		StartCoroutine(egg_cr());
	}

	private IEnumerator egg_cr()
	{
		LevelProperties.ChessQueen.Egg p = base.properties.CurrentState.egg;
		yield return base.animator.WaitForAnimationToStart(this, "Egg.AttackLoop");
		SFX_KOG_QUEEN_FabergeEggLoop();
		SFX_KOG_QUEEN_FabergeEggTeethLoop();
		float rateTime = 0f;
		float attackTime = 0f;
		float attackDuration = p.eggAttackDuration.RandomFloat();
		eggRootLeft.SetPosition(null, null, 5E-07f);
		eggRootRight.SetPosition(null, null, 0f);
		while (attackTime < attackDuration)
		{
			attackTime += (float)CupheadTime.Delta;
			if (rateTime > p.eggFireRate)
			{
				fireProjectiles();
				rateTime = 0f;
			}
			else
			{
				rateTime += (float)CupheadTime.Delta;
			}
			yield return null;
		}
		float delay = delayPattern.PopFloat();
		if (p.eggCooldownDuration + delay < maxTimeToHoldForTwoEggAttacks && ((ChessQueenLevel)Level.Current).NextPatternIsEgg())
		{
			if (p.eggCooldownDuration + delay > maxTimeToStayOpenForTwoEggAttacks)
			{
				base.animator.SetTrigger("ResetEgg");
				base.animator.SetTrigger("EndAttack");
			}
			yield return CupheadTime.WaitForSeconds(this, p.eggCooldownDuration + delay);
			StartEgg();
		}
		else
		{
			base.animator.SetTrigger("EndAttack");
			SFX_KOG_QUEEN_FabergeEggLoopStopShort();
			yield return CupheadTime.WaitForSeconds(this, p.eggCooldownDuration);
			base.animator.SetBool("Egg", value: false);
			SFX_KOG_QUEEN_FabergeEggClose();
			SFX_KOG_QUEEN_FabergeEggLoopStopEnd();
			SFX_KOG_QUEEN_FabergeEggTeethLoopStop();
			yield return base.animator.WaitForAnimationToStart(this, "Egg.End");
			yield return CupheadTime.WaitForSeconds(this, delay);
			state = States.Idle;
		}
	}

	private void fireProjectiles()
	{
		LevelProperties.ChessQueen.Egg egg = base.properties.CurrentState.egg;
		Vector2 zero = Vector2.zero;
		float num = ((!movingLeft) ? (-200) : 200);
		zero.y = egg.eggVelocityY.RandomFloat();
		zero.x = egg.eggVelocityX.RandomFloat() + num;
		eggRootLeft.transform.position += Vector3.forward * 1E-06f;
		ChessQueenLevelEgg chessQueenLevelEgg = eggPrefab.Spawn();
		chessQueenLevelEgg.Create(eggRootLeft.position, zero, egg.eggGravity, egg.eggSpawnCollisionTimer);
		zero.y = egg.eggVelocityY.RandomFloat();
		zero.x = egg.eggVelocityX.RandomFloat() + num;
		eggRootLeft.transform.position += Vector3.forward * 1E-06f;
		ChessQueenLevelEgg chessQueenLevelEgg2 = eggPrefab.Spawn();
		chessQueenLevelEgg2.Create(eggRootRight.position, zero, egg.eggGravity, egg.eggSpawnCollisionTimer);
	}

	private void die()
	{
		if (!dead)
		{
			dead = true;
			mouse.Win();
			headWobbleCurrentAmplitude = 0f;
			StopAllCoroutines();
			if (base.transform.position.x > 0f)
			{
				base.transform.SetScale(-1f);
			}
			GetComponent<LevelBossDeathExploder>().offset.x *= base.transform.localScale.x;
			base.animator.Play("Death");
			StartCoroutine(SFX_KOG_QUEEN_Death_cr());
		}
	}

	private void SFX_KOG_QUEEN_IntroTypeWriter()
	{
		AudioManager.Play("sfx_dlc_kog_queen_introtypewriter");
	}

	private void AnimationEvent_SFX_KOG_QUEEN_IntroTableFlip()
	{
		AudioManager.Play("sfx_dlc_kog_queen_introtableflip");
	}

	private void AnimationEvent_SFX_KOG_QUEEN_FabergeEggOpen()
	{
		AudioManager.Play("sfx_dlc_kog_queen_fabergeegg_open");
		emitAudioFromObject.Add("sfx_dlc_kog_queen_fabergeegg_open");
	}

	private void SFX_KOG_QUEEN_FabergeEggClose()
	{
		AudioManager.Play("sfx_dlc_kog_queen_fabergeegg_close");
		emitAudioFromObject.Add("sfx_dlc_kog_queen_fabergeegg_close");
	}

	private void SFX_KOG_QUEEN_FabergeEggTeethLoop()
	{
		AudioManager.Play("sfx_dlc_kog_queen_fabergeeggteeth_loop");
		emitAudioFromObject.Add("sfx_dlc_kog_queen_fabergeeggteeth_loop");
	}

	private void SFX_KOG_QUEEN_FabergeEggTeethLoopStop()
	{
		AudioManager.Stop("sfx_dlc_kog_queen_fabergeeggteeth_loop");
	}

	private void SFX_KOG_QUEEN_FabergeEggLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_kog_queen_fabergeegg_loop");
		AudioManager.FadeSFXVolumeLinear("sfx_dlc_kog_queen_fabergeegg_loop", 0.7f, 2f);
		emitAudioFromObject.Add("sfx_dlc_kog_queen_fabergeegg_loop");
	}

	private void SFX_KOG_QUEEN_FabergeEggLoopStopShort()
	{
		AudioManager.Stop("sfx_dlc_kog_queen_fabergeegg_loop");
	}

	private void SFX_KOG_QUEEN_FabergeEggLoopStopEnd()
	{
		AudioManager.FadeSFXVolumeLinear("sfx_dlc_kog_queen_fabergeegg_loop", 0f, 1f);
	}

	private void AnimationEvent_SFX_KOG_QUEEN_SpawnChessPieces()
	{
		AudioManager.Play("sfx_dlc_kog_queen_spawnchesspieces");
		emitAudioFromObject.Add("sfx_dlc_kog_queen_spawnchesspieces");
	}

	private IEnumerator SFX_KOG_QUEEN_Death_cr()
	{
		SFX_KOG_QUEEN_FabergeEggLoopStopShort();
		yield return CupheadTime.WaitForSeconds(this, 0.1f);
		AudioManager.Play("sfx_dlc_kog_queen_death");
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		AudioManager.Play("sfx_dlc_kog_queen_vocal_death");
		yield return CupheadTime.WaitForSeconds(this, 0.7f);
		AudioManager.PlayLoop("sfx_dlc_kog_queen_deathcrownspin_loop");
	}

	private IEnumerator SFX_KOG_QUEEN_VocalAttack_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0f);
		AudioManager.Play("sfx_dlc_kog_queen_vocal_attack");
		emitAudioFromObject.Add("sfx_dlc_kog_queen_vocal_attack");
	}

	private void AnimationEvent_SFX_KOG_QUEEN_VocalHurt()
	{
		AudioManager.Play("sfx_dlc_kog_queen_vocal_hurt");
		emitAudioFromObject.Add("sfx_dlc_kog_queen_vocal_hurt");
	}

	private void AnimationEvent_SFX_KOG_QUEEN_VocalLaughLrg()
	{
		AudioManager.Play("sfx_dlc_kog_queen_vocal_laughlrg");
		emitAudioFromObject.Add("sfx_dlc_kog_queen_vocal_laughlrg");
	}

	private void AnimationEvent_SFX_KOG_QUEEN_VocalLaughSml()
	{
		AudioManager.Play("sfx_dlc_kog_queen_vocal_laughSml");
		emitAudioFromObject.Add("sfx_dlc_kog_queen_vocal_laughSml");
	}

	private void SFX_KOG_QUEEN_CannonHitQueenDing()
	{
		AudioManager.Play("sfx_dlc_kog_queen_cannonhitqueending");
		emitAudioFromObject.Add("sfx_dlc_kog_queen_cannonhitqueending");
	}
}
