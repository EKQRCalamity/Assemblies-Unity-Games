using System;
using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
public class AttacherLightProjectileSettings : IAttacherLightSettings
{
	private static readonly RangeF INTENSITY = new RangeF(1f, 1f, 0.01f, 5f);

	private static readonly RangeF SIZE = new RangeF(1f, 1f, 0.1f, 5f);

	[ProtoMember(1)]
	[UIField(validateOnChange = true)]
	[DefaultValue(true)]
	[UIHideIf("_hideInheritColor")]
	private bool _inheritColor = true;

	[ProtoMember(2)]
	[UIField]
	[UIHideIf("_hideTint")]
	private OptionalTints _tint;

	[ProtoMember(3)]
	[UIField]
	[DefaultValue(true)]
	private bool _inheritScale = true;

	[ProtoMember(4)]
	[UIField]
	private RangeF _size = SIZE;

	[ProtoMember(5)]
	[UIField]
	private RangeF _intensity = INTENSITY;

	[ProtoMember(6)]
	[UIField]
	private OffsetRanges _positionOffset;

	[ProtoMember(7)]
	private bool _canInheritColor;

	private OffsetRanges positionOffset => _positionOffset ?? (_positionOffset = new OffsetRanges());

	public bool inheritColor
	{
		get
		{
			if (_inheritColor)
			{
				return _canInheritColor;
			}
			return false;
		}
	}

	public bool canInheritColor
	{
		private get
		{
			return _canInheritColor;
		}
		set
		{
			_canInheritColor = value;
		}
	}

	private bool _sizeSpecified => _size != SIZE;

	private bool _intensitySpecified => _intensity != INTENSITY;

	private bool _positionOffsetSpecified => _positionOffset;

	private bool _hideInheritColor => !_canInheritColor;

	private bool _hideTint => inheritColor;

	public void Apply(Random random, AttacherLight attacherLight)
	{
		attacherLight.color.tintColor = ((!inheritColor) ? _tint.GetTint(random) : null);
		attacherLight.inheritScale = _inheritScale;
		attacherLight.range.multiplier = random.Range(_size);
		attacherLight.intensity.multiplier = random.Range(_intensity);
		attacherLight.attach.axisOffsetDistances = positionOffset.GetOffset(random);
	}
}
