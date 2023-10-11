using System;

namespace TriangleNet.Geometry;

public class Segment : ISegment, IEdge
{
	private Vertex v0;

	private Vertex v1;

	private int label;

	public int Label
	{
		get
		{
			return label;
		}
		set
		{
			label = value;
		}
	}

	public int P0 => v0.id;

	public int P1 => v1.id;

	public Segment(Vertex v0, Vertex v1)
		: this(v0, v1, 0)
	{
	}

	public Segment(Vertex v0, Vertex v1, int label)
	{
		this.v0 = v0;
		this.v1 = v1;
		this.label = label;
	}

	public Vertex GetVertex(int index)
	{
		return index switch
		{
			0 => v0, 
			1 => v1, 
			_ => throw new IndexOutOfRangeException(), 
		};
	}

	public ITriangle GetTriangle(int index)
	{
		throw new NotImplementedException();
	}
}
