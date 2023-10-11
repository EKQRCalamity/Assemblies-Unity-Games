using UnityEngine;

public struct TransformTarget
{
	public Vector3 position;

	public Quaternion rotation;

	public Vector3 scale;

	public Quaternion originalRotation;

	public float rotationLerp;

	public float rotationVelocity;

	public float finishedTime;

	public bool finished => finishedTime >= 0f;

	public static TransformTarget SpringToTarget(TransformTarget current, TransformTarget target, ref TransformTarget velocities, float positionSpringConstant, float positionSpringDampening, float rotationSpringConstant, float rotationSpringDampening, float scaleSpringConstant, float scaleSpringDampening, float time, float targetRotationLerp = 1f)
	{
		MathUtil.Spring(ref current.position, ref velocities.position, target.position, positionSpringConstant, positionSpringDampening, time);
		current.rotation = MathUtil.Spring(ref velocities.rotationLerp, ref velocities.rotationVelocity, target.originalRotation, target.rotation, rotationSpringConstant, rotationSpringDampening, time, targetRotationLerp);
		MathUtil.Spring(ref current.scale, ref velocities.scale, target.scale, scaleSpringConstant, scaleSpringDampening, time);
		return current;
	}

	public TransformTarget(Vector3 position, Quaternion rotation, Vector3 scale)
	{
		this.position = position;
		this.rotation = rotation;
		this.scale = scale;
		originalRotation = rotation;
		rotationLerp = 0f;
		rotationVelocity = 0f;
		finishedTime = -1f;
	}

	public TransformTarget(Vector3 position, Quaternion rotation, Vector3 scale, Quaternion originalRotation)
		: this(position, rotation, scale)
	{
		this.originalRotation = originalRotation;
	}

	public TransformTarget(Transform transform)
		: this(transform.localPosition, transform.localRotation, transform.localScale)
	{
	}

	public void SetTransformValues(Transform transform)
	{
		transform.localPosition = position;
		transform.localRotation = rotation;
		transform.localScale = Vector3.Max(Vector3.zero, scale);
	}

	public TransformTarget ResetRotationData(bool resetLerp = true, bool resetVelocity = true)
	{
		TransformTarget result = this;
		if (resetLerp)
		{
			result.rotationLerp = 0f;
		}
		if (resetVelocity)
		{
			result.rotationVelocity = 0f;
		}
		result.finishedTime = -1f;
		return result;
	}

	public TransformTarget OffsetXPosition(float offset)
	{
		TransformTarget result = this;
		result.position.x += offset;
		return result;
	}

	public TransformTarget OffsetZPosition(float offset)
	{
		TransformTarget result = this;
		result.position.z += offset;
		return result;
	}

	public bool IsRoughlyEqual(TransformTarget other, float deltaMultiplier = 1f, float positionDelta = 1f, float scaleDelta = 0.01f, float rotationDelta = 0.1f)
	{
		if ((position - other.position).magnitude < positionDelta * deltaMultiplier && (scale - other.scale).magnitude < scaleDelta * deltaMultiplier)
		{
			return Quaternion.Angle(rotation, other.rotation) < rotationDelta * deltaMultiplier;
		}
		return false;
	}

	public override string ToString()
	{
		return $"Position: {position}, Rotation: {rotation}, Scale: {scale}, OriginalRotation: {originalRotation}, RotationLerp: {rotationLerp}, RotationVelocity: {rotationVelocity}, FinishedTime: {finishedTime}, finished: {finished}";
	}
}
