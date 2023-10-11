using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public struct Byte3 : IEquatable<Byte3>
{
	public static readonly Byte3 Zero = new Byte3(0, 0, 0);

	[ProtoMember(1)]
	public byte x;

	[ProtoMember(2)]
	public byte y;

	[ProtoMember(3)]
	public byte z;

	public Byte3(byte x, byte y, byte z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public Byte3(int x, int y, int z)
	{
		this.x = (byte)x;
		this.y = (byte)y;
		this.z = (byte)z;
	}

	public static bool operator ==(Byte3 a, Byte3 b)
	{
		if (a.x == b.x && a.y == b.y)
		{
			return a.z == b.z;
		}
		return false;
	}

	public static bool operator !=(Byte3 a, Byte3 b)
	{
		return !(a == b);
	}

	public bool Equals(Byte3 other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj is Byte3)
		{
			return this == (Byte3)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x ^ (y << 8) ^ (z << 16);
	}

	public static implicit operator Vector3(Byte3 value)
	{
		return new Vector3((int)value.x, (int)value.y, (int)value.z);
	}
}
