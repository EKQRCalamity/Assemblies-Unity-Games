using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract(IgnoreListHandling = true)]
public struct Short2 : IEquatable<Short2>
{
	public static readonly Short2 Zero = new Short2(0, 0);

	public static readonly Short2 One = new Short2(1, 1);

	public static readonly Short2 MinValue = new Short2(short.MinValue, short.MinValue);

	public static readonly Short2 MaxValue = new Short2(short.MaxValue, short.MaxValue);

	public static readonly Short2 Right = new Short2(1, 0);

	public static readonly Short2 Up = new Short2(0, 1);

	public static readonly Short2 Left = new Short2(-1, 0);

	public static readonly Short2 Down = new Short2(0, -1);

	public static readonly Short2 RightUp = new Short2(1, 1);

	public static readonly Short2 LeftUp = new Short2(-1, 1);

	public static readonly Short2 LeftDown = new Short2(-1, -1);

	public static readonly Short2 RightDown = new Short2(1, -1);

	public static readonly Short2[] Axes = new Short2[2] { Right, Up };

	public static readonly Dictionary<Short2, Short2> OtherAxis = new Dictionary<Short2, Short2>
	{
		{ Right, Up },
		{ Up, Right },
		{ Left, Down },
		{ Down, Left }
	};

	[ProtoMember(1)]
	public readonly short x;

	[ProtoMember(2)]
	public readonly short y;

	public bool shouldSerialize
	{
		get
		{
			if (x == 0)
			{
				return y != 0;
			}
			return true;
		}
	}

	public short this[int index]
	{
		get
		{
			if (index != 0)
			{
				return y;
			}
			return x;
		}
	}

	public static Short2 NearestCardinalDirection(Vector2 dir)
	{
		float num = ((dir.x > 0f) ? dir.x : (0f - dir.x));
		float num2 = ((dir.y > 0f) ? dir.y : (0f - dir.y));
		if (!(num > num2))
		{
			return new Short2(0, (dir.y > 0f) ? 1 : (-1));
		}
		return new Short2((dir.x > 0f) ? 1 : (-1), 0);
	}

	public static Short2 FromGridIndex(int index, int gridWidth)
	{
		return new Short2(index.Modulus(gridWidth), index / gridWidth);
	}

	public static int MaxIndicesInRange(int range)
	{
		if (range < 0)
		{
			return 0;
		}
		return 1 + 2 * (range + 1) * range;
	}

	public static int MaxIndicesInRange(Short2 range)
	{
		return MaxIndicesInRange(range.y) - MaxIndicesInRange(range.x - 1);
	}

	public Short2(short x, short y)
	{
		this.x = x;
		this.y = y;
	}

	public Short2(int x, int y)
	{
		this.x = (short)x;
		this.y = (short)y;
	}

	public Short2(float x, float y)
	{
		this.x = (short)Mathf.RoundToInt(x);
		this.y = (short)Mathf.RoundToInt(y);
	}

	public Short2(Vector3 position)
		: this(position.x, position.z)
	{
	}

	public Short2(Int2 int2)
		: this(int2.x, int2.y)
	{
	}

	public Short2 Rotate(int rotation)
	{
		int value = 0;
		MathUtil.Wrap(ref value, rotation, 0, 4);
		return Orient((byte)value);
	}

	public Short2 Orient(byte orientation)
	{
		return orientation switch
		{
			0 => this, 
			1 => new Short2(-y, x), 
			2 => new Short2(-x, -y), 
			3 => new Short2(y, -x), 
			_ => this, 
		};
	}

	public Short2 Unorient(byte orientation)
	{
		return Orient((byte)(4 - orientation));
	}

	public short Dot(Short2 s)
	{
		return (short)(x * s.x + y * s.y);
	}

	public Short2 SetAxis(int axis, short value)
	{
		if (axis != 0)
		{
			return new Short2(x, value);
		}
		return new Short2(value, y);
	}

	public bool HasCommonAxisValueWith(Short2 other)
	{
		if (x != other.x)
		{
			return y == other.y;
		}
		return true;
	}

	public bool IsDiagonal()
	{
		if (x != y)
		{
			return x == -y;
		}
		return true;
	}

	public bool IsCardinal()
	{
		return (x == 0) ^ (y == 0);
	}

	public bool IsAdjacentTo(Short2 other)
	{
		return (this - other).AbsMax() == 1;
	}

	public bool IsSameOrAdjacentTo(Short2 other)
	{
		if (!(this == other))
		{
			return IsAdjacentTo(other);
		}
		return true;
	}

	public int GreaterAxis()
	{
		if (((x > 0) ? x : (-x)) < ((y > 0) ? y : (-y)))
		{
			return 1;
		}
		return 0;
	}

	public int LesserAxis()
	{
		if (((x > 0) ? x : (-x)) < ((y > 0) ? y : (-y)))
		{
			return 0;
		}
		return 1;
	}

	public float Length()
	{
		return (float)Math.Sqrt(x * x + y * y);
	}

	public int LengthSquared()
	{
		return x * x + y * y;
	}

	public int ManhattenLength()
	{
		int num = ((x > 0) ? x : (-x));
		int num2 = ((y > 0) ? y : (-y));
		return num + num2;
	}

