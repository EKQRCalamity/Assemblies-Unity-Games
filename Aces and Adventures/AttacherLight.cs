using System;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class AttacherLight : Attacher
{
	[Header("Light=======================================================================================================", order = 1)]
	public bool inheritScale;

	public AttacherCurveData intensity;

	public AttacherCurveData range;

	public AttacherGradientData color;

	private Light _light;

	protected Light light
	{
		get
		{
			if (_light == null)
			{
				_light = GetComponent<Light>();
				intensity.initialValue = _light.intensity;
				range.initialValue = _light.range;
				color.initialValue = _light.color;
			}
			return _light;
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (_samplesDirty)
		{
			_OnSamplesChange();
		}
	}

	protected override void _OnSamplesChange()
	{
		if (intensity.enabled)
		{
			light.intensity = intensity.GetSampleValue(this);
		}
		if (range.enabled)
		{
			light.range = range.GetSampleValue(this) * ((inheritScale && (bool)attach.to) ? Mathf.Sqrt(attach.to.GetWorldScale().Abs().Average()).Max(0.01f) : 1f);
		}
		if (color.enabled)
		{
			light.color = color.GetSampleValue(this);
		}
	}

	public AttacherLight ApplySettings(System.Random random, IAttacherLightSettings settings, bool applyToChildren = true)
	{
		if (applyToChildren)
		{
			foreach (AttacherLight item in base.gameObject.GetComponentsInChildrenPooled<AttacherLight>())
			{
				settings.Apply(random, item);
			}
			return this;
		}
		settings.Apply(random, this);
		return this;
	}
}
