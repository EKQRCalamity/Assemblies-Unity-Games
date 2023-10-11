using System;
using ProtoBuf;

[ProtoContract]
public struct FInt : IEquatable<FInt>, IComparable<FInt>
{
	private const int SHIFT_AMOUNT = 12;

	private const long One = 4096L;

	private static readonly float OneReciprocalF = 0.00024414062f;

	private static readonly double OneReciprocalD = 0.000244140625;

	public static readonly FInt zero = 0f;

	public static FInt OneF = FromRaw(1L, useMultiple: true);

	public static FInt PI = FromRaw(12868L, useMultiple: false);

	public static FInt TwoPIF = PI * 2;

	public static FInt PIOver180F = PI / (FInt)180;

	private static int[] SIN_TABLE = new int[91]
	{
		0, 71, 142, 214, 285, 357, 428, 499, 570, 641,
		711, 781, 851, 921, 990, 1060, 1128, 1197, 1265, 1333,
		1400, 1468, 1534, 1600, 1665, 1730, 1795, 1859, 1922, 1985,
		2048, 2109, 2170, 2230, 2290, 2349, 2407, 2464, 2521, 2577,
		2632, 2686, 2740, 2793, 2845, 2896, 2946, 2995, 3043, 3091,
		3137, 3183, 3227, 3271, 3313, 3355, 3395, 3434, 3473, 3510,
		3547, 3582, 3616, 3649, 3681, 3712, 3741, 3770, 3797, 3823,
		3849, 3872, 3895, 3917, 3937, 3956, 3974, 3991, 4006, 4020,
		4033, 4045, 4056, 4065, 4073, 4080, 4086, 4090, 4093, 4095,
		4096
	};

	[ProtoMember(1)]
	public long rawValue;

	public FInt Inverse => FromRaw(-rawValue, useMultiple: false);

	private static FInt mul(FInt F1, FInt F2)
	{
		return F1 * F2;
	}

	public static FInt Sqrt(FInt f, int numberOfIterations)
	{
		if (f.rawValue < 0)
		{
			throw new ArithmeticException("Input Error");
		}
		if (f.rawValue == 0L)
		{
			return (FInt)0;
		}
		FInt fInt = f + OneF >> 1;
		for (int i = 0; i < numberOfIterations; i++)
		{
			fInt = fInt + f / fInt >> 1;
		}
		if (fInt.rawValue < 0)
		{
			throw new ArithmeticException("Overflow");
		}
		return fInt;
	}

	public static FInt Sqrt(FInt f)
	{
		byte numberOfIterations = 8;
		if (f.rawValue > 409600)
		{
			numberOfIterations = 12;
		}
		if (f.rawValue > 4096000)
		{
			numberOfIterations = 16;
		}
		return Sqrt(f, numberOfIterations);
	}

	public static FInt Sin(FInt i)
	{
		FInt j = (FInt)0;
		while (i < 0)
		{
			i += FromRaw(25736L, useMultiple: false);
		}
		if (i > FromRaw(25736L, useMultiple: false))
		{
			i %= FromRaw(25736L, useMultiple: false);
		}
		FInt fInt = i * FromRaw(10L, useMultiple: false) / FromRaw(714L, useMultiple: false);
		if (i != 0 && i != FromRaw(6434L, useMultiple: false) && i != FromRaw(12868L, useMultiple: false) && i != FromRaw(19302L, useMultiple: false) && i != FromRaw(25736L, useMultiple: false))
		{
			j = i * FromRaw(100L, useMultiple: false) / FromRaw(714L, useMultiple: false) - fInt * FromRaw(10L, useMultiple: false);
		}
		if (fInt <= FromRaw(90L, useMultiple: false))
		{
			return sin_lookup(fInt, j);
		}
		if (fInt <= FromRaw(180L, useMultiple: false))
		{
			return sin_lookup(FromRaw(180L, useMultiple: false) - fInt, j);
		}
		if (fInt <= FromRaw(270L, useMultiple: false))
		{
			return sin_lookup(fInt - FromRaw(180L, useMultiple: false), j).Inverse;
		}
		return sin_lookup(FromRaw(360L, useMultiple: false) - fInt, j).Inverse;
	}

