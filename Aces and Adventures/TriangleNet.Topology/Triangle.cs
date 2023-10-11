using TriangleNet.Geometry;

namespace TriangleNet.Topology;

public class Triangle : ITriangle
{
	internal int hash;

	internal int id;

	internal Otri[] neighbors;

	internal Vertex[] vertices;

	internal Osub[] subsegs;

	internal int label;

	internal double area;

	internal bool infected;

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

	public double Area
	{
		get
		{
			return area;
		}
		set
		{
			area = value;
		}
	}

	public Triangle()
	{
		vertices = new Vertex[3];
		subsegs = new Osub[3];
		neighbors = new Otri[3];
	}

	public Vertex GetVertex(int index)
	{
		return vertices[index];
	}

	public int GetVertexID(int index)
	{
		return vertices[index].id;
	}

	public ITriangle GetNeighbor(int index)
	{
		if (neighbors[index].tri.hash != -1)
		{
			return neighbors[index].tri;
		}
		return null;
	}

	public int GetNeighborID(int index)
	{
		if (neighbors[index].tri.hash != -1)
		{
			return neighbors[index].tri.id;
		}
		return -1;
	}

	public ISegment GetSegment(int index)
	{
		if (subsegs[index].seg.hash != -1)
		{
			return subsegs[index].seg;
		}
		return null;
	}

	public override int GetHashCode()
	{
		return hash;
	}

	public override string ToString()
	{
		return $"TID {hash}";
	}
}
