using System;
using UnityEngine;

public class AnimateMaterialColor : AnimateMaterialProperty
{
	[Header("Color")]
	public bool asMultiplier;

	public Color tint = Color.white;

	public Gradient gradient = new Gradient();

	public AnimationCurve gradientSample = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType animType;

	private Color initialValue;

	protected override Type propertyType => typeof(Color);

	public override void CacheInitialValues()
	{
		if (string.IsNullOrEmpty(propertyName))
		{
			propertyName = GetComponent<Renderer>().material.FindColor();
		}
		base.CacheInitialValues();
		if (propertyFound)
		{
			initialValue = material.GetColor(propertyName);
		}
	}

	protected override void UniqueUpdate(float t)
	{
		if (propertyFound)
		{
			material.SetColor(propertyName, GetColor(gradient, gradientSample, animType, t, 1f, tint) * (asMultiplier ? initialValue : Color.white));
		}
	}

	public void Tint(Color tint)
	{
		this.tint = tint;
	}
}
