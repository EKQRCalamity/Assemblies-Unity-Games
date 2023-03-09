using UnityEngine;

public class SaltbakerLevelHand : AbstractCollidableObject
{
	[SerializeField]
	private BasicProjectile projectile;

	[SerializeField]
	private Transform root;

	[SerializeField]
	private bool leftHand;

	private DamageDealer damageDealer;

	private void Start()
	{
		damageDealer = DamageDealer.NewEnemy();
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
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public void Shoot(float speed)
	{
		float rotation = ((!leftHand) ? 180f : 0f);
		projectile.Create(root.position, rotation, speed);
	}
}
