using System;
using UnityEngine;

public struct NearFarPlanes
{
	public readonly Vector3 right;

	public readonly Vector3 up;

	public readonly Vector3 forward;

	public readonly float nearPlaneDistance;

	public readonly Vector2 nearPlaneExtents;

	public readonly float farPlaneDistance;

	public readonly Vector2 farPlaneExtents;

	private float? _randomPointCorrectionPower;

	public float volume => (nearPlaneArea + farPlaneArea) * 0.5f * (farPlaneDistance - nearPlaneDistance);

	public float nearPlaneArea => nearPlaneExtents.x * nearPlaneExtents.y;

	public float farPlaneArea => farPlaneExtents.x * farPlaneExtents.y;

	public float randomPointCorrectionPower => (_randomPointCorrectionPower ?? (_randomPointCorrectionPower = (nearPlaneArea + farPlaneArea) * 0.5f / farPlaneArea.InsureNonZero())).Value;

	private Vector3 _GetLerpedPointOnPlane(float distance, Vector2 extents, Vector2 lerp)
	{
		Vector3 vector = forward * distance;
		Vector2 vector2 = (-extents).Lerp(extents, lerp);
		return vector + vector2.x * right + vector2.y * up;
	}

	public NearFarPlanes(Vector3 right, Vector3 up, Vector3 forward, float nearPlaneDistance, Vector2 nearPlaneExtents, float farPlaneDistance, Vector2 farPlaneExtents)
	{
		this.right = right;
		this.up = up;
		this.forward = forward;
		this.nearPlaneDistance = nearPlaneDistance;
		this.nearPlaneExtents = nearPlaneExtents;
		this.farPlaneDistance = farPlaneDistance;
		this.farPlaneExtents = farPlaneExtents;
		_randomPointCorrectionPower = null;
	}

	public Vector3 GetPointOnNearPlane(Vector2 lerp)
	{
		return _GetLerpedPointOnPlane(nearPlaneDistance, nearPlaneExtents, lerp);
	}

	public Vector3 GetPointOnFarPlane(Vector2 lerp)
	{
		return _GetLerpedPointOnPlane(farPlaneDistance, farPlaneExtents, lerp);
	}

	public Vector3 GetRandomPoint(System.Random random)
	{
		Vector2 lerp = random.Value2();
		return GetPointOnNearPlane(lerp).Lerp(GetPointOnFarPlane(lerp), Mathf.Pow(random.Value(), randomPointCorrectionPower));
	}
}
