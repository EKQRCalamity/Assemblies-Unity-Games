using UnityEngine;

[RequireComponent(typeof(Light))]
[DisallowMultipleComponent]
public class AnimateLight : AnimateComponent
{
	[Header("Range")]
	public bool rangeAsMultplier = true;

	public Vector2 range = new Vector2(1f, 1f);

	public AnimationCurve rangeSample = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType rangeAnimType;

	public float rangeRepeat = 1f;

	[Header("Color")]
	public bool colorAsMultiplier = true;

	public Color tint = Color.white;

	public Gradient gradient = new Gradient();

	public AnimationCurve gradientSample = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType colorAnimType;

	private Light light;

	private float initialRange;

	private Color initialColor;

	private bool? _animateColor;

	private bool animateColor
	{
		get
		{
			bool? flag = _animateColor;
			if (!flag.HasValue)
			{
				bool? flag2 = (_animateColor = !colorAsMultiplier || !(tint == Color.white) || !gradient.EqualTo(Color.white));
				return flag2.Value;
			}
			return flag.GetValueOrDefault();
		}
	}

	public override void CacheInitialValues()
	{
		light = GetComponent<Light>();
		initialRange = light.range;
		initialColor = light.color;
	}

	protected override void UniqueUpdate(float t)
	{
		light.range = GetValue(range, rangeSample, rangeAnimType, t, rangeRepeat) * (rangeAsMultplier ? initialRange : 1f);
		if (animateColor)
		{
			light.color = GetColor(gradient, gradientSample, colorAnimType, t, 1f, tint) * (colorAsMultiplier ? initialColor : Color.white);
		}
	}

	public void Range(Vector2 range)
	{
		this.range = range;
	}

	public void Tint(Color tint)
	{
		this.tint = tint;
	}
}
