using UnityEngine;

public class FunhousePlatformingLevelConveyorBelt : ScrollingSprite
{
	public Vector3 point;

	public bool rightToCenter;

	public float wait;

	protected override void Start()
	{
		base.Start();
		point += base.transform.position;
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		Gizmos.DrawSphere(point + base.transform.position, 10f);
	}

	protected override void Update()
	{
		wait -= CupheadTime.Delta;
		if (wait > 0f)
		{
			return;
		}
		base.Update();
		for (int i = 0; i < base.copyRenderers.Count; i++)
		{
			Vector3 position = base.copyRenderers[i].transform.position;
			position.z = position.x - point.x;
			if (rightToCenter)
			{
				position.z *= -1f;
			}
			base.copyRenderers[i].transform.position = position;
		}
	}
}
