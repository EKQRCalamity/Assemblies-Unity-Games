using UnityEngine;

public class BasicProjectile : AbstractProjectile
{
	[Space(10f)]
	public float Speed;

	public float Gravity;

	[Space(10f)]
	public Sfx SfxOnDeath;

	protected bool move = true;

	protected float _accumulativeGravity;

	protected virtual Vector3 Direction => base.transform.right;

	protected override float DestroyLifetime => 10f;

	protected override bool DestroyedAfterLeavingScreen => true;

	protected virtual string projectileMissImpactSFX => "player_weapon_peashot_miss";

	public virtual BasicProjectile Create(Vector2 position, float rotation, float speed)
	{
		BasicProjectile basicProjectile = Create(position, rotation) as BasicProjectile;
		basicProjectile.Speed = speed;
		return basicProjectile;
	}

	public virtual BasicProjectile Create(Vector2 position, float rotation, Vector2 scale, float speed)
	{
		BasicProjectile basicProjectile = Create(position, rotation, scale) as BasicProjectile;
		basicProjectile.Speed = speed;
		return basicProjectile;
	}

	protected override void Awake()
	{
		base.Awake();
		if (CompareTag("EnemyProjectile"))
		{
			DamagesType.Player = true;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnCollision(GameObject hit, CollisionPhase phase)
	{
		base.OnCollision(hit, phase);
	}

	protected override void OnCollisionOther(GameObject hit, CollisionPhase phase)
	{
		if (!(hit.tag == "Parry"))
		{
			base.OnCollisionOther(hit, phase);
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			DealDamage(hit);
		}
		base.OnCollisionEnemy(hit, phase);
	}

	protected override void OnCollisionDie(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionDie(hit, phase);
		if (base.tag == "PlayerProjectile" && phase == CollisionPhase.Enter)
		{
			if (((bool)hit.GetComponent<DamageReceiver>() && hit.GetComponent<DamageReceiver>().enabled) || ((bool)hit.GetComponent<DamageReceiverChild>() && hit.GetComponent<DamageReceiverChild>().enabled))
			{
				AudioManager.Play("player_shoot_hit_cuphead");
			}
			else
			{
				AudioManager.Play(projectileMissImpactSFX);
			}
		}
	}

	private void DealDamage(GameObject hit)
	{
		damageDealer.DealDamage(hit);
	}

	protected override void Die()
	{
		move = false;
		EffectSpawner component = GetComponent<EffectSpawner>();
		if (component != null)
		{
			Object.Destroy(component);
		}
		base.Die();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (move)
		{
			Move();
		}
	}

	protected virtual void Move()
	{
		base.transform.position += Direction * Speed * CupheadTime.FixedDelta - new Vector3(0f, _accumulativeGravity * CupheadTime.FixedDelta, 0f);
		_accumulativeGravity += Gravity * CupheadTime.FixedDelta;
	}
}
