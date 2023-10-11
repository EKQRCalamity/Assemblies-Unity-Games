using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public struct PositionAngle : IEquatable<PositionAngle>
{
	[ProtoMember(1, IsRequired = true)]
	public readonly Vector2 position;

	[ProtoMember(2, IsRequired = true)]
	public readonly float angle;

	[ProtoMember(3)]
	public readonly bool flipOrthogonalAxis;

	public Vector2 direction => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

	public PositionAngle(Vector2 position, float angle, bool flipOrthogonalAxis)
	{
		this.position = position;
		this.angle = angle;
		this.flipOrthogonalAxis = flipOrthogonalAxis;
	}

	public Quaternion GetRelativeRotation(PositionAngle relativeTo, float initialRotationDegrees)
	{
		Quaternion quaternion = Quaternion.Euler(-90f, 0f, 90f);
		quaternion = Quaternion.AngleAxis((angle - (relativeTo.angle - (initialRotationDegrees - 90f) * (MathF.PI / 180f))) * -57.29578f, quaternion.Right()) * quaternion;
		if (!flipOrthogonalAxis)
		{
			quaternion = Quaternion.AngleAxis(180f, quaternion.Forward()) * quaternion;
		}
		return quaternion;
	}

	public static bool operator ==(PositionAngle a, PositionAngle b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(PositionAngle a, PositionAngle b)
	{
		return !a.Equals(b);
	}

	public static implicit operator Vector2(PositionAngle a)
	{
		return a.position;
	}

	public static implicit operator float(PositionAngle a)
	{
		return a.angle;
	}

	public override int GetHashCode()
	{
		Vector2 vector = position;
		int hashCode = vector.GetHashCode();
		float num = angle;
		return hashCode ^ num.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is PositionAngle)
		{
			return Equals((PositionAngle)obj);
		}
		return false;
	}

	public bool Equals(PositionAngle other)
	{
		if (flipOrthogonalAxis == other.flipOrthogonalAxis && angle == other.angle)
		{
			return position == other.position;
		}
		return false;
	}
}
