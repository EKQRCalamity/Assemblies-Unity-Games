using UnityEngine;

public class AnimateVector2 : AnimateComponent
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

	public Vector2Event onValueChanged;

	private Vector2 previousValue;

	public override void CacheInitialValues()
	{
		base.CacheInitialValues();
		previousValue = new Vector2(float.MinValue, float.MinValue);
	}

	protected override void UniqueUpdate(float t)
	{
		Vector2 vector = new Vector2(GetValue(xRange, xSample, xAnimType, t, xRepeat), GetValue(yRange, ySample, yAnimType, t, yRepeat));
		if (vector != previousValue)
		{
			if (onValueChanged != null)
			{
				onValueChanged.Invoke(vector);
			}
			previousValue = vector;
		}
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
