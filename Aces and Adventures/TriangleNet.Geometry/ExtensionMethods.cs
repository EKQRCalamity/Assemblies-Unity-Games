using TriangleNet.Meshing;

namespace TriangleNet.Geometry;

public static class ExtensionMethods
{
	public static IMesh Triangulate(this IPolygon polygon)
	{
		return new GenericMesher().Triangulate(polygon, null, null);
	}

	public static IMesh Triangulate(this IPolygon polygon, ConstraintOptions options)
	{
		return new GenericMesher().Triangulate(polygon, options, null);
	}

	public static IMesh Triangulate(this IPolygon polygon, QualityOptions quality)
	{
		return new GenericMesher().Triangulate(polygon, null, quality);
	}

	public static IMesh Triangulate(this IPolygon polygon, ConstraintOptions options, QualityOptions quality)
	{
		return new GenericMesher().Triangulate(polygon, options, quality);
	}

	public static IMesh Triangulate(this IPolygon polygon, ConstraintOptions options, QualityOptions quality, ITriangulator triangulator)
	{
		return new GenericMesher(triangulator).Triangulate(polygon, options, quality);
	}

	public static bool Contains(this ITriangle triangle, Point p)
	{
		return triangle.Contains(p.X, p.Y);
	}

	public static bool Contains(this ITriangle triangle, double x, double y)
	{
		Vertex vertex = triangle.GetVertex(0);
		Vertex vertex2 = triangle.GetVertex(1);
		Vertex vertex3 = triangle.GetVertex(2);
		Point point = new Point(vertex2.X - vertex.X, vertex2.Y - vertex.Y);
		Point point2 = new Point(vertex3.X - vertex.X, vertex3.Y - vertex.Y);
		Point p = new Point(x - vertex.X, y - vertex.Y);
		Point q = new Point(0.0 - point.Y, point.X);
		Point q2 = new Point(0.0 - point2.Y, point2.X);
		double num = DotProduct(p, q2) / DotProduct(point, q2);
		double num2 = DotProduct(p, q) / DotProduct(point2, q);
		if (num >= 0.0 && num2 >= 0.0 && num + num2 <= 1.0)
		{
			return true;
		}
		return false;
	}

	public static Rectangle Bounds(this ITriangle triangle)
	{
		Rectangle rectangle = new Rectangle();
		for (int i = 0; i < 3; i++)
		{
			rectangle.Expand(triangle.GetVertex(i));
		}
		return rectangle;
	}

	internal static double DotProduct(Point p, Point q)
	{
		return p.X * q.X + p.Y * q.Y;
	}
}
