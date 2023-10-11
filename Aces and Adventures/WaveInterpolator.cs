using System;
using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Wave", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public class WaveInterpolator : Interpolator
{
	private const float FREQUENCY = 1f;

	[ProtoMember(1)]
	[UIField(min = 0.5f, max = 10, stepSize = 0.01f)]
	[DefaultValue(1f)]
	private float _frequency = 1f;

	protected override float _Interpolant(float normalizedTime)
	{
		return MathUtil.Remap(Mathf.Sin(normalizedTime * _frequency * (MathF.PI * 2f) - MathF.PI / 2f), new Vector2(-1f, 1f), new Vector2(0f, 1f));
	}
}
