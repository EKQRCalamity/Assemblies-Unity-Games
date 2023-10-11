using System;
using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
public class AttacherAudioProjectileSettings : IAttacherAudioSettings
{
	private const string CAT_SPEED = "Speed";

	private const string CAT_VOLUME = "Volume";

	private const string CAT_PITCH = "Pitch";

	private const bool SPEED_CONTROLS_VOLUME = true;

	private static readonly RangeF VOLUME_RANGE = new RangeF(0f);

	private const bool SPEED_CONTROLS_PITCH = true;

	private static readonly RangeF PITCH_RANGE = new RangeF(0.5f, 2f, 0.1f, 10f);

	private const float SPEED_EASE = 50f;

	private static readonly RangeF RANDOM_VOLUME_MULTIPLIER = new RangeF(0.75f, 1f, 0.1f);

	private static readonly RangeF RANDOM_PITCH_MULTIPLIER = ProjectileMediaData.MULTIPLIER;

	[ProtoMember(1)]
	[UIField(min = 0, max = 3f, category = "Speed")]
	[DefaultValue(1f)]
	private float _referenceSpeed = 1f;

	[ProtoMember(2)]
	[UIField(min = 1, max = 100, category = "Speed")]
	[DefaultValue(50f)]
	[UIHideIf("_hideCommon")]
	private float _speedEaseSpeed = 50f;

	[ProtoMember(3)]
	[UIField(category = "Volume")]
	private RangeF _randomVolumeMultiplier = RANDOM_VOLUME_MULTIPLIER;

	[ProtoMember(4)]
	[UIField(validateOnChange = true, category = "Volume")]
	[DefaultValue(true)]
	private bool _speedControlsVolume = true;

	[ProtoMember(5)]
	[UIField(category = "Volume")]
	[UIHideIf("_hideSpeedVolumeRange")]
	private RangeF _speedVolumeRange = VOLUME_RANGE;

	[ProtoMember(6)]
	[UIField(category = "Pitch")]
	private RangeF _randomPitchMultiplier = RANDOM_PITCH_MULTIPLIER;

	[ProtoMember(7)]
	[UIField(validateOnChange = true, category = "Pitch")]
	[DefaultValue(true)]
	private bool _speedControlsPitch = true;

	[ProtoMember(8)]
	[UIField(category = "Pitch")]
	[UIHideIf("_hideSpeedPitchRange")]
	private RangeF _speedPitchRange = PITCH_RANGE;

	[ProtoMember(9)]
	[UIField]
	private OffsetRanges _positionOffset;

	private OffsetRanges positionOffset => _positionOffset ?? (_positionOffset = new OffsetRanges());

	private bool _randomVolumeMultiplierSpecified => _randomVolumeMultiplier != RANDOM_VOLUME_MULTIPLIER;

	private bool _speedVolumeRangeSpecified => _speedVolumeRange != VOLUME_RANGE;

	private bool _randomPitchMultiplierSpecified => _randomPitchMultiplier != RANDOM_PITCH_MULTIPLIER;

	private bool _speedPitchRangeSpecified => _speedPitchRange != PITCH_RANGE;

	private bool _positionOffsetSpecified => _positionOffset;

	private bool _hideSpeedVolumeRange => !_speedControlsVolume;

	private bool _hideSpeedPitchRange => !_speedControlsPitch;

	private bool _hideCommon
	{
		get
		{
			if (_hideSpeedVolumeRange)
			{
				return _hideSpeedPitchRange;
			}
			return false;
		}
	}

	public void Apply(Random random, AttacherAudio attacherAudio)
	{
		attacherAudio.speedRange.max = _referenceSpeed;
		attacherAudio.volume.multiplier = random.Range(_randomVolumeMultiplier);
		attacherAudio.volume.speedEnabled = _speedControlsVolume;
		attacherAudio.volume.speedRange = _speedVolumeRange;
		attacherAudio.pitch.multiplier = random.Range(_randomPitchMultiplier);
		attacherAudio.pitch.speedEnabled = _speedControlsPitch;
		attacherAudio.pitch.speedRange = _speedPitchRange;
		attacherAudio.attach.axisOffsetDistances = positionOffset.GetOffset(random);
	}
}
