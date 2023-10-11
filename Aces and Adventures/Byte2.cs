using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public struct Byte2 : IEquatable<Byte2>
{
	public static readonly Byte2 Zero = new Byte2(0, 0);

	[ProtoMember(1)]
	public byte x;

	[ProtoMember(2)]
	public byte y;

	public Byte2(byte x, byte y)
	{
		this.x = x;
		this.y = y;
	}

	public Byte2(int x, int y)
	{
		this.x = (byte)x;
		this.y = (byte)y;
	}

	public bool InRangeInclusive(int v)
	{
		if (v >= x)
		{
			return v <= y;
		}
		return false;
	}

	public static bool operator ==(Byte2 a, Byte2 b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(Byte2 a, Byte2 b)
	{
		return !(a == b);
	}

	public bool Equals(Byte2 other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj is Byte2)
		{
			return this == (Byte2)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x ^ (y << 8);
	}

	public static implicit operator Vector2(Byte2 value)
	{
		return new Vector2((int)value.x, (int)value.y);
	}

	public static implicit operator Short2(Byte2 value)
	{
		return new Short2(value.x, value.y);
	}

	public override string ToString()
	{
		return $"({x}, {y})";
	}
}
