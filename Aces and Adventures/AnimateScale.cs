using UnityEngine;

[DisallowMultipleComponent]
public class AnimateScale : AnimateTransform
{
	[Header("Uniform Scale")]
	public Vector2 uScaleRange = new Vector2(1f, 1f);

	public AnimationCurve uCruve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType uAnimType;

	public float uRepeat = 1f;

	protected override bool _additive => false;

	public AnimateScale()
	{
		xRange = Vector2.one;
		yRange = Vector2.one;
		zRange = Vector2.one;
	}

	public override void CacheInitialValues()
	{
		initialVector = base.transform.localScale;
	}

	protected override void UniqueUpdate(float t)
	{
		base.transform.localScale = GetVector(base.transform.localScale, t, keepCurrentTransform: false) * GetValue(uScaleRange, uCruve, uAnimType, t, uRepeat);
	}

	public void UScaleRange(Vector2 uScaleRange)
	{
		this.uScaleRange = uScaleRange;
	}

	public void UniformScaleMax(float uniformScaleMax)
	{
		uScaleRange.y = uniformScaleMax;
	}
}
