using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Rect3D : IEquatable<Rect3D>
{
	private const float MIN_SIZE = 0.0001f;

	private static readonly Vector2 MIN_SIZE_VECTOR = new Vector2(0.0001f, 0.0001f);

	public readonly Vector3 bottomLeft;

	public readonly Vector3 topLeft;

	public readonly Vector3 topRight;

	public Vector3 center => new Vector3((bottomLeft.x + topRight.x) * 0.5f, (bottomLeft.y + topRight.y) * 0.5f, (bottomLeft.z + topRight.z) * 0.5f);

	public Vector3 normal => Vector3.Cross((topLeft - bottomLeft) * 100f, (topRight - topLeft) * 100f).normalized;

	public Vector3 bottomRight => bottomLeft + (topRight - topLeft);

	public Vector3 right => rightVector.normalized;

	public Vector3 up => upVector.normalized;

	public Vector3 forward => normal;

	public Vector3 rightVector => topRight - topLeft;

	public Vector3 upVector => topLeft - bottomLeft;

	public Quaternion rotation => Quaternion.LookRotation(forward, up);

	public Quaternion uiRotation => Quaternion.identity.LookRotationSafe(-forward, up);

	public float width => (topRight - topLeft).magnitude;

	public float height => (topLeft - bottomLeft).magnitude;

	public Vector2 size => new Vector2(width, height);

	public Vector3 min => Vector3.Min(Vector3.Min(Vector3.Min(bottomLeft, topLeft), topRight), bottomRight);

	public Vector3 max => Vector3.Max(Vector3.Max(Vector3.Max(bottomLeft, topLeft), topRight), bottomRight);

	public LineSegment bottemEdge => new LineSegment(bottomRight, bottomLeft);

	public LineSegment leftEdge => new LineSegment(bottomLeft, topLeft);

	public LineSegment topEdge => new LineSegment(topLeft, topRight);

	public LineSegment rightEdge => new LineSegment(topRight, bottomRight);

	public LineSegment this[RectTransform.Edge edge] => edge switch
	{
		RectTransform.Edge.Left => leftEdge, 
		RectTransform.Edge.Right => rightEdge, 
		RectTransform.Edge.Top => topEdge, 
		RectTransform.Edge.Bottom => bottemEdge, 
		_ => throw new ArgumentOutOfRangeException("edge", edge, null), 
	};

	public static Rect3D GetViewportRect3D(RectTransform rectTransform, Camera camera)
	{
		return new Rect3D(rectTransform).WorldToViewportRect(camera);
	}

	public static Rect GetViewportRect(RectTransform rectTransform, Camera camera)
	{
		return GetViewportRect3D(rectTransform, camera).Project2D();
	}

	public Rect3D(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight)
	{
		this.bottomLeft = bottomLeft;
		this.topLeft = topLeft;
		this.topRight = topRight;
	}

	public Rect3D(List<Vector3> cornersStartingAtBottomLeftClockwise)
	{
		bottomLeft = cornersStartingAtBottomLeftClockwise[0];
		topLeft = cornersStartingAtBottomLeftClockwise[1];
		topRight = cornersStartingAtBottomLeftClockwise[2];
	}

	public Rect3D(Triangle triangle)
	{
		bottomLeft = triangle.a;
		topLeft = triangle.b;
		topRight = triangle.c;
	}

	public Rect3D(Rect rect, float z = 0f)
		: this(rect.min.Unproject(AxisType.Z, z), new Vector3(rect.min.x, rect.max.y, z), rect.max.Unproject(AxisType.Z, z))
	{
	}

	public Rect3D(RectTransform rectTransform)
	{
		using PoolStructArrayHandle<Vector3> poolStructArrayHandle = Pools.UseArray<Vector3>(4);
		rectTransform.GetWorldCorners(poolStructArrayHandle);
		bottomLeft = poolStructArrayHandle[0];
		topLeft = poolStructArrayHandle[1];
		topRight = poolStructArrayHandle[2];
	}

	public Rect3D(Transform transform, Vector2 size)
		: this(transform.position, -transform.forward, transform.up, size)
	{
	}

	public Rect3D(Vector3 normalDirection, Vector3 upDirection, Vector3 pointA, Vector3 pointB, float minMagnitude = 1E-05f)
	{
		Vector3 vector = (pointA + pointB) * 0.5f;
		Vector3 v = pointB - pointA;
		Vector3 vector2 = v.AbsProject(upDirection, minMagnitude) * 0.5f;
		Vector3 vector3 = v.AbsProject(Vector3.Cross(normalDirection, upDirection).normalized, minMagnitude) * 0.5f;
		bottomLeft = vector - vector3 - vector2;
		topLeft = vector - vector3 + vector2;
		topRight = vector + vector3 + vector2;
	}

	public Rect3D(Vector3 center, Vector3 normalDirection, Vector3 upDirection, Vector2 size)
	{
		Vector2 vector = Vector2.Max(size * 0.5f, MIN_SIZE_VECTOR);
		Vector3 normalized = Vector3.Cross(normalDirection, upDirection).normalized;
		bottomLeft = center - normalized * vector.x - upDirection * vector.y;
		topLeft = center - normalized * vector.x + upDirection * vector.y;
		topRight = center + normalized * vector.x + upDirection * vector.y;
	}

	public Rect3D(Vector3 start, Vector3 end, Vector3 normal, float width)
	{
		Vector3 vector = Vector3.Cross(end - start, normal).normalized * (width * 0.5f);
		bottomLeft = start - vector;
		topLeft = start + vector;
		topRight = end + vector;
	}

	public PoolStructListHandle<Vector3> Corners()
	{
		PoolStructListHandle<Vector3> poolStructListHandle = Pools.UseStructList<Vector3>();
		poolStructListHandle.Add(bottomLeft);
		poolStructListHandle.Add(topLeft);
		poolStructListHandle.Add(topRight);
		poolStructListHandle.Add(bottomRight);
		return poolStructListHandle;
	}

	public Rect3D WorldToViewportRect(Camera camera)
	{
		return new Rect3D(camera.WorldToViewportPoint(bottomLeft), camera.WorldToViewportPoint(topLeft), camera.WorldToViewportPoint(topRight));
	}

	public Rect3D WorldToScreenSpaceRect(Camera camera)
	{
		return new Rect3D(camera.WorldToScreenPoint(bottomLeft), camera.WorldToScreenPoint(topLeft), camera.WorldToScreenPoint(topRight));
	}

	public Rect3D ViewportToWorldRect(Camera camera)
	{
		return new Rect3D(camera.ViewportToWorldPoint(bottomLeft), camera.ViewportToWorldPoint(topLeft), camera.ViewportToWorldPoint(topRight));
	}

	public Rect3D Project(AxisType axisToRemove = AxisType.Z)
	{
		return new Rect3D(bottomLeft.Project(axisToRemove), topLeft.Project(axisToRemove), topRight.Project(axisToRemove));
	}

	public Rect Project2D(AxisType axisToRemove = AxisType.Z)
	{
		Rect result = default(Rect);
		result.min = min.Project(axisToRemove);
		result.max = max.Project(axisToRemove);
		return result;
	}

	public Vector3 Lerp(Vector2 lerp)
	{
		return bottomLeft + (topRight - topLeft) * lerp.x + (topLeft - bottomLeft) * lerp.y;
	}

	public Rect3D Lerp(Rect3D rect, float t)
	{
		return new Rect3D(bottomLeft.Lerp(rect.bottomLeft, t), topLeft.Lerp(rect.topLeft, t), topRight.Lerp(rect.topRight, t));
	}

	public Vector2 GetLerpAmount(Vector3 point)
	{
		Couple<float?, float?> couple = _GetUVCoordinates(point);
		return new Vector2(couple.a.GetValueOrDefault(), couple.b.GetValueOrDefault());
	}

	public Rect3D Translate(Vector3 translation)
	{
		return new Rect3D(bottomLeft + translation, topLeft + translation, topRight + translation);
	}

	public Rect3D FitIntoRange(Vector3 minRange, Vector3 maxRange)
	{
		return Translate(Vector3.Max(Vector3.zero, minRange - min) + Vector3.Min(Vector3.zero, maxRange - max));
	}

	public Rect3D FitIntoView(Rect3D view)
	{
		Rect3D rect3D = this;
		Vector3 lhs = view.center - rect3D.center;
		Vector3 vector = view.right;
		Vector3 vector2 = view.up;
		float num = Vector3.Dot(lhs, vector);
		float num2 = Vector3.Dot(lhs, vector2);
		Vector2 vector3 = (rect3D.size - Vector2.Min(rect3D.size, view.size)) * 0.5f;
		Vector3 zero = Vector3.zero;
		if (num > vector3.x)
		{
			zero += vector * (num - vector3.x);
		}
		else if (num < 0f - vector3.x)
		{
			zero += vector * (num + vector3.x);
		}
		if (num2 > vector3.y)
		{
			zero += vector2 * (num2 - vector3.y);
		}
		else if (num2 < 0f - vector3.y)
		{
			zero += vector2 * (num2 + vector3.y);
		}
		return rect3D.Translate(zero);
	}

	public Rect3D Pad(Vector2 sizePadding)
	{
		return new Rect3D(center, normal, up, size + sizePadding);
	}

	public Rect3D Scale(Vector2 scale)
	{
		return new Rect3D(center, normal, up, size.Multiply(scale));
	}

	public Rect3D SetSize(Vector2 sizeToSet)
	{
		return new Rect3D(center, normal, up, sizeToSet);
	}

	public Plane ToPlane()
	{
		return new Plane(bottomLeft, topLeft, topRight);
	}

	public Vector3 GetPointOnPlaneClosestTo(Vector3 point)
	{
		return ToPlane().ClosestPointOnPlane(point);
	}

	public Triangle ToTriangle()
	{
		return new Triangle(this);
	}

	public PoolStructListHandle<LineSegment> GetEdges()
	{
		return Pools.UseStructList<LineSegment>().Add(bottemEdge).Add(leftEdge)
			.Add(topEdge)
			.Add(rightEdge);
	}

	public LineSegment GetEdgeClosestTo(Vector3 point, out RectTransform.Edge edge)
	{
		edge = RectTransform.Edge.Bottom;
		LineSegment closestEdge = default(LineSegment);
		float minDistance = float.MaxValue;
		_GetEdgeClosestTo(point, bottemEdge, RectTransform.Edge.Bottom, ref minDistance, ref closestEdge, ref edge);
		_GetEdgeClosestTo(point, leftEdge, RectTransform.Edge.Left, ref minDistance, ref closestEdge, ref edge);
		_GetEdgeClosestTo(point, topEdge, RectTransform.Edge.Top, ref minDistance, ref closestEdge, ref edge);
		_GetEdgeClosestTo(point, rightEdge, RectTransform.Edge.Right, ref minDistance, ref closestEdge, ref edge);
		return closestEdge;
	}

	private void _GetEdgeClosestTo(Vector3 point, LineSegment edge, RectTransform.Edge edgeType, ref float minDistance, ref LineSegment closestEdge, ref RectTransform.Edge closestEdgeType)
	{
		float num = edge.DistanceToPointSqrd(point);
		if (!(num > minDistance) && (num != minDistance || !(edge.Rejection(point).sqrMagnitude < closestEdge.Rejection(point).sqrMagnitude)))
		{
			minDistance = num;
			closestEdge = edge;
			closestEdgeType = edgeType;
		}
	}

	private Couple<float?, float?> _GetUVCoordinates(Vector3 point)
	{
		Vector3 rhs = rightVector;
		float sqrMagnitude = rhs.sqrMagnitude;
		Vector3 rhs2 = upVector;
		float sqrMagnitude2 = rhs2.sqrMagnitude;
		Vector3 lhs = point - bottomLeft;
		return new Couple<float?, float?>((sqrMagnitude > 0f) ? new float?(Vector3.Dot(lhs, rhs) / sqrMagnitude) : null, (sqrMagnitude2 > 0f) ? new float?(Vector3.Dot(lhs, rhs2) / sqrMagnitude2) : null);
	}

	public bool ContainsProjection(Vector3 point)
	{
		Couple<float?, float?> couple = _GetUVCoordinates(point);
		if (couple.a >= 0f && couple.a <= 1f && couple.b >= 0f)
		{
			return couple.b <= 1f;
		}
		return false;
	}

	public Vector3? GetPointNearestTo(Vector3 point)
	{
		Couple<float?, float?> couple = _GetUVCoordinates(point);
		return Lerp(new Vector2(Mathf.Clamp01(couple.a.GetValueOrDefault()), Mathf.Clamp01(couple.b.GetValueOrDefault())));
	}

	public bool Intersects(BezierSpline spline)
	{
		Vector3 vector = normal;
		using (PoolStructListHandle<BezierCurve> poolStructListHandle = spline.GetCurves())
		{
			foreach (BezierCurve item in poolStructListHandle.value)
			{
				if (ContainsProjection(item.p0) || ContainsProjection(item.p2) || ContainsProjection(item.GetPosition(0.5f)))
				{
					return true;
				}
			}
			using PoolStructListHandle<LineSegment> poolStructListHandle2 = GetEdges();
			foreach (BezierCurve item2 in poolStructListHandle.value)
			{
				foreach (LineSegment item3 in poolStructListHandle2.value)
				{
					if (item2.Intersects(item3, vector))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public Vector3? Raycast(Ray ray)
	{
		Vector3? result = ToPlane().Raycast(ray);
		if (!result.HasValue || !ContainsProjection(result.Value))
		{
			return null;
		}
		return result;
	}

	public Rect3D Encapsulate(Vector3 point)
	{
		Vector3 vector = bottomLeft;
		Vector3 vector2 = topLeft;
		Vector3 vector3 = topRight;
		Vector2 lerpAmount = GetLerpAmount(point);
		if (lerpAmount.x < 0f)
		{
			Vector3 vector4 = rightVector * lerpAmount.x;
			vector += vector4;
			vector2 += vector4;
		}
		else if (lerpAmount.x > 1f)
		{
			vector3 += rightVector * (lerpAmount.x - 1f);
		}
		if (lerpAmount.y < 0f)
		{
			vector += upVector * lerpAmount.y;
		}
		else if (lerpAmount.y > 1f)
		{
			Vector3 vector5 = upVector * (lerpAmount.y - 1f);
			vector2 += vector5;
			vector3 += vector5;
		}
		return new Rect3D(vector, vector2, vector3);
	}

	public Rect3D Encapsulate(Rect3D rect3D)
	{
		return Encapsulate(rect3D.bottomLeft).Encapsulate(rect3D.topLeft).Encapsulate(rect3D.topRight).Encapsulate(rect3D.bottomRight);
	}

	public UVCoords GetUVCoords(Rect3D rect3D)
	{
		Vector2 rhs = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 rhs2 = new Vector2(float.MinValue, float.MinValue);
		foreach (Vector3 item in rect3D.Corners())
		{
			Vector2 lerpAmount = GetLerpAmount(item);
			rhs = Vector2.Min(lerpAmount, rhs);
			rhs2 = Vector2.Max(lerpAmount, rhs2);
		}
		return new UVCoords(rhs, rhs2);
	}

	public Rect3D GetUVReferenceRect(UVCoords uvCoords)
	{
		Vector2 vector = size.Multiply((uvCoords.max - uvCoords.min).Inverse().InsureNonZero());
		Vector3 vector2 = right * vector.x;
		Vector3 vector3 = up;
		Vector3 vector4 = vector3 * vector.y;
		Vector3 vector5 = topRight + vector2 * (1f - uvCoords.max.x) + vector4 * (1f - uvCoords.max.y);
		Vector3 vector6 = bottomLeft - vector2 * uvCoords.min.x - vector4 * uvCoords.min.y;
		return new Rect3D(vector6, vector6 + Vector3.Project(vector5 - vector6, vector3), vector5);
	}

	public Rect3D GetRelativeRect3D(Vector2 relativeCenter, Vector2 relativeSize)
	{
		return new Rect3D(Lerp(relativeCenter), normal, up, size.Multiply(relativeSize));
	}

	public Rect3D GetSection(UVCoords section)
	{
		return new Rect3D(Lerp(new Vector2(section.min.x, section.min.y)), Lerp(new Vector2(section.min.x, section.max.y)), Lerp(new Vector2(section.max.x, section.max.y)));
	}

	public Rect3D SubtractSizeAlongVector(Vector3 v)
	{
		Vector3 vector = up;
		Vector3 vector2 = right;
		float num = Vector3.Dot(vector, v);
		Vector3 vector3 = num * vector;
		float num2 = Vector3.Dot(vector2, v);
		Vector3 vector4 = num2 * vector2;
		Vector3 vector5 = bottomLeft;
		Vector3 vector6 = topLeft;
		Vector3 vector7 = topRight;
		if (num > 0f)
		{
			vector5 += vector3;
		}
		else if (num < 0f)
		{
			vector6 += vector3;
			vector7 += vector3;
		}
		if (num2 < 0f)
		{
			vector7 += vector4;
		}
		else if (num2 > 0f)
		{
			vector5 += vector4;
			vector6 += vector4;
		}
		return new Rect3D(vector5, vector6, vector7);
	}

	public Rect3D AddSizeAlongVector(Vector3 v)
	{
		Vector3 vector = up;
		Vector3 vector2 = right;
		float num = Vector3.Dot(vector, v);
		Vector3 vector3 = num * vector;
		float num2 = Vector3.Dot(vector2, v);
		Vector3 vector4 = num2 * vector2;
		Vector3 vector5 = bottomLeft;
		Vector3 vector6 = topLeft;
		Vector3 vector7 = topRight;
		if (num < 0f)
		{
			vector5 += vector3;
		}
		else if (num > 0f)
		{
			vector6 += vector3;
			vector7 += vector3;
		}
		if (num2 > 0f)
		{
			vector7 += vector4;
		}
		else if (num2 < 0f)
		{
			vector5 += vector4;
			vector6 += vector4;
		}
		return new Rect3D(vector5, vector6, vector7);
	}

	public Rect3D Transform(Matrix4x4 transform)
	{
		return transform * this;
	}

	public Rect3D CreateCoplanarRectFromPoints(IEnumerable<Vector3> points)
	{
		using PoolKeepItemListHandle<Vector3> poolKeepItemListHandle = Pools.UseKeepItemList(points);
		if (poolKeepItemListHandle.Count < 2)
		{
			return this;
		}
		Rect3D result = new Rect3D(normal, up, poolKeepItemListHandle[0], poolKeepItemListHandle[1]);
		for (int i = 2; i < poolKeepItemListHandle.Count; i++)
		{
			result = result.Encapsulate(poolKeepItemListHandle[i]);
		}
		return result;
	}

	public static bool operator ==(Rect3D a, Rect3D b)
	{
		if (a.bottomLeft == b.bottomLeft && a.topLeft == b.topLeft)
		{
			return a.topRight == b.topRight;
		}
		return false;
	}

	public static bool operator !=(Rect3D a, Rect3D b)
	{
		return !(a == b);
	}

	public static implicit operator Triangle(Rect3D rect3D)
	{
		return rect3D.ToTriangle();
	}

	public static Rect3D operator *(Matrix4x4 matrix, Rect3D rect3D)
	{
		return new Rect3D(matrix.MultiplyPoint(rect3D.bottomLeft), matrix.MultiplyPoint(rect3D.topLeft), matrix.MultiplyPoint(rect3D.topRight));
	}

	public bool Equals(Rect3D other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj is Rect3D)
		{
			return Equals((Rect3D)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		Vector3 vector = bottomLeft;
		int hashCode = vector.GetHashCode();
		vector = topLeft;
		int num = hashCode ^ vector.GetHashCode();
		vector = topRight;
		return num ^ vector.GetHashCode();
	}

	public override string ToString()
	{
		return $"Center: {center}, Size: {size}, Normal: {normal}";
	}
}
