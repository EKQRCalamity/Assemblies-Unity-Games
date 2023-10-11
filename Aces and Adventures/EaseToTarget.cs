using UnityEngine;

public class EaseToTarget : ATransformToTarget
{
	[Header("Ease Settings")]
	[Range(1f, 100f)]
	public float positionEaseStiffness = 10f;

	[Range(1f, 100f)]
	public float rotationEaseStiffness = 10f;

	[Range(1f, 100f)]
	public float scaleEaseStiffness = 10f;

	protected override void _UpdatePosition(float deltaTime)
	{
		base.transform.position = MathUtil.EaseV3(base.transform.position, target.position, positionEaseStiffness, deltaTime);
	}

	protected override void _UpdateRotation(float deltaTime)
	{
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, target.rotation, MathUtil.CalculateEaseStiffnessSubjectToTime(rotationEaseStiffness, deltaTime));
	}

	protected override void _UpdateScale(float deltaTime)
	{
		base.transform.SetWorldScale(MathUtil.EaseV3(base.transform.GetWorldScale(), target.GetWorldScale(), scaleEaseStiffness, deltaTime));
	}
}
