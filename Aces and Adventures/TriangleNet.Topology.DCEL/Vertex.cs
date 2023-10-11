using System.Collections.Generic;
using TriangleNet.Geometry;

namespace TriangleNet.Topology.DCEL;

public class Vertex : Point
{
	internal HalfEdge leaving;

	public HalfEdge Leaving
	{
		get
		{
			return leaving;
		}
		set
		{
			leaving = value;
		}
	}

	public Vertex(double x, double y)
		: base(x, y)
	{
	}

	public Vertex(double x, double y, HalfEdge leaving)
		: base(x, y)
	{
		this.leaving = leaving;
	}

	public IEnumerable<HalfEdge> EnumerateEdges()
	{
		HalfEdge edge = Leaving;
		int first = edge.ID;
		do
		{
			yield return edge;
			edge = edge.Twin.Next;
		}
		while (edge.ID != first);
	}

	public override string ToString()
	{
		return $"V-ID {id}";
	}
}
