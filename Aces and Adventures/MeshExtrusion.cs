using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshExtrusion
{
	private class Edge
	{
		public int[] vertexIndex = new int[2];

		public int[] faceIndex = new int[2];
	}

	public static void ExtrudeMesh(MeshData meshData, Matrix4x4[] extrusion, bool invertFaces, bool calculateNormals)
	{
		Edge[] array = _BuildManifoldEdges(meshData);
		int num = array.Length * 2 * extrusion.Length;
		int num2 = array.Length * 6;
		int num3 = num2 * (extrusion.Length - 1);
		Vector3[] vertices = meshData.vertices;
		Vector2[] uv = meshData.uv;
		int[] triangles = meshData.triangles;
		Vector3[] array2 = new Vector3[num + meshData.vertexCount * 2];
		Vector2[] array3 = new Vector2[array2.Length];
		int[] array4 = new int[num3 + triangles.Length * 2];
		int num4 = 0;
		for (int i = 0; i < extrusion.Length; i++)
		{
			Matrix4x4 matrix4x = extrusion[i];
			Edge[] array5 = array;
			foreach (Edge edge in array5)
			{
				array2[num4] = matrix4x.MultiplyPoint(vertices[edge.vertexIndex[0]]);
				array2[num4 + 1] = matrix4x.MultiplyPoint(vertices[edge.vertexIndex[1]]);
				array3[num4] = uv[edge.vertexIndex[0]];
				array3[num4 + 1] = uv[edge.vertexIndex[1]];
				num4 += 2;
			}
		}
		for (int k = 0; k < 2; k++)
		{
			Matrix4x4 matrix4x2 = extrusion[(k != 0) ? (extrusion.Length - 1) : 0];
			int num5 = ((k == 0) ? num : (num + vertices.Length));
			for (int l = 0; l < vertices.Length; l++)
			{
				array2[num5 + l] = matrix4x2.MultiplyPoint(vertices[l]);
				array3[num5 + l] = uv[l];
			}
		}
		for (int m = 0; m < extrusion.Length - 1; m++)
		{
			int num6 = array.Length * 2 * m;
			int num7 = array.Length * 2 * (m + 1);
			for (int n = 0; n < array.Length; n++)
			{
				int num8 = m * num2 + n * 6;
				array4[num8] = num6 + n * 2;
				array4[num8 + 1] = num7 + n * 2;
				array4[num8 + 2] = num6 + n * 2 + 1;
				array4[num8 + 3] = num7 + n * 2;
				array4[num8 + 4] = num7 + n * 2 + 1;
				array4[num8 + 5] = num6 + n * 2 + 1;
			}
		}
		int num9 = triangles.Length / 3;
		int num10 = num;
		int num11 = num3;
		for (int num12 = 0; num12 < num9; num12++)
		{
			int num13 = num12 * 3;
			array4[num13 + num11] = triangles[num13 + 1] + num10;
			array4[num13 + num11 + 1] = triangles[num13 + 2] + num10;
			array4[num13 + num11 + 2] = triangles[num13] + num10;
		}
		int num14 = num + vertices.Length;
		int num15 = num3 + triangles.Length;
		for (int num16 = 0; num16 < num9; num16++)
		{
			int num17 = num16 * 3;
			array4[num17 + num15] = triangles[num17] + num14;
			array4[num17 + num15 + 1] = triangles[num17 + 2] + num14;
			array4[num17 + num15 + 2] = triangles[num17 + 1] + num14;
		}
		if (invertFaces)
		{
			array4.InvertFaces();
		}
		meshData.vertices = array2;
		meshData.uv = array3;
		meshData.triangles = array4;
		if (calculateNormals)
		{
			meshData.CalculateNormals();
		}
	}

	private static Edge[] _BuildManifoldEdges(MeshData meshData)
	{
		Edge[] array = _BuildEdges(meshData.vertexCount, meshData.triangles);
		ArrayList arrayList = new ArrayList();
		Edge[] array2 = array;
		foreach (Edge edge in array2)
		{
			if (edge.faceIndex[0] == edge.faceIndex[1])
			{
				arrayList.Add(edge);
			}
		}
		return arrayList.ToArray(typeof(Edge)) as Edge[];
	}

	private static Edge[] _BuildEdges(int vertexCount, int[] triangleArray)
	{
		int num = triangleArray.Length;
		int[] array = new int[vertexCount + num];
		int num2 = triangleArray.Length / 3;
		for (int i = 0; i < vertexCount; i++)
		{
			array[i] = -1;
		}
		Edge[] array2 = new Edge[num];
		int num3 = 0;
		for (int j = 0; j < num2; j++)
		{
			int num4 = triangleArray[j * 3 + 2];
			for (int k = 0; k < 3; k++)
			{
				int num5 = triangleArray[j * 3 + k];
				if (num4 < num5)
				{
					Edge edge = new Edge();
					edge.vertexIndex[0] = num4;
					edge.vertexIndex[1] = num5;
					edge.faceIndex[0] = j;
					edge.faceIndex[1] = j;
					array2[num3] = edge;
					int num6 = array[num4];
					if (num6 == -1)
					{
						array[num4] = num3;
					}
					else
					{
						while (true)
						{
							int num7 = array[vertexCount + num6];
							if (num7 == -1)
							{
								break;
							}
							num6 = num7;
						}
						array[vertexCount + num6] = num3;
					}
					array[vertexCount + num3] = -1;
					num3++;
				}
				num4 = num5;
			}
		}
		for (int l = 0; l < num2; l++)
		{
			int num8 = triangleArray[l * 3 + 2];
			for (int m = 0; m < 3; m++)
			{
				int num9 = triangleArray[l * 3 + m];
				if (num8 > num9)
				{
					bool flag = false;
					for (int num10 = array[num9]; num10 != -1; num10 = array[vertexCount + num10])
					{
						Edge edge2 = array2[num10];
						if (edge2.vertexIndex[1] == num8 && edge2.faceIndex[0] == edge2.faceIndex[1])
						{
							array2[num10].faceIndex[1] = l;
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						Edge edge3 = new Edge();
						edge3.vertexIndex[0] = num8;
						edge3.vertexIndex[1] = num9;
						edge3.faceIndex[0] = l;
						edge3.faceIndex[1] = l;
						array2[num3] = edge3;
						num3++;
					}
				}
				num8 = num9;
			}
		}
		Edge[] array3 = new Edge[num3];
		for (int n = 0; n < num3; n++)
		{
			array3[n] = array2[n];
		}
		return array3;
	}

	private static void _RemoveDuplicateVertices(MeshData meshData, int startVertex = 0, int endVertex = 0)
	{
		Vector3[] vertices = meshData.vertices;
		Vector2[] uv = meshData.uv;
		int[] triangles = meshData.triangles;
		endVertex = ((endVertex == 0) ? vertices.Length : endVertex);
		Dictionary<int, int> dictionary = new Dictionary<int, int>(vertices.Length);
		List<int> list = new List<int>();
		for (int i = 0; i < vertices.Length; i++)
		{
			dictionary.Add(i, i);
		}
		for (int j = startVertex; j < endVertex; j++)
		{
			for (int k = j + 1; k < endVertex; k++)
			{
				if (vertices[j] == vertices[k])
				{
					dictionary[k] = j;
					list.Add(k);
					break;
				}
			}
		}
		list.Sort();
		List<Vector3> list2 = new List<Vector3>(vertices.Length);
		List<Vector2> list3 = new List<Vector2>(uv.Length);
		for (int l = 0; l < vertices.Length; l++)
		{
			list2.Add(vertices[l]);
			list3.Add(uv[l]);
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			int index = list[num];
			list2.RemoveAt(index);
			list3.RemoveAt(index);
		}
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>(dictionary.Count);
		foreach (int key in dictionary.Keys)
		{
			int num2 = dictionary[key];
			int num3 = num2;
			for (int num4 = list.Count - 1; num4 >= 0; num4--)
			{
				if (num2 > list[num4])
				{
					num3 -= num4 + 1;
					break;
				}
			}
			dictionary2.Add(key, num3);
		}
		for (int m = 0; m < triangles.Length; m++)
		{
			triangles[m] = dictionary2[triangles[m]];
		}
		meshData.vertices = list2.ToArray();
		meshData.uv = list3.ToArray();
		meshData.triangles = triangles;
	}
}
