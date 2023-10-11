using UnityEngine;

public class AnimateMaterialVector4 : AnimateMaterialVector3
{
	[Header("W")]
	public Vector2 wRange = new Vector2(0f, 0f);

	public AnimationCurve wSample = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType wAnimType;

	public float wRepeat = 1f;

	protected override Vector4 GetValue(float t)
	{
		Vector4 vector = new Vector4(GetValue(xRange, xSample, xAnimType, t, xRepeat), GetValue(yRange, ySample, yAnimType, t, yRepeat), GetValue(zRange, zSample, zAnimType, t, zRepeat), GetValue(wRange, wSample, wAnimType, t, wRepeat));
		if (asMultiplier)
		{
			return vector.MultiplyV4(initialValue);
		}
		return vector;
	}

	public void WRange(Vector2 wRange)
	{
		this.wRange = wRange;
	}
}
