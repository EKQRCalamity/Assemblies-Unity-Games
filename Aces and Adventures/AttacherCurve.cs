using UnityEngine;

public class AttacherCurve : Attacher
{
	[Header("Curve=======================================================================================================")]
	public AttacherCurveData curve;

	public FloatEvent OnCurveSample;

	protected override void OnEnable()
	{
		base.OnEnable();
		curve.initialValue = 1f;
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
		if (curve.enabled)
		{
			OnCurveSample.Invoke(curve.GetSampleValue(this));
		}
	}
}
