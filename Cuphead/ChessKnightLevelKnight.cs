using System.Collections;
using UnityEngine;

public class ChessKnightLevelKnight : LevelProperties.ChessKnight.Entity
{
	public enum State
	{
		Intro,
		Move,
		Idle,
		Short,
		Long,
		Up,
		Taunt,
		Death
	}

	[SerializeField]
	private ParrySwitch pink;

	[SerializeField]
	private CollisionChild swordHitbox;

	[SerializeField]
	private CollisionChild upHitbox;

	[SerializeField]
	private Rangef positionBoundaryInset;

	[SerializeField]
	private Transform smokeSpawnPoint;

	[SerializeField]
	private Effect backDashSmoke;

	[SerializeField]
	private Rangef legSpeedMultiplierRange;

	[SerializeField]
	private float maximumLegVelocity;

	[SerializeField]
	private SpriteDeathPartsDLC[] deathArmor;

	[SerializeField]
	private Transform[] deathArmorSpawns;

	[SerializeField]
	private HitFlash hitFlash;

	private DamageDealer damageDealer;

	private float battleStartPosition;

	private bool goingLeft = true;

	private bool facingLeft = true;

	private AbstractPlayerController targetPlayer;

	private PatternString attackIntervalPattern;

	private PatternString movementPattern;

	private PatternString numberTauntString;

	public int tauntAttackCounter;

	private bool isTauntAttack;

