using System.Collections;
using UnityEngine;

public class BaronessLevelGumballProjectile : AbstractProjectile
{
	[SerializeField]
	private Effect trail;

	private Vector2 velocity;

	private float gravity;

	private bool isDead;

	public BaronessLevelGumballProjectile Create(Vector2 pos, Vector2 velocity, float gravity)
	{
		BaronessLevelGumballProjectile baronessLevelGumballProjectile = base.Create() as BaronessLevelGumballProjectile;
		baronessLevelGumballProjectile.velocity = velocity;
		baronessLevelGumballProjectile.transform.position = pos;
		baronessLevelGumballProjectile.gravity = gravity;
		return baronessLevelGumballProjectile;
	}

	protected override void Start()
	{
		base.Start();
		StartCoroutine(spawn_trail_cr());
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
		if (base.transform.position.y <= -360f)
		{
			Die();
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!isDead)
		{
			base.transform.AddPosition(velocity.x * CupheadTime.FixedDelta, velocity.y * CupheadTime.FixedDelta);
			velocity.y -= gravity * CupheadTime.FixedDelta;
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

	private IEnumerator spawn_trail_cr()
	{
		while (true)
		{
			yield return null;
			trail.Create(base.transform.position);
			yield return CupheadTime.WaitForSeconds(this, 0.2f);
		}
	}

	protected override void Die()
	{
		StopAllCoroutines();
		isDead = true;
		base.Die();
		base.animator.SetTrigger("Death");
	}

	private void Kill()
	{
		Object.Destroy(base.gameObject);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		trail = null;
	}
}
