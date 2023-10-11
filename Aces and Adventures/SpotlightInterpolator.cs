using UnityEngine;

public class SpotlightInterpolator : LightInterpolator
{
	private Target<float> _coneAngle;

	private Target<float> _innerConeAngle;

	private Target<float> _range;

	private Target<float> _shadows;

	private BlendedNoiseWaveVector3 _positionAnimation;

	private BlendedNoiseWaveVector3 _rotationAnimation;

	private BlendedNoiseWaveFloat _intensityAnimation = new BlendedNoiseWaveFloat(new NoiseWaveFloatData(0f, new RangeF(1f), 0f, new RangeF(1f)));

	private float _currentShadows;

	private bool _shadowsEnabled;

	protected override void OnEnable()
	{
		base.OnEnable();
		_currentShadows = ((_shadowsEnabled = _light.shadows != LightShadows.None) ? 1 : 0);
	}

	protected override void Update()
	{
		base.Update();
		if (!_light.enabled)
		{
			return;
		}
		if ((bool)_coneAngle)
		{
			_light.spotAngle = (_coneAngle.GetSample(interpolationCurve, interpolationTime, out var sample) ? _coneAngle.Finish() : Mathf.Lerp(_coneAngle.start, _coneAngle, sample));
		}
		if ((bool)_innerConeAngle)
		{
			_hdLight.innerSpotPercent = (_innerConeAngle.GetSample(interpolationCurve, interpolationTime, out var sample2) ? _innerConeAngle.Finish() : Mathf.Lerp(_innerConeAngle.start, _innerConeAngle, sample2));
		}
		if ((bool)_range)
		{
			_light.range = (_range.GetSample(interpolationCurve, interpolationTime, out var sample3) ? _range.Finish() : Mathf.Lerp(_range.start, _range, sample3));
		}
		base.transform.position = _positionBeforeAnimation + _positionAnimation.Update(Time.deltaTime, 121.235f);
		base.transform.rotation = Quaternion.Euler(_rotationAnimation.Update(Time.deltaTime, -313.624f)) * _rotationBeforeAnimation;
		_hdLight.intensity = _intensityBeforeAnimation * _intensityAnimation.UpdateMultiplier(Time.deltaTime, 5231.732f);
		_hdLight.shadowNearPlane = Mathf.Max(0.01f, _currentDistance - 0.9f);
		if ((bool)_shadows)
		{
			if (_shadows.GetCompletionRatio(interpolationTime) >= 0.5f)
			{
				_hdLight.EnableShadows(_shadows.desired > 0f);
			}
			_currentShadows = (_shadows.GetSample(interpolationCurve, interpolationTime, out var sample4) ? _shadows.Finish() : Mathf.Lerp(_shadows.start, _shadows, sample4));
			_hdLight.intensity = _intensityBeforeAnimation * Mathf.Abs(0.5f - _currentShadows) * 2f;
		}
	}

	public void SetTarget(Color32? targetColor = null, int? targetIntensity = null, Quaternion? targetRotation = null, float targetDistance = 1f, float targetConeAngle = 30f, float targetInnerConeAngle = 0f, float targetRange = 5f, NoiseWaveVector3Data? positionAnimation = null, NoiseWaveVector3Data? rotationAnimation = null, NoiseWaveFloatData? intensityAnimation = null, Transform lightTarget = null, bool shadows = true)
	{
		SetTarget(targetColor, targetIntensity, targetRotation, targetDistance, lightTarget);
		_coneAngle = new Target<float>(_light.spotAngle, targetConeAngle);
		_innerConeAngle = new Target<float>(_hdLight.innerSpotPercent, targetInnerConeAngle);
		_range = new Target<float>(_light.range, targetRange);
		if (positionAnimation.HasValue)
		{
			_positionAnimation = _positionAnimation.BlendToward(positionAnimation.Value, interpolationTime);
		}
		if (rotationAnimation.HasValue)
		{
			_rotationAnimation = _rotationAnimation.BlendToward(rotationAnimation.Value, interpolationTime);
		}
		if (intensityAnimation.HasValue)
		{
			_intensityAnimation = _intensityAnimation.BlendToward(intensityAnimation.Value, interpolationTime);
		}
		if (SetPropertyUtility.SetStruct(ref _shadowsEnabled, shadows))
		{
			_shadows = new Target<float>(_currentShadows, shadows ? 1 : 0);
			if (_hdLight.intensity < 1f)
			{
				_hdLight.EnableShadows(shadows);
				_shadows.Finish();
			}
		}
	}
}
