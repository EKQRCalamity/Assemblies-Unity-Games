using System;
using System.Collections.Generic;

namespace TriangleNet.Geometry;

public class Polygon : IPolygon
{
	private List<Vertex> points;

	private List<Point> holes;

	private List<RegionPointer> regions;

	private List<ISegment> segments;

	public List<Vertex> Points => points;

	public List<Point> Holes => holes;

	public List<RegionPointer> Regions => regions;

	public List<ISegment> Segments => segments;

	public bool HasPointMarkers { get; set; }

	public bool HasSegmentMarkers { get; set; }

	public int Count => points.Count;

	public Polygon()
		: this(3, markers: false)
	{
	}

	public Polygon(int capacity)
		: this(3, markers: false)
	{
	}

	public Polygon(int capacity, bool markers)
	{
		points = new List<Vertex>(capacity);
		holes = new List<Point>();
		regions = new List<RegionPointer>();
		segments = new List<ISegment>();
		HasPointMarkers = markers;
		HasSegmentMarkers = markers;
	}

	[Obsolete("Use polygon.Add(contour) method instead.")]
	public void AddContour(IEnumerable<Vertex> points, int marker = 0, bool hole = false, bool convex = false)
	{
		Add(new Contour(points, marker, convex), hole);
	}

	[Obsolete("Use polygon.Add(contour) method instead.")]
	public void AddContour(IEnumerable<Vertex> points, int marker, Point hole)
	{
		Add(new Contour(points, marker), hole);
	}

	public Rectangle Bounds()
	{
		List<Point> list = points.ConvertAll((Converter<Vertex, Point>)((Vertex x) => x));
		Rectangle rectangle = new Rectangle();
		rectangle.Expand(list);
		return rectangle;
	}

	public void Add(Vertex vertex)
	{
		points.Add(vertex);
	}

	public void Add(ISegment segment, bool insert = false)
	{
		segments.Add(segment);
		if (insert)
		{
			points.Add(segment.GetVertex(0));
			points.Add(segment.GetVertex(1));
		}
	}

	public void Add(ISegment segment, int index)
	{
		segments.Add(segment);
		points.Add(segment.GetVertex(index));
	}

	public void Add(Contour contour, bool hole = false)
	{
		if (hole)
		{
			Add(contour, contour.FindInteriorPoint());
			return;
		}
		points.AddRange(contour.Points);
		segments.AddRange(contour.GetSegments());
	}

	public void Add(Contour contour, Point hole)
	{
		points.AddRange(contour.Points);
		segments.AddRange(contour.GetSegments());
		holes.Add(hole);
	}
}
