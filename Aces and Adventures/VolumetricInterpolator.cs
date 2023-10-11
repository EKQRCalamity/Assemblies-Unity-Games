using Cinemachine.PostFX;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Light))]
public class VolumetricInterpolator : MonoBehaviour
{
	public LocalVolumetricFog densityVolume;

	[Range(0.01f, 10f)]
	public float interpolationTime = 1f;

	public AnimationCurve interpolationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private LightInterpolator.Target<Color32> _color;

	private LightInterpolator.Target<float> _density;

	private LightInterpolator.Target<float> _blend;

	private BlendedNoiseWaveVector3 _velocityAnimation;

	private Vector3 _textureOffset;

	private float _currentBlend;

	private float _cookieBlend;

	private float _currentDensity;

	private GameObject _profileOwner;

	private VolumeProfile _profile;

	private Light _light;

	private HDAdditionalLightData _hdLight;

	private HDCamera _hdCamera;

	private HDAdditionalCameraData _hdAdditionalCameraData;

	private bool _volumetricEnabled;

	private LightCookieBlender _cookieBlender;

	public GameObject profileOwner
	{
		get
		{
			return _profileOwner;
		}
		private set
		{
			if (SetPropertyUtility.SetObject(ref _profileOwner, value) && (bool)value)
			{
				_OnProfileOwnerChange();
			}
		}
	}

	public Light light => this.CacheComponent(ref _light);

	public HDAdditionalLightData hdLight => this.CacheComponent(ref _hdLight);

	public float density
	{
		get
		{
			return hdLight.volumetricDimmer;
		}
		set
		{
			hdLight.volumetricDimmer = value;
		}
	}

	public HDCamera hdCamera => _hdCamera ?? (_hdCamera = HDCamera.GetOrCreate(Camera.main));

	public HDAdditionalCameraData cameraData => Camera.main.CacheComponent(ref _hdAdditionalCameraData);

	public Color albedo
	{
		get
		{
			if (!_profile.TryGet<Fog>(out var component))
			{
				return Color.white;
			}
			return component.albedo.value;
		}
		set
		{
			if (_profile.TryGet<Fog>(out var component))
			{
				component.albedo.value = value;
			}
		}
	}

	public float attenuationDistance
	{
		get
		{
			if (!_profile.TryGet<Fog>(out var component))
			{
				return 1f;
			}
			return component.meanFreePath.value;
		}
		set
		{
			if (_profile.TryGet<Fog>(out var component))
			{
				component.meanFreePath.value = value;
			}
		}
	}

	public float maxHeight
	{
		get
		{
			if (!_profile.TryGet<Fog>(out var component))
			{
				return 1f;
			}
			return component.maximumHeight.value;
		}
		set
		{
			if (_profile.TryGet<Fog>(out var component))
			{
				component.maximumHeight.value = value;
			}
		}
	}

	private bool volumetricEnabled
	{
		get
		{
			return _volumetricEnabled;
		}
		set
		{
			_SetVolumetricEnabled(value);
		}
	}

	public LightCookieBlender cookieBlender => this.CacheComponent(ref _cookieBlender);

	private void _OnProfileOwnerChange()
	{
		Volume component = _profileOwner.GetComponent<Volume>();
		if ((object)component != null)
		{
			component.profile = (_profile = GameUtil.InstantiateIfNotAnInstance(component.profile));
		}
		else
		{
			CinemachineVolumeSettings component2 = _profileOwner.GetComponent<CinemachineVolumeSettings>();
			if ((object)component2 != null)
			{
				component2.m_Profile = (_profile = GameUtil.InstantiateIfNotAnInstance(component2.m_Profile));
			}
		}
		if ((bool)_profile && _profile.TryGet<Fog>(out var component3))
		{
			component3.albedo.overrideState = true;
		}
	}

	private void _OnVolumetricLightingEnabledChange(bool enableVolumetric)
	{
		_SetAtmosphericScatteringEnabled(volumetricEnabled && enableVolumetric);
	}

	private void _SetVolumetricEnabled(bool enableVolumetric)
	{
		if (SetPropertyUtility.SetStruct(ref _volumetricEnabled, enableVolumetric))
		{
			hdLight.affectsVolumetric = volumetricEnabled;
			_SetAtmosphericScatteringEnabled(volumetricEnabled && ProfileManager.options.video.postProcessing.volumetricLighting);
			if ((bool)densityVolume)
			{
				densityVolume.enabled = volumetricEnabled;
			}
		}
	}

	private void _SetAtmosphericScatteringEnabled(bool enable)
	{
		FrameSettings renderingPathCustomFrameSettings = cameraData.renderingPathCustomFrameSettings;
		renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.AtmosphericScattering, enable);
		cameraData.renderingPathCustomFrameSettings = renderingPathCustomFrameSettings;
	}

	private void OnEnable()
	{
		ProfileOptions.VideoOptions.PostProcessingOptions.OnVolumetricLightingChange += _OnVolumetricLightingEnabledChange;
	}

	private void Update()
	{
		if ((bool)_density)
		{
			_currentDensity = (_density.GetSample(interpolationCurve, interpolationTime, out var sample) ? _density.Finish() : Mathf.Lerp(_density.start, _density, sample));
			attenuationDistance = 1f / Mathf.Max(_currentDensity, 0.001f);
			volumetricEnabled = _currentDensity > 0f;
		}
		if ((bool)_blend)
		{
			density = (_currentBlend = (_blend.GetSample(interpolationCurve, interpolationTime, out var sample2) ? _blend.Finish() : Mathf.Lerp(_blend.start, _blend, sample2)));
		}
		if ((bool)cookieBlender && SetPropertyUtility.SetStruct(ref _cookieBlend, cookieBlender.blend))
		{
			maxHeight = Mathf.Pow(_cookieBlend, 3f) * 2f;
		}
		if (!hdLight.affectsVolumetric)
		{
			return;
		}
		if ((bool)_color)
		{
			float sample3;
			Color32 color = (_color.GetSample(interpolationCurve, interpolationTime, out sample3) ? _color.Finish() : Color32.Lerp(_color.start, _color, sample3));
			if ((bool)densityVolume)
			{
				densityVolume.parameters.albedo = color;
			}
			else if ((bool)_profile)
			{
				albedo = color;
			}
		}
		if ((bool)densityVolume)
		{
			_textureOffset += _velocityAnimation.Update(Time.deltaTime, 31.235f) * Time.deltaTime;
			densityVolume.parameters.textureScrollingSpeed = _textureOffset / hdCamera.time.InsureNonZero();
		}
	}

	private void OnDisable()
	{
		ProfileOptions.VideoOptions.PostProcessingOptions.OnVolumetricLightingChange -= _OnVolumetricLightingEnabledChange;
	}

	public void SetTarget(GameObject getProfileFrom, float? targetDensity = null, Color? targetColor = null, NoiseWaveVector3Data? velocityAnimation = null)
	{
		profileOwner = getProfileFrom;
		if (targetDensity.HasValue)
		{
			_density = new LightInterpolator.Target<float>(_currentDensity, targetDensity);
			_blend = new LightInterpolator.Target<float>(_currentBlend, (targetDensity > 0f) ? 1 : 0);
		}
		if (targetColor.HasValue)
		{
			_color = new LightInterpolator.Target<Color32>(light.color, targetColor);
		}
		if (velocityAnimation.HasValue)
		{
			_velocityAnimation = _velocityAnimation.BlendToward(velocityAnimation.Value, interpolationTime);
		}
	}
}
