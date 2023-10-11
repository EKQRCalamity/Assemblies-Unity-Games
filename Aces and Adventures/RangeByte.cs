using System;
using ProtoBuf;

[ProtoContract]
[UIField("Range", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public struct RangeByte
{
	private const byte DEFAULT_MAX_RANGE = 100;

	private const byte DEFAULT_MAX = 100;

	[ProtoMember(1, IsRequired = true)]
	public byte min;

	[ProtoMember(2, IsRequired = true)]
	public byte max;

	public byte minRange;

	public byte maxRange;

	public byte minDistance;

	public byte maxDistance;

	public int range => max - min;

	public RangeByte(byte min, byte max = 100, byte minRange = 0, byte maxRange = 100, byte minDistance = 0, byte maxDistance = 0)
	{
		this.min = min;
		this.max = max;
		this.minRange = minRange;
		this.maxRange = maxRange;
		this.minDistance = minDistance;
		this.maxDistance = maxDistance;
	}

	public string ToPercentString()
	{
		if (min == minRange)
		{
			return "below " + max + "%";
		}
		if (max == maxRange)
		{
			return "above " + min + "%";
		}
		if (min == max)
		{
			return max + "%";
		}
		return "between " + min + "-" + max + "%";
	}

	public string ToRangeString()
	{
		if (min != max)
		{
			return min + "-" + max;
		}
		return max.ToString();
	}

	public string ToRangeString(RangeByte? defaultValue, string noun = "", byte nounSizePercent = 50, bool showDefaultValue = false)
	{
		RangeByte rangeByte = this;
		RangeByte? rangeByte2 = defaultValue;
		if (rangeByte2.HasValue && rangeByte == rangeByte2.GetValueOrDefault() && !showDefaultValue)
		{
			return "";
		}
		if (nounSizePercent != 100 && noun != "")
		{
			noun = "<size=" + nounSizePercent + "%>" + noun + "</size>";
		}
		if (min == max && min != minRange && max != maxRange)
		{
			return min + (noun.HasVisibleCharacter() ? (" " + noun) : "");
		}
		if (max == maxRange)
		{
			return min + "+" + (noun.IsNullOrEmpty() ? "" : (" " + noun));
		}
		if (min == minRange)
		{
			return max + "-" + (noun.IsNullOrEmpty() ? "" : (" " + noun));
		}
		return "[" + min + ", " + max + "]" + noun;
	}

	public bool InRangeInclusive(float x)
	{
		if (x >= (float)(int)min)
		{
			return x <= (float)(int)max;
		}
		return false;
	}

	public bool InRangeSmart(float x)
	{
		if (!InRangeInclusive(x) && (min != minRange || !(x < (float)(int)min)))
		{
			if (max == maxRange)
			{
				return x > (float)(int)max;
			}
			return false;
		}
		return true;
	}

	public bool Threshold(float x, RangeThreshold threshold)
	{
		return InRangeSmart(x) ^ (threshold == RangeThreshold.OutsideOf);
	}

	public float Average()
	{
		return (float)(min + max) * 0.5f;
	}

	public RangeByte SetMinRange(byte newMinRange)
	{
		RangeByte result = this;
		result.minRange = newMinRange;
		result.min = Math.Max(result.min, result.minRange);
		result.max = Math.Max(result.min, result.max);
		return result;
	}

	public RangeByte SetMaxRange(byte newMaxRange)
	{
		RangeByte result = this;
		result.maxRange = newMaxRange;
		result.max = Math.Min(result.max, result.maxRange);
		result.min = Math.Min(result.min, result.max);
		return result;
	}

	public void SetComponentValues(ref byte min, ref byte max)
	{
		min = this.min;
		max = this.max;
	}

	public Short2 ToShort2()
	{
		return new Short2(min, max);
	}

	public RangeByte CopyNonSerializedFieldsFrom(RangeByte copyFrom)
	{
		RangeByte result = this;
		result.minRange = copyFrom.minRange;
		result.maxRange = copyFrom.maxRange;
		result.minDistance = copyFrom.minDistance;
		result.maxDistance = copyFrom.maxDistance;
		return result;
	}

	public RangeByte CopyNonSerializedFieldsFrom(object copyFrom)
	{
		if (!(copyFrom is RangeByte copyFrom2))
		{
			return this;
		}
		return CopyNonSerializedFieldsFrom(copyFrom2);
	}

	public static implicit operator Byte2(RangeByte range)
	{
		return new Byte2(range.min, range.max);
	}

	public static bool operator ==(RangeByte a, RangeByte b)
	{
		if (a.min == b.min)
		{
			return a.max == b.max;
		}
		return false;
	}

	public static bool operator !=(RangeByte a, RangeByte b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
