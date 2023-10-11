using UnityEngine;

public class SpringToTarget : ATransformToTarget
{
	[Header("Spring Settings")]
	[Range(10f, 1000f)]
	public float positionConstant = 100f;

	[Range(1f, 100f)]
	public float positionDampening = 10f;

	[Range(10f, 1000f)]
	public float rotationConstant = 100f;

	[Range(1f, 100f)]
	public float rotationDampening = 10f;

	[Range(10f, 1000f)]
	public float scaleConstant = 100f;

	[Range(1f, 100f)]
	public float scaleDampening = 10f;

	private Vector3 _positionVelocity;

	private Vector4 _rotationVelocity;

	private Vector3 _scaleVelocity;

	public override bool atTargetPosition
	{
		get
		{
			if (base.atTargetPosition)
			{
				return _positionVelocity.sqrMagnitude <= atTargetThreshold * atTargetThreshold;
			}
			return false;
		}
	}

	public override bool atTargetRotation
	{
		get
		{
			if (base.atTargetRotation)
			{
				return _rotationVelocity.sqrMagnitude <= atTargetThreshold * atTargetThreshold;
			}
			return false;
		}
	}

	public override bool atTargetScale
	{
		get
		{
			if (base.atTargetScale)
			{
				return _scaleVelocity.sqrMagnitude <= atTargetThreshold * atTargetThreshold;
			}
			return false;
		}
	}

	private void OnEnable()
	{
		_positionVelocity = Vector3.zero;
		_rotationVelocity = Vector4.zero;
		_scaleVelocity = Vector3.zero;
	}

	protected override void _UpdatePosition(float deltaTime)
	{
		Vector3 position = base.transform.position;
		MathUtil.Spring(ref position, ref _positionVelocity, target.position, positionConstant, positionDampening, deltaTime);
		base.transform.position = position;
	}

	protected override void _UpdateRotation(float deltaTime)
	{
		Quaternion current = base.transform.rotation;
		MathUtil.Spring(ref current, ref _rotationVelocity, target.rotation, rotationConstant, rotationDampening, deltaTime);
		base.transform.rotation = current;
	}

	protected override void _UpdateScale(float deltaTime)
	{
		Vector3 position = base.transform.GetWorldScale();
		MathUtil.Spring(ref position, ref _scaleVelocity, target.GetWorldScale(), scaleConstant, scaleDampening, deltaTime);
		base.transform.SetWorldScale(position);
	}
}
