using System;
using UnityEngine;

public struct PositionDirection : IEquatable<PositionDirection>
{
	private static readonly float PositionEqualityThreshold = 1.0000001E-06f;

	private static readonly float DirectionEqualityThreshold = Mathf.Cos(0.0017453292f);

	public readonly Vector3 position;

	public readonly Vector3 direction;

	public PositionDirection(Vector3 position, Vector3 direction)
	{
		this.position = position;
		this.direction = direction;
	}

	public PositionDirection(Transform transform, AxisType axis, bool negateAxis)
		: this(transform.position, transform.GetAxis(axis) * negateAxis.ToInt(-1, 1))
	{
	}

	public static implicit operator Ray(PositionDirection positionDirection)
	{
		return new Ray(positionDirection.position, positionDirection.direction);
	}

	public static implicit operator PositionDirection(Ray ray)
	{
		return new PositionDirection(ray.origin, ray.direction);
	}

	public bool Equals(PositionDirection other)
	{
		if ((position - other.position).sqrMagnitude < PositionEqualityThreshold)
		{
			return Vector3.Dot(direction, other.direction) >= DirectionEqualityThreshold;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is PositionDirection)
		{
			return Equals((PositionDirection)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		Vector3 vector = position;
		int hashCode = vector.GetHashCode();
		vector = direction;
		return hashCode ^ vector.GetHashCode();
	}

	public static bool operator ==(PositionDirection a, PositionDirection b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(PositionDirection a, PositionDirection b)
	{
		return !a.Equals(b);
	}
}
