using System.Collections.Generic;
using TriangleNet.Geometry;

namespace TriangleNet.Voronoi.Legacy;

public class VoronoiRegion
{
	private int id;

	private Point generator;

	private List<Point> vertices;

	private bool bounded;

	private Dictionary<int, VoronoiRegion> neighbors;

	public int ID => id;

	public Point Generator => generator;

	public ICollection<Point> Vertices => vertices;

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

	public VoronoiRegion(Vertex generator)
	{
		id = generator.id;
		this.generator = generator;
		vertices = new List<Point>();
		bounded = true;
		neighbors = new Dictionary<int, VoronoiRegion>();
	}

	public void Add(Point point)
	{
		vertices.Add(point);
	}

	public void Add(List<Point> points)
	{
		vertices.AddRange(points);
	}

	public VoronoiRegion GetNeighbor(Point p)
	{
		if (neighbors.TryGetValue(p.id, out var value))
		{
			return value;
		}
		return null;
	}

	internal void AddNeighbor(int id, VoronoiRegion neighbor)
	{
		neighbors.Add(id, neighbor);
	}

	public override string ToString()
	{
		return $"R-ID {id}";
	}
}
