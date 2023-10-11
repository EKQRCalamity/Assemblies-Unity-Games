using System.Collections.Generic;
using UnityEngine;

public struct BezierCurve
{
	public Vector3 p0;

	public Vector3 p1;

	public Vector3 p2;

	public BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		this.p0 = p0;
		this.p1 = p1;
		this.p2 = p2;
	}

	public BezierCurve(IList<Vector3> points, int startIndex)
		: this(points[startIndex], points[startIndex + 1], points[startIndex + 2])
	{
	}

	public Vector3 GetPosition(float t)
	{
		return BezierUtil.GetPosition(p0, p1, p2, t);
	}

	public Vector3 GetVelocity(float t)
	{
		return BezierUtil.GetVelocity(p0, p1, p2, t);
	}

	public float GetSpeed(float t)
	{
		return BezierUtil.GetSpeed(p0, p1, p2, t);
	}

	public Vector3 GetDirection(float t)
	{
		return BezierUtil.GetDirection(p0, p1, p2, t);
	}

	public Vector3 GetNormal(float t, Vector3 upVector)
	{
		return BezierUtil.GetNormal(p0, p1, p2, t, upVector);
	}

	public Vector3 GetAcceleration()
	{
		return BezierUtil.GetAcceleration(p0, p1, p2);
	}

	public float ArcLength()
	{
		return BezierUtil.ArcLength(p0, p1, p2);
	}

	public float ArcLength(float t)
	{
		return BezierUtil.ArcLength(p0, p1, p2, t);
	}

	public float GetTNearestToPoint(Vector3 point)
	{
		return BezierUtil.GetTNearestToPoint(p0, p1, p2, point);
	}

	public Vector3 GetPointOnCurveNearestToPoint(Vector3 point)
	{
		return BezierUtil.GetPointOnCurveNearestToPoint(p0, p1, p2, point);
	}

	public Vector3 GetOnCurveExtrema()
	{
		return BezierUtil.GetOnCurveExtrema(p0, p1, p2);
	}

	public bool Intersects(LineSegment lineSegment, Vector3 up)
	{
		return BezierUtil.Intersects(p0, p1, p2, lineSegment, up);
	}

	public static BezierCurve operator *(Matrix4x4 matrix, BezierCurve curve)
	{
		return new BezierCurve(matrix.MultiplyPoint(curve.p0), matrix.MultiplyPoint(curve.p1), matrix.MultiplyPoint(curve.p2));
	}
}
