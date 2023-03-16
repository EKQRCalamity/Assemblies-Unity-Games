using System.Collections;
using UnityEngine;

public class RumRunnersLevelMobBoss : AbstractCollidableObject
{
	private enum Direction
	{
		Attack45,
		Attack22,
		Attack0,
		Attack337,
		Attack315,
		Attack292,
		Attack270,
		Attack247,
		AttackCount
	}

	private static readonly float AcceptableAngleVariance = 15f;

	private static readonly float[] ReferenceAnglesRight = new float[8] { 45f, 22.5f, 0f, -22.5f, -45f, -67.5f, -90f, -112.5f };

	private static readonly float[] ReferenceAnglesLeft = new float[8] { 135f, 157.5f, 180f, -157.5f, -135f, -112.5f, -90f, -67.5f };

	private static readonly int StartSpeedParameter = Animator.StringToHash("StartSpeed");

	private static readonly int EndSpeedParameter = Animator.StringToHash("EndSpeed");

	[SerializeField]
	private BasicProjectile projectile;

	[SerializeField]
	private Effect projectileMuzzleFX;

	[SerializeField]
	private Vector2[] projectileRoots;

	[SerializeField]
	private Vector2 positionOffset;

	[SerializeField]
	private Vector2 flippedOffset;

	private bool begun;

	private bool dead;

	private float minMaxParameter;

	private bool shootingRight = true;

	private PatternString parryString;

	private PlayerId targetedPlayer;

	private Vector3 targetedPosition;

	private Direction targetedDirection;

	private LevelProperties.RumRunners properties;

	private RumRunnersLevelAnteater anteater;

	private Transform positioner;

	private DamageReceiver damageReceiver;

	private CircleCollider2D circleCollider;

	protected override void Awake()
	{
		base.Awake();
		circleCollider = GetComponent<CircleCollider2D>();
	}

	public void Setup(LevelProperties.RumRunners properties, RumRunnersLevelAnteater anteater, Transform positioner)
	{
		this.properties = properties;
		this.anteater = anteater;
		this.positioner = positioner;
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		parryString = new PatternString(properties.CurrentState.boss.bossProjectileParryString);
	}

	public void Begin()
	{
		begun = true;
		base.gameObject.SetActive(value: true);
		setActiveDirection(Direction.Attack0);
		base.animator.Update(0f);
		StartCoroutine(timer_cr());
		StartCoroutine(shoot_cr());
	}

	private void LateUpdate()
	{
		if (begun)
		{
			updatePosition();
		}
	}

