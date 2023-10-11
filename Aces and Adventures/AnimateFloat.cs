using UnityEngine;

public class AnimateFloat : AnimateComponent
{
	[Header("Float")]
	public Vector2 range = new Vector2(0f, 0f);

	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType animType;

	public float repeat = 1f;

	public FloatEvent onValueChanged;

	private float previousValue;

	public override void CacheInitialValues()
	{
		base.CacheInitialValues();
		previousValue = float.MinValue;
	}

	protected override void UniqueUpdate(float t)
	{
		float value = GetValue(range, curve, animType, t, repeat);
		if (value != previousValue)
		{
			if (onValueChanged != null)
			{
				onValueChanged.Invoke(value);
			}
			previousValue = value;
		}
	}

	public void Range(Vector2 range)
	{
		this.range = range;
	}
}
