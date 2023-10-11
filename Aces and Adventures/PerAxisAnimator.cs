using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Per Axis", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public class PerAxisAnimator : Vector3Animator
{
	[ProtoMember(1)]
	[UIField(stepSize = 0.01f, view = "UI/Reflection/Range Slider Advanced")]
	private RangeF _xRange = Vector3Animator.RANGE;

	[ProtoMember(2)]
	[UIField(min = 0.1f, max = 10f, stepSize = 0.01f)]
	[DefaultValue(1f)]
	private float _xFrequency = 1f;

	[ProtoMember(3)]
	[UIField(stepSize = 0.01f, view = "UI/Reflection/Range Slider Advanced")]
	[UIMargin(24f, false)]
	private RangeF _yRange = Vector3Animator.RANGE;

	[ProtoMember(4)]
	[UIField(min = 0.1f, max = 10f, stepSize = 0.01f)]
	[DefaultValue(1f)]
	private float _yFrequency = 1f;

	[ProtoMember(5)]
	[UIField(stepSize = 0.01f, view = "UI/Reflection/Range Slider Advanced")]
	[UIMargin(24f, false)]
	private RangeF _zRange = Vector3Animator.RANGE;

	[ProtoMember(6)]
	[UIField(min = 0.1f, max = 10f, stepSize = 0.01f)]
	[DefaultValue(1f)]
	private float _zFrequency = 1f;

	private bool _xRangeSpecified => _xRange != Vector3Animator.RANGE;

	private bool _yRangeSpecified => _yRange != Vector3Animator.RANGE;

	private bool _zRangeSpecified => _zRange != Vector3Animator.RANGE;

	public override Vector3 GetValue(float elapsedTime)
	{
		return new Vector3(_type.GetValue(_xRange, elapsedTime * _xFrequency), _type.GetValue(_yRange, elapsedTime * _yFrequency, 0f, 1f), _type.GetValue(_zRange, elapsedTime * _zFrequency, -1f));
	}
}