	private static FInt sin_lookup(FInt i, FInt j)
	{
		if (j > 0 && j < FromRaw(10L, useMultiple: false) && i < FromRaw(90L, useMultiple: false))
		{
			return FromRaw(SIN_TABLE[i.rawValue], useMultiple: false) + (FromRaw(SIN_TABLE[i.rawValue + 1], useMultiple: false) - FromRaw(SIN_TABLE[i.rawValue], useMultiple: false)) / FromRaw(10L, useMultiple: false) * j;
		}
		return FromRaw(SIN_TABLE[i.rawValue], useMultiple: false);
	}

	public static FInt Cos(FInt i)
	{
		return Sin(i + FromRaw(6435L, useMultiple: false));
	}

	public static FInt Tan(FInt i)
	{
		return Sin(i) / Cos(i);
	}

	public static FInt Asin(FInt F)
	{
		bool num = F < 0;
		F = Abs(F);
		if (F > OneF)
		{
			throw new ArithmeticException("Bad Asin Input:" + F.ToDouble());
		}
		FInt fInt = mul(mul(mul(mul(FromRaw(35L, useMultiple: false), F) - FromRaw(146L, useMultiple: false), F) + FromRaw(346L, useMultiple: false), F) - FromRaw(877L, useMultiple: false), F) + FromRaw(6433L, useMultiple: false);
		FInt result = PI / FromRaw(2L, useMultiple: true) - Sqrt(OneF - F) * fInt;
		if (!num)
		{
			return result;
		}
		return result.Inverse;
	}

	public static FInt Atan(FInt F)
	{
		return Asin(F / Sqrt(OneF + F * F));
	}

	public static FInt Atan2(FInt F1, FInt F2)
	{
		if (F2.rawValue == 0L && F1.rawValue == 0L)
		{
			return (FInt)0;
		}
		FInt fInt = (FInt)0;
		if (F2 > 0)
		{
			return Atan(F1 / F2);
		}
		if (F2 < 0)
		{
			if (F1 >= 0)
			{
				return PI - Atan(Abs(F1 / F2));
			}
			return (PI - Atan(Abs(F1 / F2))).Inverse;
		}
		return ((F1 >= 0) ? PI : PI.Inverse) / FromRaw(2L, useMultiple: true);
	}

	public static FInt Abs(FInt F)
	{
		if (!(F < 0))
		{
			return F;
		}
		return F.Inverse;
	}

	private static FInt FromRaw(long startingRawValue, bool useMultiple)
	{
		FInt result = default(FInt);
		result.rawValue = startingRawValue;
		if (useMultiple)
		{
			result.rawValue <<= 12;
		}
		return result;
	}

	public int ToInt()
	{
		return (int)(rawValue >> 12);
	}

	public float ToFloat()
	{
		return (float)rawValue * OneReciprocalF;
	}

	public double ToDouble()
	{
		return (double)rawValue * OneReciprocalD;
	}

	public long ToLongApproximate()
	{
		return (long)Math.Round(ToDouble());
	}

	public FInt Lerp(FInt endPoint, FInt t)
	{
		return this + (endPoint - this) * t;
	}

	public static FInt operator *(FInt one, FInt other)
	{
		FInt result = default(FInt);
		result.rawValue = one.rawValue * other.rawValue >> 12;
		return result;
	}

	public static FInt operator *(FInt one, int multi)
	{
		return one * (FInt)multi;
	}

	public static FInt operator *(int multi, FInt one)
	{
		return one * (FInt)multi;
	}

	public static FInt operator /(FInt one, FInt other)
	{
		FInt result = default(FInt);
		result.rawValue = (one.rawValue << 12) / other.rawValue;
		return result;
	}

	public static FInt operator /(FInt one, int divisor)
	{
		return one / (FInt)divisor;
	}

	public static FInt operator /(int divisor, FInt one)
	{
		return (FInt)divisor / one;
	}

	public static FInt operator %(FInt one, FInt other)
	{
		FInt result = default(FInt);
		result.rawValue = one.rawValue % other.rawValue;
		return result;
	}

	public static FInt operator %(FInt one, int divisor)
	{
		return one % (FInt)divisor;
	}

