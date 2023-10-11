using System;
using UnityEngine;
using UnityEngine.Events;

[ScriptOrder(1)]
public abstract class Attacher : MonoBehaviour
{
	[Serializable]
	public class AttacherAttachData
	{
		public Transform to;

		public Vector3 axisOffsetDistances;

		public AttachmentType type;

		[Range(0f, 1f)]
		[SerializeField]
		protected float _acceleration = 0.5f;

		[Range(0f, 1f)]
		[SerializeField]
		protected float _dampening = 0.5f;

		public Vector2 randomInitialSpeedRange;

		public bool matchRotation = true;

		public bool useScaledTime = true;

		public bool clearOnDisable { get; set; }

		public bool deactivateOnDetach { get; set; }

		public float acceleration => type.AccelerationRange().Lerp(_acceleration);

		public float dampening => type.DampeningRange().Lerp(_dampening);
	}

	[Serializable]
	public class AttacherLifetimeData
	{
		[Range(0f, 60f)]
		public float time;

		public bool detachOnExpire;

		[Range(0.01f, 10f)]
		public float detachTime = 0.333f;
	}

	[Serializable]
	public class AttacherSpeedRangeData
	{
		public bool samplingEnabled = true;

		[Range(0f, 3f)]
		public float min;

		[Range(0f, 3f)]
		public float max = 1.5f;

		[Range(0f, 50f)]
		public float easing = 25f;

		public void SetData(Vector2 range, float ease)
		{
			min = range.x;
			max = range.y;
			easing = ease;
		}
	}

	[Serializable]
	public class AttacherRelativeSpeedData
	{
		public Transform relativeTo;

		[Range(0f, 1f)]
		public float directionalityRange = 1f;

		[Range(0f, 1f)]
		[Tooltip("Higher values will result in speed curve samples being lower when [Attach To] is moving away from [Speed Relative To]")]
		public float directionalityMix = 0.5f;

		[Range(0.333f, 3f)]
		public float directionalityPower = 2f;
	}

	[Serializable]
	public class AttacherEventData
	{
		public FloatEvent OnLifetimeChange;

		public FloatEvent OnLifetimeSampleChange;

		public FloatEvent OnSpeedSampleChange;

		public FloatEvent OnDetachSampleChange;

		public UnityEvent OnDetachComplete;
	}

	[Serializable]
	public class AttacherCurveData
	{
		public bool enabled = true;

		[HideInInspectorIf("_hideCommon", false)]
		public bool speedEnabled = true;

		[HideInInspectorIf("_hideSpeed", false)]
		public Vector2 speedRange = new Vector2(0f, 1f);

		[HideInInspectorIf("_hideSpeed", false)]
		public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[HideInInspectorIf("_hideCommon", false)]
		public bool lifetimeEnabled = true;

		[HideInInspectorIf("_hideLifetime", false)]
		public Vector2 lifetimeRange = new Vector2(0f, 1f);

		[HideInInspectorIf("_hideLifetime", false)]
		public AnimationCurve lifetimeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[HideInInspectorIf("_hideCommon", false)]
		public bool detachEnabled = true;

		[HideInInspectorIf("_hideDetach", false)]
		public Vector2 detachRange = new Vector2(0f, 1f);

		[HideInInspectorIf("_hideDetach", false)]
		public AnimationCurve detachCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

		public float initialValue { get; set; }

		public float? multiplier { get; set; }

		public float GetSampleValue(Attacher attacher)
		{
			float num = initialValue;
			if (lifetimeEnabled && attacher._shouldSampleLifetime)
			{
				num *= Mathf.Lerp(lifetimeRange.x, lifetimeRange.y, lifetimeCurve.Evaluate(attacher._lifetimeSample));
			}
			if (speedEnabled && attacher._shouldSampleSpeed)
			{
				num *= Mathf.Lerp(speedRange.x, speedRange.y, speedCurve.Evaluate(attacher._speedSample));
			}
			if (detachEnabled && attacher._shouldSampleDetach)
			{
				num *= Mathf.Lerp(detachRange.x, detachRange.y, detachCurve.Evaluate(attacher._detachSample));
			}
			if (multiplier.HasValue)
			{
				num *= multiplier.Value;
			}
			return num;
		}
	}

