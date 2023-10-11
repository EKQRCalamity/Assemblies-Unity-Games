using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Noise", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public class NoiseInterpolator : Interpolator
{
	private const float FREQUENCY = 1f;

	[ProtoMember(1)]
	[UIField(min = 0.5f, max = 10, stepSize = 0.01f)]
	[DefaultValue(1f)]
	private float _frequency = 1f;

	protected override float _Interpolant(float normalizedTime)
	{
		return MathUtil.PerlinNoise(normalizedTime * _frequency, 0f, 1f, 0f) * Mathf.Sqrt(Mathf.Clamp01(normalizedTime));
	}
}
