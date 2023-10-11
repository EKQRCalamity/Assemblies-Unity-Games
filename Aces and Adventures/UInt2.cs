using System;

public struct UInt2 : IEquatable<UInt2>
{
	public readonly uint x;

	public readonly uint y;

	public UInt2(uint x, uint y)
	{
		this.x = x;
		this.y = y;
	}

	public static UInt2 operator +(UInt2 a, UInt2 b)
	{
		return new UInt2(a.x + b.x, a.y + b.y);
	}

	public bool Equals(UInt2 other)
	{
		if (x == other.x)
		{
			return y == other.y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)(x ^ y);
	}

	public override string ToString()
	{
		return $"({x}, {y})";
	}
}
