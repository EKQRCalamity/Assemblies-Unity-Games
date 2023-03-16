using UnityEngine;

public class DevilLevelTear : AbstractProjectile
{
	private float speed;

	public DevilLevelTear CreateTear(Vector2 position, float speed)
	{
		DevilLevelTear devilLevelTear = InstantiatePrefab<DevilLevelTear>();
		devilLevelTear.transform.position = position;
		devilLevelTear.speed = speed;
		devilLevelTear.animator.Play("Drop_" + Random.Range(1, 7).ToStringInvariant());
		return devilLevelTear;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.dead)
		{
			base.transform.AddPosition(0f, (0f - speed) * CupheadTime.FixedDelta);
		}
	}
}
