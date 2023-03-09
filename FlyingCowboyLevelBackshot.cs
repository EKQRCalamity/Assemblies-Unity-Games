using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelBackshot : BasicUprightProjectile
{
	private static readonly float AttackPosition = -600f;

	[SerializeField]
	private BasicProjectile projectile;

	[SerializeField]
	private Transform projectileSpawnPosition;

	[SerializeField]
	private Transform[] leftWings;

	[SerializeField]
	private Transform[] rightWings;

	private DamageReceiver damageReceiver;

	private float bulletSpeed;

	private float health;

	private bool childParryable;

	public virtual BasicProjectile Create(Vector3 position, float rotation, float speed, float bulletSpeed, float health, float anticipationStartDistance, bool childParryable)
	{
		FlyingCowboyLevelBackshot flyingCowboyLevelBackshot = Create(position, rotation, speed) as FlyingCowboyLevelBackshot;
		flyingCowboyLevelBackshot.bulletSpeed = bulletSpeed;
		flyingCowboyLevelBackshot.StartCoroutine(flyingCowboyLevelBackshot.waitToShoot_cr(speed, anticipationStartDistance));
		flyingCowboyLevelBackshot.health = health;
		flyingCowboyLevelBackshot.childParryable = childParryable;
		return flyingCowboyLevelBackshot;
	}

	protected override void Start()
	{
		base.Start();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.transform.position.x < AttackPosition)
		{
			base.transform.SetPosition(AttackPosition);
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		health -= info.damage;
		if (health < 0f && !base.dead)
		{
			Level.Current.RegisterMinionKilled();
			Die();
		}
	}

	protected override void Die()
	{
		float speed = Speed;
		base.Die();
		StopAllCoroutines();
		StartCoroutine(death_cr(speed));
	}

	private IEnumerator death_cr(float speed)
	{
		Transform leftWing = leftWings.GetRandom();
		leftWing.GetComponent<SpriteRenderer>().enabled = true;
		Transform rightWing = rightWings.GetRandom();
		rightWing.GetComponent<SpriteRenderer>().enabled = true;
		base.animator.Play("Death");
		StartCoroutine(moveWings_cr(speed, leftWing, rightWing));
		SFX_COWGIRL_P1_HorseflySpit();
		yield return base.animator.WaitForNormalizedTime(this, 1f, "Death", 0, allowEqualTime: true);
		Object.Destroy(base.gameObject);
	}

	private IEnumerator moveWings_cr(float speed, Transform leftWing, Transform rightWing)
	{
		Vector3 wingSpeedLeft = new Vector2((0f - speed) * Random.Range(0.25f, 0.5f), 0f - Random.Range(75f, 125f));
		Vector3 windSpeedRight = new Vector2((0f - speed) * Random.Range(0.25f, 0.5f), 0f - Random.Range(75f, 125f));
		while (true)
		{
			yield return null;
			Vector3 position2 = leftWing.position;
			position2 += wingSpeedLeft * CupheadTime.Delta;
			leftWing.position = position2;
			position2 = rightWing.position;
			position2 += windSpeedRight * CupheadTime.Delta;
			rightWing.position = position2;
		}
	}

	private IEnumerator waitToShoot_cr(float speed, float anticipationStartDistance)
	{
		float timeToAnticipation = anticipationStartDistance / speed;
		float remainder = MathUtilities.DecimalPart(timeToAnticipation / 1f);
		float offset = 1f - remainder;
		float totalNormalizedTime = timeToAnticipation / 1f + offset + 0.625f;
		base.animator.Update(0f);
		base.animator.Play(0, 0, 0.625f + offset);
		yield return base.animator.WaitForNormalizedTime(this, totalNormalizedTime, "Idle");
		base.animator.Play("AnticipationStart");
		while (base.transform.position.x > -550f)
		{
			yield return null;
		}
		base.animator.SetTrigger("Attack");
		float initialSpeed = Speed;
		float decelerationTime = KinematicUtilities.CalculateTimeToChangeVelocity(initialSpeed, 0f, -550f - AttackPosition);
		float elapsedTime = 0f;
		while (elapsedTime < decelerationTime)
		{
			yield return null;
			elapsedTime += (float)CupheadTime.Delta;
			Speed = Mathf.Lerp(initialSpeed, 0f, elapsedTime / decelerationTime);
		}
		move = false;
		yield return base.animator.WaitForNormalizedTime(this, 1f, "Attack", 0, allowEqualTime: true);
		Object.Destroy(base.gameObject);
	}

	private void animationEvent_ShootBullet()
	{
		AbstractPlayerController next = PlayerManager.GetNext();
		float rotation = MathUtils.DirectionToAngle(next.center - projectileSpawnPosition.position);
		BasicProjectile basicProjectile = projectile.Create(projectileSpawnPosition.position, rotation, bulletSpeed);
		basicProjectile.SetParryable(childParryable);
		basicProjectile.StartCoroutine(growBullet(basicProjectile.transform));
	}

	private IEnumerator growBullet(Transform transform)
	{
		transform.SetScale(0.6f, 0.6f);
		WaitForFrameTimePersistent wait = new WaitForFrameTimePersistent(1f / 24f);
		float elapsedTime = 0f;
		while (elapsedTime < 0.3f)
		{
			yield return wait;
			elapsedTime += wait.totalDelta;
			float scale = Mathf.Lerp(0.6f, 1f, elapsedTime / 0.3f);
			transform.SetScale(scale, scale);
		}
	}

	private void AnimationEvent_SFX_COWGIRL_P1_HorseflySpit()
	{
		AudioManager.Play("sfx_DLC_Cowgirl_P1_Horsefly_Spit");
		emitAudioFromObject.Add("sfx_DLC_Cowgirl_P1_Horsefly_Spit");
	}

	private void SFX_COWGIRL_P1_HorseflySpit()
	{
		AudioManager.Play("sfx_DLC_Cowgirl_P1_Horsefly_Death");
		emitAudioFromObject.Add("sfx_DLC_Cowgirl_P1_Horsefly_Death");
	}
}
