using UnityEngine;

public class AnimateColor : AnimateComponent
{
	[Header("Color")]
	public Color tint = Color.white;

	public Gradient gradient = new Gradient();

	public AnimationCurve gradientSample = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType animType;

	public ColorEvent onValueChanged;

	private Color previousValue;

	public override void CacheInitialValues()
	{
		base.CacheInitialValues();
		previousValue = new Color(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
	}

	protected override void UniqueUpdate(float t)
	{
		Color color = GetColor(gradient, gradientSample, animType, t, 1f, tint);
		if (color != previousValue)
		{
			if (onValueChanged != null)
			{
				onValueChanged.Invoke(color);
			}
			previousValue = color;
		}
	}

	public void Tint(Color tint)
	{
		this.tint = tint;
	}
}
