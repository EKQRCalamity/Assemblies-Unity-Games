using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Bounce", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public class BounceInterpolator : Interpolator
{
	[ProtoMember(1)]
	[UIField(min = 0.5f, max = 10, onValueChangedMethod = "_SetDirty")]
	[DefaultValue(2)]
	private float _numberOfBounces = 2f;

	[ProtoMember(2)]
	[UIField(min = 0.1f, max = 1, onValueChangedMethod = "_SetDirty")]
	[DefaultValue(0.71f)]
	private float _bounciness = 0.71f;

	private Vector2? _initalVelocityAndGravity;

	private Vector2 initialVelocityAndGravity
	{
		get
		{
			if (!_initalVelocityAndGravity.HasValue)
			{
				float num = 1f / MathUtil.GeometricSeriesSum(1f, _bounciness, _numberOfBounces);
				_initalVelocityAndGravity = new Vector2(4f / num, -8f / (num * num));
			}
			return _initalVelocityAndGravity.Value;
		}
	}

	protected override float _Interpolant(float normalizedTime)
	{
		return MathUtil.BounceHeight(normalizedTime, 0f, initialVelocityAndGravity.x, initialVelocityAndGravity.y, 0f, _bounciness).height;
	}

	private void _SetDirty()
	{
		_initalVelocityAndGravity = null;
	}
}
