using UnityEngine;

public class AirplaneLevelTurretBullet : AbstractProjectile
{
	private Vector2 velocity;

	private float gravity;

	public AirplaneLevelTurretBullet Create(Vector2 pos, Vector2 velocity, float gravity)
	{
		AirplaneLevelTurretBullet airplaneLevelTurretBullet = base.Create() as AirplaneLevelTurretBullet;
		airplaneLevelTurretBullet.velocity = velocity;
		airplaneLevelTurretBullet.transform.position = pos;
		airplaneLevelTurretBullet.gravity = gravity;
		return airplaneLevelTurretBullet;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		base.transform.AddPosition(velocity.x * CupheadTime.FixedDelta, velocity.y * CupheadTime.FixedDelta);
		velocity.y -= gravity * CupheadTime.FixedDelta;
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase == CollisionPhase.Enter)
		{
			damageDealer.DealDamage(hit);
			AudioManager.Play("sfx_dlc_dogfight_p1_terrierplane_baseball_impact");
		}
	}
}
