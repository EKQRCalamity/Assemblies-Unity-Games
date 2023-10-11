using System;
using TriangleNet.Topology;

namespace TriangleNet.Tools;

public class AdjacencyMatrix
{
	private int nnz;

	private int[] pcol;

	private int[] irow;

	public readonly int N;

	public int[] ColumnPointers => pcol;

	public int[] RowIndices => irow;

	public AdjacencyMatrix(Mesh mesh)
	{
		N = mesh.vertices.Count;
		pcol = AdjacencyCount(mesh);
		nnz = pcol[N];
		irow = AdjacencySet(mesh, pcol);
		SortIndices();
	}

	public AdjacencyMatrix(int[] pcol, int[] irow)
	{
		N = pcol.Length - 1;
		nnz = pcol[N];
		this.pcol = pcol;
		this.irow = irow;
		if (pcol[0] != 0)
		{
			throw new ArgumentException("Expected 0-based indexing.", "pcol");
		}
		if (irow.Length < nnz)
		{
			throw new ArgumentException();
		}
	}

	public int Bandwidth()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < N; i++)
		{
			for (int j = pcol[i]; j < pcol[i + 1]; j++)
			{
				int num3 = irow[j];
				num = Math.Max(num, i - num3);
				num2 = Math.Max(num2, num3 - i);
			}
		}
		return num + 1 + num2;
	}

	private int[] AdjacencyCount(Mesh mesh)
	{
		int n = N;
		int[] array = new int[n + 1];
		for (int i = 0; i < n; i++)
		{
			array[i] = 1;
		}
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			int id = triangle.id;
			int id2 = triangle.vertices[0].id;
			int id3 = triangle.vertices[1].id;
			int id4 = triangle.vertices[2].id;
			int id5 = triangle.neighbors[2].tri.id;
			if (id5 < 0 || id < id5)
			{
				array[id2]++;
				array[id3]++;
			}
			id5 = triangle.neighbors[0].tri.id;
			if (id5 < 0 || id < id5)
			{
				array[id3]++;
				array[id4]++;
			}
			id5 = triangle.neighbors[1].tri.id;
			if (id5 < 0 || id < id5)
			{
				array[id4]++;
				array[id2]++;
			}
		}
		for (int num = n; num > 0; num--)
		{
			array[num] = array[num - 1];
		}
		array[0] = 0;
		for (int j = 1; j <= n; j++)
		{
			array[j] = array[j - 1] + array[j];
		}
		return array;
	}

	private int[] AdjacencySet(Mesh mesh, int[] pcol)
	{
		int n = N;
		int[] array = new int[n];
		Array.Copy(pcol, array, n);
		int[] array2 = new int[pcol[n]];
		for (int i = 0; i < n; i++)
		{
			array2[array[i]] = i;
			array[i]++;
		}
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			int id = triangle.id;
			int id2 = triangle.vertices[0].id;
			int id3 = triangle.vertices[1].id;
			int id4 = triangle.vertices[2].id;
			int id5 = triangle.neighbors[2].tri.id;
			if (id5 < 0 || id < id5)
			{
				array2[array[id2]++] = id3;
				array2[array[id3]++] = id2;
			}
			id5 = triangle.neighbors[0].tri.id;
			if (id5 < 0 || id < id5)
			{
				array2[array[id3]++] = id4;
				array2[array[id4]++] = id3;
			}
			id5 = triangle.neighbors[1].tri.id;
			if (id5 < 0 || id < id5)
			{
				array2[array[id2]++] = id4;
				array2[array[id4]++] = id2;
			}
		}
		return array2;
	}

	public void SortIndices()
	{
		int n = N;
		int[] array = irow;
		for (int i = 0; i < n; i++)
		{
			int num = pcol[i];
			int num2 = pcol[i + 1];
			Array.Sort(array, num, num2 - num);
		}
	}
}
