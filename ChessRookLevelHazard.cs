using System.Collections;
using UnityEngine;

public class ChessRookLevelHazard : AbstractProjectile
{
	private float speed;

	public ChessRookLevelHazard Create(Vector3 position, float speed)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = position;
		this.speed = speed;
		Move();
		return this;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void Move()
	{
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		while (base.transform.position.x > -740f)
		{
			base.transform.position += Vector3.left * speed * CupheadTime.FixedDelta;
			yield return wait;
		}
		this.Recycle();
	}
}
