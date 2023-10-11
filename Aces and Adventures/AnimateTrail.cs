using UnityEngine;

public class AnimateTrail : AnimateComponent
{
	[Header("Size")]
	public bool animateSize;

	public Vector2 lifetimeSizeRange = new Vector2(0f, 0.2f);

	public AnimationCurve lifetimeSize = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	public AnimationType lifetimeSizeAnimType = AnimationType.Pendulum;

	[Range(4f, 20f)]
	public int lifetimeSizeFidelity = 12;

	public float lifetimeSizeRepeat = 1f;

	[Header("Color")]
	public bool animateColor;

	public bool animateAlpha;

	public Gradient lifetimeColor = new Gradient();

	public AnimationType lifetimeColorAnimType = AnimationType.Pendulum;

	[Range(2f, 8f)]
	public int lifetimeColorFidelity = 4;

	public float lifetimeColorRepeat = 1f;

	private bool _useSimpleSize;

	private bool _useSimpleColor;

	public override void CacheInitialValues()
	{
	}

	protected override void UniqueUpdate(float t)
	{
	}

	public void AnimateSize(bool b)
	{
		animateSize = b;
	}

	public void LifetimeSizeRange(Vector2 range)
	{
		lifetimeSizeRange = range;
	}

	public void AnimateColor(bool b)
	{
		animateColor = b;
	}

	public void AnimateAlpha(bool b)
	{
		animateAlpha = b;
	}
}
