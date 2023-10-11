using System;
using UnityEngine;

public struct PositionRotation : IEquatable<PositionRotation>
{
	public static readonly Comparison<PositionRotation> OriginDistanceComparer = (PositionRotation a, PositionRotation b) => MathUtil.Compare(a.position.sqrMagnitude, b.position.sqrMagnitude);

	public static readonly Comparison<PositionRotation> OriginDistanceComparerDescending = (PositionRotation a, PositionRotation b) => MathUtil.Compare(b.position.sqrMagnitude, a.position.sqrMagnitude);

	public readonly Vector3 position;

	public readonly Quaternion rotation;

	public Vector3 forward => rotation.Forward();

	public Vector3 right => rotation.Right();

	public Vector3 up => rotation.Up();

	public PositionRotation(Vector3 position, Quaternion rotation)
	{
		this.position = position;
		this.rotation = rotation;
	}

	public PositionRotation(Vector3 position)
		: this(position, Quaternion.identity)
	{
	}

	public PositionRotation(Transform transform)
		: this(transform.position, transform.rotation)
	{
	}

	public static PositionRotation operator *(PositionRotation a, PositionRotation b)
	{
		return new PositionRotation(a.position + a.rotation * b.position, a.rotation * b.rotation);
	}

	public static PositionRotation operator +(PositionRotation a, Vector3 b)
	{
		return new PositionRotation(a.position + b, a.rotation);
	}

	public static implicit operator PositionRotation(Transform transform)
	{
		return new PositionRotation(transform.position, transform.rotation);
	}

	public bool Equals(PositionRotation other)
	{
		if (position == other.position)
		{
			return rotation == other.rotation;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is PositionRotation)
		{
			return Equals((PositionRotation)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		Vector3 vector = position;
		int hashCode = vector.GetHashCode();
		Quaternion quaternion = rotation;
		return hashCode ^ quaternion.GetHashCode();
	}

	public static bool operator ==(PositionRotation a, PositionRotation b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(PositionRotation a, PositionRotation b)
	{
		return !a.Equals(b);
	}
}
