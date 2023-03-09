using UnityEngine;

public class GraveyardLevelPlatform : AbstractMonoBehaviour
{
	private float t;

	private Vector3 center;

	[SerializeField]
	private float radius = 700f;

	[SerializeField]
	private float speed = 315f;

	[SerializeField]
	private float maxAngle = 30f;

	private void Start()
	{
		center = base.transform.position;
		t = -0.8f;
	}

	private void Update()
	{
		t += (float)CupheadTime.Delta * speed;
		base.transform.position = center + (Vector3)MathUtils.AngleToDirection(-90f + Mathf.Sin(t) * maxAngle) * radius;
	}
}
