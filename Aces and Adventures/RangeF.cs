using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Range", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public struct RangeF
{
	private const float DEFAULT_MAX_RANGE = 1f;

	private const float DEFAULT_MAX = 1f;

	[ProtoMember(1, IsRequired = true)]
	public float min;

	[ProtoMember(2, IsRequired = true)]
	public float max;

	public float minRange;

	public float maxRange;

	public float minDistance;

	public float maxDistance;

	public float range => max - min;

	public float boundrayRange => maxRange - minRange;

	public RangeF(float min, float max = 1f, float minRange = 0f, float maxRange = 1f, float minDistance = 0f, float maxDistance = 0f)
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
			return "below " + MathUtil.ToPercentInt(max) + "%";
		}
		if (max == maxRange)
		{
			return "above " + MathUtil.ToPercentInt(min) + "%";
		}
		if (min == max)
		{
			return MathUtil.ToPercentInt(max) + "%";
		}
		return "between " + MathUtil.ToPercentInt(min) + "-" + MathUtil.ToPercentInt(max) + "%";
	}

	public string ToPercentStringShort()
	{
		if (range == 1f)
		{
			return " ";
		}
		if (min == minRange)
		{
			return " < " + MathUtil.ToPercentInt(max) + "% ";
		}
		if (max == maxRange)
		{
			return " > " + MathUtil.ToPercentInt(min) + "% ";
		}
		if (min == max)
		{
			return " " + MathUtil.ToPercentInt(max) + "% ";
		}
		return " " + MathUtil.ToPercentInt(min) + "-" + MathUtil.ToPercentInt(max) + "% ";
	}

	public bool InRangeInclusive(float x)
	{
		if (x >= min)
		{
			return x <= max;
		}
		return false;
	}

	public float DistanceRatio(float value)
	{
		if (!(value < min))
		{
			if (!(value > max))
			{
				return 0f;
			}
			return MathUtil.GetLerpAmount(max, maxRange, value);
		}
		return 1f - MathUtil.GetLerpAmount(minRange, min, value);
	}

	public float Lerp(float t)
	{
		return MathUtil.Lerp(min, max, t);
	}

	public RangeF LerpRange(RangeF other, float t)
	{
		return new RangeF(Mathf.Lerp(min, other.min, t), Mathf.Lerp(max, other.max, t), Mathf.Min(minRange, other.minRange), Mathf.Max(maxRange, other.maxRange));
	}

	public float Average()
	{
		return (min + max) * 0.5f;
	}

	public float GetLerpAmount(float value)
	{
		return MathUtil.GetLerpAmount(min, max, value);
	}

	public RangeF SetMinMax(float min, float max)
	{
		RangeF result = this;
		result.min = min;
		result.max = max;
		return result;
	}

	public RangeF SetMinMax(Vector2 minMax)
	{
		return SetMinMax(minMax.x, minMax.y);
	}

	public RangeF CopyNonSerializedFieldsFrom(RangeF copyFrom)
	{
		RangeF result = this;
		result.minRange = copyFrom.minRange;
		result.maxRange = copyFrom.maxRange;
		result.minDistance = copyFrom.minDistance;
		result.maxDistance = copyFrom.maxDistance;
		return result;
	}

	public RangeF CopyNonSerializedFieldsFrom(object copyFrom)
	{
		if (!(copyFrom is RangeF copyFrom2))
		{
			return this;
		}
		return CopyNonSerializedFieldsFrom(copyFrom2);
	}

	public RangeF Scale(float scale)
	{
		return new RangeF(min * scale, max * scale, minRange * scale, maxRange * scale, minDistance * scale, maxDistance * scale);
	}

	public static implicit operator Vector2(RangeF range)
	{
		return new Vector2(range.min, range.max);
	}

	public static bool operator ==(RangeF a, RangeF b)
	{
		if (a.min == b.min)
		{
			return a.max == b.max;
		}
		return false;
	}

	public static bool operator !=(RangeF a, RangeF b)
	{
		return !(a == b);
	}

	public static RangeF operator *(RangeF range, float f)
	{
		return new RangeF(Mathf.Clamp(range.min * f, range.minRange, range.maxRange), Mathf.Clamp(range.max * f, range.minRange, range.maxRange));
	}

	public static RangeF operator *(float f, RangeF range)
	{
		return new RangeF(Mathf.Clamp(range.min * f, range.minRange, range.maxRange), Mathf.Clamp(range.max * f, range.minRange, range.maxRange));
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return $"[{min:0.##},{max:0.##}]";
	}
}
