using System.Collections.Generic;
using TriangleNet.Geometry;

namespace TriangleNet.Topology.DCEL;

public class Face
{
	public static readonly Face Empty;

	internal int id;

	internal Point generator;

	internal HalfEdge edge;

	internal bool bounded;

	public int ID
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public HalfEdge Edge
	{
		get
		{
			return edge;
		}
		set
		{
			edge = value;
		}
	}

	public bool Bounded
	{
		get
		{
			return bounded;
		}
		set
		{
			bounded = value;
		}
	}

	static Face()
	{
		Empty = new Face(null);
		Empty.id = -1;
	}

	public Face(Point generator)
		: this(generator, null)
	{
	}

	public Face(Point generator, HalfEdge edge)
	{
		this.generator = generator;
		this.edge = edge;
		bounded = true;
		if (generator != null)
		{
			id = generator.ID;
		}
	}

	public IEnumerable<HalfEdge> EnumerateEdges()
	{
		HalfEdge edge = Edge;
		int first = edge.ID;
		do
		{
			yield return edge;
			edge = edge.Next;
		}
		while (edge.ID != first);
	}

	public override string ToString()
	{
		return $"F-ID {id}";
	}
}
