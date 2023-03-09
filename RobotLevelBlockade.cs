using System.Collections;
using UnityEngine;

public class RobotLevelBlockade : AbstractCollidableObject
{
	private const float heightOffset = 300f;

	private DamageDealer damageDealer;

	private RobotLevelBlockade rootSegment;

	private int xSpeed;

	private int ySpeed;

	public RobotLevelBlockade Create(Vector3 origin, int dir)
	{
		GameObject gameObject = Object.Instantiate(base.gameObject);
		gameObject.transform.position = origin + Vector3.up * dir * 300f;
		rootSegment = gameObject.GetComponent<RobotLevelBlockade>();
		return rootSegment;
	}

	public void InitBlockade(int dir, int xSpeed, int ySpeed)
	{
		this.xSpeed = xSpeed;
		this.ySpeed = ySpeed * -dir;
		StartCoroutine(move_cr());
	}

	protected override void Awake()
	{
		damageDealer = DamageDealer.NewEnemy();
		base.Awake();
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
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private IEnumerator move_cr()
	{
		while (true)
		{
			base.transform.position += (Vector3.left * xSpeed + Vector3.up * ySpeed) * CupheadTime.Delta;
			yield return null;
		}
	}
}
