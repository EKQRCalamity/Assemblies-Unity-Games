using System;
using TriangleNet.Topology;

namespace TriangleNet.Geometry;

public class Vertex : Point
{
	internal int hash;

	internal VertexType type;

	internal Otri tri;

	public VertexType Type => type;

	public double this[int i] => i switch
	{
		0 => x, 
		1 => y, 
		_ => throw new ArgumentOutOfRangeException("Index must be 0 or 1."), 
	};

	public Vertex()
		: this(0.0, 0.0, 0)
	{
	}

	public Vertex(double x, double y)
		: this(x, y, 0)
	{
	}

	public Vertex(double x, double y, int mark)
		: base(x, y, mark)
	{
		type = VertexType.InputVertex;
	}

	public override int GetHashCode()
	{
		return hash;
	}
}
