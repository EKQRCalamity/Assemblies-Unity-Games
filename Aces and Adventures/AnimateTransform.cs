using UnityEngine;

public abstract class AnimateTransform : AnimateComponent
{
	[Header("X")]
	public Vector2 xRange = new Vector2(0f, 0f);

	public AnimationCurve xSample = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType xAnimType;

	public float xRepeat = 1f;

	[Header("Y")]
	public Vector2 yRange = new Vector2(0f, 0f);

	public AnimationCurve ySample = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType yAnimType;

	public float yRepeat = 1f;

	[Header("Z")]
	public Vector2 zRange = new Vector2(0f, 0f);

	public AnimationCurve zSample = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType zAnimType;

	public float zRepeat = 1f;

	protected Vector3 initialVector;

	protected virtual bool _additive => true;

	protected Vector3 GetVector(Vector3 current, float t, bool keepCurrentTransform = true)
	{
		Vector3 result = initialVector;
		if (xRange.x != xRange.y)
		{
			float value = GetValue(xRange, xSample, xAnimType, t, xRepeat);
			if (_additive)
			{
				result.x += value;
			}
			else
			{
				result.x *= value;
			}
		}
		else if (keepCurrentTransform)
		{
			result.x = current.x;
		}
		if (yRange.x != yRange.y)
		{
			float value2 = GetValue(yRange, ySample, yAnimType, t, yRepeat);
			if (_additive)
			{
				result.y += value2;
			}
			else
			{
				result.y *= value2;
			}
		}
		else if (keepCurrentTransform)
		{
			result.y = current.y;
		}
		if (zRange.x != zRange.y)
		{
			float value3 = GetValue(zRange, zSample, zAnimType, t, zRepeat);
			if (_additive)
			{
				result.z += value3;
			}
			else
			{
				result.z *= value3;
			}
		}
		else if (keepCurrentTransform)
		{
			result.z = current.z;
		}
		return result;
	}

	public void XRange(Vector2 xRange)
	{
		this.xRange = xRange;
	}

	public void YRange(Vector2 yRange)
	{
		this.yRange = yRange;
	}

	public void ZRange(Vector2 zRange)
	{
		this.zRange = zRange;
	}
}
