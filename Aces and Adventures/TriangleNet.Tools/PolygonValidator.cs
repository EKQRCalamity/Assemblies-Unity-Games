using System;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Logging;

namespace TriangleNet.Tools;

public static class PolygonValidator
{
	public static bool IsConsistent(IPolygon poly)
	{
		ILog<LogItem> instance = Log.Instance;
		List<Vertex> points = poly.Points;
		int num = 0;
		int num2 = 0;
		int count = points.Count;
		if (count < 3)
		{
			instance.Warning("Polygon must have at least 3 vertices.", "PolygonValidator.IsConsistent()");
			return false;
		}
		foreach (Vertex item in points)
		{
			if (item == null)
			{
				num++;
				instance.Warning($"Point {num2} is null.", "PolygonValidator.IsConsistent()");
			}
			else if (double.IsNaN(item.x) || double.IsNaN(item.y))
			{
				num++;
				instance.Warning($"Point {num2} has invalid coordinates.", "PolygonValidator.IsConsistent()");
			}
			else if (double.IsInfinity(item.x) || double.IsInfinity(item.y))
			{
				num++;
				instance.Warning($"Point {num2} has invalid coordinates.", "PolygonValidator.IsConsistent()");
			}
			num2++;
		}
		num2 = 0;
		foreach (ISegment segment in poly.Segments)
		{
			if (segment == null)
			{
				num++;
				instance.Warning($"Segment {num2} is null.", "PolygonValidator.IsConsistent()");
				return false;
			}
			Vertex vertex = segment.GetVertex(0);
			Vertex vertex2 = segment.GetVertex(1);
			if (vertex.x == vertex2.x && vertex.y == vertex2.y)
			{
				num++;
				instance.Warning($"Endpoints of segment {num2} are coincident (IDs {vertex.id} / {vertex2.id}).", "PolygonValidator.IsConsistent()");
			}
			num2++;
		}
		num = ((points[0].id != points[1].id) ? (num + CheckDuplicateIDs(poly)) : (num + CheckVertexIDs(poly, count)));
		return num == 0;
	}

	public static bool HasDuplicateVertices(IPolygon poly)
	{
		ILog<LogItem> instance = Log.Instance;
		int num = 0;
		Vertex[] array = poly.Points.ToArray();
		VertexSorter.Sort(array);
		for (int i = 1; i < array.Length; i++)
		{
			if (array[i - 1] == array[i])
			{
				num++;
				instance.Warning($"Found duplicate point {array[i]}.", "PolygonValidator.HasDuplicateVertices()");
			}
		}
		return num > 0;
	}

	public static bool HasBadAngles(IPolygon poly, double threshold = 2E-12)
	{
		ILog<LogItem> instance = Log.Instance;
		int num = 0;
		int num2 = 0;
		Point point = null;
		Point point2 = null;
		_ = poly.Points.Count;
		foreach (ISegment segment in poly.Segments)
		{
			Point point3 = point;
			Point point4 = point2;
			point = segment.GetVertex(0);
			point2 = segment.GetVertex(1);
			if (!(point == point2) && !(point3 == point4))
			{
				if (point3 != null && point4 != null && point == point4 && point2 != null && IsBadAngle(point3, point, point2, threshold))
				{
					num++;
					instance.Warning($"Bad segment angle found at index {num2}.", "PolygonValidator.HasBadAngles()");
				}
				num2++;
			}
		}
		return num > 0;
	}

	private static bool IsBadAngle(Point a, Point b, Point c, double threshold = 0.0)
	{
		double x = DotProduct(a, b, c);
		return Math.Abs(Math.Atan2(CrossProductLength(a, b, c), x)) <= threshold;
	}

	private static double DotProduct(Point a, Point b, Point c)
	{
		return (a.x - b.x) * (c.x - b.x) + (a.y - b.y) * (c.y - b.y);
	}

	private static double CrossProductLength(Point a, Point b, Point c)
	{
		return (a.x - b.x) * (c.y - b.y) - (a.y - b.y) * (c.x - b.x);
	}

	private static int CheckVertexIDs(IPolygon poly, int count)
	{
		ILog<LogItem> instance = Log.Instance;
		int num = 0;
		int num2 = 0;
		foreach (ISegment segment in poly.Segments)
		{
			Vertex vertex = segment.GetVertex(0);
			Vertex vertex2 = segment.GetVertex(1);
			if (vertex.id < 0 || vertex.id >= count)
			{
				num++;
				instance.Warning($"Segment {num2} has invalid startpoint.", "PolygonValidator.IsConsistent()");
			}
			if (vertex2.id < 0 || vertex2.id >= count)
			{
				num++;
				instance.Warning($"Segment {num2} has invalid endpoint.", "PolygonValidator.IsConsistent()");
			}
			num2++;
		}
		return num;
	}

	private static int CheckDuplicateIDs(IPolygon poly)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (Vertex point in poly.Points)
		{
			if (!hashSet.Add(point.id))
			{
				Log.Instance.Warning("Found duplicate vertex ids.", "PolygonValidator.IsConsistent()");
				return 1;
			}
		}
		return 0;
	}
}
