using System;
using UnityEngine;

[Serializable]
public class TransformSpringSettings
{
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

	public TransformSpringSettings SetAsLerpBetween(TransformSpringSettings a, TransformSpringSettings b, float t)
	{
		positionConstant = MathUtil.Lerp(a.positionConstant, b.positionConstant, t);
		positionDampening = MathUtil.Lerp(a.positionDampening, b.positionDampening, t);
		rotationConstant = MathUtil.Lerp(a.rotationConstant, b.rotationConstant, t);
		rotationDampening = MathUtil.Lerp(a.rotationDampening, b.rotationDampening, t);
		scaleConstant = MathUtil.Lerp(a.scaleConstant, b.scaleConstant, t);
		scaleDampening = MathUtil.Lerp(a.scaleDampening, b.scaleDampening, t);
		return this;
	}
}
