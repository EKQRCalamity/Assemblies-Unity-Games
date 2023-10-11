using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dynamite3D.RealIvy;

[Serializable]
public class RTMeshData
{
	private int vertCount;

	private int vertexIndex;

	public Vector3[] vertices;

	public Vector3[] normals;

	public Vector2[] uv;

	public Vector2[] uv2;

	public Color[] colors;

	public int[] triangleIndices;

	public int[][] triangles;

	public RTMeshData(int numVertices, int numSubmeshes, List<int> numTrianglesPerSubmesh)
	{
		Vector3[] array = new Vector3[numVertices];
		Vector3[] array2 = new Vector3[numVertices];
		Vector2[] array3 = new Vector2[numVertices];
		Color[] array4 = new Color[numVertices];
		int[][] array5 = new int[numSubmeshes][];
		for (int i = 0; i < array5.Length; i++)
		{
			array5[i] = new int[numTrianglesPerSubmesh[i]];
		}
		SetValues(array, array2, array3, array4, array5);
	}

	public RTMeshData(Mesh mesh)
	{
		int subMeshCount = mesh.subMeshCount;
		Vector3[] array = mesh.vertices;
		Vector3[] array2 = mesh.normals;
		Vector2[] array3 = mesh.uv;
		_ = mesh.uv2;
		Color[] array4 = mesh.colors;
		int[][] array5 = new int[subMeshCount][];
		for (int i = 0; i < array5.Length; i++)
		{
			array5[i] = mesh.GetTriangles(i);
		}
		SetValues(array, array2, array3, array4, array5);
	}

	private void SetValues(Vector3[] vertices, Vector3[] normals, Vector2[] uv, Color[] colors, int[][] triangles)
	{
		this.vertices = vertices;
		this.normals = normals;
		this.uv = uv;
		this.colors = colors;
		this.triangles = triangles;
		triangleIndices = new int[triangles.Length];
		vertexIndex = 0;
	}

	public void CopyDataFromIndex(int index, int lastTriCount, int numTris, RTMeshData copyFrom)
	{
		vertices[index] = copyFrom.vertices[index];
		normals[index] = copyFrom.normals[index];
		uv[index] = copyFrom.uv[index];
	}

	public void AddTriangle(int sumbesh, int value)
	{
		if (triangleIndices[sumbesh] >= triangles[sumbesh].Length)
		{
			int newSize = triangles[sumbesh].Length * 2;
			Array.Resize(ref triangles[sumbesh], newSize);
		}
		triangles[sumbesh][triangleIndices[sumbesh]] = value;
		triangleIndices[sumbesh]++;
	}

	public void AddVertex(Vector3 vertexValue, Vector3 normalValue, Vector2 uvValue, Color color)
	{
		if (vertCount >= vertices.Length)
		{
			Resize();
		}
		vertices[vertexIndex] = vertexValue;
		normals[vertexIndex] = normalValue;
		uv[vertexIndex] = uvValue;
		colors[vertexIndex] = color;
		vertexIndex++;
		vertCount++;
	}

	private void Resize()
	{
		int newSize = vertices.Length * 2;
		Array.Resize(ref vertices, newSize);
		Array.Resize(ref normals, newSize);
		Array.Resize(ref uv, newSize);
		Array.Resize(ref colors, newSize);
	}

	public int VertexCount()
	{
		return vertCount;
	}

	public void Clear()
	{
		vertCount = 0;
		vertexIndex = 0;
		for (int i = 0; i < triangleIndices.Length; i++)
		{
			triangleIndices[i] = 0;
		}
	}
}
