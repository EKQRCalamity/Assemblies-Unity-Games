using Cinemachine.PostFX;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class IndirectLightInterpolator : MonoBehaviour
{
	[Range(0.01f, 10f)]
	public float interpolationTime = 1f;

	public AnimationCurve interpolationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private GameObject _profileOwner;

	private VolumeProfile _profile;

	private LightInterpolator.Target<float> _multiplier;

	private LightInterpolator.Target<float> _reflectMultiplier;

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

	public float multiplier
	{
		get
		{
			if (!_profile.TryGet<IndirectLightingController>(out var component))
			{
				return 1f;
			}
			return component.indirectDiffuseLightingMultiplier.value;
		}
		set
		{
			if (_profile.TryGet<IndirectLightingController>(out var component))
			{
				component.indirectDiffuseLightingMultiplier.value = value;
			}
		}
	}

	public float reflectMultiplier
	{
		get
		{
			if (!_profile.TryGet<IndirectLightingController>(out var component))
			{
				return 1f;
			}
			return component.reflectionLightingMultiplier.value;
		}
		set
		{
			if (_profile.TryGet<IndirectLightingController>(out var component))
			{
				component.reflectionLightingMultiplier.value = value;
			}
		}
	}

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
		if ((bool)_profile && _profile.TryGet<IndirectLightingController>(out var component3))
		{
			MinFloatParameter indirectDiffuseLightingMultiplier = component3.indirectDiffuseLightingMultiplier;
			bool overrideState = (component3.reflectionLightingMultiplier.overrideState = true);
			indirectDiffuseLightingMultiplier.overrideState = overrideState;
		}
	}

	private void Update()
	{
		if ((bool)_profile)
		{
			if ((bool)_multiplier)
			{
				multiplier = (_multiplier.GetSample(interpolationCurve, interpolationTime, out var sample) ? _multiplier.Finish() : Mathf.Lerp(_multiplier.start, _multiplier, sample));
			}
			if ((bool)_reflectMultiplier)
			{
				reflectMultiplier = (_reflectMultiplier.GetSample(interpolationCurve, interpolationTime, out var sample2) ? _reflectMultiplier.Finish() : Mathf.Lerp(_reflectMultiplier.start, _reflectMultiplier, sample2));
			}
		}
	}

	public void SetTarget(GameObject getProfileFrom, float targetMultiplier)
	{
		profileOwner = getProfileFrom;
		if ((bool)_profile)
		{
			_multiplier = new LightInterpolator.Target<float>(multiplier, targetMultiplier);
			_reflectMultiplier = new LightInterpolator.Target<float>(reflectMultiplier, targetMultiplier);
		}
	}
}
