using System;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace TriangleNet.Voronoi.Legacy;

[Obsolete("Use TriangleNet.Voronoi.StandardVoronoi class instead.")]
public class SimpleVoronoi : IVoronoi
{
	private IPredicates predicates = RobustPredicates.Default;

	private Mesh mesh;

	private Point[] points;

	private Dictionary<int, VoronoiRegion> regions;

	private Dictionary<int, Point> rayPoints;

	private int rayIndex;

	private Rectangle bounds;

	public Point[] Points => points;

	public ICollection<VoronoiRegion> Regions => regions.Values;

	public IEnumerable<IEdge> Edges => EnumerateEdges();

	public SimpleVoronoi(Mesh mesh)
	{
		this.mesh = mesh;
		Generate();
	}

	private void Generate()
	{
		mesh.Renumber();
		mesh.MakeVertexMap();
		points = new Point[mesh.triangles.Count + mesh.hullsize];
		regions = new Dictionary<int, VoronoiRegion>(mesh.vertices.Count);
		rayPoints = new Dictionary<int, Point>();
		rayIndex = 0;
		bounds = new Rectangle();
		ComputeCircumCenters();
		foreach (Vertex value in mesh.vertices.Values)
		{
			regions.Add(value.id, new VoronoiRegion(value));
		}
		foreach (VoronoiRegion value2 in regions.Values)
		{
			ConstructCell(value2);
		}
	}

	private void ComputeCircumCenters()
	{
		Otri otri = default(Otri);
		double xi = 0.0;
		double eta = 0.0;
		foreach (TriangleNet.Topology.Triangle triangle2 in mesh.triangles)
		{
			TriangleNet.Topology.Triangle triangle = (otri.tri = triangle2);
			Point point = predicates.FindCircumcenter(otri.Org(), otri.Dest(), otri.Apex(), ref xi, ref eta);
			point.id = triangle.id;
			points[triangle.id] = point;
			bounds.Expand(point);
		}
		double num = Math.Max(bounds.Width, bounds.Height);
		bounds.Resize(num, num);
	}

	private void ConstructCell(VoronoiRegion region)
	{
		Vertex obj = region.Generator as Vertex;
		List<Point> list = new List<Point>();
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		Otri ot4 = default(Otri);
		Osub os = default(Osub);
		obj.tri.Copy(ref ot2);
		ot2.Copy(ref ot);
		ot2.Onext(ref ot3);
		if (ot3.tri.id == -1)
		{
			ot2.Oprev(ref ot4);
			if (ot4.tri.id != -1)
			{
				ot2.Copy(ref ot3);
				ot2.Oprev();
				ot2.Copy(ref ot);
			}
		}
		while (ot3.tri.id != -1)
		{
			list.Add(points[ot.tri.id]);
			region.AddNeighbor(ot.tri.id, regions[ot.Apex().id]);
			if (ot3.Equals(ot2))
			{
				region.Add(list);
				return;
			}
			ot3.Copy(ref ot);
			ot3.Onext();
		}
		region.Bounded = false;
		int count = mesh.triangles.Count;
		ot.Lprev(ref ot3);
		ot3.Pivot(ref os);
		int hash = os.seg.hash;
		list.Add(points[ot.tri.id]);
		region.AddNeighbor(ot.tri.id, regions[ot.Apex().id]);
		if (!rayPoints.TryGetValue(hash, out var value))
		{
			Vertex vertex = ot.Org();
			Vertex vertex2 = ot.Apex();
			BoxRayIntersection(points[ot.tri.id], vertex.y - vertex2.y, vertex2.x - vertex.x, out value);
			value.id = count + rayIndex;
			points[count + rayIndex] = value;
			rayIndex++;
			rayPoints.Add(hash, value);
		}
		list.Add(value);
		list.Reverse();
		ot2.Copy(ref ot);
		ot.Oprev(ref ot4);
		while (ot4.tri.id != -1)
		{
			list.Add(points[ot4.tri.id]);
			region.AddNeighbor(ot4.tri.id, regions[ot4.Apex().id]);
			ot4.Copy(ref ot);
			ot4.Oprev();
		}
		ot.Pivot(ref os);
		hash = os.seg.hash;
		if (!rayPoints.TryGetValue(hash, out value))
		{
			Vertex vertex = ot.Org();
			Vertex vertex3 = ot.Dest();
			BoxRayIntersection(points[ot.tri.id], vertex3.y - vertex.y, vertex.x - vertex3.x, out value);
			value.id = count + rayIndex;
			rayPoints.Add(hash, value);
			points[count + rayIndex] = value;
			rayIndex++;
		}
		list.Add(value);
		region.AddNeighbor(value.id, regions[ot.Dest().id]);
		list.Reverse();
		region.Add(list);
	}

	private bool BoxRayIntersection(Point pt, double dx, double dy, out Point intersect)
	{
		double x = pt.x;
		double y = pt.y;
		double left = bounds.Left;
		double right = bounds.Right;
		double bottom = bounds.Bottom;
		double top = bounds.Top;
		if (x < left || x > right || y < bottom || y > top)
		{
			intersect = null;
			return false;
		}
		double num;
		double x2;
		double y2;
		if (dx < 0.0)
		{
			num = (left - x) / dx;
			x2 = left;
			y2 = y + num * dy;
		}
		else if (dx > 0.0)
		{
			num = (right - x) / dx;
			x2 = right;
			y2 = y + num * dy;
		}
		else
		{
			num = double.MaxValue;
			x2 = (y2 = 0.0);
		}
		double num2;
		double x3;
		double y3;
		if (dy < 0.0)
		{
			num2 = (bottom - y) / dy;
			x3 = x + num2 * dx;
			y3 = bottom;
		}
		else if (dy > 0.0)
		{
			num2 = (top - y) / dy;
			x3 = x + num2 * dx;
			y3 = top;
		}
		else
		{
			num2 = double.MaxValue;
			x3 = (y3 = 0.0);
		}
		if (num < num2)
		{
			intersect = new Point(x2, y2);
		}
		else
		{
			intersect = new Point(x3, y3);
		}
		return true;
	}

	private IEnumerable<IEdge> EnumerateEdges()
	{
		List<IEdge> list = new List<IEdge>(Regions.Count * 2);
		foreach (VoronoiRegion region in Regions)
		{
			Point point = null;
			Point point2 = null;
			foreach (Point vertex in region.Vertices)
			{
				if (point == null)
				{
					point = vertex;
					point2 = vertex;
				}
				else
				{
					list.Add(new Edge(point2.id, vertex.id));
					point2 = vertex;
				}
			}
			if (region.Bounded && point != null)
			{
				list.Add(new Edge(point2.id, point.id));
			}
		}
		return list;
	}
}