	private IEnumerator shoot_cr()
	{
		LevelProperties.RumRunners.Boss p = properties.CurrentState.boss;
		yield return CupheadTime.WaitForSeconds(this, p.initialDelay);
		while (true)
		{
			AbstractPlayerController player = PlayerManager.GetNext();
			targetedPlayer = player.id;
			targetedPosition = player.center;
			Vector3 bossCenter = circleCollider.bounds.center;
			float angle = MathUtils.DirectionToAngle(targetedPosition - bossCenter);
			if ((shootingRight && targetedPosition.x < bossCenter.x) || (!shootingRight && targetedPosition.x > bossCenter.x))
			{
				base.animator.SetTrigger("Turn");
			}
			targetedDirection = chooseDirection(angle, canOvershoot: true);
			setActiveDirection(targetedDirection);
			base.animator.SetTrigger("Attack");
			int animatorHash2 = Animator.StringToHash("AttackMiddle");
			while (getAnimatorCurrentStateInfo().shortNameHash != animatorHash2)
			{
				yield return null;
			}
			while (getAnimatorCurrentStateInfo().normalizedTime < 1f)
			{
				yield return null;
			}
			shoot();
			base.animator.SetTrigger("Continue");
			animatorHash2 = Animator.StringToHash("AttackEnd");
			while (getAnimatorCurrentStateInfo().shortNameHash != animatorHash2)
			{
				yield return null;
			}
			while (getAnimatorCurrentStateInfo().shortNameHash == animatorHash2)
			{
				yield return null;
			}
			float totalAttackDelay = p.coinDelay.GetFloatAt(minMaxParameter);
			if (totalAttackDelay > 2f / 3f)
			{
				if (totalAttackDelay <= 17f / 24f)
				{
					base.animator.SetFloat(StartSpeedParameter, 1.2f);
				}
				float waitTime = totalAttackDelay - 2f / 3f;
				if (waitTime > 0f)
				{
					yield return CupheadTime.WaitForSeconds(this, waitTime);
				}
				continue;
			}
			float value;
			float value2;
			if (totalAttackDelay > 7f / 12f)
			{
				value = 1.2f;
				value2 = 1f;
			}
			else if (totalAttackDelay > 13f / 24f)
			{
				value = 1.2f;
				value2 = 1.3333334f;
			}
			else if (totalAttackDelay > 0.5f)
			{
				value = 1.5f;
				value2 = 1.3333334f;
			}
			else if (totalAttackDelay > 11f / 24f)
			{
				value = 2f;
				value2 = 1.3333334f;
			}
			else
			{
				value = 2f;
				value2 = 2f;
			}
			base.animator.SetFloat(StartSpeedParameter, value);
			base.animator.SetFloat(EndSpeedParameter, value2);
		}
	}

	private IEnumerator timer_cr()
	{
		LevelProperties.RumRunners.Boss p = properties.CurrentState.boss;
		float totalTime = p.coinMinMaxTime;
		float elapsedTime = 0f;
		while (elapsedTime < totalTime)
		{
			elapsedTime += (float)CupheadTime.Delta;
			minMaxParameter = Mathf.Clamp01(elapsedTime / totalTime);
			yield return null;
		}
		minMaxParameter = 1f;
	}

