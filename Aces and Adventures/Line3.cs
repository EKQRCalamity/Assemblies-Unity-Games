using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public struct Line3 : IEquatable<Line3>
{
	public readonly Vector3 start;

	public readonly Vector3 end;

	public Line3(Vector3 start, Vector3 end)
	{
		this.start = start;
		this.end = end;
	}

	public static bool operator ==(Line3 a, Line3 b)
	{
		if (a.start == b.start)
		{
			return a.end == b.end;
		}
		return false;
	}

	public static bool operator !=(Line3 a, Line3 b)
	{
		if (!(a.start != b.start))
		{
			return a.end != b.end;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is Line3)
		{
			return this == (Line3)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		Vector3 vector = start;
		int hashCode = vector.GetHashCode();
		vector = end;
		return hashCode ^ (vector.GetHashCode() << 16);
	}

	public bool Equals(Line3 other)
	{
		if (start == other.start)
		{
			return end == other.end;
		}
		return false;
	}
}
