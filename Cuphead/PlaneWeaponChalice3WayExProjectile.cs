using System;
using UnityEngine;

public class PlaneWeaponChalice3WayExProjectile : AbstractProjectile
{
	public enum State
	{
		Idle,
		Frozen,
		Paused,
		Launched
	}

	private const float Y_OFFSET = 10f;

	public float FreezeTime;

	private State state;

	private float timeSinceFrozen;

	private float arcTimer;

	public float arcSpeed = 5f;

	public float arcX = 500f;

	public float arcY = 500f;

	public float damageAfterLaunch = 20f;

	public float speedAfterLaunch = 3000f;

	public float accelAfterLaunch = 100f;

	public float minXDistance = 500f;

	public float xDistanceNoTarget = 500f;

	public int ID;

	public PlaneWeaponChalice3WayExProjectile partner;

	private Vector3 accelVectorAfterLaunch;

	private Vector3 velocityAfterLaunch;

	private Collider2D target;

	public float pauseTime = 0.5f;

	public float vDirection = 1f;

	[SerializeField]
	private SpriteRenderer magnet;

	[SerializeField]
	private SpriteRenderer deathSpark;

	[SerializeField]
	private Effect shootFX;

	[SerializeField]
	private Effect smokeFX;

	[SerializeField]
	private Effect sparkleFX;

	[SerializeField]
	private float firstSmokeDelay = 0.7f;

	[SerializeField]
	private float smokeDelay = 0.09f;

	[SerializeField]
	private float sparkleDelay = 0.15f;

	[SerializeField]
	private float sparkleRadius = 20f;

	private float smokeTimer;

	private float sparkleTimer;

	protected override void OnDealDamage(float damage, DamageReceiver receiver, DamageDealer damageDealer)
	{
		base.OnDealDamage(damage, receiver, damageDealer);
		if (state == State.Idle)
		{
			Freeze();
			partner.Freeze();
		}
		AudioManager.Play("player_plane_weapon_ex_chomp");
		emitAudioFromObject.Add("player_plane_weapon_ex_chomp");
	}

