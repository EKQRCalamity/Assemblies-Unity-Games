using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public struct SimpleCurve
{
	public static readonly SimpleCurve Default = new SimpleCurve(1f);

	public static readonly SimpleCurve EaseInOut = new SimpleCurve(0f, null, 1f);

	[ProtoMember(1, IsRequired = true)]
	[UIField(dynamicInitMethod = "_CommonInit")]
	private float _startValue;

	[ProtoMember(2, IsRequired = true)]
	[UIField(dynamicInitMethod = "_CommonInit")]
	private float? _midValue;

	[ProtoMember(3, IsRequired = true)]
	[UIField(dynamicInitMethod = "_CommonInit")]
	private float? _endValue;

	private AnimationCurve _curve;

	private float _min;

	private float _max;

	public AnimationCurve curve
	{
		get
		{
			if (_curve == null)
			{
				List<Keyframe> list = new List<Keyframe>();
				list.Add(new Keyframe(0f, _startValue, 0f, 0f));
				if (_midValue.HasValue)
				{
					list.Add(new Keyframe(0.5f, _midValue.Value));
				}
				if (_endValue.HasValue)
				{
					list.Add(new Keyframe(1f, _endValue.Value, 0f, 0f));
				}
				_curve = new AnimationCurve
				{
					keys = list.ToArray()
				};
				if (_midValue.HasValue)
				{
					_curve.SmoothTangents(1, 0f);
				}
			}
			return _curve;
		}
	}

	public SimpleCurve(float startValue, float? midValue = null, float? endValue = null, float min = 0f, float max = 1f)
	{
		_startValue = startValue;
		_midValue = midValue;
		_endValue = endValue;
		_min = min;
		_max = max;
		_curve = null;
	}

	public static implicit operator AnimationCurve(SimpleCurve simpleCurve)
	{
		return simpleCurve.curve;
	}

	public static bool operator ==(SimpleCurve a, SimpleCurve b)
	{
		if (a._startValue == b._startValue && a._midValue == b._midValue)
		{
			return a._endValue == b._endValue;
		}
		return false;
	}

	public static bool operator !=(SimpleCurve a, SimpleCurve b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (obj is SimpleCurve)
		{
			return (SimpleCurve)obj == this;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	private void OnValueChanged()
	{
		_curve = null;
	}

	private void _CommonInit(UIFieldAttribute uiField)
	{
		uiField.min = _min;
		uiField.max = _max;
	}
}
