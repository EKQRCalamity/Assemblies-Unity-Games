using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace TriangleNet.Meshing.Iterators;

public class EdgeIterator : IEnumerator<Edge>, IEnumerator, IDisposable
{
	private IEnumerator<TriangleNet.Topology.Triangle> triangles;

	private Otri tri;

	private Otri neighbor;

	private Osub sub;

	private Edge current;

	private Vertex p1;

	private Vertex p2;

	public Edge Current => current;

	object IEnumerator.Current => current;

	public EdgeIterator(Mesh mesh)
	{
		triangles = mesh.triangles.GetEnumerator();
		triangles.MoveNext();
		tri.tri = triangles.Current;
		tri.orient = 0;
	}

	public void Dispose()
	{
		triangles.Dispose();
	}

	public bool MoveNext()
	{
		if (tri.tri == null)
		{
			return false;
		}
		current = null;
		while (current == null)
		{
			if (tri.orient == 3)
			{
				if (!triangles.MoveNext())
				{
					return false;
				}
				tri.tri = triangles.Current;
				tri.orient = 0;
			}
			tri.Sym(ref neighbor);
			if (tri.tri.id < neighbor.tri.id || neighbor.tri.id == -1)
			{
				p1 = tri.Org();
				p2 = tri.Dest();
				tri.Pivot(ref sub);
				current = new Edge(p1.id, p2.id, sub.seg.boundary);
			}
			tri.orient++;
		}
		return true;
	}

	public void Reset()
	{
		triangles.Reset();
	}
}
