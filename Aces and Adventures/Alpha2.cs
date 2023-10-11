using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public struct Alpha2 : IEquatable<Alpha2>
{
	public static readonly Alpha2 Center = new Alpha2(0.5f, 0.5f);

	[ProtoMember(1, IsRequired = true)]
	private readonly byte _x;

	[ProtoMember(2, IsRequired = true)]
	private readonly byte _y;

	public float x => (float)(int)_x * MathUtil.OneTwoFiftyFifth;

	public float y => (float)(int)_y * MathUtil.OneTwoFiftyFifth;

	public byte xByte => _x;

	public byte yByte => _y;

	public Vector2 value => new Vector2(x, y);

	public Vector2 signedValue => new Vector2(x + x - 1f, y + y - 1f);

	public static Alpha2 Signed(Vector2 v)
	{
		return new Alpha2((v.x + 1f) * 0.5f, (v.y + 1f) * 0.5f);
	}

	public static Vector2 SignedValue(byte x, byte y)
	{
		return new Alpha2(x, y).signedValue;
	}

	public Alpha2(byte x, byte y)
	{
		_x = x;
		_y = y;
	}

	public Alpha2(float x, float y)
	{
		_x = (byte)Mathf.RoundToInt(Mathf.Clamp01(x) * 255f);
		_y = (byte)Mathf.RoundToInt(Mathf.Clamp01(y) * 255f);
	}

	public Alpha2(Vector2 value)
		: this(value.x, value.y)
	{
	}

	public static bool operator ==(Alpha2 a, Alpha2 b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Alpha2 a, Alpha2 b)
	{
		return !a.Equals(b);
	}

	public static implicit operator Vector2(Alpha2 a)
	{
		return a.value;
	}

	public override int GetHashCode()
	{
		return _x ^ (_y << 8);
	}

	public override bool Equals(object obj)
	{
		if (obj is Alpha2)
		{
			return Equals((Alpha2)obj);
		}
		return false;
	}

	public bool Equals(Alpha2 other)
	{
		if (_x == other._x)
		{
			return _y == other._y;
		}
		return false;
	}
}