	[Serializable]
	public class AttacherMinMaxCurveData
	{
		public bool enabled = true;

		public bool speedEnabled = true;

		public Vector2 speedRange = new Vector2(0f, 1f);

		public AnimationCurve speedCurveMin = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

		public AnimationCurve speedCurveMax = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public bool lifetimeEnabled = true;

		public Vector2 lifetimeRange = new Vector2(0f, 1f);

		public AnimationCurve lifetimeCurveMin = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

		public AnimationCurve lifetimeCurveMax = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public bool detachEnabled = true;

		public Vector2 detachRange = new Vector2(0f, 1f);

		public AnimationCurve detachCurveMin = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		public AnimationCurve detachCurveMax = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public Vector2 initialValue { get; set; }

		public Vector2 GetSampleValue(Attacher attacher)
		{
			Vector2 vector = initialValue;
			if (lifetimeEnabled && attacher._shouldSampleLifetime)
			{
				vector = vector.Multiply(new Vector2(lifetimeRange.Lerp(lifetimeCurveMin.Evaluate(attacher._lifetimeSample)), lifetimeRange.Lerp(lifetimeCurveMax.Evaluate(attacher._lifetimeSample))));
			}
			if (speedEnabled && attacher._shouldSampleSpeed)
			{
				vector = vector.Multiply(new Vector2(speedRange.Lerp(speedCurveMin.Evaluate(attacher._speedSample)), speedRange.Lerp(speedCurveMax.Evaluate(attacher._speedSample))));
			}
			if (detachEnabled && attacher._shouldSampleDetach)
			{
				vector = vector.Multiply(new Vector2(detachRange.Lerp(detachCurveMin.Evaluate(attacher._detachSample)), detachRange.Lerp(detachCurveMax.Evaluate(attacher._detachSample))));
			}
			return vector;
		}
	}

	[Serializable]
	public class AttacherGradientData
	{
		public bool enabled = true;

		[HideInInspectorIf("_hideCommon", false)]
		public bool speedEnabled = true;

		[HideInInspectorIf("_hideSpeed", false)]
		[GradientUsage(true)]
		public Gradient speedGradient;

		[HideInInspectorIf("_hideCommon", false)]
		public bool lifetimeEnabled = true;

		[HideInInspectorIf("_hideLifetime", false)]
		[GradientUsage(true)]
		public Gradient lifetimeGradient;

		[HideInInspectorIf("_hideCommon", false)]
		public bool detachEnabled = true;

		[HideInInspectorIf("_hideDetach", false)]
		[GradientUsage(true)]
		public Gradient detachGradient;

		public Color initialValue { get; set; }

		public Color? tintColor { get; set; }

		public Color GetSampleValue(Attacher attacher)
		{
			Color color = initialValue;
			if (lifetimeEnabled && attacher._shouldSampleLifetime)
			{
				color = color.Multiply(lifetimeGradient.Evaluate(attacher._lifetimeSample));
			}
			if (speedEnabled && attacher._shouldSampleSpeed)
			{
				color = color.Multiply(speedGradient.Evaluate(attacher._speedSample));
			}
			if (detachEnabled && attacher._shouldSampleDetach)
			{
				color = color.Multiply(detachGradient.Evaluate(attacher._detachSample));
			}
			if (tintColor.HasValue)
			{
				color = color.ToHSV(null, 0f, null).ToRGB().Multiply(tintColor.Value);
			}
			return color;
		}
	}

	[Serializable]
	public class CustomPropertyCurve
	{
		public string propertyName;

		[Range(0f, 10f)]
		public float defaultValue = 1f;

		public AttacherCurveData curves;
	}

	protected const float MAX_SPEED = 3f;

	public AttacherAttachData attach;

