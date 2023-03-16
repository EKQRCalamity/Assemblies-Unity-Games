using System.Collections;
using UnityEngine;

public class RetroArcadeBouncyBall : RetroArcadeEnemy
{
	private LevelProperties.RetroArcade.Bouncy properties;

	private RetroArcadeBouncyManager manager;

	private Vector3 velocity;

	private float currentAngle;

	private float startAngle;

	public RetroArcadeBouncyBall Create(Vector3 pos, RetroArcadeBouncyManager manager, LevelProperties.RetroArcade.Bouncy properties, float startAngle)
	{
		RetroArcadeBouncyBall retroArcadeBouncyBall = InstantiatePrefab<RetroArcadeBouncyBall>();
		retroArcadeBouncyBall.transform.position = pos;
		retroArcadeBouncyBall.startAngle = startAngle;
		retroArcadeBouncyBall.properties = properties;
		retroArcadeBouncyBall.GetComponent<Collider2D>().enabled = false;
		return retroArcadeBouncyBall;
	}

	public void StartMoving(Vector3 middlePos)
	{
		hp = 1f;
		base.transform.parent = null;
		GetComponent<Collider2D>().enabled = true;
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		velocity = MathUtils.AngleToDirection(startAngle);
		while (true)
		{
			base.transform.position += velocity * properties.groupMoveSpeed * CupheadTime.FixedDelta;
			yield return new WaitForFixedUpdate();
		}
	}

	protected override void OnCollisionCeiling(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionCeiling(hit, phase);
		Vector3 newVelocity = velocity;
		newVelocity.y = Mathf.Min(newVelocity.y, 0f - newVelocity.y);
		ChangeDir(newVelocity);
	}

	protected override void OnCollisionGround(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionGround(hit, phase);
		Vector3 newVelocity = velocity;
		newVelocity.y = Mathf.Max(newVelocity.y, 0f - newVelocity.y);
		ChangeDir(newVelocity);
	}

	protected void ChangeDir(Vector3 newVelocity)
	{
		velocity = newVelocity;
		currentAngle = Mathf.Atan2(velocity.y, velocity.x) * 57.29578f;
		base.transform.SetEulerAngles(0f, 0f, currentAngle);
	}

	protected override void OnCollisionWalls(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionWalls(hit, phase);
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

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (!base.IsDead)
		{
			base.OnCollisionPlayer(hit, phase);
		}
	}

	public override void Dead()
	{
		base.Dead();
		GetComponent<Collider2D>().enabled = true;
		GetComponent<DamageReceiver>().enabled = false;
		Object.Destroy(GetComponent<Rigidbody2D>());
	}
}
