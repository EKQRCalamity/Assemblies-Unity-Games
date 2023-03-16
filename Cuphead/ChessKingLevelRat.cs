using System.Collections;
using UnityEngine;

public class ChessKingLevelRat : AbstractCollidableObject
{
	private float speed;

	private float startPosX;

	private DamageDealer damageDealer;

	public void Init(Vector3 position, float speed)
	{
		base.transform.position = position;
		startPosX = base.transform.position.x;
		this.speed = speed;
		Move();
	}

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
	}

	protected void Update()
	{
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

	private void Move()
	{
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		float leftBound = (float)Level.Current.Left + 75f;
		float rightBound = startPosX;
		bool goingRight = false;
		while (true)
		{
			if (goingRight)
			{
				while (base.transform.position.x < rightBound)
				{
					base.transform.position += Vector3.right * speed * CupheadTime.FixedDelta;
					yield return wait;
				}
				goingRight = false;
				base.transform.SetScale(1f, 1f, 1f);
			}
			else
			{
				while (base.transform.position.x > leftBound)
				{
					base.transform.position += Vector3.left * speed * CupheadTime.FixedDelta;
					yield return wait;
				}
				goingRight = true;
				base.transform.SetScale(-1f, 1f, 1f);
			}
			yield return null;
		}
	}
}
