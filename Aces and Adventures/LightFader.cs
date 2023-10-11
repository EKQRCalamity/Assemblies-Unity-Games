using UnityEngine;

[RequireComponent(typeof(Light))]
[DisallowMultipleComponent]
[ScriptOrder(1)]
public class LightFader : MonoBehaviour
{
	public bool useIntensityAwareFadeTime = true;

	[Range(0f, 5f)]
	public float fadeTime = 0.25f;

	public bool useScaledTime = true;

	public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	public bool postProcessIntensity = true;

	[Range(0f, 0.1f)]
	public float immediateFadeIntensityThreshold = 0.01f;

	private float _originalIntensity;

	private float _elapsedFadeTime;

	private float _fadeTime;

	private Light _lightToFade;

	private Light lightToFade
	{
		get
		{
			if (!_lightToFade)
			{
				return _lightToFade = GetComponent<Light>();
			}
			return _lightToFade;
		}
	}

	private void OnEnable()
	{
		_elapsedFadeTime = 0f;
		_originalIntensity = lightToFade.intensity;
		_fadeTime = (useIntensityAwareFadeTime ? (_originalIntensity * fadeTime) : fadeTime);
		if (_originalIntensity <= immediateFadeIntensityThreshold)
		{
			lightToFade.enabled = false;
		}
	}

	private void LateUpdate()
	{
		if (lightToFade.enabled)
		{
			_elapsedFadeTime += GameUtil.GetDeltaTime(useScaledTime);
			float num = Mathf.Clamp01(_elapsedFadeTime / Mathf.Max(_fadeTime, MathUtil.BigEpsilon));
			lightToFade.intensity = (postProcessIntensity ? lightToFade.intensity : _originalIntensity) * fadeOutCurve.Evaluate(num);
			if (!(num < 1f))
			{
				lightToFade.enabled = false;
				base.enabled = false;
			}
		}
	}
}
