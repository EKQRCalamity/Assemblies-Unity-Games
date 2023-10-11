using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Uniform", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public class UniformAnimator : Vector3Animator
{
	[ProtoMember(1)]
	[UIField(stepSize = 0.01f, view = "UI/Reflection/Range Slider Advanced")]
	private RangeF _range = Vector3Animator.RANGE;

	[ProtoMember(2)]
	[UIField(min = 0.1f, max = 10f, stepSize = 0.01f)]
	[DefaultValue(1f)]
	private float _frequency = 1f;

	private bool _rangeSpecified => _range != Vector3Animator.RANGE;

	public override Vector3 GetValue(float elapsedTime)
	{
		return _type.GetValue(_range, elapsedTime * _frequency).ToVector3();
	}
}
