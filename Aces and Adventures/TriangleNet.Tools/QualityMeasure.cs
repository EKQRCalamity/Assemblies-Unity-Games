using System;
using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace TriangleNet.Tools;

public class QualityMeasure
{
	private class AreaMeasure
	{
		public double area_min = double.MaxValue;

		public double area_max = double.MinValue;

		public double area_total;

		public int area_zero;

		public void Reset()
		{
			area_min = double.MaxValue;
			area_max = double.MinValue;
			area_total = 0.0;
			area_zero = 0;
		}

		public double Measure(Point a, Point b, Point c)
		{
			double num = 0.5 * Math.Abs(a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));
			area_min = Math.Min(area_min, num);
			area_max = Math.Max(area_max, num);
			area_total += num;
			if (num == 0.0)
			{
				area_zero++;
			}
			return num;
		}
	}

	private class AlphaMeasure
	{
		public double alpha_min;

		public double alpha_max;

		public double alpha_ave;

		public double alpha_area;

		public void Reset()
		{
			alpha_min = double.MaxValue;
			alpha_max = double.MinValue;
			alpha_ave = 0.0;
			alpha_area = 0.0;
		}

		private double acos(double c)
		{
			if (c <= -1.0)
			{
				return Math.PI;
			}
			if (1.0 <= c)
			{
				return 0.0;
			}
			return Math.Acos(c);
		}

		public double Measure(double ab, double bc, double ca, double area)
		{
			double val = double.MaxValue;
			double num = ab * ab;
			double num2 = bc * bc;
			double num3 = ca * ca;
			double val2;
			double val3;
			double val4;
			if (ab != 0.0 || bc != 0.0 || ca != 0.0)
			{
				val2 = ((ca != 0.0 && ab != 0.0) ? acos((num3 + num - num2) / (2.0 * ca * ab)) : Math.PI);
				val3 = ((ab != 0.0 && bc != 0.0) ? acos((num + num2 - num3) / (2.0 * ab * bc)) : Math.PI);
				val4 = ((bc != 0.0 && ca != 0.0) ? acos((num2 + num3 - num) / (2.0 * bc * ca)) : Math.PI);
			}
			else
			{
				val2 = Math.PI * 2.0 / 3.0;
				val3 = Math.PI * 2.0 / 3.0;
				val4 = Math.PI * 2.0 / 3.0;
			}
			val = Math.Min(val, val2);
			val = Math.Min(val, val3);
			val = Math.Min(val, val4);
			val = val * 3.0 / Math.PI;
			alpha_ave += val;
			alpha_area += area * val;
			alpha_min = Math.Min(val, alpha_min);
			alpha_max = Math.Max(val, alpha_max);
			return val;
		}

		public void Normalize(int n, double area_total)
		{
			if (n > 0)
			{
				alpha_ave /= n;
			}
			else
			{
				alpha_ave = 0.0;
			}
			if (0.0 < area_total)
			{
				alpha_area /= area_total;
			}
			else
			{
				alpha_area = 0.0;
			}
		}
	}

	private class Q_Measure
	{
		public double q_min;

		public double q_max;

		public double q_ave;

		public double q_area;

		public void Reset()
		{
			q_min = double.MaxValue;
			q_max = double.MinValue;
			q_ave = 0.0;
			q_area = 0.0;
		}

		public double Measure(double ab, double bc, double ca, double area)
		{
			double num = (bc + ca - ab) * (ca + ab - bc) * (ab + bc - ca) / (ab * bc * ca);
			q_min = Math.Min(q_min, num);
			q_max = Math.Max(q_max, num);
			q_ave += num;
			q_area += num * area;
			return num;
		}

		public void Normalize(int n, double area_total)
		{
			if (n > 0)
			{
				q_ave /= n;
			}
			else
			{
				q_ave = 0.0;
			}
			if (area_total > 0.0)
			{
				q_area /= area_total;
			}
			else
			{
				q_area = 0.0;
			}
		}
	}

	private AreaMeasure areaMeasure;

	private AlphaMeasure alphaMeasure;

	private Q_Measure qMeasure;

	private Mesh mesh;

	public double AreaMinimum => areaMeasure.area_min;

	public double AreaMaximum => areaMeasure.area_max;

	public double AreaRatio => areaMeasure.area_max / areaMeasure.area_min;

	public double AlphaMinimum => alphaMeasure.alpha_min;

	public double AlphaMaximum => alphaMeasure.alpha_max;

	public double AlphaAverage => alphaMeasure.alpha_ave;

	public double AlphaArea => alphaMeasure.alpha_area;

	public double Q_Minimum => qMeasure.q_min;

	public double Q_Maximum => qMeasure.q_max;

	public double Q_Average => qMeasure.q_ave;

	public double Q_Area => qMeasure.q_area;

	public QualityMeasure()
	{
		areaMeasure = new AreaMeasure();
		alphaMeasure = new AlphaMeasure();
		qMeasure = new Q_Measure();
	}

	public void Update(Mesh mesh)
	{
		this.mesh = mesh;
		areaMeasure.Reset();
		alphaMeasure.Reset();
		qMeasure.Reset();
		Compute();
	}

	private void Compute()
	{
		int num = 0;
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			num++;
			Point point = triangle.vertices[0];
			Point point2 = triangle.vertices[1];
			Point point3 = triangle.vertices[2];
			double num2 = point.x - point2.x;
			double num3 = point.y - point2.y;
			double ab = Math.Sqrt(num2 * num2 + num3 * num3);
			double num4 = point2.x - point3.x;
			num3 = point2.y - point3.y;
			double bc = Math.Sqrt(num4 * num4 + num3 * num3);
			double num5 = point3.x - point.x;
			num3 = point3.y - point.y;
			double ca = Math.Sqrt(num5 * num5 + num3 * num3);
			double area = areaMeasure.Measure(point, point2, point3);
			alphaMeasure.Measure(ab, bc, ca, area);
			qMeasure.Measure(ab, bc, ca, area);
		}
		alphaMeasure.Normalize(num, areaMeasure.area_total);
		qMeasure.Normalize(num, areaMeasure.area_total);
	}

	public int Bandwidth()
	{
		if (mesh == null)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			for (int i = 0; i < 3; i++)
			{
				int id = triangle.GetVertex(i).id;
				for (int j = 0; j < 3; j++)
				{
					int id2 = triangle.GetVertex(j).id;
					num2 = Math.Max(num2, id2 - id);
					num = Math.Max(num, id - id2);
				}
			}
		}
		return num + 1 + num2;
	}
}
