using UnityEngine;

public class PlatformingLevelRotator : AbstractCollidableObject
{
	public float speed = 180f;

	private DamageDealer damageDealer;

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
	}

	private void Update()
	{
		base.transform.AddEulerAngles(0f, 0f, (0f - speed) * (float)CupheadTime.Delta);
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
}
