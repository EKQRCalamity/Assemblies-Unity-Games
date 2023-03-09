using UnityEngine;

public class MausoleumLevelUrn : AbstractCollidableObject
{
	private DamageDealer damageDealer;

	public static Vector3 URN_POS { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		URN_POS = base.transform.position;
		damageDealer = DamageDealer.NewEnemy();
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}
}
