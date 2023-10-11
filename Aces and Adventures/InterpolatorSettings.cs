using ProtoBuf;

[ProtoContract]
[UIField]
public class InterpolatorSettings
{
	private static readonly RangeF REPITITIONS = new RangeF(1f, 1f, 0.1f, 10f);

	[ProtoMember(1)]
	[UIField]
	private RangeF _repititions = REPITITIONS;

	[ProtoMember(2)]
	[UIField]
	private WrapMethod? _wrapMethod;

	[ProtoMember(3)]
	[UIField]
	private Interpolator _interpolationMethod;

	protected Interpolator type => _interpolationMethod ?? (_interpolationMethod = new EaseInterpolator());

	private bool _repititionsSpecified => _repititions != REPITITIONS;

	public float Interpolate(float normalizedTime, float repititionRangeSample)
	{
		return type.Interpolant(normalizedTime * _repititions.Lerp(repititionRangeSample), _wrapMethod);
	}
}