	public AttacherLifetimeData life;

	public AttacherSpeedRangeData speedRange;

	public AttacherRelativeSpeedData relativeSpeed;

	public AttacherEventData events;

	private float? _previousSpeedSample;

	private Vector3? _previousPosition;

	private Vector3 _velocity;

	private float _elapsedTime;

	private float? _detachElapsedTime;

	protected float _lifetimeSample;

	protected float _speedSample;

	protected float _detachSample;

	protected bool _samplesDirty;

	private Attacher[] _childAttachers;

	protected System.Random _random = new System.Random();

	private float? detachElapsedTime
	{
		get
		{
			return _detachElapsedTime;
		}
		set
		{
			if (!_detachElapsedTime.HasValue && value.HasValue)
			{
				_OnBeginDetach();
			}
			_detachElapsedTime = value;
		}
	}

	protected bool _shouldSampleLifetime => true;

	protected bool _shouldSampleSpeed => speedRange.samplingEnabled;

	protected bool _shouldSampleDetach => detachElapsedTime.HasValue;

	protected bool _isDetaching => detachElapsedTime.HasValue;

	protected Attacher[] childAttachers => _childAttachers ?? (_childAttachers = base.gameObject.GetComponentsInChildren<Attacher>(includeInactive: true));

	protected virtual float _offsetMultiplier => 1f;

	protected virtual void OnDisable()
	{
		_previousSpeedSample = null;
		_previousPosition = null;
		if (attach.clearOnDisable)
		{
			attach.to = null;
		}
	}

	protected virtual void OnEnable()
	{
		_velocity = _random.Direction() * _random.Range(attach.randomInitialSpeedRange);
		_elapsedTime = 0f;
		detachElapsedTime = null;
		_lifetimeSample = 0f;
		_speedSample = 0f;
		_detachSample = 0f;
		_OnSamplesChange();
	}

	protected virtual void LateUpdate()
	{
		_samplesDirty = false;
		float deltaTime = GameUtil.GetDeltaTime(attach.useScaledTime, smoothed: false);
		if (detachElapsedTime.HasValue)
		{
			if (detachElapsedTime < life.detachTime)
			{
				detachElapsedTime += deltaTime;
				_detachSample = Mathf.Clamp01(detachElapsedTime.Value / life.detachTime);
				_OnDetachSampleChange(_detachSample);
			}
			else if (_ShouldSignalDetachComplete())
			{
				_OnDetachComplete();
				return;
			}
		}
		if (life.time > 0f && _elapsedTime < life.time)
		{
			_elapsedTime += deltaTime;
			_lifetimeSample = Mathf.Clamp01(_elapsedTime / life.time);
			_OnLifetimeSampleChange(_lifetimeSample);
			if (life.detachOnExpire && _elapsedTime >= life.time)
			{
				detachElapsedTime = Mathf.Min(_elapsedTime - life.time, life.detachTime - 0.0001f);
			}
		}
		if ((bool)attach.to && !attach.to.gameObject.activeInHierarchy)
		{
			detachElapsedTime = detachElapsedTime.GetValueOrDefault();
			attach.to = null;
			return;
		}
		Transform transform = (attach.to ? attach.to : base.transform);
		Vector3 vector = ((!(transform == base.transform)) ? (transform.position + (transform.transform.right * attach.axisOffsetDistances.x + transform.transform.up * attach.axisOffsetDistances.y + transform.transform.forward * attach.axisOffsetDistances.z) * _offsetMultiplier) : (base.transform.parent ? base.transform.parent.position : base.transform.position));
		_previousPosition = _previousPosition ?? vector;
		Vector3 attachPosition = attach.type.GetAttachPosition(_previousPosition.Value, vector, ref _velocity, attach.acceleration, attach.dampening, deltaTime);
		Vector3 vector2 = (attachPosition - _previousPosition.Value) / deltaTime.InsureNonZero();
		base.transform.position = attachPosition;
		_previousPosition = base.transform.position;
		if (attach.matchRotation)
		{
			base.transform.rotation = transform.rotation;
		}
		if (speedRange.samplingEnabled)
		{
			float num = vector2.magnitude;
			if ((bool)relativeSpeed.relativeTo)
			{
				num *= Mathf.Lerp(1f, Mathf.Pow(Mathf.Clamp01(MathUtil.Remap(Vector3.Dot(vector2.normalized, (relativeSpeed.relativeTo.transform.position - base.transform.position).normalized), new Vector2(0f - relativeSpeed.directionalityRange, 1f), new Vector2(0f, 1f))), relativeSpeed.directionalityPower), relativeSpeed.directionalityMix);
			}
			_speedSample = Mathf.Clamp01(MathUtil.GetLerpAmount(speedRange.min, speedRange.max, num));
			if (_previousSpeedSample.HasValue && speedRange.easing > 0f)
			{
				_speedSample = MathUtil.DeltaSnap(MathUtil.Ease(_previousSpeedSample.Value, _speedSample, speedRange.easing, deltaTime), _speedSample, 0.001f);
			}
			if (_previousSpeedSample != _speedSample)
			{
				_previousSpeedSample = _speedSample;
				_OnSpeedSampleChange(_speedSample);
			}
		}
	}

