using System;
using System.Collections.Generic;

namespace TriangleNet.Geometry;

public class Contour
{
	private int marker;

	private bool convex;

	public List<Vertex> Points { get; set; }

	public Contour(IEnumerable<Vertex> points)
		: this(points, 0, convex: false)
	{
	}

	public Contour(IEnumerable<Vertex> points, int marker)
		: this(points, marker, convex: false)
	{
	}

	public Contour(IEnumerable<Vertex> points, int marker, bool convex)
	{
		AddPoints(points);
		this.marker = marker;
		this.convex = convex;
	}

	public List<ISegment> GetSegments()
	{
		List<ISegment> list = new List<ISegment>();
		List<Vertex> points = Points;
		int num = points.Count - 1;
		for (int i = 0; i < num; i++)
		{
			list.Add(new Segment(points[i], points[i + 1], marker));
		}
		list.Add(new Segment(points[num], points[0], marker));
		return list;
	}

	public Point FindInteriorPoint(int limit = 5, double eps = 2E-05)
	{
		if (convex)
		{
			int count = Points.Count;
			Point point = new Point(0.0, 0.0);
			for (int i = 0; i < count; i++)
			{
				point.x += Points[i].x;
				point.y += Points[i].y;
			}
			point.x /= count;
			point.y /= count;
			return point;
		}
		return FindPointInPolygon(Points, limit, eps);
	}

	private void AddPoints(IEnumerable<Vertex> points)
	{
		Points = new List<Vertex>(points);
		int index = Points.Count - 1;
		if (Points[0] == Points[index])
		{
			Points.RemoveAt(index);
		}
	}

	private static Point FindPointInPolygon(List<Vertex> contour, int limit, double eps)
	{
		List<Point> points = contour.ConvertAll((Converter<Vertex, Point>)((Vertex x) => x));
		Rectangle rectangle = new Rectangle();
		rectangle.Expand(points);
		int count = contour.Count;
		Point point = new Point();
		RobustPredicates robustPredicates = new RobustPredicates();
		Point point2 = contour[0];
		Point point3 = contour[1];
		for (int i = 0; i < count; i++)
		{
			Point point4 = contour[(i + 2) % count];
			double x2 = point3.x;
			double y = point3.y;
			double value = robustPredicates.CounterClockwise(point2, point3, point4);
			double num;
			double num2;
			if (Math.Abs(value) < eps)
			{
				num = (point4.y - point2.y) / 2.0;
				num2 = (point2.x - point4.x) / 2.0;
			}
			else
			{
				num = (point2.x + point4.x) / 2.0 - x2;
				num2 = (point2.y + point4.y) / 2.0 - y;
			}
			point2 = point3;
			point3 = point4;
			value = 1.0;
			for (int j = 0; j < limit; j++)
			{
				point.x = x2 + num * value;
				point.y = y + num2 * value;
				if (rectangle.Contains(point) && IsPointInPolygon(point, contour))
				{
					return point;
				}
				point.x = x2 - num * value;
				point.y = y - num2 * value;
				if (rectangle.Contains(point) && IsPointInPolygon(point, contour))
				{
					return point;
				}
				value /= 2.0;
			}
		}
		throw new Exception();
	}

	private static bool IsPointInPolygon(Point point, List<Vertex> poly)
	{
		bool flag = false;
		double x = point.x;
		double y = point.y;
		int count = poly.Count;
		int i = 0;
		int index = count - 1;
		for (; i < count; i++)
		{
			if (((poly[i].y < y && poly[index].y >= y) || (poly[index].y < y && poly[i].y >= y)) && (poly[i].x <= x || poly[index].x <= x))
			{
				flag ^= poly[i].x + (y - poly[i].y) / (poly[index].y - poly[i].y) * (poly[index].x - poly[i].x) < x;
			}
			index = i;
		}
		return flag;
	}
}
