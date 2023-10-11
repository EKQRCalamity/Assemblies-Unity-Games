using System;
using System.Collections.Generic;

namespace TriangleNet.Geometry;

public class Rectangle
{
	private double xmin;

	private double ymin;

	private double xmax;

	private double ymax;

	public double Left => xmin;

	public double Right => xmax;

	public double Bottom => ymin;

	public double Top => ymax;

	public double Width => xmax - xmin;

	public double Height => ymax - ymin;

	public Rectangle()
	{
		xmin = (ymin = double.MaxValue);
		xmax = (ymax = double.MinValue);
	}

	public Rectangle(Rectangle other)
		: this(other.Left, other.Bottom, other.Right, other.Top)
	{
	}

	public Rectangle(double x, double y, double width, double height)
	{
		xmin = x;
		ymin = y;
		xmax = x + width;
		ymax = y + height;
	}

	public void Resize(double dx, double dy)
	{
		xmin -= dx;
		xmax += dx;
		ymin -= dy;
		ymax += dy;
	}

	public void Expand(Point p)
	{
		xmin = Math.Min(xmin, p.x);
		ymin = Math.Min(ymin, p.y);
		xmax = Math.Max(xmax, p.x);
		ymax = Math.Max(ymax, p.y);
	}

	public void Expand(IEnumerable<Point> points)
	{
		foreach (Point point in points)
		{
			Expand(point);
		}
	}

	public void Expand(Rectangle other)
	{
		xmin = Math.Min(xmin, other.xmin);
		ymin = Math.Min(ymin, other.ymin);
		xmax = Math.Max(xmax, other.xmax);
		ymax = Math.Max(ymax, other.ymax);
	}

	public bool Contains(double x, double y)
	{
		if (x >= xmin && x <= xmax && y >= ymin)
		{
			return y <= ymax;
		}
		return false;
	}

	public bool Contains(Point pt)
	{
		return Contains(pt.x, pt.y);
	}

	public bool Contains(Rectangle other)
	{
		if (xmin <= other.Left && other.Right <= xmax && ymin <= other.Bottom)
		{
			return other.Top <= ymax;
		}
		return false;
	}

	public bool Intersects(Rectangle other)
	{
		if (other.Left < xmax && xmin < other.Right && other.Bottom < ymax)
		{
			return ymin < other.Top;
		}
		return false;
	}
}
