using UnityEngine;

public class SaltbakerLevelPestle : AbstractProjectile
{
	private Vector3 speed;

	private float gravity;

	public SaltbakerLevelPestle Init(Vector3 spawnPos, float velocityX, float velocityY, float gravity)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = spawnPos;
		speed = new Vector3(velocityX, velocityY);
		this.gravity = gravity;
		return this;
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
		Move();
	}

	private void Move()
	{
		speed += new Vector3(0f, gravity * CupheadTime.FixedDelta);
		base.transform.Translate(speed * CupheadTime.FixedDelta);
	}
}
