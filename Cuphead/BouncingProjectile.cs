using System.Collections;
using UnityEngine;

public class BouncingProjectile : AbstractProjectile
{
	public bool isMoving;

	protected float speed;

	protected float currentAngle;

	protected Vector3 velocity;

	protected override void Start()
	{
		base.Start();
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		while (true)
		{
			if (isMoving)
			{
				base.transform.position += velocity * speed * CupheadTime.FixedDelta;
			}
			yield return new WaitForFixedUpdate();
		}
	}

	protected override void OnCollisionCeiling(GameObject hit, CollisionPhase phase)
	{
		Vector3 newVelocity = velocity;
		newVelocity.y = Mathf.Min(newVelocity.y, 0f - newVelocity.y);
		ChangeDir(newVelocity);
	}

	protected override void OnCollisionGround(GameObject hit, CollisionPhase phase)
	{
		Vector3 newVelocity = velocity;
		newVelocity.y = Mathf.Max(newVelocity.y, 0f - newVelocity.y);
		ChangeDir(newVelocity);
	}

	protected override void OnCollisionWalls(GameObject hit, CollisionPhase phase)
	{
		Vector3 newVelocity = velocity;
		if (base.transform.position.x > 0f)
		{
			newVelocity.x = Mathf.Min(newVelocity.x, 0f - newVelocity.x);
			ChangeDir(newVelocity);
		}
		else
		{
			newVelocity.x = Mathf.Max(newVelocity.x, 0f - newVelocity.x);
			ChangeDir(newVelocity);
		}
	}

	protected virtual void ChangeDir(Vector3 newVelocity)
	{
		velocity = newVelocity;
		currentAngle = Mathf.Atan2(velocity.y, velocity.x) * 57.29578f;
		base.transform.SetEulerAngles(0f, 0f, currentAngle);
	}
}
