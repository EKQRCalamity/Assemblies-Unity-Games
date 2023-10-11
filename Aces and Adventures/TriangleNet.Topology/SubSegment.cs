using TriangleNet.Geometry;

namespace TriangleNet.Topology;

public class SubSegment : ISegment, IEdge
{
	internal int hash;

	internal Osub[] subsegs;

	internal Vertex[] vertices;

	internal Otri[] triangles;

	internal int boundary;

	public int P0 => vertices[0].id;

	public int P1 => vertices[1].id;

	public int Label => boundary;

	public SubSegment()
	{
		vertices = new Vertex[4];
		boundary = 0;
		subsegs = new Osub[2];
		triangles = new Otri[2];
	}

	public Vertex GetVertex(int index)
	{
		return vertices[index];
	}

	public ITriangle GetTriangle(int index)
	{
		if (triangles[index].tri.hash != -1)
		{
			return triangles[index].tri;
		}
		return null;
	}

	public override int GetHashCode()
	{
		return hash;
	}

	public override string ToString()
	{
		return $"SID {hash}";
	}
}
