using UnityEngine;

public struct LineSegment
{
	public readonly Vector3 a;

	public readonly Vector3 b;

	public Vector3 midpoint => (a + b) * 0.5f;

	public Vector3 direction => directionVector.normalized;

	public Vector3 directionVector => b - a;

	public float length => directionVector.magnitude;

	public LineSegment(Vector3 a, Vector3 b)
	{
		this.a = a;
		this.b = b;
	}

	public float DistanceToPointSqrd(Vector3 point)
	{
		return MathUtil.DistanceToLineSegmentSqrd(a, b, point);
	}

	public Vector3 GetClosestPointTo(Vector3 point)
	{
		return MathUtil.FindPointOnLineSegmentClosestToPoint(a, b, point);
	}

	public float DistanceToLineSegment(LineSegment otherLineSegment)
	{
		Vector3 vector = directionVector;
		Vector3 vector2 = otherLineSegment.directionVector;
		Vector3 vector3 = a - otherLineSegment.a;
		float num = Vector3.Dot(vector, vector);
		float num2 = Vector3.Dot(vector, vector2);
		float num3 = Vector3.Dot(vector2, vector2);
		float num4 = Vector3.Dot(vector, vector3);
		float num5 = Vector3.Dot(vector2, vector3);
		float num7;
		float num6;
		float num8;
		float num9;
		if ((num7 = (num6 = num * num3 - num2 * num2)) < MathUtil.BigEpsilon)
		{
			num8 = 0f;
			num6 = 1f;
			num9 = num5;
			num7 = num3;
		}
		else
		{
			num8 = num2 * num5 - num3 * num4;
			num9 = num * num5 - num2 * num4;
			if ((double)num8 < 0.0)
			{
				num8 = 0f;
				num9 = num5;
				num7 = num3;
			}
			else if (num8 > num6)
			{
				num8 = num6;
				num9 = num5 + num2;
				num7 = num3;
			}
		}
		if (num9 < 0f)
		{
			num9 = 0f;
			if (0f - num4 < 0f)
			{
				num8 = 0f;
			}
			else if (0f - num4 > num)
			{
				num8 = num6;
			}
			else
			{
				num8 = 0f - num4;
				num6 = num;
			}
		}
		else if (num9 > num7)
		{
			num9 = num7;
			if (0f - num4 + num2 < 0f)
			{
				num8 = 0f;
			}
			else if (0f - num4 + num2 > num)
			{
				num8 = num6;
			}
			else
			{
				num8 = 0f - num4 + num2;
				num6 = num;
			}
		}
		float num10 = ((Mathf.Abs(num8) < MathUtil.BigEpsilon) ? 0f : (num8 / num6));
		float num11 = ((Mathf.Abs(num9) < MathUtil.BigEpsilon) ? 0f : (num9 / num7));
		return (vector3 + num10 * vector - num11 * vector2).magnitude;
	}

	public LineSegment Pad(float padding)
	{
		Vector3 vector = midpoint;
		Vector3 normalized = (a - vector).normalized;
		return new LineSegment(a + normalized * padding, b - normalized * padding);
	}

	public LineSegment Scale(float scale)
	{
		Vector3 vector = midpoint;
		return new LineSegment(vector + (a - vector) * scale, vector + (b - vector) * scale);
	}

	public Vector3 Project(Vector3 point)
	{
		return Vector3.Project(point - a, b - a) + a;
	}

	public Vector3 Rejection(Vector3 point)
	{
		return point - Project(point);
	}

	public LineSegment Mirror(Vector3 relativeToPoint, float shortSideScale = 1f)
	{
		Vector3 vector = Project(relativeToPoint);
		Vector3 vector2 = a - vector;
		Vector3 vector3 = b - vector;
		if (!(vector2.sqrMagnitude >= vector3.sqrMagnitude))
		{
			return new LineSegment(a.Lerp(vector - vector3, shortSideScale), vector + vector3);
		}
		return new LineSegment(vector + vector2, b.Lerp(vector - vector2, shortSideScale));
	}

	public bool ContainsProjection(Vector3 point)
	{
		Vector3 rhs = directionVector;
		float sqrMagnitude = rhs.sqrMagnitude;
		if (sqrMagnitude == 0f)
		{
			return false;
		}
		float num = Vector3.Dot(point - a, rhs) / sqrMagnitude;
		if (num >= 0f)
		{
			return num <= 1f;
		}
		return false;
	}

	public static LineSegment operator +(LineSegment lineSegment, Vector3 v)
	{
		return new LineSegment(lineSegment.a + v, lineSegment.b + v);
	}

	public static LineSegment operator -(LineSegment lineSegment, Vector3 v)
	{
		return new LineSegment(lineSegment.a - v, lineSegment.b - v);
	}

	public override string ToString()
	{
		return $"[{a}, {b}]";
	}
}
