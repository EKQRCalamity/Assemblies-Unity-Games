using UnityEngine;

public class SnowCultLevelBatDrip : SnowCultLevelBatEffect
{
	private const float GROUND_OFFSET = -20f;

	[SerializeField]
	private float gravity = 10f;

	public Vector3 vel;

	private void FixedUpdate()
	{
		base.transform.position += vel * CupheadTime.FixedDelta;
		vel.y -= gravity * CupheadTime.FixedDelta;
		if (base.transform.position.y <= (float)Level.Current.Ground + -20f)
		{
			vel = Vector3.zero;
			gravity = 0f;
			base.animator.Play("Splat" + colorString);
		}
	}
}
