using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightCurves : ACurves
{
	[Header("Light=======================================================================================================", order = 1)]
	public FloatCurve intensity;

	public FloatCurve range;

	public ColorCurve color;

	private Light _light;

	protected Light light
	{
		get
		{
			if (!(_light != null))
			{
				return _InitializeValues();
			}
			return _light;
		}
	}

	private Light _InitializeValues()
	{
		_light = GetComponent<Light>();
		intensity.initialValue = _light.intensity;
		range.initialValue = _light.range;
		color.initialValue = _light.color;
		return _light;
	}

	private void Awake()
	{
		_ = light;
	}

	protected override void _Input(float t)
	{
		if (intensity.enabled)
		{
			light.intensity = intensity.GetValue(t);
		}
		if (range.enabled)
		{
			light.range = range.GetValue(t);
		}
		if (color.enabled)
		{
			light.color = color.GetValue(light.color, t);
		}
	}
}
