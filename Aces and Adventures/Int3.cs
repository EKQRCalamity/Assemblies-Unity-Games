using System;

[Serializable]
public struct Int3 : IEquatable<Int3>
{
	public static readonly Int3 zero = default(Int3);

	public static readonly Int3 one = new Int3(1, 1, 1);

	public int x;

	public int y;

	public int z;

	public Int3(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public static bool operator ==(Int3 a, Int3 b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Int3 a, Int3 b)
	{
		return !a.Equals(b);
	}

	public bool Equals(Int3 other)
	{
		if (x == other.x && y == other.y)
		{
			return z == other.z;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Int3 other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x ^ (y << 11) ^ (z << 22);
	}
}
