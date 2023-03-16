using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BasicDamageDealingObject : AbstractCollidableObject
{
	[SerializeField]
	private float damageRate = 0.2f;

	private DamageDealer damageDealer;

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
		damageDealer.SetRate(damageRate);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}
}
