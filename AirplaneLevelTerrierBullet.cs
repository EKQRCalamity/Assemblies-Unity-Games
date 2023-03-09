using UnityEngine;

public class AirplaneLevelTerrierBullet : BasicProjectile
{
	private const float BASE_ACCELERATION = 1.6f;

	protected Vector3 velocity;

	protected Vector3 startVelocity;

	protected Vector3 endVelocity;

	protected float accelT;

	protected float accel;

	public AirplaneLevelTerrierBullet Create(Vector2 position, float rotation, float speed, float acceleration)
	{
		AirplaneLevelTerrierBullet airplaneLevelTerrierBullet = Create(position, rotation) as AirplaneLevelTerrierBullet;
		airplaneLevelTerrierBullet.endVelocity = MathUtils.AngleToDirection(rotation) * speed;
		airplaneLevelTerrierBullet.startVelocity = airplaneLevelTerrierBullet.endVelocity.normalized * 0.1f;
		airplaneLevelTerrierBullet.accelT = 0f;
		airplaneLevelTerrierBullet.accel = acceleration;
		airplaneLevelTerrierBullet.transform.rotation = Quaternion.identity;
		float num = Vector3.SignedAngle(airplaneLevelTerrierBullet.velocity, Vector3.up, Vector3.forward);
		while (Mathf.Abs(num) > 45f)
		{
			num -= 90f * Mathf.Sign(num);
		}
		airplaneLevelTerrierBullet.transform.Rotate(new Vector3(0f, 0f, 0f - num));
		return airplaneLevelTerrierBullet;
	}

	public void PlayWow()
	{
		base.animator.Play("WowIntro");
	}

	protected override void Move()
	{
		accelT += CupheadTime.FixedDelta * accel * 1.6f;
		velocity = Vector3.Lerp(startVelocity, endVelocity, accelT);
		base.transform.position += velocity * CupheadTime.FixedDelta;
	}
}
