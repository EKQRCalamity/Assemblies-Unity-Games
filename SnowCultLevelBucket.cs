using UnityEngine;

public class SnowCultLevelBucket : MonoBehaviour
{
	[SerializeField]
	private float fallSpeed = 10f;

	[SerializeField]
	private float accel = 1f;

	private void FixedUpdate()
	{
		base.transform.position += fallSpeed * Vector3.down * CupheadTime.FixedDelta;
		fallSpeed += CupheadTime.FixedDelta * accel;
	}
}