	public State state { get; private set; }

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.color = Color.red;
		float x = -640f + positionBoundaryInset.minimum;
		Gizmos.DrawLine(new Vector3(x, -500f), new Vector3(x, 500f));
		Gizmos.color = Color.green;
		x = -640f + positionBoundaryInset.maximum;
		Gizmos.DrawLine(new Vector3(x, -500f), new Vector3(x, 500f));
		Gizmos.color = Color.red;
		x = 640f - positionBoundaryInset.minimum;
		Gizmos.DrawLine(new Vector3(x, -500f), new Vector3(x, 500f));
		Gizmos.color = Color.green;
		x = 640f - positionBoundaryInset.maximum;
		Gizmos.DrawLine(new Vector3(x, -500f), new Vector3(x, 500f));
	}

	public override void LevelInit(LevelProperties.ChessKnight properties)
	{
		base.LevelInit(properties);
		Level.Current.OnIntroEvent += onIntroEventHandler;
		LevelProperties.ChessKnight.Knight knight = properties.CurrentState.knight;
		attackIntervalPattern = new PatternString(knight.attackIntervalString);
		AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		AbstractPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		if (player2 != null && !player2.IsDead)
		{
			targetPlayer = ((!Rand.Bool()) ? player2 : player);
		}
		battleStartPosition = base.transform.position.x;
		movementPattern = new PatternString(properties.CurrentState.movement.movementString);
		numberTauntString = new PatternString(properties.CurrentState.tauntAttack.numberTauntString);
		numberTauntString.SetSubStringIndex(-1);
		tauntAttackCounter = numberTauntString.PopInt();
	}

	protected override void Awake()
	{
		base.Awake();
		Vector3 position = base.transform.position;
		position.x = 640f - (positionBoundaryInset.maximum + positionBoundaryInset.minimum) * 0.5f;
		base.transform.position = position;
	}

	private void Start()
	{
		damageDealer = DamageDealer.NewEnemy();
		pink.OnActivate += GotParried;
		swordHitbox.OnPlayerCollision += OnCollisionPlayer;
		upHitbox.OnPlayerCollision += OnCollisionPlayer;
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private void GotParried()
	{
		base.properties.DealDamage((!PlayerManager.BothPlayersActive()) ? 10f : ChessKingLevelKing.multiplayerDamageNerf);
		hitFlash.Flash(0.7f);
		StartCoroutine(parry_timer_cr());
		if (base.properties.CurrentHealth <= 0f && state != State.Death)
		{
			state = State.Death;
			death();
		}
	}

	private IEnumerator parry_timer_cr()
	{
		pink.gameObject.SetActive(value: false);
		yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.knight.parryCooldown);
		pink.gameObject.SetActive(value: true);
	}

	private void onIntroEventHandler()
	{
		StartCoroutine(intro_cr());
	}

	private IEnumerator intro_cr()
	{
		base.animator.SetTrigger("Intro");
		yield return base.animator.WaitForNormalizedTime(this, 1f, "Intro");
		EndAttack();
	}

	private void EndAttack()
	{
		isTauntAttack = false;
		SFX_KOG_KNIGHT_RecoverFoley();
		state = State.Move;
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		if (tauntAttackCounter > 0)
		{
			LevelProperties.ChessKnight.Movement p = base.properties.CurrentState.movement;
			float idleTime = attackIntervalPattern.PopFloat();
			float idleT = 0f;
			float moveT = 0f;
			YieldInstruction wait = new WaitForFixedUpdate();
			float startPosition = base.transform.position.x;
			float movementAmount = movementPattern.PopFloat();
			goingLeft = chooseGoingLeft(movementAmount);
			float endPosition = getWalkingEndPosition(movementAmount);
			float moveTime = Mathf.Abs(endPosition - startPosition) / p.movementSpeed;
			base.animator.SetBool("FacingLeft", facingLeft);
			base.animator.SetBool("Walking", value: true);
			while (true)
			{
				idleT += CupheadTime.FixedDelta;
				Vector3 previousPosition = base.transform.position;
				if (moveT < moveTime)
				{
					moveT += CupheadTime.FixedDelta;
					if (p.hasEasing)
					{
						float t = EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, 0f, 1f, moveT / moveTime);
						base.transform.SetPosition(Mathf.Lerp(startPosition, endPosition, t));
					}
					else if (goingLeft && base.transform.position.x > endPosition)
					{
						base.transform.position += Vector3.left * p.movementSpeed * CupheadTime.FixedDelta;
					}
					else if (!goingLeft && base.transform.position.x < endPosition)
					{
						base.transform.position += Vector3.right * p.movementSpeed * CupheadTime.FixedDelta;
					}
				}
				else
				{
					if (idleT > idleTime)
					{
						break;
					}
					goingLeft = !goingLeft;
					base.transform.SetPosition(endPosition);
					startPosition = endPosition;
					endPosition = getWalkingEndPosition(movementPattern.PopFloat());
					moveTime = Mathf.Abs(endPosition - startPosition) / p.movementSpeed;
					moveT = 0f;
				}
				updateAnimatorSpeed(base.transform.position, previousPosition);
				yield return wait;
			}
			goingLeft = !goingLeft;
		}
		CheckTaunt();
	}

	private bool chooseGoingLeft(float movementAmount)
	{
		bool flag = Rand.Bool();
		float num = ((!facingLeft) ? ((!flag) ? (-640f + positionBoundaryInset.maximum) : (-640f + positionBoundaryInset.minimum)) : ((!flag) ? (640f - positionBoundaryInset.minimum) : (640f - positionBoundaryInset.maximum)));
		float num2 = num - base.transform.position.x;
		if (Mathf.Abs(num2 / movementAmount) < 0.5f)
		{
			flag = !flag;
		}
		return flag;
	}

	private float getWalkingEndPosition(float movementAmount)
	{
		float endPosition = base.transform.position.x + ((!goingLeft) ? movementAmount : (0f - movementAmount)) * 2f;
		return clampEndPosition(endPosition, facingLeft);
	}

	private void CheckTaunt()
	{
		AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		AbstractPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		float tauntDistance = base.properties.CurrentState.taunt.tauntDistance;
		float num = Mathf.Abs(player.transform.position.x - base.transform.position.x);
		float num2 = ((!(player2 != null) || player2.IsDead) ? num : Mathf.Abs(player2.transform.position.x - base.transform.position.x));
		if (num > tauntDistance && num2 > tauntDistance)
		{
			if (tauntAttackCounter <= 0)
			{
				isTauntAttack = true;
				StartCoroutine(long_cr());
			}
			else
			{
				StartCoroutine(taunt_cr());
			}
		}
		else
		{
			state = State.Idle;
		}
	}

	private IEnumerator taunt_cr()
	{
		state = State.Taunt;
		base.animator.SetBool("Taunting", value: true);
		base.animator.SetBool("Walking", value: false);
		yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.taunt.tauntDuration);
		base.animator.SetBool("Taunting", value: false);
		if (shouldBackDash())
		{
			base.animator.SetTrigger("BackDash");
			yield return base.animator.WaitForNormalizedTime(this, 1f, "Taunt.Exit");
			yield return StartCoroutine(backDash_cr());
		}
		else if (shouldTurn())
		{
			base.animator.SetTrigger("Turn");
			yield return base.animator.WaitForNormalizedTime(this, 1f, "Taunt.Exit");
			yield return base.animator.WaitForNormalizedTime(this, 1f, "Turn");
			turn();
		}
		else
		{
			yield return base.animator.WaitForNormalizedTime(this, 1f, "Taunt.Exit");
		}
		tauntAttackCounter--;
		EndAttack();
	}

	public void Short()
	{
		state = State.Short;
		StartCoroutine(short_cr());
	}

	private IEnumerator short_cr()
	{
		tauntAttackCounter = numberTauntString.PopInt();
		LevelProperties.ChessKnight.ShortAttack p = base.properties.CurrentState.shortAttack;
		base.animator.SetTrigger("RegularAttack");
		base.animator.SetBool("Walking", value: false);
		yield return CupheadTime.WaitForSeconds(this, p.shortAntiDuration);
		base.animator.SetTrigger("OnAttack");
		yield return CupheadTime.WaitForSeconds(this, p.shortAttackDuration);
		base.animator.SetTrigger("OnAttackEnd");
		yield return base.animator.WaitForAnimationToStart(this, "RegularAttack.RecoveryHold");
		yield return CupheadTime.WaitForSeconds(this, p.shortRecoveryDuration);
		if (shouldBackDash())
		{
			base.animator.SetTrigger("BackDash");
			yield return StartCoroutine(backDash_cr());
		}
		else
		{
			base.animator.SetTrigger("ExitRecovery");
			yield return base.animator.WaitForNormalizedTime(this, 1f, "RegularAttack.RecoveryExit");
		}
		EndAttack();
	}

	public void Long()
	{
		isTauntAttack = false;
		state = State.Long;
		StartCoroutine(long_cr());
	}

	private IEnumerator long_cr()
	{
		tauntAttackCounter = numberTauntString.PopInt();
		float antiDuration = ((!isTauntAttack) ? base.properties.CurrentState.longAttack.longAntiDuration : base.properties.CurrentState.tauntAttack.tauntAttackAntiDuration);
		float attackTime = ((!isTauntAttack) ? base.properties.CurrentState.longAttack.longAttackTime : base.properties.CurrentState.tauntAttack.tauntAttackTime);
		float attackDist = ((!isTauntAttack) ? base.properties.CurrentState.longAttack.longAttackDist : base.properties.CurrentState.tauntAttack.tauntAttackDist);
		float attackRecovery = ((!isTauntAttack) ? base.properties.CurrentState.longAttack.longRecoveryDuration : base.properties.CurrentState.tauntAttack.tauntAttackRecoveryDuration);
		AbstractPlayerController player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		AbstractPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		base.animator.SetBool("Walking", value: false);
		if (isTauntAttack)
		{
			base.animator.Play("DashAttack.Anticipation");
			base.animator.SetBool("Taunting", value: false);
		}
		else
		{
			base.animator.SetTrigger("DashAttack");
		}
		if (antiDuration > 0.7f)
		{
			yield return CupheadTime.WaitForSeconds(this, antiDuration);
			base.animator.SetTrigger("OnAttack");
		}
		else
		{
			if (!isTauntAttack)
			{
				yield return base.animator.WaitForAnimationToStart(this, "DashAttack.Anticipation");
			}
			yield return CupheadTime.WaitForSeconds(this, Mathf.Max(antiDuration, 5f / 24f));
			base.animator.Play("DashAttack.Attack");
			base.animator.Update(0f);
		}
		float t = 0f;
		float time = attackTime;
		float startPosition = base.transform.position.x;
		float endPosition2 = ((!facingLeft) ? (base.transform.position.x + attackDist) : (base.transform.position.x - attackDist));
		endPosition2 = clampEndPosition(endPosition2, !facingLeft);
		YieldInstruction wait = new WaitForFixedUpdate();
		while (t < time)
		{
			t += CupheadTime.FixedDelta;
			base.transform.SetPosition(Mathf.Lerp(startPosition, endPosition2, t / time));
			yield return wait;
		}
		base.animator.SetTrigger("OnAttackEnd");
		recovery(attackRecovery);
		SFX_KOG_KNIGHT_RecoverFoley();
	}

	public void Up()
	{
		state = State.Up;
		StartCoroutine(up_cr());
	}

	private IEnumerator up_cr()
	{
		tauntAttackCounter = numberTauntString.PopInt();
		LevelProperties.ChessKnight.UpAttack p = base.properties.CurrentState.upAttack;
		base.animator.SetTrigger("MoonAttack");
		base.animator.SetBool("Walking", value: false);
		yield return CupheadTime.WaitForSeconds(this, p.upAntiDuration);
		base.animator.SetTrigger("OnAttack");
		yield return base.animator.WaitForAnimationToStart(this, "Recovery");
		recovery(p.upRecoveryDuration);
		SFX_KOG_KNIGHT_RecoverFoley();
	}

	private void recovery(float duration)
	{
		StartCoroutine(recovery_cr(duration));
	}

	private IEnumerator recovery_cr(float duration)
	{
		SFX_KOG_KNIGHT_MoonSlash_Panting();
		yield return CupheadTime.WaitForSeconds(this, duration);
		if (shouldBackDash())
		{
			base.animator.SetTrigger("BackDash");
			yield return StartCoroutine(backDash_cr());
		}
		else if (shouldTurn())
		{
			base.animator.SetTrigger("Turn");
			yield return base.animator.WaitForNormalizedTime(this, 1f, "Turn");
			turn();
		}
		else
		{
			base.animator.SetTrigger("ExitRecovery");
			yield return base.animator.WaitForNormalizedTime(this, 1f, "RecoveryExit");
		}
		SFX_KOG_KNIGHT_MoonSlash_PantingStop();
		EndAttack();
	}

	private bool shouldFaceLeft()
	{
		if (targetPlayer == null || targetPlayer.IsDead)
		{
			targetPlayer = PlayerManager.GetNext();
		}
		return targetPlayer.transform.position.x < base.transform.position.x;
	}

	private bool shouldTurn()
	{
		return shouldFaceLeft() != facingLeft;
	}

	private void turn()
	{
		facingLeft = !facingLeft;
		base.transform.SetScale(facingLeft ? 1 : (-1));
	}

	private bool shouldBackDash()
	{
		float num = ((!shouldFaceLeft()) ? 640 : (-640));
		float num2 = 640f;
		float num3 = Mathf.Abs(num - base.transform.position.x);
		bool flag = false;
		if (PlayerManager.BothPlayersActive())
		{
			AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne);
			AbstractPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
			if (Mathf.Sign(base.transform.position.x - player.transform.position.x) != Mathf.Sign(base.transform.position.x - player2.transform.position.x))
			{
				flag = true;
			}
		}
		return num3 < num2 && !flag;
	}

	private IEnumerator backDash_cr()
	{
		float returnSpeed = ((!isTauntAttack) ? base.properties.CurrentState.longAttack.longReturnSpeed : base.properties.CurrentState.tauntAttack.tauntAttackReturnSpeed);
		facingLeft = shouldFaceLeft();
		base.transform.SetScale(facingLeft ? 1 : (-1));
		float startPosition = base.transform.position.x;
		float endPosition = ((!facingLeft) ? (-640f + positionBoundaryInset.minimum) : (640f - positionBoundaryInset.minimum));
		float time = Mathf.Abs(endPosition - base.transform.position.x) / returnSpeed;
		float t = 0f;
		StartCoroutine(backDashAnimation_cr());
		Effect smoke = backDashSmoke.Spawn(smokeSpawnPoint.position);
		smoke.transform.SetScale(facingLeft ? 1 : (-1));
		YieldInstruction wait = new WaitForFixedUpdate();
		while (t < time)
		{
			Vector3 previousPosition = base.transform.position;
			t += CupheadTime.FixedDelta;
			if (base.properties.CurrentState.movement.hasEasing)
			{
				float t2 = EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, 0f, 1f, t / time);
				base.transform.SetPosition(Mathf.Lerp(startPosition, endPosition, t2));
			}
			else if (goingLeft && base.transform.position.x > endPosition)
			{
				base.transform.position += Vector3.left * returnSpeed * CupheadTime.FixedDelta;
			}
			else if (!goingLeft && base.transform.position.x < endPosition)
			{
				base.transform.position += Vector3.right * returnSpeed * CupheadTime.FixedDelta;
			}
			updateAnimatorSpeed(base.transform.position, previousPosition);
			yield return wait;
		}
	}

	private IEnumerator backDashAnimation_cr()
	{
		yield return base.animator.WaitForNormalizedTime(this, 1f, "BackDash.Dash");
		SFX_KOG_KNIGHT_RecoverFoley();
		base.animator.SetBool("FacingLeft", facingLeft);
		base.animator.SetBool("Walking", value: true);
	}

	private void death()
	{
		StopAllCoroutines();
		SFX_KOG_KNIGHT_Die();
		base.animator.SetBool("Walking", value: false);
		base.animator.SetTrigger("Death");
		for (int i = 0; i < deathArmor.Length; i++)
		{
			SpriteDeathPartsDLC spriteDeathPartsDLC = Object.Instantiate(deathArmor[i], deathArmorSpawns[i].position, Quaternion.identity);
			spriteDeathPartsDLC.transform.localScale = new Vector3(0f - base.transform.localScale.x, 1f);
			spriteDeathPartsDLC.transform.parent = base.transform;
			spriteDeathPartsDLC.SetVelocity(new Vector3(spriteDeathPartsDLC.transform.localPosition.x * 3f * base.transform.localScale.x, spriteDeathPartsDLC.transform.localPosition.y * 6f + 800f));
		}
	}

	private void updateAnimatorSpeed(Vector3 currentPosition, Vector3 previousPosition)
	{
		if (!CupheadTime.IsPaused())
		{
			Vector3 vector = (currentPosition - previousPosition) / CupheadTime.FixedDelta;
			base.animator.SetFloat("XSpeed", vector.x);
			float value = MathUtilities.LerpMapping(Mathf.Abs(vector.x), 0f, maximumLegVelocity, legSpeedMultiplierRange.minimum, legSpeedMultiplierRange.maximum, clamp: true);
			base.animator.SetFloat("LegSpeed", value);
		}
	}

	private float clampEndPosition(float endPosition, bool isRightSide)
	{
		if (isRightSide)
		{
			float min = 640f - positionBoundaryInset.maximum;
			float max = 640f - positionBoundaryInset.minimum;
			endPosition = Mathf.Clamp(endPosition, min, max);
		}
		else
		{
			float min2 = -640f + positionBoundaryInset.minimum;
			float max2 = -640f + positionBoundaryInset.maximum;
			endPosition = Mathf.Clamp(endPosition, min2, max2);
		}
		return endPosition;
	}

	private void AnimationEvent_SFX_KOG_KNIGHT_AttackUpwards_Stab()
	{
		AudioManager.Play("sfx_dlc_kog_knight_attackupwards_stab");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_attackupwards_stab");
	}

	private void AnimationEvent_SFX_KOG_KNIGHT_AttackUpwards_Start()
	{
		AudioManager.Play("sfx_dlc_kog_knight_attackupwards_start");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_attackupwards_start");
	}

	private void SFX_KOG_KNIGHT_Die()
	{
		AudioManager.Play("sfx_dlc_kog_knight_die");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_die");
	}

	private void AnimationEvent_SFX_KOG_KNIGHT_Foley_Walk()
	{
		AudioManager.Play("sfx_dlc_kog_knight_foley_walk");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_foley_walk");
	}

	private void AnimationEvent_SFX_KOG_KNIGHT_Intro_ShieldBash()
	{
		AudioManager.Play("sfx_dlc_kog_knight_intro_shieldbash");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_intro_shieldbash");
	}

	private void AnimationEvent_SFX_KOG_KNIGHT_Intro_Visor()
	{
		AudioManager.Play("sfx_dlc_kog_knight_intro_visor");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_intro_visor");
	}

	private void AnimationEvent_SFX_KOG_KNIGHT_MoonSlash_End()
	{
		AudioManager.Stop("sfx_dlc_kog_knight_moonslash_panting");
		AudioManager.Play("sfx_dlc_kog_knight_moonslash_end");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_moonslash_end");
	}

	private void SFX_KOG_KNIGHT_MoonSlash_Panting()
	{
		AudioManager.Play("sfx_dlc_kog_knight_moonslash_panting");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_moonslash_panting");
	}

	private void SFX_KOG_KNIGHT_MoonSlash_PantingStop()
	{
		AudioManager.Stop("sfx_dlc_kog_knight_moonslash_panting");
	}

	private void AnimationEvent_SFX_KOG_KNIGHT_MoonSlash_Start()
	{
		AudioManager.Play("sfx_dlc_kog_knight_moonslash_start");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_moonslash_start");
	}

	private void AnimationEvent_SFX_KOG_KNIGHT_MoonSlash_Swing()
	{
		AudioManager.Play("sfx_dlc_kog_knight_moonslash_swing");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_moonslash_swing");
	}

	private void AnimationEvent_SFX_KOG_KNIGHT_TauntHand()
	{
		AudioManager.Play("sfx_dlc_kog_knight_taunthand");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_taunthand");
	}

	private void AnimationEvent_SFX_KOG_KNIGHT_Dash_End()
	{
		AudioManager.Play("sfx_dlc_kog_knight_dash_end");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_dash_end");
	}

	private void SFX_KOG_KNIGHT_RecoverFoley()
	{
		AudioManager.Play("sfx_dlc_kog_knight_recoverfoley");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_recoverfoley");
	}

	private void AnimationEvent_SFX_KOG_KNIGHT_Dash_Start()
	{
		AudioManager.Play("sfx_dlc_kog_knight_dash_start");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_dash_start");
	}

	private void AnimationEvent_SFX_KOG_KNIGHT_Dash_Attack()
	{
		AudioManager.Play("sfx_dlc_kog_knight_dash_attack");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_dash_attack");
	}

	private void AnimationEvent_SFX_KOG_KNIGHT_Vocal_Attack()
	{
		AudioManager.Play("sfx_dlc_kog_knight_vocal_attack");
		emitAudioFromObject.Add("sfx_dlc_kog_knight_vocal_attack");
	}
}
