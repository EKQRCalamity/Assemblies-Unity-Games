using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class RotationRanges
{
	private static readonly RangeInt ROTATION = new RangeInt(0, 0, -360, 360);

	[ProtoMember(1)]
	[UIField]
	public RangeInt pitch = ROTATION;

	[ProtoMember(2)]
	[UIField]
	public RangeInt yaw = ROTATION;

	[ProtoMember(3)]
	[UIField]
	public RangeInt roll = ROTATION;

	private bool pitchSpecified => pitch != ROTATION;

	private bool yawSpecified => yaw != ROTATION;

	private bool rollSpecified => roll != ROTATION;

	public Quaternion GetRotation(System.Random random)
	{
		return Quaternion.Euler(random.RangeInt(pitch), random.RangeInt(yaw), random.RangeInt(roll));
	}

	public static implicit operator bool(RotationRanges ranges)
	{
		if (ranges != null)
		{
			if (!ranges.pitchSpecified && !ranges.yawSpecified)
			{
				return ranges.rollSpecified;
			}
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return (pitch.ToRangeString(ROTATION, "Pitch", 66).SpaceIfNotEmpty() + yaw.ToRangeString(ROTATION, "Yaw", 66).SpaceIfNotEmpty() + roll.ToRangeString(ROTATION, "Roll", 66)).Trim();
	}
}
