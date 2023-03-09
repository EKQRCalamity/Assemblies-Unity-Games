using System;
using System.Collections;
using UnityEngine;

public class ChessBOldALevelWall : AbstractProjectile
{
	private ChessBOldALevelBishop parent;

	private bool isClockwise;

	private float angle;

	private float loopSize;

	private float speed;

	protected override float DestroyLifetime => 0f;

	public void StartRotate(float angle, ChessBOldALevelBishop parent, float loopSize, float speed, bool isClockwise, float scale)
	{
		ResetLifetime();
		ResetDistance();
		this.angle = angle;
		this.parent = parent;
		this.speed = speed;
		this.loopSize = loopSize;
		this.isClockwise = isClockwise;
		base.transform.SetScale(scale);
		StartCoroutine(move_wall_cr());
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private IEnumerator move_wall_cr()
	{
		Vector3 handleRotation2 = Vector3.zero;
		if (angle == 0f || angle == 180f)
		{
			base.transform.position = (Vector2)parent.transform.position + MathUtils.AngleToDirection(angle) * loopSize;
		}
		else
		{
			base.transform.position = (Vector2)parent.transform.position + MathUtils.AngleToDirection(angle) * loopSize;
		}
		angle *= (float)Math.PI / 180f;
		while (true)
		{
			if (isClockwise)
			{
				angle += speed * CupheadTime.FixedDelta;
			}
			else
			{
				angle -= speed * CupheadTime.FixedDelta;
			}
			handleRotation2 = new Vector3(Mathf.Sin(angle) * loopSize, Mathf.Cos(angle) * loopSize, 0f);
			base.transform.position = parent.transform.position;
			base.transform.position += handleRotation2;
			Vector3 diff = parent.transform.position - base.transform.position;
			diff.Normalize();
			base.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(diff.y, diff.x) * 57.29578f + 90f);
			yield return new WaitForFixedUpdate();
		}
	}

	public void Dead()
	{
		this.Recycle();
	}
}