	public static FInt operator %(int divisor, FInt one)
	{
		return (FInt)divisor % one;
	}

	public static FInt operator +(FInt one, FInt other)
	{
		FInt result = default(FInt);
		result.rawValue = one.rawValue + other.rawValue;
		return result;
	}

	public static FInt operator +(FInt one, int other)
	{
		return one + (FInt)other;
	}

	public static FInt operator +(int other, FInt one)
	{
		return one + (FInt)other;
	}

	public static FInt operator -(FInt one, FInt other)
	{
		FInt result = default(FInt);
		result.rawValue = one.rawValue - other.rawValue;
		return result;
	}

	public static FInt operator -(FInt one, int other)
	{
		return one - (FInt)other;
	}

	public static FInt operator -(int other, FInt one)
	{
		return (FInt)other - one;
	}

	public static bool operator ==(FInt one, FInt other)
	{
		return one.rawValue == other.rawValue;
	}

	public static bool operator ==(FInt one, int other)
	{
		return one == (FInt)other;
	}

	public static bool operator ==(int other, FInt one)
	{
		return (FInt)other == one;
	}

	public static bool operator !=(FInt one, FInt other)
	{
		return one.rawValue != other.rawValue;
	}

	public static bool operator !=(FInt one, int other)
	{
		return one != (FInt)other;
	}

	public static bool operator !=(int other, FInt one)
	{
		return (FInt)other != one;
	}

	public static bool operator >=(FInt one, FInt other)
	{
		return one.rawValue >= other.rawValue;
	}

	public static bool operator >=(FInt one, int other)
	{
		return one >= (FInt)other;
	}

	public static bool operator >=(int other, FInt one)
	{
		return (FInt)other >= one;
	}

	public static bool operator <=(FInt one, FInt other)
	{
		return one.rawValue <= other.rawValue;
	}

	public static bool operator <=(FInt one, int other)
	{
		return one <= (FInt)other;
	}

	public static bool operator <=(int other, FInt one)
	{
		return (FInt)other <= one;
	}

	public static bool operator >(FInt one, FInt other)
	{
		return one.rawValue > other.rawValue;
	}

	public static bool operator >(FInt one, int other)
	{
		return one > (FInt)other;
	}

	public static bool operator >(int other, FInt one)
	{
		return (FInt)other > one;
	}

	public static bool operator <(FInt one, FInt other)
	{
		return one.rawValue < other.rawValue;
	}

	public static bool operator <(FInt one, int other)
	{
		return one < (FInt)other;
	}

	public static bool operator <(int other, FInt one)
	{
		return (FInt)other < one;
	}

	public static implicit operator float(FInt src)
	{
		return (float)src.rawValue * OneReciprocalF;
	}

	public static implicit operator FInt(float src)
	{
		FInt result = default(FInt);
		result.rawValue = (long)Math.Round(src * 4096f);
		return result;
	}

	public static explicit operator int(FInt src)
	{
		return (int)(src.rawValue >> 12);
	}

	public static explicit operator FInt(int src)
	{
		return FromRaw(src, useMultiple: true);
	}

	public static explicit operator FInt(long src)
	{
		return FromRaw(src, useMultiple: true);
	}

	public static explicit operator FInt(ulong src)
	{
		return FromRaw((long)src, useMultiple: true);
	}

	public static FInt operator <<(FInt one, int Amount)
	{
		return FromRaw(one.rawValue << Amount, useMultiple: false);
	}

	public static FInt operator >>(FInt one, int Amount)
	{
		return FromRaw(one.rawValue >> Amount, useMultiple: false);
	}

	public bool Equals(FInt other)
	{
		return rawValue == other.rawValue;
	}

	public int CompareTo(FInt other)
	{
		if (rawValue <= other.rawValue)
		{
			if (rawValue != other.rawValue)
			{
				return -1;
			}
			return 0;
		}
		return 1;
	}

	public override bool Equals(object obj)
	{
		if (obj is FInt)
		{
			return ((FInt)obj).rawValue == rawValue;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return rawValue.GetHashCode();
	}

	public override string ToString()
	{
		return $"Float = {(float)this}, Raw = {rawValue}";
	}
}
