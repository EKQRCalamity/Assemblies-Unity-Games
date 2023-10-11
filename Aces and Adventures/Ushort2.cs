using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public struct Ushort2 : IEquatable<Ushort2>
{
	public static readonly Ushort2 Zero = new Ushort2(0, 0);

	[ProtoMember(1)]
	public ushort x;

	[ProtoMember(2)]
	public ushort y;

	public Ushort2(ushort x, ushort y)
	{
		this.x = x;
		this.y = y;
	}

	public ushort Max()
	{
		if (x < y)
		{
			return y;
		}
		return x;
	}

	public Vector2 ToVector2()
	{
		return new Vector2((int)x, (int)y);
	}

	public static Ushort2 operator +(Ushort2 left, Ushort2 right)
	{
		return new Ushort2((ushort)(left.x + right.x), (ushort)(left.y + right.y));
	}

	public static Ushort2 operator -(Ushort2 left, Ushort2 right)
	{
		return new Ushort2((ushort)(left.x - right.x), (ushort)(left.y - right.y));
	}

	public static bool operator ==(Ushort2 a, Ushort2 b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(Ushort2 a, Ushort2 b)
	{
		return !(a == b);
	}

	public static implicit operator Int2(Ushort2 s)
	{
		return new Int2(s.x, s.y);
	}

	public override bool Equals(object obj)
	{
		if (obj is Ushort2)
		{
			return this == (Ushort2)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x ^ (y << 16);
	}

	public bool Equals(Ushort2 other)
	{
		if (x == other.x)
		{
			return y == other.y;
		}
		return false;
	}
}
