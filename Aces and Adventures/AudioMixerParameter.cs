using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerParameter
{
	private readonly AudioMixer _mixer;

	private readonly string _name;

	private readonly float _min;

	private readonly float _max;

	private Dictionary<UnityEngine.Object, float> _multipliers;

	public float value
	{
		get
		{
			_mixer.GetFloat(_name, out var result);
			return result;
		}
		set
		{
			_mixer.SetFloat(_name, Mathf.Clamp(value, _min, _max));
		}
	}

	public float valueNormalized
	{
		get
		{
			return MathUtil.GetLerpAmount(_min, _max, value);
		}
		set
		{
			this.value = Mathf.Lerp(_min, _max, value);
		}
	}

	public AudioMixer mixer => _mixer;

	public string name => _name;

	private Dictionary<UnityEngine.Object, float> multipliers => _multipliers ?? (_multipliers = new Dictionary<UnityEngine.Object, float>());

	public float valueMultiplier
	{
		get
		{
			if (!_multipliers.IsNullOrEmpty())
			{
				return _GetValueMultiplier();
			}
			return 1f;
		}
	}

	public AudioMixerParameter(AudioMixer mixer, string name, float min = -80f, float max = 0f)
	{
		_mixer = mixer;
		_name = name;
		_min = min;
		_max = max;
	}

	private float _GetValueMultiplier()
	{
		float num = 1f;
		foreach (KeyValuePair<UnityEngine.Object, float> item in _multipliers.EnumeratePairsSafe())
		{
			if ((bool)item.Key)
			{
				num *= item.Value;
			}
			else
			{
				_multipliers.Remove(item.Key);
			}
		}
		return num;
	}

	private IEnumerator _Fade(UnityEngine.Object fader, Func<bool> keepFadedWhile, float fadeOutTime, float fadeInTime)
	{
		fadeOutTime = Mathf.Max(0.01f, fadeOutTime);
		float time = fadeOutTime;
		while (keepFadedWhile())
		{
			AudioMixerParameter audioMixerParameter = this;
			float num;
			time = (num = time - GameUtil.GetDeltaTime(useScaledTime: false));
			audioMixerParameter.SetMultiplier(fader, MathUtil.CubicSplineInterpolant(Mathf.Clamp01(num / fadeOutTime)));
			yield return null;
		}
		fadeInTime = Mathf.Max(0.01f, fadeInTime);
		time = Mathf.Clamp01(time / fadeOutTime) * fadeInTime;
		while (time <= fadeInTime)
		{
			AudioMixerParameter audioMixerParameter2 = this;
			float num;
			time = (num = time + GameUtil.GetDeltaTime(useScaledTime: false));
			audioMixerParameter2.SetMultiplier(fader, MathUtil.CubicSplineInterpolant(Mathf.Clamp01(num / fadeInTime)));
			yield return null;
		}
		RemoveMultiplier(fader);
	}

	public void SetMultiplier(UnityEngine.Object obj, float multiplier)
	{
		multipliers[obj] = multiplier;
	}

	public void ToggleMultiplier(UnityEngine.Object obj, float multiplier)
	{
		if (!multipliers.Remove(obj))
		{
			multipliers[obj] = multiplier;
		}
	}

	public bool RemoveMultiplier(UnityEngine.Object obj)
	{
		return multipliers.Remove(obj);
	}

	public Job Fade(UnityEngine.Object fader, Func<bool> keepFadedWhile, float fadeOutTime = 0.2f, float fadeInTime = 0.2f)
	{
		return Job.Process(_Fade(fader, keepFadedWhile, fadeOutTime, fadeInTime));
	}

	public static implicit operator bool(AudioMixerParameter p)
	{
		float num;
		return p._mixer.GetFloat(p._name, out num);
	}

	public static implicit operator float(AudioMixerParameter p)
	{
		return p.value;
	}
}
