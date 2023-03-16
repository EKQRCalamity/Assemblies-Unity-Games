using UnityEngine;

public class TrainLevelEngineBossTail : ParrySwitch
{
	private CircleCollider2D circleCollider;

	private Transform target;

	public bool tailEnabled
	{
		get
		{
			return !(circleCollider == null) && circleCollider.enabled;
		}
		set
		{
			if (circleCollider != null)
			{
				circleCollider.enabled = value;
			}
		}
	}

	public static TrainLevelEngineBossTail Create(Transform target)
	{
		GameObject gameObject = new GameObject("Engine_Boss_Tail");
		TrainLevelEngineBossTail trainLevelEngineBossTail = gameObject.AddComponent<TrainLevelEngineBossTail>();
		trainLevelEngineBossTail.target = target;
		trainLevelEngineBossTail.tag = "ParrySwitch";
		return trainLevelEngineBossTail;
	}

	protected override void Awake()
	{
		base.Awake();
		circleCollider = base.gameObject.AddComponent<CircleCollider2D>();
		circleCollider.radius = 40f;
		circleCollider.isTrigger = true;
	}

	private void Update()
	{
		UpdateLocation();
	}

	private void LateUpdate()
	{
		UpdateLocation();
	}

	private void UpdateLocation()
	{
		if (target != null)
		{
			base.transform.position = target.position;
		}
	}
}
