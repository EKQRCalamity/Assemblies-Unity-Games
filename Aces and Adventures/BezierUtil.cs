using System;
using System.Collections.Generic;
using UnityEngine;

public static class BezierUtil
{
	public static Vector3 GetPosition(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		if (MathUtil.PointsAreCollinear(p0, p1, p2))
		{
			return Vector3.Lerp(p0, p2, t);
		}
		float num = 1f - t;
		return num * num * p0 + 2f * num * t * p1 + t * t * p2;
	}

	public static Vector3 GetPosition(IList<Vector3> points, float t, int index = 0)
	{
		return GetPosition(points[index], points[index + 1], points[index + 2], t);
	}

	public static Vector3 GetVelocity(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		if (MathUtil.PointsAreCollinear(p0, p1, p2))
		{
			return p2 - p0;
		}
		return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
	}

	public static Vector3 GetVelocity(IList<Vector3> points, float t, int index = 0)
	{
		return GetVelocity(points[index], points[index + 1], points[index + 2], t);
	}

	public static float GetSpeed(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		return GetVelocity(p0, p1, p2, t).magnitude;
	}

	public static float GetSpeed(IList<Vector3> points, float t, int index = 0)
	{
		return GetSpeed(points[index], points[index + 1], points[index + 2], t);
	}

	public static Vector3 GetDirection(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		return GetVelocity(p0, p1, p2, t).normalized;
	}

	public static Vector3 GetDirection(IList<Vector3> points, float t, int index = 0)
	{
		return GetDirection(points[index], points[index + 1], points[index + 2], t);
	}

	public static Vector3 GetNormal(Vector3 p0, Vector3 p1, Vector3 p2, float t, Vector3 upVector)
	{
		return Vector3.Cross(GetDirection(p0, p1, p2, t), upVector).normalized;
	}

	public static Vector3 GetNormal(IList<Vector3> points, float t, Vector3 upVector, int index = 0)
	{
		return GetNormal(points[index], points[index + 1], points[index + 2], t, upVector);
	}

	public static Vector3 GetAcceleration(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		if (MathUtil.PointsAreCollinear(p0, p1, p2))
		{
			return Vector3.zero;
		}
		return 2f * (p2 - 2f * p1 + p0);
	}

	public static Vector3 GetAcceleration(IList<Vector3> points, int index = 0)
	{
		return GetAcceleration(points[index], points[index + 1], points[index + 2]);
	}

	public static float ArcLength(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		if (p0 == p2)
		{
			return float.Epsilon;
		}
		if (p0 == p1 || p1 == p2 || MathUtil.PointsAreCollinear(p0, p1, p2))
		{
			return (p2 - p0).magnitude;
		}
		Vector3 vector = default(Vector3);
		vector.x = p0.x - 2f * p1.x + p2.x;
		vector.y = p0.y - 2f * p1.y + p2.y;
		vector.z = p0.z - 2f * p1.z + p2.z;
		Vector3 vector2 = default(Vector3);
		vector2.x = 2f * p1.x - 2f * p0.x;
		vector2.y = 2f * p1.y - 2f * p0.y;
		vector2.z = 2f * p1.z - 2f * p0.z;
		float num = 4f * (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
		float num2 = 4f * (vector.x * vector2.x + vector.y * vector2.y + vector.z * vector2.z);
		float num3 = vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z;
		float num4 = 2f * Mathf.Sqrt(num + num2 + num3);
		float num5 = Mathf.Sqrt(num);
		float num6 = 2f * num * num5;
		float num7 = 2f * Mathf.Sqrt(num3);
		float num8 = num2 / num5;
		return (num6 * num4 + num5 * num2 * (num4 - num7) + (4f * num3 * num - num2 * num2) * Mathf.Log((2f * num5 + num8 + num4) / (num8 + num7))) / (4f * num6);
	}

	public static float ArcLength(IList<Vector3> points, int index = 0)
	{
		return ArcLength(points[index], points[index + 1], points[index + 2]);
	}

	public static float ArcLength(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		if (MathUtil.PointsAreCollinear(p0, p1, p2))
		{
			return t * (p2 - p0).magnitude;
		}
		Vector3 vector = p0 - (p1 + p1) + p2;
		Vector3 vector2 = p1 + p1 - (p0 + p0);
		float num = 4f * (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
		float num2 = 4f * (vector.x * vector2.x + vector.y * vector2.y + vector.z * vector2.z);
		float num3 = vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z;
		float num4 = num2 / (num + num);
		float num5 = num3 / num;
		float num6 = t + num4;
		float num7 = num4 * num4;
		float num8 = num5 - num7;
		double num9 = Math.Sqrt(num6 * num6 + num8);
		double num10 = Math.Sqrt(num7 + num8);
		return (float)(Math.Sqrt(num) * 0.5 * ((double)num6 * num9 - (double)num4 * num10 + (double)num8 * Math.Log(((double)num6 + num9) / ((double)num4 + num10))));
	}

	public static float ArcLength(IList<Vector3> points, float t, int index = 0)
	{
		return ArcLength(points[index], points[index + 1], points[index + 2], t);
	}

	public static float GetTNearestToPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 point)
	{
		if (MathUtil.PointsAreCollinear(p0, p1, p2))
		{
			return MathUtil.GetLerpAmount(p0, p2, point);
		}
		Vector3 lhs = p0 - point;
		Vector3 vector = p2 - point;
		Vector3 vector2 = p1 - p0;
		Vector3 rhs = p0 - 2f * p1 + p2;
		float sqrMagnitude = rhs.sqrMagnitude;
		float b = 3f * Vector3.Dot(vector2, rhs);
		float c = 2f * vector2.sqrMagnitude + Vector3.Dot(lhs, rhs);
		float d = Vector3.Dot(lhs, vector2);
		Vector4 vector3 = MathUtil.SolveCubicEquation(sqrMagnitude, b, c, d);
		float? num = null;
		float num2 = float.MaxValue;
		float sqrMagnitude2 = lhs.sqrMagnitude;
		float sqrMagnitude3 = vector.sqrMagnitude;
		if (vector3.w != 0f)
		{
			for (int i = 0; (float)i < vector3.w; i++)
			{
				float num3 = vector3[i];
				if (num3 >= 0f && num3 <= 1f)
				{
					Vector3 position = GetPosition(p0, p1, p2, num3);
					float sqrMagnitude4 = (point - position).sqrMagnitude;
					if (sqrMagnitude4 < num2)
					{
						num = num3;
						num2 = sqrMagnitude4;
					}
				}
			}
			if (num.HasValue && num2 < sqrMagnitude2 && num2 < sqrMagnitude3)
			{
				return num.Value;
			}
		}
		return (!(sqrMagnitude2 < sqrMagnitude3)) ? 1 : 0;
	}

	public static float GetTNearestToPoint(IList<Vector3> points, Vector3 point, int index = 0)
	{
		return GetTNearestToPoint(points[index], points[index + 1], points[index + 2], point);
	}

	public static Vector3 GetPointOnCurveNearestToPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 point)
	{
		return GetPosition(p0, p1, p2, GetTNearestToPoint(p0, p1, p2, point));
	}

