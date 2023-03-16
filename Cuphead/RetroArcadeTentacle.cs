using System.Collections;
using UnityEngine;

public class RetroArcadeTentacle : AbstractProjectile
{
	[SerializeField]
	private RetroArcadeTentacleTarget target;

	[SerializeField]
	private Transform targetRoot;

	private const float OFFSET = 15f;

	private float verticalSpeed;

	private float horizontalSpeed;

	private bool onLeft;

	private bool canMove;

	private Vector3 startPos;

	public virtual AbstractProjectile Init(Vector3 pos, float targetPosY, bool onLeft, float verticalSpeed, float horizontalSpeed)
	{
		ResetLifetime();
		ResetDistance();
		target.transform.SetLocalPosition(targetRoot.localPosition.x + ((!onLeft) ? (-15f) : 15f), targetRoot.localPosition.y + targetPosY);
		this.verticalSpeed = verticalSpeed;
		this.horizontalSpeed = horizontalSpeed;
		this.onLeft = onLeft;
		base.transform.position = pos;
		startPos = pos;
		StartCoroutine(move_cr());
		return this;
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
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		Vector3 direction = ((!onLeft) ? Vector3.left : Vector3.right);
		canMove = true;
		while (base.transform.position.y < 0f)
		{
			base.transform.position += Vector3.up * verticalSpeed * CupheadTime.FixedDelta;
			yield return wait;
		}
		while (canMove && !target.IsDead)
		{
			base.transform.position += direction * horizontalSpeed * CupheadTime.FixedDelta;
			yield return wait;
		}
		while (base.transform.position.y > startPos.y)
		{
			base.transform.position += Vector3.down * verticalSpeed * CupheadTime.FixedDelta;
			yield return wait;
		}
		this.Recycle();
	}

	protected override void OnCollision(GameObject hit, CollisionPhase phase)
	{
		base.OnCollision(hit, phase);
		if ((bool)hit.GetComponent<RetroArcadeTentacle>())
		{
			canMove = false;
		}
	}
}
