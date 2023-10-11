using ProtoBuf;

[ProtoContract]
[UIField]
[ProtoInclude(10, typeof(EaseInterpolator))]
[ProtoInclude(11, typeof(WaveInterpolator))]
[ProtoInclude(12, typeof(NoiseInterpolator))]
[ProtoInclude(13, typeof(BounceInterpolator))]
public abstract class Interpolator
{
	protected abstract float _Interpolant(float normalizedTime);

	public float Interpolant(float normalizedTime, WrapMethod? wrapMethod)
	{
		return _Interpolant(wrapMethod.Wrap(normalizedTime));
	}
}
