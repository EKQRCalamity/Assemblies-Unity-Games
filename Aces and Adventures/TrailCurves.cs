using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailCurves : ACurves
{
	[Header("Trail==================================================================================")]
	public FloatCurve width;

	public FloatCurve lifetime;

	public ColorCurve startColor;

	public ColorCurve endColor;

	private TrailRenderer _trail;

	protected TrailRenderer trail
	{
		get
		{
			if (!(_trail != null))
			{
				return _InitializeValues();
			}
			return _trail;
		}
	}

	private TrailRenderer _InitializeValues()
	{
		_trail = GetComponent<TrailRenderer>();
		width.initialValue = _trail.widthMultiplier;
		lifetime.initialValue = _trail.time;
		startColor.initialValue = _trail.startColor;
		endColor.initialValue = _trail.endColor;
		return _trail;
	}

	private void Awake()
	{
		_ = trail;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		trail.Clear();
	}

	protected override void _Input(float t)
	{
		if (width.enabled)
		{
			trail.widthMultiplier = width.GetValue(t);
		}
		if (lifetime.enabled)
		{
			trail.time = lifetime.GetValue(t);
		}
		if (startColor.enabled)
		{
			trail.startColor = startColor.GetValue(trail.startColor, t);
		}
		if (endColor.enabled)
		{
			trail.endColor = endColor.GetValue(trail.endColor, t);
		}
	}
}
