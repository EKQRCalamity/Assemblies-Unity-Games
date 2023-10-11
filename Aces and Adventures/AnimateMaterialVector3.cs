using UnityEngine;

public class AnimateMaterialVector3 : AnimateMaterialVector
{
	[Header("Z")]
	public Vector2 zRange = new Vector2(0f, 0f);

	public AnimationCurve zSample = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType zAnimType;

	public float zRepeat = 1f;

	protected override Vector4 GetValue(float t)
	{
		Vector4 vector = new Vector3(GetValue(xRange, xSample, xAnimType, t, xRepeat), GetValue(yRange, ySample, yAnimType, t, yRepeat), GetValue(zRange, zSample, zAnimType, t, zRepeat));
		if (asMultiplier)
		{
			return vector.MultiplyV4(initialValue);
		}
		return vector;
	}

	public void ZRange(Vector2 zRange)
	{
		this.zRange = zRange;
	}
}
