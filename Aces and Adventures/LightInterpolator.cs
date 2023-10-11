using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Light))]
public class LightInterpolator : MonoBehaviour
{
	public struct Target<T> where T : struct
	{
		public T start;

		public T? desired;

		public float time;

		public Target(T start, T? desired)
		{
			this.start = start;
			this.desired = desired;
			time = Time.time;
		}

		public float GetCompletionRatio(float interpolationTime)
		{
			if (!desired.HasValue)
			{
				return 1f;
			}
			return Mathf.Clamp01((Time.time - time) / interpolationTime);
		}

		public bool GetSample(AnimationCurve curve, float interpolationTime, out float sample)
		{
			float num = Mathf.Clamp01((Time.time - time) / interpolationTime);
			sample = curve.Evaluate(num);
			return num >= 1f;
		}

		public T Finish()
		{
			T result = this;
			desired = null;
			return result;
		}

		public static implicit operator bool(Target<T> target)
		{
			return target.desired.HasValue;
		}

		public static implicit operator T(Target<T> target)
		{
			return target.desired ?? target.start;
		}
	}

	[Range(0.01f, 10f)]
	public float interpolationTime = 1f;

	public AnimationCurve interpolationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public Transform target;

	private Target<Color32> _color;

	private Target<float> _intensity;

	private Target<Quaternion> _rotation;

	private Target<float> _distance;

	protected Light _light;

	protected HDAdditionalLightData _hdLight;

	protected float _currentDistance;

	protected Vector3 _positionBeforeAnimation;

	protected Quaternion _rotationBeforeAnimation;

	protected float _intensityBeforeAnimation;

	public Vector3 center
	{
		get
		{
			if (!target)
			{
				return Vector3.zero;
			}
			return target.position;
		}
	}

	public float distance => (center - base.transform.position).magnitude;

	private void Awake()
	{
		_light = GetComponent<Light>();
		_hdLight = GetComponent<HDAdditionalLightData>();
		_rotationBeforeAnimation = base.transform.rotation;
		_intensityBeforeAnimation = _hdLight.intensity;
	}

	protected virtual void OnEnable()
	{
		_currentDistance = distance;
	}

	protected virtual void Update()
	{
		if ((bool)_color)
		{
			_light.color = (_color.GetSample(interpolationCurve, interpolationTime, out var sample) ? _color.Finish() : Color32.Lerp(_color.start, _color, sample));
		}
		if ((bool)_intensity)
		{
			float sample2;
			float intensityBeforeAnimation = (_hdLight.intensity = (_intensity.GetSample(interpolationCurve, interpolationTime, out sample2) ? _intensity.Finish() : Mathf.Lerp(_intensity.start, _intensity, sample2)));
			_intensityBeforeAnimation = intensityBeforeAnimation;
			_light.enabled = _hdLight.intensity > 0f;
		}
		if ((bool)_distance)
		{
			_currentDistance = (_distance.GetSample(interpolationCurve, interpolationTime, out var sample3) ? _distance.Finish() : Mathf.Lerp(_distance.start, _distance, sample3));
		}
		if ((bool)_rotation)
		{
			float sample4;
			Quaternion rotationBeforeAnimation = (base.transform.rotation = (_rotation.GetSample(interpolationCurve, interpolationTime, out sample4) ? _rotation.Finish() : Quaternion.Slerp(_rotation.start, _rotation, sample4)));
			_rotationBeforeAnimation = rotationBeforeAnimation;
			Vector3 positionBeforeAnimation = (base.transform.position = center - base.transform.forward * _currentDistance);
			_positionBeforeAnimation = positionBeforeAnimation;
		}
	}

	public void SetTarget(Color32? targetColor = null, int? targetIntensity = null, Quaternion? targetRotation = null, float targetDistance = 1f, Transform lightTarget = null)
	{
		if (targetColor.HasValue)
		{
			_color = new Target<Color32>(_light.color, targetColor);
		}
		if (targetIntensity.HasValue)
		{
			_intensity = new Target<float>(_intensityBeforeAnimation, targetIntensity);
		}
		if (targetRotation.HasValue)
		{
			_rotation = new Target<Quaternion>(_rotationBeforeAnimation, targetRotation);
		}
		_distance = new Target<float>(distance, targetDistance);
		target = lightTarget;
	}

	public void SetTarget(int targetIntensity)
	{
		_intensity = new Target<float>(_intensityBeforeAnimation, targetIntensity);
	}
}
