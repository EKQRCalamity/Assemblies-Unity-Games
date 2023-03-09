using UnityEngine;

public class MouseLevelGhostMouseBall : AbstractProjectile
{
	public enum State
	{
		Init,
		Moving,
		Dead
	}

	[SerializeField]
	private BasicProjectile childProjectile;

	private State state;

	private Vector2 velocity;

	private float childSpeed;

	public MouseLevelGhostMouseBall Create(Vector2 pos, float speed, float childSpeed)
	{
		MouseLevelGhostMouseBall mouseLevelGhostMouseBall = InstantiatePrefab<MouseLevelGhostMouseBall>();
		Vector2 vector = new Vector2(PlayerManager.GetNext().transform.position.x, Level.Current.Ground);
		Vector2 normalized = (vector - pos).normalized;
		mouseLevelGhostMouseBall.transform.position = pos;
		mouseLevelGhostMouseBall.velocity = speed * normalized;
		mouseLevelGhostMouseBall.childSpeed = childSpeed;
		mouseLevelGhostMouseBall.state = State.Moving;
		mouseLevelGhostMouseBall.transform.Rotate(0f, 0f, MathUtils.DirectionToAngle(normalized) - 90f);
		return mouseLevelGhostMouseBall;
	}

	protected override void Update()
	{
		base.Update();
		if (state == State.Moving)
		{
			if (base.transform.position.y < (float)Level.Current.Ground)
			{
				Explode();
			}
			else
			{
				base.transform.AddPosition(velocity.x * (float)CupheadTime.Delta, velocity.y * (float)CupheadTime.Delta);
			}
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (damageDealer != null && phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void Explode()
	{
		state = State.Dead;
		childProjectile.Create(base.transform.position, 0f, Vector2.one, childSpeed);
		childProjectile.Create(base.transform.position, 0f, new Vector2(1f, -1f), 0f - childSpeed);
		Die();
	}

	protected override void Die()
	{
		base.Die();
		base.transform.SetLocalEulerAngles(0f, 0f, 0f);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		childProjectile = null;
	}
}
