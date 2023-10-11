using System;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class AttacherTrail : Attacher
{
	private const float MIN_WIDTH = 0.001f;

	private const float MIN_LIFE = 0.001f;

	[Header("Trail==================================================================================")]
	public AttacherCurveData width;

	public AttacherCurveData lifetime;

	public AttacherGradientData startColor;

	public AttacherGradientData endColor;

	private TrailRenderer _trail;

	private float? _initialEndWidth;

	public TrailRenderer trail
	{
		get
		{
			if (!_trail)
			{
				return _InitializeValues();
			}
			return _trail;
		}
	}

	public float? widthMultiplier { get; set; }

	public float initialEndWidth
	{
		get
		{
			float valueOrDefault = _initialEndWidth.GetValueOrDefault();
			if (!_initialEndWidth.HasValue)
			{
				valueOrDefault = trail.endWidth;
				_initialEndWidth = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	protected override float _offsetMultiplier
	{
		get
		{
			if (!widthMultiplier.HasValue)
			{
				return 1f;
			}
			return widthMultiplier.Value * 0.5f * MathUtil.SqrtTwo;
		}
	}

	private TrailRenderer _InitializeValues()
	{
		_trail = GetComponent<TrailRenderer>();
		width.initialValue = _trail.widthMultiplier;
		lifetime.initialValue = _trail.time;
		startColor.initialValue = _trail.startColor;
		endColor.initialValue = _trail.endColor;
		return _trail;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		trail.Clear();
		widthMultiplier = null;
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
		if (width.enabled)
		{
			trail.widthMultiplier = Mathf.Max(0.001f, width.GetSampleValue(this) * (widthMultiplier ?? 1f));
		}
		if (lifetime.enabled)
		{
			trail.time = Mathf.Max(0.001f, lifetime.GetSampleValue(this));
		}
		if (startColor.enabled)
		{
			trail.startColor = startColor.GetSampleValue(this);
		}
		if (endColor.enabled)
		{
			trail.endColor = endColor.GetSampleValue(this);
		}
	}

	protected override bool _ShouldSignalDetachComplete()
	{
		if (base._ShouldSignalDetachComplete())
		{
			if (trail.positionCount != 0 && !(trail.widthMultiplier <= 0.001f))
			{
				return trail.time <= 0.001f;
			}
			return true;
		}
		return false;
	}

	public AttacherTrail ApplySettings(System.Random random, IAttacherTrailSettings settings, bool applyToChildren = true)
	{
		if (applyToChildren)
		{
			foreach (AttacherTrail item in base.gameObject.GetComponentsInChildrenPooled<AttacherTrail>())
			{
				settings.Apply(random, item);
			}
			return this;
		}
		settings.Apply(random, this);
		return this;
	}
}
