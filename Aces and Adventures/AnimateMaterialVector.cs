using System;
using UnityEngine;

public abstract class AnimateMaterialVector : AnimateMaterialProperty
{
	[Header("Vector")]
	public bool asMultiplier;

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

	protected Vector4 initialValue;

	protected override Type propertyType => typeof(Vector4);

	public override void CacheInitialValues()
	{
		base.CacheInitialValues();
		if (propertyFound)
		{
			initialValue = material.GetVector(propertyName);
		}
	}

	protected override void UniqueUpdate(float t)
	{
		if (propertyFound)
		{
			material.SetVector(propertyName, GetValue(t));
		}
	}

	protected virtual Vector4 GetValue(float t)
	{
		Vector4 vector = new Vector2(GetValue(xRange, xSample, xAnimType, t, xRepeat), GetValue(yRange, ySample, yAnimType, t, yRepeat));
		if (asMultiplier)
		{
			return vector.MultiplyV4(initialValue);
		}
		return vector;
	}

	public void XRange(Vector2 xRange)
	{
		this.xRange = xRange;
	}

	public void YRange(Vector2 yRange)
	{
		this.yRange = yRange;
	}
}
