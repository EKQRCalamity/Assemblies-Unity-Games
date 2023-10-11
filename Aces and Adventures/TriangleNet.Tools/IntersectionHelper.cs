using TriangleNet.Geometry;

namespace TriangleNet.Tools;

public static class IntersectionHelper
{
	public static void IntersectSegments(Point p0, Point p1, Point q0, Point q1, ref Point c0)
	{
		double num = p1.x - p0.x;
		double num2 = p1.y - p0.y;
		double num3 = q1.x - q0.x;
		double num4 = q1.y - q0.y;
		double num5 = p0.x - q0.x;
		double num6 = p0.y - q0.y;
		double num7 = num * num4 - num2 * num3;
		double num8 = (num3 * num6 - num4 * num5) / num7;
		c0.x = p0.X + num8 * num;
		c0.y = p0.Y + num8 * num2;
	}

	public static bool LiangBarsky(Rectangle rect, Point p0, Point p1, ref Point c0, ref Point c1)
	{
		double left = rect.Left;
		double right = rect.Right;
		double bottom = rect.Bottom;
		double top = rect.Top;
		double x = p0.X;
		double y = p0.Y;
		double x2 = p1.X;
		double y2 = p1.Y;
		double num = 0.0;
		double num2 = 1.0;
		double num3 = x2 - x;
		double num4 = y2 - y;
		double num5 = 0.0;
		double num6 = 0.0;
		for (int i = 0; i < 4; i++)
		{
			if (i == 0)
			{
				num5 = 0.0 - num3;
				num6 = 0.0 - (left - x);
			}
			if (i == 1)
			{
				num5 = num3;
				num6 = right - x;
			}
			if (i == 2)
			{
				num5 = 0.0 - num4;
				num6 = 0.0 - (bottom - y);
			}
			if (i == 3)
			{
				num5 = num4;
				num6 = top - y;
			}
			double num7 = num6 / num5;
			if (num5 == 0.0 && num6 < 0.0)
			{
				return false;
			}
			if (num5 < 0.0)
			{
				if (num7 > num2)
				{
					return false;
				}
				if (num7 > num)
				{
					num = num7;
				}
			}
			else if (num5 > 0.0)
			{
				if (num7 < num)
				{
					return false;
				}
				if (num7 < num2)
				{
					num2 = num7;
				}
			}
		}
		c0.X = x + num * num3;
		c0.Y = y + num * num4;
		c1.X = x + num2 * num3;
		c1.Y = y + num2 * num4;
		return true;
	}

	public static bool BoxRayIntersection(Rectangle rect, Point p0, Point p1, ref Point c1)
	{
		double x = p0.X;
		double y = p0.Y;
		double num = p1.x - x;
		double num2 = p1.y - y;
		double left = rect.Left;
		double right = rect.Right;
		double bottom = rect.Bottom;
		double top = rect.Top;
		if (x < left || x > right || y < bottom || y > top)
		{
			return false;
		}
		double num3;
		double x2;
		double y2;
		if (num < 0.0)
		{
			num3 = (left - x) / num;
			x2 = left;
			y2 = y + num3 * num2;
		}
		else if (num > 0.0)
		{
			num3 = (right - x) / num;
			x2 = right;
			y2 = y + num3 * num2;
		}
		else
		{
			num3 = double.MaxValue;
			x2 = (y2 = 0.0);
		}
		double num4;
		double x3;
		double y3;
		if (num2 < 0.0)
		{
			num4 = (bottom - y) / num2;
			x3 = x + num4 * num;
			y3 = bottom;
		}
		else if (num2 > 0.0)
		{
			num4 = (top - y) / num2;
			x3 = x + num4 * num;
			y3 = top;
		}
		else
		{
			num4 = double.MaxValue;
			x3 = (y3 = 0.0);
		}
		if (num3 < num4)
		{
			c1.x = x2;
			c1.y = y2;
		}
		else
		{
			c1.x = x3;
			c1.y = y3;
		}
		return true;
	}
}