	public static Vector3 GetPointOnCurveNearestToPoint(IList<Vector3> points, Vector3 point, int index = 0)
	{
		return GetPointOnCurveNearestToPoint(points[index], points[index + 1], points[index + 2], point);
	}

	public static Vector3 GetOnCurveExtrema(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		if (MathUtil.PointsAreCollinear(p0, p1, p2))
		{
			return p2;
		}
		Vector3 vector = (p0 - p1).Multiply((p0 - 2f * p1 + p2).Inverse()).Clamp();
		return new Vector3(GetPosition(p0, p1, p2, vector.x).x, GetPosition(p0, p1, p2, vector.y).y, GetPosition(p0, p1, p2, vector.z).z);
	}

	public static Vector3 GetOnCurveExtrema(IList<Vector3> points, int index = 0)
	{
		return GetOnCurveExtrema(points[index], points[index + 1], points[index + 2]);
	}

	private static Vector3 _GetLineSegmentIntersectQuadraticComponents(Vector3 p0, Vector3 p1, Vector3 p2, LineSegment lineSegment, Vector3 up)
	{
		Vector3 vector = -lineSegment.a;
		p0 += vector;
		p1 += vector;
		p2 += vector;
		lineSegment += vector;
		Plane plane = new Plane(Vector3.Cross(up, lineSegment.direction).normalized, lineSegment.midpoint);
		float num = Vector3.Dot(p0, plane.normal);
		float num2 = Vector3.Dot(p1, plane.normal);
		float num3 = Vector3.Dot(p2, plane.normal);
		return new Vector3(num - (num2 + num2) + num3, num2 + num2 - (num + num), num - plane.distance);
	}

	public static bool Intersects(Vector3 p0, Vector3 p1, Vector3 p2, LineSegment lineSegment, Vector3 up)
	{
		Vector3 vector = _GetLineSegmentIntersectQuadraticComponents(p0, p1, p2, lineSegment, up);
		float num = MathUtil.QuadraticNegative(vector.x, vector.y, vector.z);
		if (num >= 0f && num <= 1f && lineSegment.ContainsProjection(GetPosition(p0, p1, p2, num)))
		{
			return true;
		}
		float num2 = MathUtil.QuadraticPositive(vector.x, vector.y, vector.z);
		if (num2 >= 0f && num2 <= 1f)
		{
			return lineSegment.ContainsProjection(GetPosition(p0, p1, p2, num2));
		}
		return false;
	}

	public static void PathToQuadraticCurveData(List<Short2> path, List<Vector3> curveDataOutput, float yPosition = 0f)
	{
		if (path.Count < 2)
		{
			throw new ArgumentException("Path must have at least 2 points in order to create curve data.");
		}
		curveDataOutput?.Clear();
		curveDataOutput = curveDataOutput ?? new List<Vector3>();
		for (int i = 0; i < path.Count; i++)
		{
			Vector3 vector = path[i].ToVector3(yPosition);
			Vector3 vector2 = ((i != path.Count - 1) ? path[i + 1].ToVector3(yPosition) : vector);
			vector2 = (vector2 + vector) * 0.5f;
			if (i == 0)
			{
				curveDataOutput.Add(vector);
			}
			curveDataOutput.Add(vector);
			curveDataOutput.Add(vector2);
		}
	}
}