	protected virtual void _OnSamplesChange()
	{
	}

	protected virtual void _OnBeginDetach()
	{
		if (_childAttachers == null)
		{
			return;
		}
		Attacher[] array = _childAttachers;
		foreach (Attacher attacher in array)
		{
			if (attacher != this)
			{
				attacher.Finish();
			}
		}
	}

	protected virtual bool _ShouldSignalDetachComplete()
	{
		if (!detachElapsedTime.HasValue || detachElapsedTime < life.detachTime)
		{
			return false;
		}
		if (_childAttachers != null)
		{
			Attacher[] array = _childAttachers;
			foreach (Attacher attacher in array)
			{
				if (attacher != this && (bool)attacher && attacher.isActiveAndEnabled && !attacher._ShouldSignalDetachComplete())
				{
					return false;
				}
			}
		}
		return true;
	}

	private void _OnSpeedSampleChange(float sample)
	{
		_samplesDirty = true;
		events.OnSpeedSampleChange.Invoke(sample);
	}

	private void _OnLifetimeSampleChange(float sample)
	{
		_samplesDirty = true;
		events.OnLifetimeSampleChange.Invoke(sample);
	}

	private void _OnDetachSampleChange(float sample)
	{
		_samplesDirty = true;
		events.OnDetachSampleChange.Invoke(sample);
	}

	private void _OnDetachComplete()
	{
		events.OnDetachComplete.Invoke();
		if (attach.deactivateOnDetach)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void SetLifetime(float lifetime)
	{
		if (SetPropertyUtility.SetStruct(ref life.time, lifetime))
		{
			events.OnLifetimeChange.Invoke(lifetime);
		}
	}

	public void SetLifetime(float lifetime, bool? detachOnExpire)
	{
		SetLifetime(lifetime);
		if (detachOnExpire.HasValue)
		{
			life.detachOnExpire = detachOnExpire.Value;
		}
	}

	public Attacher Attach(Transform attachTo, float lifetime = 0f, bool deactivateOnDetach = true, bool clearOnDisable = true, bool setChildrenAttacherLifetimes = true, bool? detachOnExpire = null)
	{
		attach.to = attachTo;
		SetLifetime(lifetime, detachOnExpire);
		attach.deactivateOnDetach = deactivateOnDetach;
		attach.clearOnDisable = clearOnDisable;
		if (setChildrenAttacherLifetimes && lifetime > 0f)
		{
			Attacher[] array = childAttachers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetLifetime(lifetime, detachOnExpire);
			}
		}
		return this;
	}

	public void Finish()
	{
		detachElapsedTime = detachElapsedTime.GetValueOrDefault();
	}

	public void FinishAll()
	{
		Attacher[] array = childAttachers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Finish();
		}
	}
}
