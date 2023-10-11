using System;
using UnityEngine;

public struct TransformData : IEquatable<TransformData>
{
	public static readonly TransformData identity = new TransformData(Vector3.zero, Quaternion.identity, Vector3.one);

	public readonly Vector3 position;

	public readonly Quaternion rotation;

	public readonly Vector3 scale;

	public static TransformData FromMatrix(Matrix4x4 matrix)
	{
		return new TransformData(matrix.GetTranslation(), matrix.rotation, matrix.lossyScale);
	}

	public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
	{
		this.position = position;
		this.rotation = rotation;
		this.scale = scale;
	}

	public TransformData(Transform transform)
		: this(transform.position, transform.rotation, transform.GetWorldScale())
	{
	}

	public void SetValues(Transform transform)
	{
		transform.position = position;
		transform.rotation = rotation;
		transform.SetWorldScale(scale);
	}

	public TransformData Lerp(TransformData target, float t)
	{
		return new TransformData(Vector3.LerpUnclamped(position, target.position, t), Quaternion.SlerpUnclamped(rotation, target.rotation, t), Vector3.LerpUnclamped(scale, target.scale, t));
	}

	public TransformData Spring(TransformData target, ref TransformVelocities velocities, TransformSpringSettings settings, float deltaTime)
	{
		Vector3 vector = position;
		Quaternion current = rotation;
		Vector3 vector2 = scale;
		return new TransformData(MathUtil.Spring(ref vector, ref velocities.position, target.position, settings.positionConstant, settings.positionDampening, deltaTime), MathUtil.Spring(ref current, ref velocities.rotation, target.rotation, settings.rotationConstant, settings.rotationDampening, deltaTime), MathUtil.Spring(ref vector2, ref velocities.scale, target.scale, settings.scaleConstant, settings.scaleDampening, deltaTime));
	}

	public void Spring(Transform transform, ref TransformVelocities velocities, TransformSpringSettings settings, float deltaTime)
	{
		new TransformData(transform).Spring(this, ref velocities, settings, deltaTime).SetValues(transform);
	}

	public bool ApproximatelyEqual(TransformData other, float distanceThreshold = 0.01f, float degreeThreshold = 1f, float scaleThreshold = 0.01f, Vector3? perAxisDistanceThresholds = null, bool useScaleThreshold = true)
	{
		Vector3 vector = other.position - position;
		if (!perAxisDistanceThresholds.HasValue)
		{
			if (vector.sqrMagnitude > distanceThreshold * distanceThreshold)
			{
				return false;
			}
		}
		else
		{
			Vector3 value = perAxisDistanceThresholds.Value;
			Vector3 vector2 = rotation.Inverse() * vector;
			if (Math.Abs(vector2.x) > value.x || Math.Abs(vector2.y) > value.y || Math.Abs(vector2.z) > value.z)
			{
				return false;
			}
		}
		if (Quaternion.Angle(other.rotation, rotation) < degreeThreshold)
		{
			if (useScaleThreshold)
			{
				return (other.scale - scale).sqrMagnitude <= scaleThreshold * scaleThreshold;
			}
			return true;
		}
		return false;
	}

	public Matrix4x4 ToMatrix(bool includePosition = true, bool includeRotation = true, bool includeScale = true)
	{
		return Matrix4x4.TRS(includePosition ? position : Vector3.zero, includeRotation ? rotation : Quaternion.identity, includeScale ? scale : Vector3.one);
	}

	public TransformData Transform(Matrix4x4 transform)
	{
		return new TransformData(transform.MultiplyPoint(position), transform.rotation * rotation, transform.lossyScale.Multiply(scale));
	}

	public TransformData TransformTRS(Matrix4x4 transform)
	{
		return new TransformData(transform.GetTranslation() + position, rotation * transform.rotation, transform.lossyScale.Multiply(scale));
	}

	public TransformData Translate(Vector3 translation)
	{
		return new TransformData(position + translation, rotation, scale);
	}

	public TransformData Rotate(Quaternion rotate)
	{
		return new TransformData(position, rotation * rotate, scale);
	}

	public static implicit operator TransformData?(Transform transform)
	{
		if (!transform)
		{
			return null;
		}
		return new TransformData(transform);
	}

	public static bool operator ==(TransformData a, TransformData b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(TransformData a, TransformData b)
	{
		return !a.Equals(b);
	}

	public bool Equals(TransformData other)
	{
		if (position == other.position && rotation == other.rotation)
		{
			return scale == other.scale;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is TransformData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		Vector3 vector = position;
		return vector.GetHashCode();
	}

	public override string ToString()
	{
		return $"Position: {position}, Rotation: {rotation.eulerAngles}, Scale: {scale}";
	}
}
