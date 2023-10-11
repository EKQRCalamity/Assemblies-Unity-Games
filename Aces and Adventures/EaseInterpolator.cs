using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField("Ease", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public class EaseInterpolator : Interpolator
{
	private const float EASE_POWER = 2f;

	[ProtoMember(1)]
	[UIField(validateOnChange = true)]
	[DefaultValue(EaseType.EaseIn)]
	private EaseType _easingMethod = EaseType.EaseIn;

	[ProtoMember(2)]
	[UIField(min = 1, max = 5, stepSize = 0.01f)]
	[DefaultValue(2f)]
	[UIHideIf("_hideEasePower")]
	private float _easePower = 2f;

	private bool _hideEasePower
	{
		get
		{
			if (_easingMethod != 0)
			{
				return _easingMethod == EaseType.EaseInAndOut;
			}
			return true;
		}
	}

	protected override float _Interpolant(float normalizedTime)
	{
		return _easingMethod.Ease(0f, 1f, normalizedTime, _easePower);
	}
}