	private void die()
	{
		if (!dead)
		{
			dead = true;
			StopAllCoroutines();
			base.gameObject.SetActive(value: false);
			anteater.RealDeath();
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		properties.DealDamage(info.damage);
		if (properties.CurrentHealth <= 0f && !dead)
		{
			die();
		}
	}

	private void animationEvent_Flip()
	{
		shootingRight = !shootingRight;
		Vector3 localScale = base.transform.localScale;
		localScale.x *= -1f;
		base.transform.localScale = localScale;
		if (PlayerManager.DoesPlayerExist(targetedPlayer))
		{
			targetedPosition = PlayerManager.GetPlayer(targetedPlayer).center;
		}
		Vector3 center = circleCollider.bounds.center;
		float angle = MathUtils.DirectionToAngle(targetedPosition - center);
		targetedDirection = chooseDirection(angle, canOvershoot: true);
		setActiveDirection(targetedDirection);
		updatePosition();
	}

	private void shoot()
	{
		Vector3 vector = ((!PlayerManager.DoesPlayerExist(targetedPlayer)) ? targetedPosition : PlayerManager.GetPlayer(targetedPlayer).center);
		Vector3 vector2 = base.transform.TransformPoint(projectileRoots[(int)targetedDirection]);
		float num = MathUtils.DirectionToAngle(vector - vector2);
		float num2 = ((!shootingRight) ? ReferenceAnglesLeft[(int)targetedDirection] : ReferenceAnglesRight[(int)targetedDirection]);
		float num3 = (shootingRight ? num : ((!(num > 0f)) ? (-180f - num) : (180f - num)));
		float num4 = (shootingRight ? num2 : ((!(num2 > 0f)) ? (-180f - num2) : (180f - num2)));
		if (Mathf.Abs(num3 - num4) > AcceptableAngleVariance)
		{
			Direction direction = chooseDirection(num, canOvershoot: true);
			int num5 = direction - targetedDirection;
			if (Mathf.Abs(num5) > 1)
			{
				num5 = (int)Mathf.Sign(num5);
				targetedDirection = (Direction)Mathf.Clamp((int)(targetedDirection + num5), 0, 8);
			}
			else
			{
				targetedDirection = direction;
			}
			setActiveDirection(targetedDirection);
			vector2 = base.transform.TransformPoint(projectileRoots[(int)targetedDirection]);
		}
		if (shootingRight)
		{
			num2 = ReferenceAnglesRight[(int)targetedDirection];
			num = Mathf.Clamp(num, num2 - AcceptableAngleVariance, num2 + AcceptableAngleVariance);
		}
		else if (targetedDirection == Direction.Attack0)
		{
			num = ((!(num < 0f)) ? Mathf.Clamp(num, 180f - AcceptableAngleVariance, 180f + AcceptableAngleVariance) : Mathf.Clamp(num, -180f - AcceptableAngleVariance, -180f + AcceptableAngleVariance));
		}
		else
		{
			num2 = ReferenceAnglesLeft[(int)targetedDirection];
			num = Mathf.Clamp(num, num2 - AcceptableAngleVariance, num2 + AcceptableAngleVariance);
		}
		float floatAt = properties.CurrentState.boss.coinSpeed.GetFloatAt(minMaxParameter);
		BasicProjectile basicProjectile = projectile.Create(vector2, num, floatAt);
		projectileMuzzleFX.Create(vector2).transform.SetEulerAngles(0f, 0f, num);
		basicProjectile.SetParryable(parryString.PopLetter() == 'P');
		SFX_RUMRUN_P4_Snail_ProjectileShoot();
	}

	private void updatePosition()
	{
		Vector3 position = positioner.position;
		if (!shootingRight)
		{
			position += (Vector3)flippedOffset;
		}
		base.transform.position = position + (Vector3)positionOffset;
	}

	private Direction chooseDirection(float angle, bool canOvershoot)
	{
		if (shootingRight)
		{
			if (angle > 33.75f)
			{
				return Direction.Attack45;
			}
			if (angle > 11.25f)
			{
				return Direction.Attack22;
			}
			if (angle > -11.25f)
			{
				return Direction.Attack0;
			}
			if (angle > -33.75f)
			{
				return Direction.Attack337;
			}
			if (angle > -56.25f)
			{
				return Direction.Attack315;
			}
			if (angle > -78.75f)
			{
				return Direction.Attack292;
			}
			if (!canOvershoot)
			{
				return Direction.Attack270;
			}
			if (angle > -101.25f)
			{
				return Direction.Attack270;
			}
			return Direction.Attack247;
		}
		if (angle >= 168.75f || angle <= -168.75f)
		{
			return Direction.Attack0;
		}
		if (angle < -146.25f)
		{
			return Direction.Attack337;
		}
		if (angle < -123.75f)
		{
			return Direction.Attack315;
		}
		if (angle < -101.25f)
		{
			return Direction.Attack292;
		}
		if ((!canOvershoot && angle < 0f) || (canOvershoot && angle < -78.75f))
		{
			return Direction.Attack270;
		}
		if (canOvershoot && angle < 0f)
		{
			return Direction.Attack247;
		}
		if (angle < 168.75f)
		{
			return Direction.Attack22;
		}
		return Direction.Attack45;
	}

	private AnimatorStateInfo getAnimatorCurrentStateInfo()
	{
		return base.animator.GetCurrentAnimatorStateInfo((int)(targetedDirection + 1));
	}

	private void setActiveDirection(Direction direction)
	{
		for (int i = 1; i <= 8; i++)
		{
			float weight = ((direction != (Direction)(i - 1)) ? 0f : 1f);
			base.animator.SetLayerWeight(i, weight);
		}
	}

	private void SFX_RUMRUN_P4_Snail_ProjectileShoot()
	{
		AudioManager.Play("sfx_dlc_rumrun_p4_snail_projectile_shoot");
	}
}
