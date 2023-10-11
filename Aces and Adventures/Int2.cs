using System;
using UnityEngine;

[Serializable]
public struct Int2 : IEquatable<Int2>
{
	public static readonly Int2 zero = default(Int2);

	public static readonly Int2 one = new Int2(1, 1);

	public static readonly Int2 MinValue = new Int2(int.MinValue, int.MinValue);

	public int x;

	public int y;

	public int min
	{
		get
		{
			if (x >= y)
			{
				return y;
			}
			return x;
		}
	}

	public int max
	{
		get
		{
			if (x <= y)
			{
				return y;
			}
			return x;
		}
	}

	public int area => x * y;

	public float aspectRatio => (float)x / ((float)y).InsureNonZero();

	public Int2(int value)
	{
		x = (y = value);
	}

	public Int2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Int2(Vector2 v)
		: this(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y))
	{
	}

	public static Int2 operator +(Int2 a, Int2 b)
	{
		return new Int2(a.x + b.x, a.y + b.y);
	}

	public static Int2 operator -(Int2 a, Int2 b)
	{
		return new Int2(a.x - b.x, a.y - b.y);
	}

	public static Int2 operator -(Int2 a, int b)
	{
		return new Int2(a.x - b, a.y - b);
	}

	public static Int2 operator *(Int2 a, float f)
	{
		return new Int2(Mathf.RoundToInt((float)a.x * f), Mathf.RoundToInt((float)a.y * f));
	}

	public static Int2 operator -(Int2 a)
	{
		return new Int2(-a.x, -a.y);
	}

	public static bool operator ==(Int2 a, Int2 b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(Int2 a, Int2 b)
	{
		if (a.x == b.x)
		{
			return a.y != b.y;
		}
		return true;
	}

	public static implicit operator Vector2(Int2 a)
	{
		return new Vector2(a.x, a.y);
	}

	public bool Equals(Int2 other)
	{
		return this == other;
	}

	public override bool Equals(object o)
	{
		if (o is Int2)
		{
			return this == (Int2)o;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (x & BitMask.First16Bits) ^ ((y & BitMask.First16Bits) << 16);
	}

	public override string ToString()
	{
		return "(" + x + ", " + y + ")";
	}

	public bool InRangeInclusive(int v)
	{
		if (v >= x)
		{
			return v <= y;
		}
		return false;
	}

	public Int2 Min(Int2 b)
	{
		return new Int2(Math.Min(x, b.x), Math.Min(y, b.y));
	}

	public Int2 Max(Int2 b)
	{
		return new Int2(Math.Max(x, b.x), Math.Max(y, b.y));
	}

	public float GetLerpAmount(float f)
	{
		return MathUtil.GetLerpAmount(x, y, f);
	}

	public Int2 MakeMinToMax()
	{
		if (x > y)
		{
			return new Int2(y, x);
		}
		return this;
	}

	public Int2 Clamp(Int2 range)
	{
		return new Int2(Mathf.Clamp(x, range.x, range.y), Mathf.Clamp(y, range.x, range.y));
	}

	public Int2 MultiplyFloor(float multiplier)
	{
		return new Int2(Mathf.FloorToInt((float)x * multiplier), Mathf.FloorToInt((float)y * multiplier));
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, y);
	}

	public void Set(ref int min, ref int max)
	{
		min = x;
		max = y;
	}
}
