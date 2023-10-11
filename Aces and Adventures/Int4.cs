using System;

[Serializable]
public struct Int4 : IEquatable<Int4>
{
	public int x;

	public int y;

	public int z;

	public int w;

	public Int4(int x, int y, int z, int w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public bool Equals(Int4 other)
	{
		if (x == other.x && y == other.y && z == other.z)
		{
			return w == other.w;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Int4)
		{
			return Equals((Int4)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x ^ (y << 8) ^ (z << 16) ^ (w << 24);
	}
}
