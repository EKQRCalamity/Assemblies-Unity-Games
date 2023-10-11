using UnityEngine;

public class EaseToRect3D : MonoBehaviour
{
	public bool useScaleTime;

	[Range(1f, 100f)]
	public float easeStiffness = 5f;

	public bool maintainNormalDistance;

	[HideInInspector]
	public Rect3D rect3D;

	private void LateUpdate()
	{
		Vector3? pointNearestTo = rect3D.GetPointNearestTo(base.transform.position);
		if (pointNearestTo.HasValue)
		{
			Vector3 value = pointNearestTo.Value;
			if (maintainNormalDistance)
			{
				value += Vector3.Project(base.transform.position - pointNearestTo.Value, rect3D.normal);
			}
			base.transform.position = MathUtil.EaseV3(base.transform.position, value, easeStiffness, GameUtil.GetDeltaTime(useScaleTime));
		}
	}
}
