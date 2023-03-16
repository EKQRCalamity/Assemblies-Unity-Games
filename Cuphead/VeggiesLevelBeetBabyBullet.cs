using UnityEngine;

public class VeggiesLevelBeetBabyBullet : AbstractProjectile
{
	public enum State
	{
		Go,
		Dead
	}

	private State state;

	private float speed;

	public VeggiesLevelBeetBabyBullet Create(float speed, Vector2 pos, float rot)
	{
		VeggiesLevelBeetBabyBullet veggiesLevelBeetBabyBullet = Create(pos, rot) as VeggiesLevelBeetBabyBullet;
		veggiesLevelBeetBabyBullet.CollisionDeath.OnlyPlayer();
		veggiesLevelBeetBabyBullet.DamagesType.OnlyPlayer();
		veggiesLevelBeetBabyBullet.speed = speed;
		return veggiesLevelBeetBabyBullet;
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Update()
	{
		base.Update();
		if (state != State.Dead)
		{
			base.transform.position += base.transform.right * speed * CupheadTime.Delta;
			if (base.transform.position.y < (float)Level.Current.Ground)
			{
				Die();
			}
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		damageDealer.DealDamage(hit);
		base.OnCollisionPlayer(hit, phase);
	}

	protected override void Die()
	{
		base.Die();
		state = State.Dead;
		base.animator.SetTrigger("Death");
		GetComponent<Collider2D>().enabled = false;
	}
}
