using System;
using ProtoBuf;

[ProtoContract]
[UIField]
public struct RangeEnum<T> : IEquatable<RangeEnum<T>> where T : struct, IConvertible
{
	[ProtoMember(1, IsRequired = true)]
	private T _min;

	[ProtoMember(2, IsRequired = true)]
	private T _max;

	[UIField(validateOnChange = true, filter = UIEnumFilterFlags.SkipIncludeAndExcludeLogic)]
	public T min
	{
		get
		{
			return _min;
		}
		set
		{
			_max = EnumUtil.Maximum(_min = value, _max);
		}
	}

	[UIField(validateOnChange = true, filter = UIEnumFilterFlags.SkipIncludeAndExcludeLogic)]
	public T max
	{
		get
		{
			return _max;
		}
		set
		{
			_min = EnumUtil.Minimum(_min, _max = value);
		}
	}

	public RangeEnum(T min, T max)
	{
		_min = min;
		_max = max;
	}

	public bool InRange(T value)
	{
		return EnumUtil.InRange(value, _min, _max);
	}

	public string ToRangeString(string noun = "", byte nounSizePercent = 50)
	{
		return EnumUtil.ToRangeString(_min, _max, noun, nounSizePercent);
	}

	public static bool operator ==(RangeEnum<T> a, RangeEnum<T> b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(RangeEnum<T> a, RangeEnum<T> b)
	{
		return !a.Equals(b);
	}

	public bool Equals(RangeEnum<T> other)
	{
		if (EnumUtil.Equals(_min, other._min))
		{
			return EnumUtil.Equals(_max, other._max);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is RangeEnum<T>)
		{
			return Equals((RangeEnum<T>)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return CastTo<int>.From(_min) ^ (CastTo<int>.From(_max) << 16);
	}
}
