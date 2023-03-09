using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetroArcadeTrafficUFO : AbstractCollidableObject
{
	private const float DIST_TO_SWITCH = 3f;

	private DamageDealer damageDealer;

	public bool IsMoving { get; private set; }

	public bool IsDead { get; private set; }

	private void Start()
	{
		base.gameObject.SetActive(value: false);
		damageDealer = DamageDealer.NewEnemy();
	}

	private void Update()
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

	public void StartMoving(List<Vector3> positions, float speed, float delay)
	{
		IsMoving = true;
		StartCoroutine(check_pieces_cr());
		StartCoroutine(move_cr(positions, speed, delay));
	}

	private IEnumerator move_cr(List<Vector3> positions, float speed, float delay)
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		for (int i = 0; i < positions.Count; i++)
		{
			yield return CupheadTime.WaitForSeconds(this, delay);
			Vector3 dir = (positions[i] - base.transform.position).normalized;
			while (Vector3.Distance(base.transform.position, positions[i]) > 3f)
			{
				base.transform.position += dir * speed * CupheadTime.FixedDelta;
				yield return wait;
			}
			yield return null;
		}
		IsMoving = false;
	}

	private IEnumerator check_pieces_cr()
	{
		RetroArcadeTrafficUFOPiece[] pieces = GetComponentsInChildren<RetroArcadeTrafficUFOPiece>();
		int countDeadOnes2 = 0;
		while (true)
		{
			countDeadOnes2 = 0;
			for (int i = 0; i < pieces.Length; i++)
			{
				if (pieces[i].IsDead)
				{
					countDeadOnes2++;
				}
			}
			if (countDeadOnes2 >= pieces.Length)
			{
				break;
			}
			yield return null;
		}
		IsDead = true;
		yield return null;
	}
}
