using System;

public struct PositionOrientPair : IEquatable<PositionOrientPair>
{
	public readonly Short2 position;

	public readonly Orient8 orient;

	public PositionOrientPair(Short2 position, Orient8 orient)
	{
		this.position = position;
		this.orient = orient;
	}

	public static bool operator ==(PositionOrientPair a, PositionOrientPair b)
	{
		if (a.position == b.position)
		{
			return a.orient == b.orient;
		}
		return false;
	}

	public static bool operator !=(PositionOrientPair a, PositionOrientPair b)
	{
		if (a.position == b.position)
		{
			return a.orient != b.orient;
		}
		return true;
	}

	public static implicit operator Short2(PositionOrientPair p)
	{
		return p.position;
	}

	public static implicit operator Orient8(PositionOrientPair p)
	{
		return p.orient;
	}

	public static implicit operator Orients8(PositionOrientPair p)
	{
		return p.orient.ToOrients8();
	}

	public override bool Equals(object obj)
	{
		if (obj is PositionOrientPair positionOrientPair)
		{
			return positionOrientPair.Equals(this);
		}
		return false;
	}

	public override int GetHashCode()
	{
		Short2 @short = position;
		return (@short.GetHashCode() & BitMask.First24Bits) ^ (int)((uint)orient << 24);
	}

	public override string ToString()
	{
		Short2 @short = position;
		return @short.ToString() + ", " + orient;
	}

	public bool Equals(PositionOrientPair other)
	{
		if (position == other.position)
		{
			return orient == other.orient;
		}
		return false;
	}
}
