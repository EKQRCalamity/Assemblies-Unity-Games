using System;
using UnityEngine;

public class AnimateMaterialFloat : AnimateMaterialProperty
{
	[Header("Float")]
	public bool asMultiplier;

	public Vector2 range = new Vector2(0f, 0f);

	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType animType;

	private float initialValue;

	protected override Type propertyType => typeof(float);

	public override void CacheInitialValues()
	{
		base.CacheInitialValues();
		if (propertyFound)
		{
			initialValue = material.GetFloat(propertyName);
		}
	}

	protected override void UniqueUpdate(float t)
	{
		if (propertyFound)
		{
			material.SetFloat(propertyName, GetValue(range, curve, animType, t, 1f) * (asMultiplier ? initialValue : 1f));
		}
	}

	public void Range(Vector2 range)
	{
		this.range = range;
	}
}
