using System;

namespace TriangleNet.Geometry;

public class RegionPointer
{
	internal Point point;

	internal int id;

	internal double area;

	public double Area
	{
		get
		{
			return area;
		}
		set
		{
			if (value < 0.0)
			{
				throw new ArgumentException("Area constraints must not be negative.");
			}
			area = value;
		}
	}

	public RegionPointer(double x, double y, int id)
		: this(x, y, id, 0.0)
	{
	}

	public RegionPointer(double x, double y, int id, double area)
	{
		point = new Point(x, y);
		this.id = id;
		this.area = area;
	}
}
