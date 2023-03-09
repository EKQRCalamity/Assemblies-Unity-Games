using UnityEngine;

public class RobotLevelHoseLaser : AbstractCollidableObject
{
	private DamageDealer damageDealer;

	public RobotLevelHoseLaser Create(Vector3 pos, float angle, RobotLevelRobotBodyPart parent)
	{
		GameObject gameObject = Object.Instantiate(base.gameObject);
		gameObject.transform.parent = parent.transform;
		gameObject.transform.position = pos;
		gameObject.transform.SetEulerAngles(null, null, angle);
		GetComponent<Collider2D>().enabled = false;
		return gameObject.GetComponent<RobotLevelHoseLaser>();
	}

	protected override void Awake()
	{
		base.Awake();
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
}
