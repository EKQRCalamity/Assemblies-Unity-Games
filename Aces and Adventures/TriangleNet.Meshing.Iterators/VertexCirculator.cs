using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace TriangleNet.Meshing.Iterators;

public class VertexCirculator
{
	private List<Otri> cache = new List<Otri>();

	public VertexCirculator(Mesh mesh)
	{
		mesh.MakeVertexMap();
	}

	public IEnumerable<Vertex> EnumerateVertices(Vertex vertex)
	{
		BuildCache(vertex, vertices: true);
		foreach (Otri item in cache)
		{
			yield return item.Dest();
		}
	}

	public IEnumerable<ITriangle> EnumerateTriangles(Vertex vertex)
	{
		BuildCache(vertex, vertices: false);
		foreach (Otri item in cache)
		{
			yield return item.tri;
		}
	}

	private void BuildCache(Vertex vertex, bool vertices)
	{
		cache.Clear();
		Otri tri = vertex.tri;
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		tri.Copy(ref ot);
		while (ot.tri.id != -1)
		{
			cache.Add(ot);
			ot.Copy(ref ot2);
			ot.Onext();
			if (ot.Equals(tri))
			{
				break;
			}
		}
		if (ot.tri.id != -1)
		{
			return;
		}
		tri.Copy(ref ot);
		if (vertices)
		{
			ot2.Lnext();
			cache.Add(ot2);
		}
		ot.Oprev();
		while (ot.tri.id != -1)
		{
			cache.Insert(0, ot);
			ot.Oprev();
			if (ot.Equals(tri))
			{
				break;
			}
		}
	}
}
