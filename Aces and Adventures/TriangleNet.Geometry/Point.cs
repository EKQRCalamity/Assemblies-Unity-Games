using System;

namespace TriangleNet.Geometry;

public class Point : IComparable<Point>, IEquatable<Point>
{
	internal int id;

	internal int label;

	internal double x;

	internal double y;

	internal double z;

	public int ID
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public double X
	{
		get
		{
			return x;
		}
		set
		{
			x = value;
		}
	}

	public double Y
	{
		get
		{
			return y;
		}
		set
		{
			y = value;
		}
	}

	public double Z
	{
		get
		{
			return z;
		}
		set
		{
			z = value;
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

	public Point()
		: this(0.0, 0.0, 0)
	{
	}

	public Point(double x, double y)
		: this(x, y, 0)
	{
	}

	public Point(double x, double y, int label)
	{
		this.x = x;
		this.y = y;
		this.label = label;
	}

	public static bool operator ==(Point a, Point b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		return a.Equals(b);
	}

	public static bool operator !=(Point a, Point b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Point point))
		{
			return false;
		}
		if (x == point.x)
		{
			return y == point.y;
		}
		return false;
	}

	public bool Equals(Point p)
	{
		if ((object)p == null)
		{
			return false;
		}
		if (x == p.x)
		{
			return y == p.y;
		}
		return false;
	}

	public int CompareTo(Point other)
	{
		if (x == other.x && y == other.y)
		{
			return 0;
		}
		if (!(x < other.x) && (x != other.x || !(y < other.y)))
		{
			return 1;
		}
		return -1;
	}

	public override int GetHashCode()
	{
		return (19 * 31 + x.GetHashCode()) * 31 + y.GetHashCode();
	}

	public override string ToString()
	{
		return $"[{x},{y}]";
	}
}
