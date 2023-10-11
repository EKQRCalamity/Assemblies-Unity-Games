using System;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace TriangleNet.Voronoi.Legacy;

[Obsolete("Use TriangleNet.Voronoi.BoundedVoronoi class instead.")]
public class BoundedVoronoiLegacy : IVoronoi
{
	private IPredicates predicates = RobustPredicates.Default;

	private Mesh mesh;

	private Point[] points;

	private List<VoronoiRegion> regions;

	private List<Point> segPoints;

	private int segIndex;

	private Dictionary<int, SubSegment> subsegMap;

	private bool includeBoundary = true;

	public Point[] Points => points;

	public ICollection<VoronoiRegion> Regions => regions;

	public IEnumerable<IEdge> Edges => EnumerateEdges();

	public BoundedVoronoiLegacy(Mesh mesh)
		: this(mesh, includeBoundary: true)
	{
	}

	public BoundedVoronoiLegacy(Mesh mesh, bool includeBoundary)
	{
		this.mesh = mesh;
		this.includeBoundary = includeBoundary;
		Generate();
	}

	private void Generate()
	{
		mesh.Renumber();
		mesh.MakeVertexMap();
		regions = new List<VoronoiRegion>(mesh.vertices.Count);
		points = new Point[mesh.triangles.Count];
		segPoints = new List<Point>(mesh.subsegs.Count * 4);
		ComputeCircumCenters();
		TagBlindTriangles();
		foreach (Vertex value in mesh.vertices.Values)
		{
			if (value.type == VertexType.FreeVertex || value.label == 0)
			{
				ConstructCell(value);
			}
			else if (includeBoundary)
			{
				ConstructBoundaryCell(value);
			}
		}
		int num = points.Length;
		Array.Resize(ref points, num + segPoints.Count);
		for (int i = 0; i < segPoints.Count; i++)
		{
			points[num + i] = segPoints[i];
		}
		segPoints.Clear();
		segPoints = null;
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
		}
	}

	private void TagBlindTriangles()
	{
		int num = 0;
		subsegMap = new Dictionary<int, SubSegment>();
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Osub seg = default(Osub);
		Osub os = default(Osub);
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			triangle.infected = false;
		}
		foreach (SubSegment value in mesh.subsegs.Values)
		{
			Stack<TriangleNet.Topology.Triangle> stack = new Stack<TriangleNet.Topology.Triangle>();
			seg.seg = value;
			seg.orient = 0;
			seg.Pivot(ref ot);
			if (ot.tri.id != -1 && !ot.tri.infected)
			{
				stack.Push(ot.tri);
			}
			seg.Sym();
			seg.Pivot(ref ot);
			if (ot.tri.id != -1 && !ot.tri.infected)
			{
				stack.Push(ot.tri);
			}
			while (stack.Count > 0)
			{
				ot.tri = stack.Pop();
				ot.orient = 0;
				if (!TriangleIsBlinded(ref ot, ref seg))
				{
					continue;
				}
				ot.tri.infected = true;
				num++;
				subsegMap.Add(ot.tri.hash, seg.seg);
				ot.orient = 0;
				while (ot.orient < 3)
				{
					ot.Sym(ref ot2);
					ot2.Pivot(ref os);
					if (ot2.tri.id != -1 && !ot2.tri.infected && os.seg.hash == -1)
					{
						stack.Push(ot2.tri);
					}
					ot.orient++;
				}
			}
		}
		num = 0;
	}

	private bool TriangleIsBlinded(ref Otri tri, ref Osub seg)
	{
		Vertex p = tri.Org();
		Vertex p2 = tri.Dest();
		Vertex p3 = tri.Apex();
		Vertex p4 = seg.Org();
		Vertex p5 = seg.Dest();
		Point p6 = points[tri.tri.id];
		if (SegmentsIntersect(p4, p5, p6, p, out var p7, strictIntersect: true))
		{
			return true;
		}
		if (SegmentsIntersect(p4, p5, p6, p2, out p7, strictIntersect: true))
		{
			return true;
		}
		if (SegmentsIntersect(p4, p5, p6, p3, out p7, strictIntersect: true))
		{
			return true;
		}
		return false;
	}

	private void ConstructCell(Vertex vertex)
	{
		VoronoiRegion voronoiRegion = new VoronoiRegion(vertex);
		regions.Add(voronoiRegion);
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		Osub osub = default(Osub);
		Osub os = default(Osub);
		int count = mesh.triangles.Count;
		List<Point> list = new List<Point>();
		vertex.tri.Copy(ref ot2);
		if (ot2.Org() != vertex)
		{
			throw new Exception("ConstructCell: inconsistent topology.");
		}
		ot2.Copy(ref ot);
		ot2.Onext(ref ot3);
		do
		{
			Point point = points[ot.tri.id];
			Point p = points[ot3.tri.id];
			Point p2;
			if (!ot.tri.infected)
			{
				list.Add(point);
				if (ot3.tri.infected)
				{
					os.seg = subsegMap[ot3.tri.hash];
					if (SegmentsIntersect(os.Org(), os.Dest(), point, p, out p2, strictIntersect: true))
					{
						p2.id = count + segIndex++;
						segPoints.Add(p2);
						list.Add(p2);
					}
				}
			}
			else
			{
				osub.seg = subsegMap[ot.tri.hash];
				if (!ot3.tri.infected)
				{
					if (SegmentsIntersect(osub.Org(), osub.Dest(), point, p, out p2, strictIntersect: true))
					{
						p2.id = count + segIndex++;
						segPoints.Add(p2);
						list.Add(p2);
					}
				}
				else
				{
					os.seg = subsegMap[ot3.tri.hash];
					if (!osub.Equal(os))
					{
						if (SegmentsIntersect(osub.Org(), osub.Dest(), point, p, out p2, strictIntersect: true))
						{
							p2.id = count + segIndex++;
							segPoints.Add(p2);
							list.Add(p2);
						}
						if (SegmentsIntersect(os.Org(), os.Dest(), point, p, out p2, strictIntersect: true))
						{
							p2.id = count + segIndex++;
							segPoints.Add(p2);
							list.Add(p2);
						}
					}
				}
			}
			ot3.Copy(ref ot);
			ot3.Onext();
		}
		while (!ot.Equals(ot2));
		voronoiRegion.Add(list);
	}

	private void ConstructBoundaryCell(Vertex vertex)
	{
		VoronoiRegion voronoiRegion = new VoronoiRegion(vertex);
		regions.Add(voronoiRegion);
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		Otri ot4 = default(Otri);
		Osub osub = default(Osub);
		Osub os = default(Osub);
		int count = mesh.triangles.Count;
		List<Point> list = new List<Point>();
		vertex.tri.Copy(ref ot2);
		if (ot2.Org() != vertex)
		{
			throw new Exception("ConstructBoundaryCell: inconsistent topology.");
		}
		ot2.Copy(ref ot);
		ot2.Onext(ref ot3);
		ot2.Oprev(ref ot4);
		if (ot4.tri.id != -1)
		{
			while (ot4.tri.id != -1 && !ot4.Equals(ot2))
			{
				ot4.Copy(ref ot);
				ot4.Oprev();
			}
			ot.Copy(ref ot2);
			ot.Onext(ref ot3);
		}
		Point point;
		if (ot4.tri.id == -1)
		{
			point = new Point(vertex.x, vertex.y);
			point.id = count + segIndex++;
			segPoints.Add(point);
			list.Add(point);
		}
		Vertex vertex2 = ot.Org();
		Vertex vertex3 = ot.Dest();
		point = new Point((vertex2.x + vertex3.x) / 2.0, (vertex2.y + vertex3.y) / 2.0);
		point.id = count + segIndex++;
		segPoints.Add(point);
		list.Add(point);
		do
		{
			Point point2 = points[ot.tri.id];
			if (ot3.tri.id == -1)
			{
				if (!ot.tri.infected)
				{
					list.Add(point2);
				}
				vertex2 = ot.Org();
				Vertex vertex4 = ot.Apex();
				point = new Point((vertex2.x + vertex4.x) / 2.0, (vertex2.y + vertex4.y) / 2.0);
				point.id = count + segIndex++;
				segPoints.Add(point);
				list.Add(point);
				break;
			}
			Point p = points[ot3.tri.id];
			if (!ot.tri.infected)
			{
				list.Add(point2);
				if (ot3.tri.infected)
				{
					os.seg = subsegMap[ot3.tri.hash];
					if (SegmentsIntersect(os.Org(), os.Dest(), point2, p, out point, strictIntersect: true))
					{
						point.id = count + segIndex++;
						segPoints.Add(point);
						list.Add(point);
					}
				}
			}
			else
			{
				osub.seg = subsegMap[ot.tri.hash];
				Vertex p2 = osub.Org();
				Vertex p3 = osub.Dest();
				if (!ot3.tri.infected)
				{
					vertex3 = ot.Dest();
					Vertex vertex4 = ot.Apex();
					Point p4 = new Point((vertex3.x + vertex4.x) / 2.0, (vertex3.y + vertex4.y) / 2.0);
					if (SegmentsIntersect(p2, p3, p4, point2, out point, strictIntersect: false))
					{
						point.id = count + segIndex++;
						segPoints.Add(point);
						list.Add(point);
					}
					if (SegmentsIntersect(p2, p3, point2, p, out point, strictIntersect: true))
					{
						point.id = count + segIndex++;
						segPoints.Add(point);
						list.Add(point);
					}
				}
				else
				{
					os.seg = subsegMap[ot3.tri.hash];
					if (!osub.Equal(os))
					{
						if (SegmentsIntersect(p2, p3, point2, p, out point, strictIntersect: true))
						{
							point.id = count + segIndex++;
							segPoints.Add(point);
							list.Add(point);
						}
						if (SegmentsIntersect(os.Org(), os.Dest(), point2, p, out point, strictIntersect: true))
						{
							point.id = count + segIndex++;
							segPoints.Add(point);
							list.Add(point);
						}
					}
					else
					{
						Point p5 = new Point((vertex2.x + vertex3.x) / 2.0, (vertex2.y + vertex3.y) / 2.0);
						if (SegmentsIntersect(p2, p3, p5, p, out point, strictIntersect: false))
						{
							point.id = count + segIndex++;
							segPoints.Add(point);
							list.Add(point);
						}
					}
				}
			}
			ot3.Copy(ref ot);
			ot3.Onext();
		}
		while (!ot.Equals(ot2));
		voronoiRegion.Add(list);
	}

	private bool SegmentsIntersect(Point p1, Point p2, Point p3, Point p4, out Point p, bool strictIntersect)
	{
		p = null;
		double x = p1.x;
		double y = p1.y;
		double x2 = p2.x;
		double y2 = p2.y;
		double x3 = p3.x;
		double y3 = p3.y;
		double x4 = p4.x;
		double y4 = p4.y;
		if ((x == x2 && y == y2) || (x3 == x4 && y3 == y4))
		{
			return false;
		}
		if ((x == x3 && y == y3) || (x2 == x3 && y2 == y3) || (x == x4 && y == y4) || (x2 == x4 && y2 == y4))
		{
			return false;
		}
		x2 -= x;
		y2 -= y;
		x3 -= x;
		y3 -= y;
		x4 -= x;
		y4 -= y;
		double num = Math.Sqrt(x2 * x2 + y2 * y2);
		double num2 = x2 / num;
		double num3 = y2 / num;
		double num4 = x3 * num2 + y3 * num3;
		y3 = y3 * num2 - x3 * num3;
		x3 = num4;
		double num5 = x4 * num2 + y4 * num3;
		y4 = y4 * num2 - x4 * num3;
		x4 = num5;
		if ((y3 < 0.0 && y4 < 0.0) || (y3 >= 0.0 && y4 >= 0.0 && strictIntersect))
		{
			return false;
		}
		double num6 = x4 + (x3 - x4) * y4 / (y4 - y3);
		if (num6 < 0.0 || (num6 > num && strictIntersect))
		{
			return false;
		}
		p = new Point(x + num6 * num2, y + num6 * num3);
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
