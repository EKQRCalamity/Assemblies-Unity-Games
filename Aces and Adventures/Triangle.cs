using System;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle : IEquatable<Triangle>
{
	public readonly Vector3 a;

	public readonly Vector3 b;

	public readonly Vector3 c;

	public Vector3 normal => normalVector.normalized;

	public Vector3 normalVector => Vector3.Cross(b - a, c - b);

	public Vector3 center => (a + b + c) * (1f / 3f);

	public float surfaceArea => Vector3.Cross(b - a, c - a).magnitude * 0.5f;

	public Vector3 this[int index]
	{
		get
		{
			index %= 3;
			return index switch
			{
				0 => a, 
				1 => b, 
				2 => c, 
				_ => a, 
			};
		}
	}

	public static Triangle CreateClockwiseFromPoints(Vector3 a, Vector3 b, Vector3 c)
	{
		using PoolStructListHandle<Vector3> poolStructListHandle = Pools.UseStructList<Vector3>();
		using PoolHandle<ClockwiseComparerVector3> poolHandle = ClockwiseComparerVector3.Use(new Triangle(a, b, c).center);
		poolStructListHandle.Add(a).Add(b).Add(c);
		poolStructListHandle.value.StableSort(poolHandle.value);
		return new Triangle(poolStructListHandle[0], poolStructListHandle[1], poolStructListHandle[2]);
	}

	public static Triangle CreateCCWFromPoints(Vector3 a, Vector3 b, Vector3 c)
	{
		using PoolStructListHandle<Vector3> poolStructListHandle = Pools.UseStructList<Vector3>();
		using PoolHandle<ClockwiseComparerVector3> poolHandle = ClockwiseComparerVector3.Use(new Triangle(a, b, c).center);
		poolStructListHandle.Add(a).Add(b).Add(c);
		poolStructListHandle.value.StableSort(poolHandle.value);
		return new Triangle(poolStructListHandle[2], poolStructListHandle[1], poolStructListHandle[0]);
	}

	public Triangle(Vector3 a, Vector3 b, Vector3 c)
	{
		this.a = a;
		this.b = b;
		this.c = c;
	}

	public Triangle(List<Vector3> vertices, List<int> triangleIndices, int triangleStartIndex)
		: this(vertices[triangleIndices[triangleStartIndex]], vertices[triangleIndices[triangleStartIndex + 1]], vertices[triangleIndices[triangleStartIndex + 2]])
	{
	}

	public Triangle(Rect3D rect3D)
	{
		a = rect3D.bottomLeft;
		b = rect3D.topLeft;
		c = rect3D.topRight;
	}

	public Rect3D ToRect3D()
	{
		Triangle triangle = ToCCWTriangle();
		int startVertexIndex;
		Vector3 onNormal = triangle.LongestSide(out startVertexIndex);
		Vector3 vector = triangle[startVertexIndex + 1];
		return new Rect3D(triangle[startVertexIndex], vector, vector + (triangle[startVertexIndex + 2] - vector).Rejection(onNormal));
	}

	public Triangle ToContainingRightTriangle()
	{
		Vector3 vector = c - a;
		Vector3 vector2 = a - b;
		return new Triangle((Vector3.Dot(vector, vector2) >= 0f) ? (a + Vector3.Project(vector, vector2)) : (a + Vector3.Reflect(-vector, (c - b).normalized)), b, c);
	}

	public Triangle ToClockwiseTriangle()
	{
		return CreateClockwiseFromPoints(a, b, c);
	}

	public Triangle ToCCWTriangle()
	{
		return CreateCCWFromPoints(a, b, c);
	}

	public Triangle ViewportToWorldTriangle(Camera camera)
	{
		return new Triangle(camera.ViewportToWorldPoint(a), camera.ViewportToWorldPoint(b), camera.ViewportToWorldPoint(c));
	}

	public Triangle ReverseWinding()
	{
		return new Triangle(c, b, a);
	}

	public Triangle Transform(Matrix4x4 matrix)
	{
		return new Triangle(matrix.MultiplyPoint(a), matrix.MultiplyPoint(b), matrix.MultiplyPoint(c));
	}

	public Triangle Translate(Vector3 translation)
	{
		return new Triangle(a + translation, b + translation, c + translation);
	}

	public Triangle Scale(Vector3 scale)
	{
		return Scale(scale, center);
	}

	public Triangle Scale(Vector3 scale, Vector3 relativeToPoint)
	{
		return new Triangle(relativeToPoint + (a - relativeToPoint).Multiply(scale), relativeToPoint + (b - relativeToPoint).Multiply(scale), relativeToPoint + (c - relativeToPoint).Multiply(scale));
	}

	public Triangle Pad(float padding)
	{
		return Pad(padding, center);
	}

	public Triangle Pad(float padding, Vector3 from)
	{
		return new Triangle(a + (a - from).normalized * padding, b + (b - from).normalized * padding, c + (c - from).normalized * padding);
	}

	public Vector3 Barycentric(Vector3 point)
	{
		Vector3 vector = b - a;
		Vector3 vector2 = c - a;
		Vector3 lhs = point - a;
		float num = Vector3.Dot(vector, vector);
		float num2 = Vector3.Dot(vector, vector2);
		float num3 = Vector3.Dot(vector2, vector2);
		float num4 = Vector3.Dot(lhs, vector);
		float num5 = Vector3.Dot(lhs, vector2);
		float num6 = 1f / (num * num3 - num2 * num2).InsureNonZero();
		float num7 = (num3 * num4 - num2 * num5) * num6;
		float num8 = (num * num5 - num2 * num4) * num6;
		return new Vector3(1f - num7 - num8, num7, num8);
	}

	public bool Contains(Vector3 point)
	{
		Vector3 vector = Barycentric(point);
		if (vector.x >= 0f && vector.y >= 0f)
		{
			return vector.x + vector.y <= 1f;
		}
		return false;
	}

	public Vector3 LongestSide(out int startVertexIndex)
	{
		startVertexIndex = 0;
		Vector3 result = default(Vector3);
		float num = float.MinValue;
		for (int i = 0; i < 3; i++)
		{
			Vector3 vector = this[i + 1] - this[i];
			float sqrMagnitude = vector.sqrMagnitude;
			if (!(sqrMagnitude < num))
			{
				num = sqrMagnitude;
				result = vector;
				startVertexIndex = i;
			}
		}
		return result;
	}

	public LineSegment GetSide(int sideIndex)
	{
		return new LineSegment(this[sideIndex], this[sideIndex + 1]);
	}

	public Vector3 GetDirectionFromCenterToFurthestVertex()
	{
		Vector3 vector = center;
		Vector3 vector2 = a - vector;
		Vector3 vector3 = b - vector;
		Vector3 vector4 = c - vector;
		float sqrMagnitude = vector2.sqrMagnitude;
		float sqrMagnitude2 = vector3.sqrMagnitude;
		float sqrMagnitude3 = vector4.sqrMagnitude;
		if (!(sqrMagnitude > sqrMagnitude2) || !(sqrMagnitude > sqrMagnitude3))
		{
			if (!(sqrMagnitude2 > sqrMagnitude) || !(sqrMagnitude2 > sqrMagnitude3))
			{
				return vector4 / Mathf.Sqrt(sqrMagnitude3);
			}
			return vector3 / Mathf.Sqrt(sqrMagnitude2);
		}
		return vector2 / Mathf.Sqrt(sqrMagnitude);
	}

	public IEnumerable<Vector3> EnumerateVertices()
	{
		yield return a;
		yield return b;
		yield return c;
	}

	public static bool operator ==(Triangle a, Triangle b)
	{
		if (a.a == b.a && a.b == b.b)
		{
			return a.c == b.c;
		}
		return false;
	}

	public static bool operator !=(Triangle a, Triangle b)
	{
		return !(a == b);
	}

	public bool Equals(Triangle other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj is Triangle)
		{
			return Equals((Triangle)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		Vector3 vector = a;
		int hashCode = vector.GetHashCode();
		vector = b;
		int num = hashCode ^ vector.GetHashCode();
		vector = c;
		return num ^ vector.GetHashCode();
	}
}