	public short Min()
	{
		if (x >= y)
		{
			return y;
		}
		return x;
	}

	public short Max()
	{
		if (x <= y)
		{
			return y;
		}
		return x;
	}

	public Short2 Min(Short2 other)
	{
		return new Short2((x < other.x) ? x : other.x, (y < other.y) ? y : other.y);
	}

	public Short2 Max(Short2 other)
	{
		return new Short2((x > other.x) ? x : other.x, (y > other.y) ? y : other.y);
	}

	public short AbsMin()
	{
		short num = ((x > 0) ? x : ((short)(-x)));
		short num2 = ((y > 0) ? y : ((short)(-y)));
		if (num >= num2)
		{
			return num2;
		}
		return num;
	}

	public short AbsMax()
	{
		short num = ((x > 0) ? x : ((short)(-x)));
		short num2 = ((y > 0) ? y : ((short)(-y)));
		if (num <= num2)
		{
			return num2;
		}
		return num;
	}

	public Short2 Abs()
	{
		return new Short2(Mathf.Abs(x), Mathf.Abs(y));
	}

	public Short2 Clamp(short min = 0, short max = short.MaxValue)
	{
		return new Short2(Mathf.Clamp(x, min, max), Mathf.Clamp(y, min, max));
	}

	public Short2 Clamp(Short2 min, Short2 max)
	{
		return new Short2(Mathf.Clamp(x, min.x, max.x), Mathf.Clamp(y, min.y, max.y));
	}

	public Short2 ClampManhattanLength(int minLength, int maxLength)
	{
		Short2 result = this;
		int num = ManhattenLength();
		if (num < minLength)
		{
			float num2 = (float)minLength / (float)num;
			result = new Short2(Mathf.RoundToInt((float)result.x * num2), Mathf.RoundToInt((float)result.y * num2));
		}
		else if (num > maxLength)
		{
			float num3 = (float)maxLength / (float)num;
			result = new Short2(Mathf.RoundToInt((float)result.x * num3), Mathf.RoundToInt((float)result.y * num3));
		}
		return result;
	}

	public Short2 Unitize()
	{
		return new Short2((x != 0) ? ((x > 0) ? 1 : (-1)) : 0, (y != 0) ? ((y > 0) ? 1 : (-1)) : 0);
	}

	public Short2 NearestUnitized()
	{
		return ToVector2().normalized.RoundToShort2();
	}

	public Short2 ToCardinal()
	{
		return SetAxis(LesserAxis(), 0);
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, y);
	}

	public Vector3 ToVector3(float y = 0f)
	{
		return new Vector3(x, y, this.y);
	}

	public SRect ToSRect()
	{
		return new SRect(this, this);
	}

	public Short2 Project(Short2 direction)
	{
		return Dot(direction) * direction;
	}

	public Short2 ClosestPoint(IEnumerable<Short2> points)
	{
		Short2 result = this;
		int num = int.MaxValue;
		foreach (Short2 point in points)
		{
			int num2 = (this - point).ManhattenLength();
			if (num2 < num)
			{
				result = point;
				num = num2;
			}
		}
		return result;
	}

	public Short2 FurthestPoint(IEnumerable<Short2> points)
	{
		Short2 result = this;
		int num = int.MinValue;
		foreach (Short2 point in points)
		{
			int num2 = (this - point).ManhattenLength();
			if (num2 > num)
			{
				result = point;
				num = num2;
			}
		}
		return result;
	}

	public static Short2 operator +(Short2 left, Short2 right)
	{
		return new Short2((short)(left.x + right.x), (short)(left.y + right.y));
	}

	public static Short2 operator -(Short2 left, Short2 right)
	{
		return new Short2((short)(left.x - right.x), (short)(left.y - right.y));
	}

	public static Short2 operator -(Short2 s)
	{
		return new Short2(-s.x, -s.y);
	}

	public static Short2 operator *(Short2 left, Short2 right)
	{
		return new Short2((short)(left.x * right.x), (short)(left.y * right.y));
	}

	public static Short2 operator *(Short2 s, int multiplier)
	{
		return new Short2(s.x * multiplier, s.y * multiplier);
	}

	public static Short2 operator /(Short2 s, int denomenator)
	{
		return new Short2(s.x / denomenator, s.y / denomenator);
	}

	public static Short2 operator *(int multiplier, Short2 s)
	{
		return new Short2(s.x * multiplier, s.y * multiplier);
	}

	public static Short2 operator /(int denomenator, Short2 s)
	{
		return new Short2(s.x / denomenator, s.y / denomenator);
	}

	public static implicit operator Int2(Short2 s)
	{
		return new Int2(s.x, s.y);
	}

	public static bool operator ==(Short2 a, Short2 b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(Short2 a, Short2 b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (obj is Short2)
		{
			return this == (Short2)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x ^ (y << 16);
	}

	public override string ToString()
	{
		short num = x;
		string text = num.ToString();
		num = y;
		return text + "," + num;
	}

	public bool Equals(Short2 other)
	{
		if (x == other.x)
		{
			return y == other.y;
		}
		return false;
	}
}
