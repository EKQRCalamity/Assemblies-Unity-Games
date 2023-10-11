using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class OffsetRanges
{
	private static readonly RangeF OFFSET = new RangeF(0f, 0f, -2f, 2f).Scale(0.1f);

	[ProtoMember(1)]
	[UIField(view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001f)]
	public RangeF horizontal = OFFSET;

	[ProtoMember(2)]
	[UIField(view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001f)]
	public RangeF vertical = OFFSET;

	[ProtoMember(3)]
	[UIField(view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001f)]
	public RangeF forward = OFFSET;

	public bool hasOffset
	{
		get
		{
			if (!horizontalSpecified && !verticalSpecified)
			{
				return forwardSpecified;
			}
			return true;
		}
	}

	private bool horizontalSpecified => horizontal != OFFSET;

	private bool verticalSpecified => vertical != OFFSET;

	private bool forwardSpecified => forward != OFFSET;

	public OffsetRanges()
	{
	}

	public OffsetRanges(RangeF range)
	{
		horizontal = range;
		vertical = range;
		forward = range;
	}

	public Vector3 GetOffset(System.Random random)
	{
		return new Vector3(random.Range(horizontal), random.Range(vertical), random.Range(forward));
	}

	public Vector3 GetAverageOffset(Quaternion? rotation)
	{
		return (rotation ?? Quaternion.identity) * new Vector3(horizontal.Average(), vertical.Average(), forward.Average());
	}

	public override string ToString()
	{
		return (horizontalSpecified ? (" + [" + StringUtil.ToRange(horizontal.min, horizontal.max) + "] <size=66%>horizontal</size>") : "") + (verticalSpecified ? (" + [" + StringUtil.ToRange(vertical.min, vertical.max) + "] <size=66%>vertical</size>") : "") + (forwardSpecified ? (" + [" + StringUtil.ToRange(forward.min, forward.max) + "] <size=66%>forward</size>") : "");
	}

	public static implicit operator bool(OffsetRanges offset)
	{
		return offset?.hasOffset ?? false;
	}
}
