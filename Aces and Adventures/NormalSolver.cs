using System;
using System.Collections.Generic;
using UnityEngine;

public static class NormalSolver
{
	private struct VertexKey
	{
		private readonly long _x;

		private readonly long _y;

		private readonly long _z;

		private const int Tolerance = 100000;

		public VertexKey(Vector3 position)
		{
			_x = (long)Mathf.Round(position.x * 100000f);
			_y = (long)Mathf.Round(position.y * 100000f);
			_z = (long)Mathf.Round(position.z * 100000f);
		}

		public override bool Equals(object obj)
		{
			VertexKey vertexKey = (VertexKey)obj;
			if (_x == vertexKey._x && _y == vertexKey._y)
			{
				return _z == vertexKey._z;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((_x * 7) ^ (_y * 13) ^ (_z * 27)).GetHashCode();
		}
	}

	private sealed class VertexEntry
	{
		public int[] TriangleIndex = new int[4];

		public int[] VertexIndex = new int[4];

		private int _reserved = 4;

		private int _count;

		public int Count => _count;

		public void Add(int vertIndex, int triIndex)
		{
			if (_reserved == _count)
			{
				_reserved *= 2;
				Array.Resize(ref TriangleIndex, _reserved);
				Array.Resize(ref VertexIndex, _reserved);
			}
			TriangleIndex[_count] = triIndex;
			VertexIndex[_count] = vertIndex;
			_count++;
		}
	}

	public static Vector3[] CalculateNormals(Vector3[] vertices, int[] triangles, float angle)
	{
		Vector3[] array = new Vector3[vertices.Length];
		Vector3[] array2 = new Vector3[triangles.Length / 3];
		angle *= MathF.PI / 180f;
		Dictionary<VertexKey, VertexEntry> dictionary = new Dictionary<VertexKey, VertexEntry>(vertices.Length);
		for (int i = 0; i < triangles.Length; i += 3)
		{
			int num = triangles[i];
			int num2 = triangles[i + 1];
			int num3 = triangles[i + 2];
			Vector3 lhs = vertices[num2] - vertices[num];
			Vector3 rhs = vertices[num3] - vertices[num];
			Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
			int num4 = i / 3;
			array2[num4] = normalized;
			VertexKey key = new VertexKey(vertices[num]);
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = new VertexEntry();
				dictionary.Add(key, value);
			}
			value.Add(num, num4);
			key = new VertexKey(vertices[num2]);
			if (!dictionary.TryGetValue(key, out value))
			{
				value = new VertexEntry();
				dictionary.Add(key, value);
			}
			value.Add(num2, num4);
			key = new VertexKey(vertices[num3]);
			if (!dictionary.TryGetValue(key, out value))
			{
				value = new VertexEntry();
				dictionary.Add(key, value);
			}
			value.Add(num3, num4);
		}
		foreach (VertexEntry value2 in dictionary.Values)
		{
			for (int j = 0; j < value2.Count; j++)
			{
				Vector3 vector = default(Vector3);
				for (int k = 0; k < value2.Count; k++)
				{
					if (value2.VertexIndex[j] == value2.VertexIndex[k])
					{
						vector += array2[value2.TriangleIndex[k]];
					}
					else if (Mathf.Acos(Mathf.Clamp(Vector3.Dot(array2[value2.TriangleIndex[j]], array2[value2.TriangleIndex[k]]), -0.99999f, 0.99999f)) <= angle)
					{
						vector += array2[value2.TriangleIndex[k]];
					}
				}
				array[value2.VertexIndex[j]] = vector.normalized;
			}
		}
		return array;
	}

	public static Vector3[] CalculateNormalsSimple(Vector3[] vertices, int[] triangles)
	{
		Vector3[] array = new Vector3[vertices.Length];
		for (int i = 0; i < triangles.Length; i += 3)
		{
			int num = triangles[i];
			int num2 = triangles[i + 1];
			int num3 = triangles[i + 2];
			Vector3 vector = MathUtil.TriangleNormal(vertices[num], vertices[num2], vertices[num3]);
			array[num] += vector;
			array[num2] += vector;
			array[num3] += vector;
		}
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = array[j].normalized;
		}
		return array;
	}

	public static Vector4[] CalculateTangents(Vector3[] vertices, Vector3[] normals, Vector2[] texcoords, int[] triangles)
	{
		int num = vertices.Length;
		int num2 = triangles.Length / 3;
		Vector4[] array = new Vector4[num];
		Vector3[] array2 = new Vector3[num];
		Vector3[] array3 = new Vector3[num];
		int num3 = 0;
		for (int i = 0; i < num2; i++)
		{
			int num4 = triangles[num3];
			int num5 = triangles[num3 + 1];
			int num6 = triangles[num3 + 2];
			Vector3 vector = vertices[num4];
			Vector3 vector2 = vertices[num5];
			Vector3 vector3 = vertices[num6];
			Vector2 vector4 = texcoords[num4];
			Vector2 vector5 = texcoords[num5];
			Vector2 vector6 = texcoords[num6];
			float num7 = vector2.x - vector.x;
			float num8 = vector3.x - vector.x;
			float num9 = vector2.y - vector.y;
			float num10 = vector3.y - vector.y;
			float num11 = vector2.z - vector.z;
			float num12 = vector3.z - vector.z;
			float num13 = vector5.x - vector4.x;
			float num14 = vector6.x - vector4.x;
			float num15 = vector5.y - vector4.y;
			float num16 = vector6.y - vector4.y;
			num13 = ((num13 != 0f) ? num13 : 1f);
			num14 = ((num14 != 0f) ? num14 : 1f);
			num15 = ((num15 != 0f) ? num15 : 1f);
			num16 = ((num16 != 0f) ? num16 : 1f);
			float num17 = num13 * num16 - num14 * num15;
			num17 = ((num17 != 0f) ? num17 : MathUtil.BigEpsilon);
			float num18 = 1f / num17;
			Vector3 vector7 = new Vector3((num16 * num7 - num15 * num8) * num18, (num16 * num9 - num15 * num10) * num18, (num16 * num11 - num15 * num12) * num18);
			Vector3 vector8 = new Vector3((num13 * num8 - num14 * num7) * num18, (num13 * num10 - num14 * num9) * num18, (num13 * num12 - num14 * num11) * num18);
			array2[num4] += vector7;
			array2[num5] += vector7;
			array2[num6] += vector7;
			array3[num4] += vector8;
			array3[num5] += vector8;
			array3[num6] += vector8;
			num3 += 3;
		}
		for (int j = 0; j < num; j++)
		{
			Vector3 normal = normals[j];
			Vector3 tangent = array2[j];
			Vector3.OrthoNormalize(ref normal, ref tangent);
			array[j].x = tangent.x;
			array[j].y = tangent.y;
			array[j].z = tangent.z;
			array[j].w = ((Vector3.Dot(Vector3.Cross(normal, tangent), array3[j]) < 0f) ? (-1f) : 1f);
		}
		return array;
	}
}
