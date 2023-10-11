using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
public class RotationSpeedRanges
{
	[ProtoContract(EnumPassthru = true)]
	public enum RotationSpeedType : byte
	{
		RevolutionsPerLifetime,
		RevolutionsPerSecond
	}

	private static readonly RangeF ROTATION_SPEED = new RangeF(0f, 0f, -10f, 10f);

	[ProtoMember(1)]
	[UIField(validateOnChange = true)]
	private RotationSpeedType _rotationSpeedType;

	[ProtoMember(2)]
	[UIField]
	public RangeF pitchSpeed = ROTATION_SPEED;

	[ProtoMember(3)]
	[UIField]
	public RangeF yawSpeed = ROTATION_SPEED;

	[ProtoMember(4)]
	[UIField]
	public RangeF rollSpeed = ROTATION_SPEED;

	[ProtoMember(5)]
	[UIField("Round To Nearest Complete Revolution", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	[DefaultValue(true)]
	[UIHideIf("_hideRotationSpeedIntegerStep")]
	private bool _rotationSpeedIntegerStep = true;

	public bool roundRotationSpeed
	{
		get
		{
			if (_rotationSpeedIntegerStep)
			{
				return _rotationSpeedType == RotationSpeedType.RevolutionsPerLifetime;
			}
			return false;
		}
	}

	private bool pitchSpeedSpecified => pitchSpeed != ROTATION_SPEED;

	private bool yawSpeedSpecified => yawSpeed != ROTATION_SPEED;

	private bool rollSpeedSpecified => rollSpeed != ROTATION_SPEED;

	private bool _hideRotationSpeedIntegerStep => _rotationSpeedType != RotationSpeedType.RevolutionsPerLifetime;

	public float GetRotationSpeedMultiplier(float lifetime)
	{
		if (_rotationSpeedType != RotationSpeedType.RevolutionsPerSecond)
		{
			return 360f / lifetime.InsureNonZero();
		}
		return 360f;
	}

	public static implicit operator bool(RotationSpeedRanges ranges)
	{
		if (ranges != null)
		{
			if (!ranges.pitchSpeedSpecified && !ranges.yawSpeedSpecified)
			{
				return ranges.rollSpeedSpecified;
			}
			return true;
		}
		return false;
	}
}