	public void Freeze()
	{
		state = State.Frozen;
		timeSinceFrozen = 0f;
		deathSpark.transform.localScale = new Vector3(0.5f, 0.5f);
		deathSpark.flipX = Rand.Bool();
		deathSpark.flipY = Rand.Bool();
		deathSpark.transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(0, 360));
		base.animator.Play("Spark", 1, 0f);
	}

	public void SetArcPosition()
	{
		base.transform.localPosition = new Vector3(Mathf.Sin(EaseUtils.Linear(0.15f, 1f, arcTimer) * (float)Math.PI) * arcX, 10f + EaseUtils.Linear(0f, 1f, arcTimer) * (float)Math.PI * vDirection * arcY);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.dead)
		{
			return;
		}
		smokeTimer += CupheadTime.FixedDelta;
		if (smokeTimer > firstSmokeDelay)
		{
			smokeFX.Create(base.transform.position);
			smokeTimer -= smokeDelay;
		}
		sparkleTimer += CupheadTime.FixedDelta;
		if (sparkleTimer > sparkleDelay)
		{
			sparkleFX.Create(base.transform.position + (Vector3)MathUtils.AngleToDirection(UnityEngine.Random.Range(0, 360)) * UnityEngine.Random.Range(0f, sparkleRadius));
			sparkleTimer -= sparkleDelay;
		}
		switch (state)
		{
		case State.Idle:
			SetArcPosition();
			arcTimer += arcSpeed / (float)Math.PI * CupheadTime.FixedDelta;
			base.transform.localScale = new Vector3(Mathf.Lerp(0.5f, 1f, arcTimer), Mathf.Lerp(0.5f, 1f, arcTimer));
			if (arcTimer > 1f)
			{
				state = State.Paused;
				damageDealer.SetDamage(damageAfterLaunch);
				CollisionDeath.Enemies = true;
				base.transform.localScale = new Vector3(1f, 1f);
			}
			break;
		case State.Frozen:
			timeSinceFrozen += CupheadTime.FixedDelta;
			if (timeSinceFrozen > FreezeTime)
			{
				state = State.Idle;
			}
			break;
		case State.Paused:
			pauseTime -= CupheadTime.FixedDelta;
			if (pauseTime <= 0f)
			{
				FindTarget();
				state = State.Launched;
				Vector3 vector = base.transform.parent.position + Vector3.right * xDistanceNoTarget;
				base.transform.parent = null;
				if (target != null && target.gameObject.activeInHierarchy && target.isActiveAndEnabled)
				{
					vector = target.transform.position;
					vector.x = Mathf.Clamp(vector.x, base.transform.position.x + minXDistance, vector.x);
				}
				velocityAfterLaunch = (vector - base.transform.position).normalized;
				accelVectorAfterLaunch = velocityAfterLaunch * accelAfterLaunch;
				velocityAfterLaunch *= speedAfterLaunch;
			}
			break;
		case State.Launched:
			base.transform.position += velocityAfterLaunch * CupheadTime.FixedDelta;
			velocityAfterLaunch += accelVectorAfterLaunch * CupheadTime.FixedDelta;
			if (velocityAfterLaunch.x > 0f)
			{
				if (!base.animator.GetCurrentAnimatorStateInfo(0).IsName("Shoot"))
				{
					base.animator.Play("Shoot");
					shootFX.Create(base.transform.position + Vector3.left * 20f);
				}
				magnet.transform.eulerAngles = new Vector3(0f, 0f, MathUtils.DirectionToAngle(velocityAfterLaunch));
			}
			break;
		}
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		DealDamage(hit);
		base.OnCollisionEnemy(hit, phase);
	}

	protected override void OnCollisionOther(GameObject hit, CollisionPhase phase)
	{
		if (!(hit.tag == "Parry"))
		{
			base.OnCollisionOther(hit, phase);
		}
	}

	private void DealDamage(GameObject hit)
	{
		damageDealer.DealDamage(hit);
	}

	protected override void Die()
	{
		base.Die();
		magnet.transform.eulerAngles = Vector3.zero;
		magnet.flipX = Rand.Bool();
		deathSpark.transform.localScale = new Vector3(1f, 1f);
		deathSpark.flipX = Rand.Bool();
		deathSpark.flipY = Rand.Bool();
		deathSpark.transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(0, 360));
		base.animator.Play("Spark", 1, 0f);
		base.animator.Play((ID != 0) ? "DieB" : "DieA");
	}

	public void FindTarget()
	{
		if (partner != null && ID == 1)
		{
			return;
		}
		float num = float.MaxValue;
		Collider2D collider2D = null;
		Vector2 vector = base.transform.parent.position;
		DamageReceiver[] array = UnityEngine.Object.FindObjectsOfType<DamageReceiver>();
		foreach (DamageReceiver damageReceiver in array)
		{
			if (!damageReceiver.gameObject.activeInHierarchy || damageReceiver.type != 0 || damageReceiver.transform.position.x < base.transform.position.x)
			{
				continue;
			}
			Collider2D[] components = damageReceiver.GetComponents<Collider2D>();
			foreach (Collider2D collider2D2 in components)
			{
				if (collider2D2.isActiveAndEnabled && CupheadLevelCamera.Current.ContainsPoint(collider2D2.bounds.center, collider2D2.bounds.size / 2f))
				{
					float num2 = Mathf.Abs(MathUtils.DirectionToAngle((Vector2)collider2D2.bounds.center - vector));
					if (num2 < num)
					{
						num = num2;
						collider2D = collider2D2;
					}
				}
			}
			DamageReceiverChild[] componentsInChildren = damageReceiver.GetComponentsInChildren<DamageReceiverChild>();
			foreach (DamageReceiverChild damageReceiverChild in componentsInChildren)
			{
				Collider2D[] components2 = damageReceiverChild.GetComponents<Collider2D>();
				foreach (Collider2D collider2D3 in components2)
				{
					if (collider2D3.isActiveAndEnabled && CupheadLevelCamera.Current.ContainsPoint(collider2D3.bounds.center, collider2D3.bounds.size / 2f))
					{
						float num3 = Mathf.Abs(MathUtils.DirectionToAngle((Vector2)collider2D3.bounds.center - vector));
						if (num3 < num)
						{
							num = num3;
							collider2D = collider2D3;
						}
					}
				}
			}
		}
		target = collider2D;
		if (partner != null)
		{
			partner.target = collider2D;
		}
	}

	public override void OnLevelEnd()
	{
	}
}
