using TriangleNet.Geometry;

namespace TriangleNet.IO;

public class InputTriangle : ITriangle
{
	internal int[] vertices;

	internal int label;

	internal double area;

	public int ID
	{
		get
		{
			return 0;
		}
		set
		{
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

	public InputTriangle(int p0, int p1, int p2)
	{
		vertices = new int[3] { p0, p1, p2 };
	}

	public Vertex GetVertex(int index)
	{
		return null;
	}

	public int GetVertexID(int index)
	{
		return vertices[index];
	}

	public ITriangle GetNeighbor(int index)
	{
		return null;
	}

	public int GetNeighborID(int index)
	{
		return -1;
	}

	public ISegment GetSegment(int index)
	{
		return null;
	}
}
