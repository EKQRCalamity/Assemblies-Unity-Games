using System;
using ProtoBuf;

[ProtoContract]
[UIField("Range", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public struct RangeInt
{
	private const int DEFAULT_MAX_RANGE = 100;

	private const int DEFAULT_MAX = 100;

	[ProtoMember(1, IsRequired = true)]
	public int min;

	[ProtoMember(2, IsRequired = true)]
	public int max;

	public int minRange;

	public int maxRange;

	public int minDistance;

	public int maxDistance;

	public int range => max - min;

	public int absMax => Math.Max(Math.Abs(min), Math.Abs(max));

	public RangeInt(int min, int max = 100, int minRange = 0, int maxRange = 100, int minDistance = 0, int maxDistance = 0)
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

	public string ToRangeString(RangeInt? defaultValue = null, string noun = "", byte nounSizePercent = 50)
	{
		if (this == defaultValue)
		{
			return "";
		}
		if (nounSizePercent != 100 && noun != "")
		{
			noun = "<size=" + nounSizePercent + "%>" + noun + "</size>";
		}
		if (min == max && min != minRange && max != maxRange)
		{
			return "[" + min + "]" + noun;
		}
		if (max == maxRange)
		{
			return min + "+ " + noun;
		}
		if (min == minRange)
		{
			return max + "- " + noun;
		}
		return "[" + min + ", " + max + "]" + noun;
	}

	public bool InRangeInclusive(float x)
	{
		if (x >= (float)min)
		{
			return x <= (float)max;
		}
		return false;
	}

	public bool InRangeSmart(float x)
	{
		if (!InRangeInclusive(x) && (min != minRange || !(x < (float)min)))
		{
			if (max == maxRange)
			{
				return x > (float)max;
			}
			return false;
		}
		return true;
	}

	public float Lerp(float t)
	{
		return MathUtil.Lerp(min, max, t);
	}

	public RangeInt CopyNonSerializedFieldsFrom(RangeInt copyFrom)
	{
		RangeInt result = this;
		result.minRange = copyFrom.minRange;
		result.maxRange = copyFrom.maxRange;
		result.minDistance = copyFrom.minDistance;
		result.maxDistance = copyFrom.maxDistance;
		return result;
	}

	public RangeInt CopyNonSerializedFieldsFrom(object copyFrom)
	{
		if (!(copyFrom is RangeInt copyFrom2))
		{
			return this;
		}
		return CopyNonSerializedFieldsFrom(copyFrom2);
	}

	public static implicit operator Int2(RangeInt range)
	{
		return new Int2(range.min, range.max);
	}

	public static bool operator ==(RangeInt a, RangeInt b)
	{
		if (a.min == b.min)
		{
			return a.max == b.max;
		}
		return false;
	}

	public static bool operator !=(RangeInt a, RangeInt b)
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
