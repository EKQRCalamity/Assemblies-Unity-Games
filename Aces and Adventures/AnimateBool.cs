using UnityEngine;

public class AnimateBool : AnimateComponent
{
	[Header("Boolean")]
	public float threshold = 0.5f;

	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType animType;

	public float repeat = 1f;

	public BoolEvent onValueChanged;

	private bool? previousValue;

	public override void CacheInitialValues()
	{
		base.CacheInitialValues();
		previousValue = null;
	}

	protected override void UniqueUpdate(float t)
	{
		bool flag = GetValue(Vector2.up, curve, animType, t, repeat) >= threshold;
		if (!previousValue.HasValue || flag != previousValue)
		{
			if (onValueChanged != null)
			{
				onValueChanged.Invoke(flag);
			}
			previousValue = flag;
		}
	}

	public void Threshold(float threshold)
	{
		this.threshold = threshold;
	}
}
