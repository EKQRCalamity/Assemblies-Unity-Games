using System;
using UnityEngine;

public abstract class ATransformToTarget : MonoBehaviour
{
	[Flags]
	public enum AtTargetFlags
	{
		Position = 1,
		Rotation = 2,
		Scale = 4
	}

	public Transform target;

	public bool useScaledTime = true;

	[Header("Enabled Transform Properties")]
	public bool positionEnabled = true;

	[EnumFlags]
	public AxesFlags positionFreeAxes;

	public bool rotationEnabled = true;

	public bool scaleEnabled;

	[Range(0f, 100f)]
	public float atTargetThreshold = 0.033f;

	public AtTargetFlags atTargetChecks = AtTargetFlags.Position;

	public bool finishImmediatelyUponDisable;

	public virtual bool atTargetPosition
	{
		get
		{
			if ((bool)target)
			{
				return (target.position - base.transform.position).sqrMagnitude <= atTargetThreshold * atTargetThreshold;
			}
			return false;
		}
	}

	public virtual bool atTargetRotation
	{
		get
		{
			if ((bool)target)
			{
				return Quaternion.Angle(target.rotation, base.transform.rotation) <= atTargetThreshold;
			}
			return false;
		}
	}

	public virtual bool atTargetScale
	{
		get
		{
			if ((bool)target)
			{
				return (target.GetWorldScale() - base.transform.GetWorldScale()).sqrMagnitude <= atTargetThreshold * atTargetThreshold;
			}
			return false;
		}
	}

	public virtual bool atTarget
	{
		get
		{
			if ((!positionEnabled || !EnumUtil.HasFlag(atTargetChecks, AtTargetFlags.Position) || atTargetPosition) && (!rotationEnabled || !EnumUtil.HasFlag(atTargetChecks, AtTargetFlags.Rotation) || atTargetRotation))
			{
				if (scaleEnabled && EnumUtil.HasFlag(atTargetChecks, AtTargetFlags.Scale))
				{
					return atTargetScale;
				}
				return true;
			}
			return false;
		}
	}

	protected virtual bool _isValid => target;

	public event Action onTargetReached;

	protected abstract void _UpdatePosition(float deltaTime);

	protected abstract void _UpdateRotation(float deltaTime);

	protected abstract void _UpdateScale(float deltaTime);

	public virtual void Finish()
	{
		this.onTargetReached = null;
	}

	private void OnDisable()
	{
		if (finishImmediatelyUponDisable && (bool)this)
		{
			Finish();
		}
	}

	private void LateUpdate()
	{
		if (!_isValid)
		{
			return;
		}
		float deltaTime = GameUtil.GetDeltaTime(useScaledTime);
		if (positionEnabled)
		{
			Vector3 position = base.transform.position;
			_UpdatePosition(deltaTime);
			if (positionFreeAxes != 0)
			{
				foreach (AxisType item in EnumUtil.FlagsConverted<AxesFlags, AxisType>(positionFreeAxes))
				{
					base.transform.position = base.transform.position.SetAxis(item, position[(int)item]);
				}
			}
		}
		if (rotationEnabled)
		{
			_UpdateRotation(deltaTime);
		}
		if (scaleEnabled)
		{
			_UpdateScale(deltaTime);
		}
		if (this.onTargetReached != null && atTarget)
		{
			this.onTargetReached();
			this.onTargetReached = null;
		}
	}
}
