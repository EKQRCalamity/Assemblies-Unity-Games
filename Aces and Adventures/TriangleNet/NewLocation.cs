using System;
using TriangleNet.Geometry;
using TriangleNet.Tools;
using TriangleNet.Topology;

namespace TriangleNet;

internal class NewLocation
{
	private const double EPS = 1E-50;

	private IPredicates predicates;

	private Mesh mesh;

	private Behavior behavior;

	private double[] petalx = new double[20];

	private double[] petaly = new double[20];

	private double[] petalr = new double[20];

	private double[] wedges = new double[500];

	private double[] initialConvexPoly = new double[500];

	private double[] points_p = new double[500];

	private double[] points_q = new double[500];

	private double[] points_r = new double[500];

	private double[] poly1 = new double[100];

	private double[] poly2 = new double[100];

	private double[][] polys = new double[3][];

	public NewLocation(Mesh mesh, IPredicates predicates)
	{
		this.mesh = mesh;
		this.predicates = predicates;
		behavior = mesh.behavior;
	}

	public Point FindLocation(Vertex org, Vertex dest, Vertex apex, ref double xi, ref double eta, bool offcenter, Otri badotri)
	{
		if (behavior.MaxAngle == 0.0)
		{
			return FindNewLocationWithoutMaxAngle(org, dest, apex, ref xi, ref eta, offcenter: true, badotri);
		}
		return FindNewLocation(org, dest, apex, ref xi, ref eta, offcenter: true, badotri);
	}

	private Point FindNewLocationWithoutMaxAngle(Vertex torg, Vertex tdest, Vertex tapex, ref double xi, ref double eta, bool offcenter, Otri badotri)
	{
		double offconstant = behavior.offconstant;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		int num6 = 0;
		int num7 = 0;
		Otri neighotri = default(Otri);
		double[] thirdpoint = new double[2];
		double xi2 = 0.0;
		double eta2 = 0.0;
		double[] p = new double[5];
		double[] p2 = new double[4];
		double num8 = 0.06;
		double num9 = 1.0;
		double num10 = 1.0;
		int num11 = 0;
		double[] newloc = new double[2];
		double num12 = 0.0;
		double num13 = 0.0;
		Statistic.CircumcenterCount++;
		double num14 = tdest.x - torg.x;
		double num15 = tdest.y - torg.y;
		double num16 = tapex.x - torg.x;
		double num17 = tapex.y - torg.y;
		double num18 = tapex.x - tdest.x;
		double num19 = tapex.y - tdest.y;
		double num20 = num14 * num14 + num15 * num15;
		double num21 = num16 * num16 + num17 * num17;
		double num22 = (tdest.x - tapex.x) * (tdest.x - tapex.x) + (tdest.y - tapex.y) * (tdest.y - tapex.y);
		double num23;
		if (Behavior.NoExact)
		{
			num23 = 0.5 / (num14 * num17 - num16 * num15);
		}
		else
		{
			num23 = 0.5 / predicates.CounterClockwise(tdest, tapex, torg);
			Statistic.CounterClockwiseCount--;
		}
		double num24 = (num17 * num20 - num15 * num21) * num23;
		double num25 = (num14 * num21 - num16 * num20) * num23;
		Point point = new Point(torg.x + num24, torg.y + num25);
		Otri deltri = badotri;
		num6 = LongestShortestEdge(num21, num22, num20);
		Point point2;
		Point point3;
		Point point4;
		switch (num6)
		{
		case 123:
			num = num16;
			num2 = num17;
			num3 = num21;
			num4 = num22;
			num5 = num20;
			point2 = tdest;
			point3 = torg;
			point4 = tapex;
			break;
		case 132:
			num = num16;
			num2 = num17;
			num3 = num21;
			num4 = num20;
			num5 = num22;
			point2 = tdest;
			point3 = tapex;
			point4 = torg;
			break;
		case 213:
			num = num18;
			num2 = num19;
			num3 = num22;
			num4 = num21;
			num5 = num20;
			point2 = torg;
			point3 = tdest;
			point4 = tapex;
			break;
		case 231:
			num = num18;
			num2 = num19;
			num3 = num22;
			num4 = num20;
			num5 = num21;
			point2 = torg;
			point3 = tapex;
			point4 = tdest;
			break;
		case 312:
			num = num14;
			num2 = num15;
			num3 = num20;
			num4 = num21;
			num5 = num22;
			point2 = tapex;
			point3 = tdest;
			point4 = torg;
			break;
		default:
			num = num14;
			num2 = num15;
			num3 = num20;
			num4 = num22;
			num5 = num21;
			point2 = tapex;
			point3 = torg;
			point4 = tdest;
			break;
		}
		if (offcenter && offconstant > 0.0)
		{
			switch (num6)
			{
			case 213:
			case 231:
			{
				double num26 = 0.5 * num - offconstant * num2;
				double num27 = 0.5 * num2 + offconstant * num;
				if (num26 * num26 + num27 * num27 < (num24 - num14) * (num24 - num14) + (num25 - num15) * (num25 - num15))
				{
					num24 = num14 + num26;
					num25 = num15 + num27;
				}
				else
				{
					num7 = 1;
				}
				break;
			}
			case 123:
			case 132:
			{
				double num26 = 0.5 * num + offconstant * num2;
				double num27 = 0.5 * num2 - offconstant * num;
				if (num26 * num26 + num27 * num27 < num24 * num24 + num25 * num25)
				{
					num24 = num26;
					num25 = num27;
				}
				else
				{
					num7 = 1;
				}
				break;
			}
			default:
			{
				double num26 = 0.5 * num - offconstant * num2;
				double num27 = 0.5 * num2 + offconstant * num;
				if (num26 * num26 + num27 * num27 < num24 * num24 + num25 * num25)
				{
					num24 = num26;
					num25 = num27;
				}
				else
				{
					num7 = 1;
				}
				break;
			}
			}
		}
		if (num7 == 1)
		{
			double num28 = (num4 + num3 - num5) / (2.0 * Math.Sqrt(num4) * Math.Sqrt(num3));
			bool flag = num28 < 0.0 || Math.Abs(num28 - 0.0) <= 1E-50;
			num11 = DoSmoothing(deltri, torg, tdest, tapex, ref newloc);
			if (num11 > 0)
			{
				Statistic.RelocationCount++;
				num24 = newloc[0] - torg.x;
				num25 = newloc[1] - torg.y;
				num12 = torg.x;
				num13 = torg.y;
				switch (num11)
				{
				case 1:
					mesh.DeleteVertex(ref deltri);
					break;
				case 2:
					deltri.Lnext();
					mesh.DeleteVertex(ref deltri);
					break;
				case 3:
					deltri.Lprev();
					mesh.DeleteVertex(ref deltri);
					break;
				}
			}
			else
			{
				double num29 = Math.Sqrt(num3) / (2.0 * Math.Sin(behavior.MinAngle * Math.PI / 180.0));
				double num30 = (point3.x + point4.x) / 2.0;
				double num31 = (point3.y + point4.y) / 2.0;
				double num32 = num30 + Math.Sqrt(num29 * num29 - num3 / 4.0) * (point3.y - point4.y) / Math.Sqrt(num3);
				double num33 = num31 + Math.Sqrt(num29 * num29 - num3 / 4.0) * (point4.x - point3.x) / Math.Sqrt(num3);
				double num34 = num30 - Math.Sqrt(num29 * num29 - num3 / 4.0) * (point3.y - point4.y) / Math.Sqrt(num3);
				double num35 = num31 - Math.Sqrt(num29 * num29 - num3 / 4.0) * (point4.x - point3.x) / Math.Sqrt(num3);
				double num36 = (num32 - point2.x) * (num32 - point2.x);
				double num37 = (num33 - point2.y) * (num33 - point2.y);
				double num38 = (num34 - point2.x) * (num34 - point2.x);
				double num39 = (num35 - point2.y) * (num35 - point2.y);
				double x;
				double y;
				if (num36 + num37 <= num38 + num39)
				{
					x = num32;
					y = num33;
				}
				else
				{
					x = num34;
					y = num35;
				}
				bool neighborsVertex = GetNeighborsVertex(badotri, point3.x, point3.y, point2.x, point2.y, ref thirdpoint, ref neighotri);
				double num40 = num24;
				double num41 = num25;
				if (!neighborsVertex)
				{
					Vertex org = neighotri.Org();
					Vertex dest = neighotri.Dest();
					Vertex apex = neighotri.Apex();
					Point point5 = predicates.FindCircumcenter(org, dest, apex, ref xi2, ref eta2);
					double num42 = point3.y - point2.y;
					double num43 = point2.x - point3.x;
					num42 = point.x + num42;
					num43 = point.y + num43;
					CircleLineIntersection(point.x, point.y, num42, num43, x, y, num29, ref p);
					double x2 = (point3.x + point2.x) / 2.0;
					double y2 = (point3.y + point2.y) / 2.0;
					double num44;
					double num45;
					if (ChooseCorrectPoint(x2, y2, p[3], p[4], point.x, point.y, flag))
					{
						num44 = p[3];
						num45 = p[4];
					}
					else
					{
						num44 = p[1];
						num45 = p[2];
					}
					PointBetweenPoints(num44, num45, point.x, point.y, point5.x, point5.y, ref p2);
					if (p[0] > 0.0)
					{
						if (Math.Abs(p2[0] - 1.0) <= 1E-50)
						{
							if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, point5.x, point5.y))
							{
								num40 = num24;
								num41 = num25;
							}
							else
							{
								num40 = p2[2] - torg.x;
								num41 = p2[3] - torg.y;
							}
						}
						else if (IsBadTriangleAngle(point4.x, point4.y, point3.x, point3.y, num44, num45))
						{
							double num46 = Math.Sqrt((num44 - point.x) * (num44 - point.x) + (num45 - point.y) * (num45 - point.y));
							double num47 = point.x - num44;
							double num48 = point.y - num45;
							num47 /= num46;
							num48 /= num46;
							num44 += num47 * num8 * Math.Sqrt(num3);
							num45 += num48 * num8 * Math.Sqrt(num3);
							if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num44, num45))
							{
								num40 = num24;
								num41 = num25;
							}
							else
							{
								num40 = num44 - torg.x;
								num41 = num45 - torg.y;
							}
						}
						else
						{
							num40 = num44 - torg.x;
							num41 = num45 - torg.y;
						}
						if ((point2.x - point.x) * (point2.x - point.x) + (point2.y - point.y) * (point2.y - point.y) > num9 * ((point2.x - (num40 + torg.x)) * (point2.x - (num40 + torg.x)) + (point2.y - (num41 + torg.y)) * (point2.y - (num41 + torg.y))))
						{
							num40 = num24;
							num41 = num25;
						}
					}
				}
				bool neighborsVertex2 = GetNeighborsVertex(badotri, point4.x, point4.y, point2.x, point2.y, ref thirdpoint, ref neighotri);
				double num49 = num24;
				double num50 = num25;
				if (!neighborsVertex2)
				{
					Vertex org = neighotri.Org();
					Vertex dest = neighotri.Dest();
					Vertex apex = neighotri.Apex();
					Point point5 = predicates.FindCircumcenter(org, dest, apex, ref xi2, ref eta2);
					double num42 = point4.y - point2.y;
					double num43 = point2.x - point4.x;
					num42 = point.x + num42;
					num43 = point.y + num43;
					CircleLineIntersection(point.x, point.y, num42, num43, x, y, num29, ref p);
					double x3 = (point4.x + point2.x) / 2.0;
					double y3 = (point4.y + point2.y) / 2.0;
					double num44;
					double num45;
					if (ChooseCorrectPoint(x3, y3, p[3], p[4], point.x, point.y, isObtuse: false))
					{
						num44 = p[3];
						num45 = p[4];
					}
					else
					{
						num44 = p[1];
						num45 = p[2];
					}
					PointBetweenPoints(num44, num45, point.x, point.y, point5.x, point5.y, ref p2);
					if (p[0] > 0.0)
					{
						if (Math.Abs(p2[0] - 1.0) <= 1E-50)
						{
							if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, point5.x, point5.y))
							{
								num49 = num24;
								num50 = num25;
							}
							else
							{
								num49 = p2[2] - torg.x;
								num50 = p2[3] - torg.y;
							}
						}
						else if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num44, num45))
						{
							double num46 = Math.Sqrt((num44 - point.x) * (num44 - point.x) + (num45 - point.y) * (num45 - point.y));
							double num47 = point.x - num44;
							double num48 = point.y - num45;
							num47 /= num46;
							num48 /= num46;
							num44 += num47 * num8 * Math.Sqrt(num3);
							num45 += num48 * num8 * Math.Sqrt(num3);
							if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num44, num45))
							{
								num49 = num24;
								num50 = num25;
							}
							else
							{
								num49 = num44 - torg.x;
								num50 = num45 - torg.y;
							}
						}
						else
						{
							num49 = num44 - torg.x;
							num50 = num45 - torg.y;
						}
						if ((point2.x - point.x) * (point2.x - point.x) + (point2.y - point.y) * (point2.y - point.y) > num9 * ((point2.x - (num49 + torg.x)) * (point2.x - (num49 + torg.x)) + (point2.y - (num50 + torg.y)) * (point2.y - (num50 + torg.y))))
						{
							num49 = num24;
							num50 = num25;
						}
					}
				}
				if (flag)
				{
					num24 = num40;
					num25 = num41;
				}
				else if (num10 * ((point2.x - (num49 + torg.x)) * (point2.x - (num49 + torg.x)) + (point2.y - (num50 + torg.y)) * (point2.y - (num50 + torg.y))) > (point2.x - (num40 + torg.x)) * (point2.x - (num40 + torg.x)) + (point2.y - (num41 + torg.y)) * (point2.y - (num41 + torg.y)))
				{
					num24 = num49;
					num25 = num50;
				}
				else
				{
					num24 = num40;
					num25 = num41;
				}
			}
		}
		Point point6 = new Point();
		if (num11 <= 0)
		{
			point6.x = torg.x + num24;
			point6.y = torg.y + num25;
		}
		else
		{
			point6.x = num12 + num24;
			point6.y = num13 + num25;
		}
		xi = (num17 * num24 - num16 * num25) * (2.0 * num23);
		eta = (num14 * num25 - num15 * num24) * (2.0 * num23);
		return point6;
	}

	private Point FindNewLocation(Vertex torg, Vertex tdest, Vertex tapex, ref double xi, ref double eta, bool offcenter, Otri badotri)
	{
		double offconstant = behavior.offconstant;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		int num6 = 0;
		int num7 = 0;
		Otri neighotri = default(Otri);
		double[] thirdpoint = new double[2];
		double xi2 = 0.0;
		double eta2 = 0.0;
		double[] p = new double[5];
		double[] p2 = new double[4];
		double num8 = 0.06;
		double num9 = 1.0;
		double num10 = 1.0;
		int num11 = 0;
		double[] newloc = new double[2];
		double num12 = 0.0;
		double num13 = 0.0;
		double num14 = 0.0;
		double num15 = 0.0;
		double[] p3 = new double[3];
		double[] p4 = new double[4];
		Statistic.CircumcenterCount++;
		double num16 = tdest.x - torg.x;
		double num17 = tdest.y - torg.y;
		double num18 = tapex.x - torg.x;
		double num19 = tapex.y - torg.y;
		double num20 = tapex.x - tdest.x;
		double num21 = tapex.y - tdest.y;
		double num22 = num16 * num16 + num17 * num17;
		double num23 = num18 * num18 + num19 * num19;
		double num24 = (tdest.x - tapex.x) * (tdest.x - tapex.x) + (tdest.y - tapex.y) * (tdest.y - tapex.y);
		double num25;
		if (Behavior.NoExact)
		{
			num25 = 0.5 / (num16 * num19 - num18 * num17);
		}
		else
		{
			num25 = 0.5 / predicates.CounterClockwise(tdest, tapex, torg);
			Statistic.CounterClockwiseCount--;
		}
		double num26 = (num19 * num22 - num17 * num23) * num25;
		double num27 = (num16 * num23 - num18 * num22) * num25;
		Point point = new Point(torg.x + num26, torg.y + num27);
		Otri deltri = badotri;
		num6 = LongestShortestEdge(num23, num24, num22);
		Point point2;
		Point point3;
		Point point4;
		switch (num6)
		{
		case 123:
			num = num18;
			num2 = num19;
			num3 = num23;
			num4 = num24;
			num5 = num22;
			point2 = tdest;
			point3 = torg;
			point4 = tapex;
			break;
		case 132:
			num = num18;
			num2 = num19;
			num3 = num23;
			num4 = num22;
			num5 = num24;
			point2 = tdest;
			point3 = tapex;
			point4 = torg;
			break;
		case 213:
			num = num20;
			num2 = num21;
			num3 = num24;
			num4 = num23;
			num5 = num22;
			point2 = torg;
			point3 = tdest;
			point4 = tapex;
			break;
		case 231:
			num = num20;
			num2 = num21;
			num3 = num24;
			num4 = num22;
			num5 = num23;
			point2 = torg;
			point3 = tapex;
			point4 = tdest;
			break;
		case 312:
			num = num16;
			num2 = num17;
			num3 = num22;
			num4 = num23;
			num5 = num24;
			point2 = tapex;
			point3 = tdest;
			point4 = torg;
			break;
		default:
			num = num16;
			num2 = num17;
			num3 = num22;
			num4 = num24;
			num5 = num23;
			point2 = tapex;
			point3 = torg;
			point4 = tdest;
			break;
		}
		if (offcenter && offconstant > 0.0)
		{
			switch (num6)
			{
			case 213:
			case 231:
			{
				double num28 = 0.5 * num - offconstant * num2;
				double num29 = 0.5 * num2 + offconstant * num;
				if (num28 * num28 + num29 * num29 < (num26 - num16) * (num26 - num16) + (num27 - num17) * (num27 - num17))
				{
					num26 = num16 + num28;
					num27 = num17 + num29;
				}
				else
				{
					num7 = 1;
				}
				break;
			}
			case 123:
			case 132:
			{
				double num28 = 0.5 * num + offconstant * num2;
				double num29 = 0.5 * num2 - offconstant * num;
				if (num28 * num28 + num29 * num29 < num26 * num26 + num27 * num27)
				{
					num26 = num28;
					num27 = num29;
				}
				else
				{
					num7 = 1;
				}
				break;
			}
			default:
			{
				double num28 = 0.5 * num - offconstant * num2;
				double num29 = 0.5 * num2 + offconstant * num;
				if (num28 * num28 + num29 * num29 < num26 * num26 + num27 * num27)
				{
					num26 = num28;
					num27 = num29;
				}
				else
				{
					num7 = 1;
				}
				break;
			}
			}
		}
		if (num7 == 1)
		{
			double num30 = (num4 + num3 - num5) / (2.0 * Math.Sqrt(num4) * Math.Sqrt(num3));
			bool flag = num30 < 0.0 || Math.Abs(num30 - 0.0) <= 1E-50;
			num11 = DoSmoothing(deltri, torg, tdest, tapex, ref newloc);
			if (num11 > 0)
			{
				Statistic.RelocationCount++;
				num26 = newloc[0] - torg.x;
				num27 = newloc[1] - torg.y;
				num12 = torg.x;
				num13 = torg.y;
				switch (num11)
				{
				case 1:
					mesh.DeleteVertex(ref deltri);
					break;
				case 2:
					deltri.Lnext();
					mesh.DeleteVertex(ref deltri);
					break;
				case 3:
					deltri.Lprev();
					mesh.DeleteVertex(ref deltri);
					break;
				}
			}
			else
			{
				double num31 = Math.Acos((num4 + num5 - num3) / (2.0 * Math.Sqrt(num4) * Math.Sqrt(num5))) * 180.0 / Math.PI;
				num31 = ((!(behavior.MinAngle > num31)) ? (num31 + 0.5) : behavior.MinAngle);
				double num32 = Math.Sqrt(num3) / (2.0 * Math.Sin(num31 * Math.PI / 180.0));
				double num33 = (point3.x + point4.x) / 2.0;
				double num34 = (point3.y + point4.y) / 2.0;
				double num35 = num33 + Math.Sqrt(num32 * num32 - num3 / 4.0) * (point3.y - point4.y) / Math.Sqrt(num3);
				double num36 = num34 + Math.Sqrt(num32 * num32 - num3 / 4.0) * (point4.x - point3.x) / Math.Sqrt(num3);
				double num37 = num33 - Math.Sqrt(num32 * num32 - num3 / 4.0) * (point3.y - point4.y) / Math.Sqrt(num3);
				double num38 = num34 - Math.Sqrt(num32 * num32 - num3 / 4.0) * (point4.x - point3.x) / Math.Sqrt(num3);
				double num39 = (num35 - point2.x) * (num35 - point2.x);
				double num40 = (num36 - point2.y) * (num36 - point2.y);
				double num41 = (num37 - point2.x) * (num37 - point2.x);
				double num42 = (num38 - point2.y) * (num38 - point2.y);
				double num43;
				double num44;
				if (num39 + num40 <= num41 + num42)
				{
					num43 = num35;
					num44 = num36;
				}
				else
				{
					num43 = num37;
					num44 = num38;
				}
				bool neighborsVertex = GetNeighborsVertex(badotri, point3.x, point3.y, point2.x, point2.y, ref thirdpoint, ref neighotri);
				double num45 = num26;
				double num46 = num27;
				double num47 = Math.Sqrt((num43 - num33) * (num43 - num33) + (num44 - num34) * (num44 - num34));
				double num48 = (num43 - num33) / num47;
				double num49 = (num44 - num34) / num47;
				double num50 = num43 + num48 * num32;
				double num51 = num44 + num49 * num32;
				double num52 = (2.0 * behavior.MaxAngle + num31 - 180.0) * Math.PI / 180.0;
				double num53 = num50 * Math.Cos(num52) + num51 * Math.Sin(num52) + num43 - num43 * Math.Cos(num52) - num44 * Math.Sin(num52);
				double num54 = (0.0 - num50) * Math.Sin(num52) + num51 * Math.Cos(num52) + num44 + num43 * Math.Sin(num52) - num44 * Math.Cos(num52);
				double num55 = num50 * Math.Cos(num52) - num51 * Math.Sin(num52) + num43 - num43 * Math.Cos(num52) + num44 * Math.Sin(num52);
				double num56 = num50 * Math.Sin(num52) + num51 * Math.Cos(num52) + num44 - num43 * Math.Sin(num52) - num44 * Math.Cos(num52);
				double num57;
				double num58;
				double num59;
				double num60;
				if (ChooseCorrectPoint(num55, num56, point3.x, point3.y, num53, num54, isObtuse: true))
				{
					num57 = num53;
					num58 = num54;
					num59 = num55;
					num60 = num56;
				}
				else
				{
					num57 = num55;
					num58 = num56;
					num59 = num53;
					num60 = num54;
				}
				double num61 = (point3.x + point2.x) / 2.0;
				double num62 = (point3.y + point2.y) / 2.0;
				if (!neighborsVertex)
				{
					Vertex org = neighotri.Org();
					Vertex dest = neighotri.Dest();
					Vertex apex = neighotri.Apex();
					Point point5 = predicates.FindCircumcenter(org, dest, apex, ref xi2, ref eta2);
					double num63 = point3.y - point2.y;
					double num64 = point2.x - point3.x;
					num63 = point.x + num63;
					num64 = point.y + num64;
					CircleLineIntersection(point.x, point.y, num63, num64, num43, num44, num32, ref p);
					double num65;
					double num66;
					if (ChooseCorrectPoint(num61, num62, p[3], p[4], point.x, point.y, flag))
					{
						num65 = p[3];
						num66 = p[4];
					}
					else
					{
						num65 = p[1];
						num66 = p[2];
					}
					double x = point3.x;
					double y = point3.y;
					num48 = point4.x - point3.x;
					num49 = point4.y - point3.y;
					double x2 = num57;
					double y2 = num58;
					LineLineIntersection(point.x, point.y, num63, num64, x, y, x2, y2, ref p3);
					if (p3[0] > 0.0)
					{
						num14 = p3[1];
						num15 = p3[2];
					}
					PointBetweenPoints(num65, num66, point.x, point.y, point5.x, point5.y, ref p2);
					if (p[0] > 0.0)
					{
						if (Math.Abs(p2[0] - 1.0) <= 1E-50)
						{
							PointBetweenPoints(p2[2], p2[3], point.x, point.y, num14, num15, ref p4);
							if (Math.Abs(p4[0] - 1.0) <= 1E-50 && p3[0] > 0.0)
							{
								if ((point2.x - num57) * (point2.x - num57) + (point2.y - num58) * (point2.y - num58) > num9 * ((point2.x - num14) * (point2.x - num14) + (point2.y - num15) * (point2.y - num15)) && IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num57, num58) && MinDistanceToNeighbor(num57, num58, ref neighotri) > MinDistanceToNeighbor(num14, num15, ref neighotri))
								{
									num45 = num57 - torg.x;
									num46 = num58 - torg.y;
								}
								else if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num14, num15))
								{
									double num67 = Math.Sqrt((num14 - point.x) * (num14 - point.x) + (num15 - point.y) * (num15 - point.y));
									double num68 = point.x - num14;
									double num69 = point.y - num15;
									num68 /= num67;
									num69 /= num67;
									num14 += num68 * num8 * Math.Sqrt(num3);
									num15 += num69 * num8 * Math.Sqrt(num3);
									if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num14, num15))
									{
										num45 = num26;
										num46 = num27;
									}
									else
									{
										num45 = num14 - torg.x;
										num46 = num15 - torg.y;
									}
								}
								else
								{
									num45 = p4[2] - torg.x;
									num46 = p4[3] - torg.y;
								}
							}
							else if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, point5.x, point5.y))
							{
								num45 = num26;
								num46 = num27;
							}
							else
							{
								num45 = p2[2] - torg.x;
								num46 = p2[3] - torg.y;
							}
						}
						else
						{
							PointBetweenPoints(num65, num66, point.x, point.y, num14, num15, ref p4);
							if (Math.Abs(p4[0] - 1.0) <= 1E-50 && p3[0] > 0.0)
							{
								if ((point2.x - num57) * (point2.x - num57) + (point2.y - num58) * (point2.y - num58) > num9 * ((point2.x - num14) * (point2.x - num14) + (point2.y - num15) * (point2.y - num15)) && IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num57, num58) && MinDistanceToNeighbor(num57, num58, ref neighotri) > MinDistanceToNeighbor(num14, num15, ref neighotri))
								{
									num45 = num57 - torg.x;
									num46 = num58 - torg.y;
								}
								else if (IsBadTriangleAngle(point4.x, point4.y, point3.x, point3.y, num14, num15))
								{
									double num67 = Math.Sqrt((num14 - point.x) * (num14 - point.x) + (num15 - point.y) * (num15 - point.y));
									double num68 = point.x - num14;
									double num69 = point.y - num15;
									num68 /= num67;
									num69 /= num67;
									num14 += num68 * num8 * Math.Sqrt(num3);
									num15 += num69 * num8 * Math.Sqrt(num3);
									if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num14, num15))
									{
										num45 = num26;
										num46 = num27;
									}
									else
									{
										num45 = num14 - torg.x;
										num46 = num15 - torg.y;
									}
								}
								else
								{
									num45 = p4[2] - torg.x;
									num46 = p4[3] - torg.y;
								}
							}
							else if (IsBadTriangleAngle(point4.x, point4.y, point3.x, point3.y, num65, num66))
							{
								double num67 = Math.Sqrt((num65 - point.x) * (num65 - point.x) + (num66 - point.y) * (num66 - point.y));
								double num68 = point.x - num65;
								double num69 = point.y - num66;
								num68 /= num67;
								num69 /= num67;
								num65 += num68 * num8 * Math.Sqrt(num3);
								num66 += num69 * num8 * Math.Sqrt(num3);
								if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num65, num66))
								{
									num45 = num26;
									num46 = num27;
								}
								else
								{
									num45 = num65 - torg.x;
									num46 = num66 - torg.y;
								}
							}
							else
							{
								num45 = num65 - torg.x;
								num46 = num66 - torg.y;
							}
						}
						if ((point2.x - point.x) * (point2.x - point.x) + (point2.y - point.y) * (point2.y - point.y) > num9 * ((point2.x - (num45 + torg.x)) * (point2.x - (num45 + torg.x)) + (point2.y - (num46 + torg.y)) * (point2.y - (num46 + torg.y))))
						{
							num45 = num26;
							num46 = num27;
						}
					}
				}
				bool neighborsVertex2 = GetNeighborsVertex(badotri, point4.x, point4.y, point2.x, point2.y, ref thirdpoint, ref neighotri);
				double num70 = num26;
				double num71 = num27;
				double num72 = (point4.x + point2.x) / 2.0;
				double num73 = (point4.y + point2.y) / 2.0;
				if (!neighborsVertex2)
				{
					Vertex org = neighotri.Org();
					Vertex dest = neighotri.Dest();
					Vertex apex = neighotri.Apex();
					Point point5 = predicates.FindCircumcenter(org, dest, apex, ref xi2, ref eta2);
					double num63 = point4.y - point2.y;
					double num64 = point2.x - point4.x;
					num63 = point.x + num63;
					num64 = point.y + num64;
					CircleLineIntersection(point.x, point.y, num63, num64, num43, num44, num32, ref p);
					double num65;
					double num66;
					if (ChooseCorrectPoint(num72, num73, p[3], p[4], point.x, point.y, isObtuse: false))
					{
						num65 = p[3];
						num66 = p[4];
					}
					else
					{
						num65 = p[1];
						num66 = p[2];
					}
					double x = point4.x;
					double y = point4.y;
					num48 = point3.x - point4.x;
					num49 = point3.y - point4.y;
					double x2 = num59;
					double y2 = num60;
					LineLineIntersection(point.x, point.y, num63, num64, x, y, x2, y2, ref p3);
					if (p3[0] > 0.0)
					{
						num14 = p3[1];
						num15 = p3[2];
					}
					PointBetweenPoints(num65, num66, point.x, point.y, point5.x, point5.y, ref p2);
					if (p[0] > 0.0)
					{
						if (Math.Abs(p2[0] - 1.0) <= 1E-50)
						{
							PointBetweenPoints(p2[2], p2[3], point.x, point.y, num14, num15, ref p4);
							if (Math.Abs(p4[0] - 1.0) <= 1E-50 && p3[0] > 0.0)
							{
								if ((point2.x - num59) * (point2.x - num59) + (point2.y - num60) * (point2.y - num60) > num9 * ((point2.x - num14) * (point2.x - num14) + (point2.y - num15) * (point2.y - num15)) && IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num59, num60) && MinDistanceToNeighbor(num59, num60, ref neighotri) > MinDistanceToNeighbor(num14, num15, ref neighotri))
								{
									num70 = num59 - torg.x;
									num71 = num60 - torg.y;
								}
								else if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num14, num15))
								{
									double num67 = Math.Sqrt((num14 - point.x) * (num14 - point.x) + (num15 - point.y) * (num15 - point.y));
									double num68 = point.x - num14;
									double num69 = point.y - num15;
									num68 /= num67;
									num69 /= num67;
									num14 += num68 * num8 * Math.Sqrt(num3);
									num15 += num69 * num8 * Math.Sqrt(num3);
									if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num14, num15))
									{
										num70 = num26;
										num71 = num27;
									}
									else
									{
										num70 = num14 - torg.x;
										num71 = num15 - torg.y;
									}
								}
								else
								{
									num70 = p4[2] - torg.x;
									num71 = p4[3] - torg.y;
								}
							}
							else if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, point5.x, point5.y))
							{
								num70 = num26;
								num71 = num27;
							}
							else
							{
								num70 = p2[2] - torg.x;
								num71 = p2[3] - torg.y;
							}
						}
						else
						{
							PointBetweenPoints(num65, num66, point.x, point.y, num14, num15, ref p4);
							if (Math.Abs(p4[0] - 1.0) <= 1E-50 && p3[0] > 0.0)
							{
								if ((point2.x - num59) * (point2.x - num59) + (point2.y - num60) * (point2.y - num60) > num9 * ((point2.x - num14) * (point2.x - num14) + (point2.y - num15) * (point2.y - num15)) && IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num59, num60) && MinDistanceToNeighbor(num59, num60, ref neighotri) > MinDistanceToNeighbor(num14, num15, ref neighotri))
								{
									num70 = num59 - torg.x;
									num71 = num60 - torg.y;
								}
								else if (IsBadTriangleAngle(point4.x, point4.y, point3.x, point3.y, num14, num15))
								{
									double num67 = Math.Sqrt((num14 - point.x) * (num14 - point.x) + (num15 - point.y) * (num15 - point.y));
									double num68 = point.x - num14;
									double num69 = point.y - num15;
									num68 /= num67;
									num69 /= num67;
									num14 += num68 * num8 * Math.Sqrt(num3);
									num15 += num69 * num8 * Math.Sqrt(num3);
									if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num14, num15))
									{
										num70 = num26;
										num71 = num27;
									}
									else
									{
										num70 = num14 - torg.x;
										num71 = num15 - torg.y;
									}
								}
								else
								{
									num70 = p4[2] - torg.x;
									num71 = p4[3] - torg.y;
								}
							}
							else if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num65, num66))
							{
								double num67 = Math.Sqrt((num65 - point.x) * (num65 - point.x) + (num66 - point.y) * (num66 - point.y));
								double num68 = point.x - num65;
								double num69 = point.y - num66;
								num68 /= num67;
								num69 /= num67;
								num65 += num68 * num8 * Math.Sqrt(num3);
								num66 += num69 * num8 * Math.Sqrt(num3);
								if (IsBadTriangleAngle(point3.x, point3.y, point4.x, point4.y, num65, num66))
								{
									num70 = num26;
									num71 = num27;
								}
								else
								{
									num70 = num65 - torg.x;
									num71 = num66 - torg.y;
								}
							}
							else
							{
								num70 = num65 - torg.x;
								num71 = num66 - torg.y;
							}
						}
						if ((point2.x - point.x) * (point2.x - point.x) + (point2.y - point.y) * (point2.y - point.y) > num9 * ((point2.x - (num70 + torg.x)) * (point2.x - (num70 + torg.x)) + (point2.y - (num71 + torg.y)) * (point2.y - (num71 + torg.y))))
						{
							num70 = num26;
							num71 = num27;
						}
					}
				}
				if (flag)
				{
					if (neighborsVertex && neighborsVertex2)
					{
						if (num10 * ((point2.x - num72) * (point2.x - num72) + (point2.y - num73) * (point2.y - num73)) > (point2.x - num61) * (point2.x - num61) + (point2.y - num62) * (point2.y - num62))
						{
							num26 = num70;
							num27 = num71;
						}
						else
						{
							num26 = num45;
							num27 = num46;
						}
					}
					else if (neighborsVertex)
					{
						if (num10 * ((point2.x - (num70 + torg.x)) * (point2.x - (num70 + torg.x)) + (point2.y - (num71 + torg.y)) * (point2.y - (num71 + torg.y))) > (point2.x - num61) * (point2.x - num61) + (point2.y - num62) * (point2.y - num62))
						{
							num26 = num70;
							num27 = num71;
						}
						else
						{
							num26 = num45;
							num27 = num46;
						}
					}
					else if (neighborsVertex2)
					{
						if (num10 * ((point2.x - num72) * (point2.x - num72) + (point2.y - num73) * (point2.y - num73)) > (point2.x - (num45 + torg.x)) * (point2.x - (num45 + torg.x)) + (point2.y - (num46 + torg.y)) * (point2.y - (num46 + torg.y)))
						{
							num26 = num70;
							num27 = num71;
						}
						else
						{
							num26 = num45;
							num27 = num46;
						}
					}
					else if (num10 * ((point2.x - (num70 + torg.x)) * (point2.x - (num70 + torg.x)) + (point2.y - (num71 + torg.y)) * (point2.y - (num71 + torg.y))) > (point2.x - (num45 + torg.x)) * (point2.x - (num45 + torg.x)) + (point2.y - (num46 + torg.y)) * (point2.y - (num46 + torg.y)))
					{
						num26 = num70;
						num27 = num71;
					}
					else
					{
						num26 = num45;
						num27 = num46;
					}
				}
				else if (neighborsVertex && neighborsVertex2)
				{
					if (num10 * ((point2.x - num72) * (point2.x - num72) + (point2.y - num73) * (point2.y - num73)) > (point2.x - num61) * (point2.x - num61) + (point2.y - num62) * (point2.y - num62))
					{
						num26 = num70;
						num27 = num71;
					}
					else
					{
						num26 = num45;
						num27 = num46;
					}
				}
				else if (neighborsVertex)
				{
					if (num10 * ((point2.x - (num70 + torg.x)) * (point2.x - (num70 + torg.x)) + (point2.y - (num71 + torg.y)) * (point2.y - (num71 + torg.y))) > (point2.x - num61) * (point2.x - num61) + (point2.y - num62) * (point2.y - num62))
					{
						num26 = num70;
						num27 = num71;
					}
					else
					{
						num26 = num45;
						num27 = num46;
					}
				}
				else if (neighborsVertex2)
				{
					if (num10 * ((point2.x - num72) * (point2.x - num72) + (point2.y - num73) * (point2.y - num73)) > (point2.x - (num45 + torg.x)) * (point2.x - (num45 + torg.x)) + (point2.y - (num46 + torg.y)) * (point2.y - (num46 + torg.y)))
					{
						num26 = num70;
						num27 = num71;
					}
					else
					{
						num26 = num45;
						num27 = num46;
					}
				}
				else if (num10 * ((point2.x - (num70 + torg.x)) * (point2.x - (num70 + torg.x)) + (point2.y - (num71 + torg.y)) * (point2.y - (num71 + torg.y))) > (point2.x - (num45 + torg.x)) * (point2.x - (num45 + torg.x)) + (point2.y - (num46 + torg.y)) * (point2.y - (num46 + torg.y)))
				{
					num26 = num70;
					num27 = num71;
				}
				else
				{
					num26 = num45;
					num27 = num46;
				}
			}
		}
		Point point6 = new Point();
		if (num11 <= 0)
		{
			point6.x = torg.x + num26;
			point6.y = torg.y + num27;
		}
		else
		{
			point6.x = num12 + num26;
			point6.y = num13 + num27;
		}
		xi = (num19 * num26 - num18 * num27) * (2.0 * num25);
		eta = (num16 * num27 - num17 * num26) * (2.0 * num25);
		return point6;
	}

	private int LongestShortestEdge(double aodist, double dadist, double dodist)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (dodist < aodist && dodist < dadist)
		{
			num2 = 3;
			if (aodist < dadist)
			{
				num = 2;
				num3 = 1;
			}
			else
			{
				num = 1;
				num3 = 2;
			}
		}
		else if (aodist < dadist)
		{
			num2 = 1;
			if (dodist < dadist)
			{
				num = 2;
				num3 = 3;
			}
			else
			{
				num = 3;
				num3 = 2;
			}
		}
		else
		{
			num2 = 2;
			if (aodist < dodist)
			{
				num = 3;
				num3 = 1;
			}
			else
			{
				num = 1;
				num3 = 3;
			}
		}
		return num2 * 100 + num3 * 10 + num;
	}

	private int DoSmoothing(Otri badotri, Vertex torg, Vertex tdest, Vertex tapex, ref double[] newloc)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		double[] array = new double[6];
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		bool flag = false;
		num = GetStarPoints(badotri, torg, tdest, tapex, 1, ref points_p);
		if (torg.type == VertexType.FreeVertex && num != 0 && ValidPolygonAngles(num, points_p) && ((behavior.MaxAngle != 0.0) ? GetWedgeIntersection(num, points_p, ref newloc) : GetWedgeIntersectionWithoutMaxAngle(num, points_p, ref newloc)))
		{
			array[0] = newloc[0];
			array[1] = newloc[1];
			num4++;
			num5 = 1;
		}
		num2 = GetStarPoints(badotri, torg, tdest, tapex, 2, ref points_q);
		if (tdest.type == VertexType.FreeVertex && num2 != 0 && ValidPolygonAngles(num2, points_q) && ((behavior.MaxAngle != 0.0) ? GetWedgeIntersection(num2, points_q, ref newloc) : GetWedgeIntersectionWithoutMaxAngle(num2, points_q, ref newloc)))
		{
			array[2] = newloc[0];
			array[3] = newloc[1];
			num4++;
			num6 = 2;
		}
		num3 = GetStarPoints(badotri, torg, tdest, tapex, 3, ref points_r);
		if (tapex.type == VertexType.FreeVertex && num3 != 0 && ValidPolygonAngles(num3, points_r) && ((behavior.MaxAngle != 0.0) ? GetWedgeIntersection(num3, points_r, ref newloc) : GetWedgeIntersectionWithoutMaxAngle(num3, points_r, ref newloc)))
		{
			array[4] = newloc[0];
			array[5] = newloc[1];
			num4++;
			num7 = 3;
		}
		if (num4 > 0)
		{
			if (num5 > 0)
			{
				newloc[0] = array[0];
				newloc[1] = array[1];
				return num5;
			}
			if (num6 > 0)
			{
				newloc[0] = array[2];
				newloc[1] = array[3];
				return num6;
			}
			if (num7 > 0)
			{
				newloc[0] = array[4];
				newloc[1] = array[5];
				return num7;
			}
		}
		return 0;
	}

	private int GetStarPoints(Otri badotri, Vertex p, Vertex q, Vertex r, int whichPoint, ref double[] points)
	{
		Otri neighotri = default(Otri);
		double first_x = 0.0;
		double first_y = 0.0;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double[] thirdpoint = new double[2];
		int num5 = 0;
		switch (whichPoint)
		{
		case 1:
			first_x = p.x;
			first_y = p.y;
			num = r.x;
			num2 = r.y;
			num3 = q.x;
			num4 = q.y;
			break;
		case 2:
			first_x = q.x;
			first_y = q.y;
			num = p.x;
			num2 = p.y;
			num3 = r.x;
			num4 = r.y;
			break;
		case 3:
			first_x = r.x;
			first_y = r.y;
			num = q.x;
			num2 = q.y;
			num3 = p.x;
			num4 = p.y;
			break;
		}
		Otri badotri2 = badotri;
		points[num5] = num;
		num5++;
		points[num5] = num2;
		num5++;
		thirdpoint[0] = num;
		thirdpoint[1] = num2;
		do
		{
			if (!GetNeighborsVertex(badotri2, first_x, first_y, num, num2, ref thirdpoint, ref neighotri))
			{
				badotri2 = neighotri;
				num = thirdpoint[0];
				num2 = thirdpoint[1];
				points[num5] = thirdpoint[0];
				num5++;
				points[num5] = thirdpoint[1];
				num5++;
				continue;
			}
			num5 = 0;
			break;
		}
		while (!(Math.Abs(thirdpoint[0] - num3) <= 1E-50) || !(Math.Abs(thirdpoint[1] - num4) <= 1E-50));
		return num5 / 2;
	}

	private bool GetNeighborsVertex(Otri badotri, double first_x, double first_y, double second_x, double second_y, ref double[] thirdpoint, ref Otri neighotri)
	{
		Otri ot = default(Otri);
		bool result = false;
		Vertex vertex = null;
		Vertex vertex2 = null;
		Vertex vertex3 = null;
		int num = 0;
		int num2 = 0;
		badotri.orient = 0;
		while (badotri.orient < 3)
		{
			badotri.Sym(ref ot);
			if (ot.tri.id != -1)
			{
				vertex = ot.Org();
				vertex2 = ot.Dest();
				vertex3 = ot.Apex();
				if ((vertex.x != vertex2.x || vertex.y != vertex2.y) && (vertex2.x != vertex3.x || vertex2.y != vertex3.y) && (vertex.x != vertex3.x || vertex.y != vertex3.y))
				{
					num = 0;
					if (Math.Abs(first_x - vertex.x) < 1E-50 && Math.Abs(first_y - vertex.y) < 1E-50)
					{
						num = 11;
					}
					else if (Math.Abs(first_x - vertex2.x) < 1E-50 && Math.Abs(first_y - vertex2.y) < 1E-50)
					{
						num = 12;
					}
					else if (Math.Abs(first_x - vertex3.x) < 1E-50 && Math.Abs(first_y - vertex3.y) < 1E-50)
					{
						num = 13;
					}
					num2 = 0;
					if (Math.Abs(second_x - vertex.x) < 1E-50 && Math.Abs(second_y - vertex.y) < 1E-50)
					{
						num2 = 21;
					}
					else if (Math.Abs(second_x - vertex2.x) < 1E-50 && Math.Abs(second_y - vertex2.y) < 1E-50)
					{
						num2 = 22;
					}
					else if (Math.Abs(second_x - vertex3.x) < 1E-50 && Math.Abs(second_y - vertex3.y) < 1E-50)
					{
						num2 = 23;
					}
				}
			}
			if ((num == 11 && (num2 == 22 || num2 == 23)) || (num == 12 && (num2 == 21 || num2 == 23)) || (num == 13 && (num2 == 21 || num2 == 22)))
			{
				break;
			}
			badotri.orient++;
		}
		switch (num)
		{
		case 0:
			result = true;
			break;
		case 11:
			switch (num2)
			{
			case 22:
				thirdpoint[0] = vertex3.x;
				thirdpoint[1] = vertex3.y;
				break;
			case 23:
				thirdpoint[0] = vertex2.x;
				thirdpoint[1] = vertex2.y;
				break;
			default:
				result = true;
				break;
			}
			break;
		case 12:
			switch (num2)
			{
			case 21:
				thirdpoint[0] = vertex3.x;
				thirdpoint[1] = vertex3.y;
				break;
			case 23:
				thirdpoint[0] = vertex.x;
				thirdpoint[1] = vertex.y;
				break;
			default:
				result = true;
				break;
			}
			break;
		case 13:
			switch (num2)
			{
			case 21:
				thirdpoint[0] = vertex2.x;
				thirdpoint[1] = vertex2.y;
				break;
			case 22:
				thirdpoint[0] = vertex.x;
				thirdpoint[1] = vertex.y;
				break;
			default:
				result = true;
				break;
			}
			break;
		default:
			if (num2 == 0)
			{
				result = true;
			}
			break;
		}
		neighotri = ot;
		return result;
	}

	private bool GetWedgeIntersectionWithoutMaxAngle(int numpoints, double[] points, ref double[] newloc)
	{
		if (2 * numpoints > petalx.Length)
		{
			petalx = new double[2 * numpoints];
			petaly = new double[2 * numpoints];
			petalr = new double[2 * numpoints];
			wedges = new double[2 * numpoints * 16 + 36];
		}
		double[] p = new double[3];
		int num = 0;
		double num2 = points[2 * numpoints - 4];
		double num3 = points[2 * numpoints - 3];
		double num4 = points[2 * numpoints - 2];
		double num5 = points[2 * numpoints - 1];
		double num6 = behavior.MinAngle * Math.PI / 180.0;
		double num7;
		double num8;
		if (behavior.goodAngle == 1.0)
		{
			num7 = 0.0;
			num8 = 0.0;
		}
		else
		{
			num7 = 0.5 / Math.Tan(num6);
			num8 = 0.5 / Math.Sin(num6);
		}
		for (int i = 0; i < numpoints * 2; i += 2)
		{
			double num9 = points[i];
			double num10 = points[i + 1];
			double num11 = num4 - num2;
			double num12 = num5 - num3;
			double num13 = Math.Sqrt(num11 * num11 + num12 * num12);
			petalx[i / 2] = num2 + 0.5 * num11 - num7 * num12;
			petaly[i / 2] = num3 + 0.5 * num12 + num7 * num11;
			petalr[i / 2] = num8 * num13;
			petalx[numpoints + i / 2] = petalx[i / 2];
			petaly[numpoints + i / 2] = petaly[i / 2];
			petalr[numpoints + i / 2] = petalr[i / 2];
			double num14 = (num2 + num4) / 2.0;
			double num15 = (num3 + num5) / 2.0;
			double num16 = Math.Sqrt((petalx[i / 2] - num14) * (petalx[i / 2] - num14) + (petaly[i / 2] - num15) * (petaly[i / 2] - num15));
			double num17 = (petalx[i / 2] - num14) / num16;
			double num18 = (petaly[i / 2] - num15) / num16;
			double num19 = petalx[i / 2] + num17 * petalr[i / 2];
			double num20 = petaly[i / 2] + num18 * petalr[i / 2];
			num17 = num4 - num2;
			num18 = num5 - num3;
			double num21 = num4 * Math.Cos(num6) - num5 * Math.Sin(num6) + num2 - num2 * Math.Cos(num6) + num3 * Math.Sin(num6);
			double num22 = num4 * Math.Sin(num6) + num5 * Math.Cos(num6) + num3 - num2 * Math.Sin(num6) - num3 * Math.Cos(num6);
			wedges[i * 16] = num2;
			wedges[i * 16 + 1] = num3;
			wedges[i * 16 + 2] = num21;
			wedges[i * 16 + 3] = num22;
			num17 = num2 - num4;
			num18 = num3 - num5;
			double num23 = num2 * Math.Cos(num6) + num3 * Math.Sin(num6) + num4 - num4 * Math.Cos(num6) - num5 * Math.Sin(num6);
			double num24 = (0.0 - num2) * Math.Sin(num6) + num3 * Math.Cos(num6) + num5 + num4 * Math.Sin(num6) - num5 * Math.Cos(num6);
			wedges[i * 16 + 4] = num23;
			wedges[i * 16 + 5] = num24;
			wedges[i * 16 + 6] = num4;
			wedges[i * 16 + 7] = num5;
			num17 = num19 - petalx[i / 2];
			num18 = num20 - petaly[i / 2];
			double num25 = num19;
			double num26 = num20;
			for (int j = 1; j < 4; j++)
			{
				double num27 = num19 * Math.Cos((Math.PI / 3.0 - num6) * (double)j) + num20 * Math.Sin((Math.PI / 3.0 - num6) * (double)j) + petalx[i / 2] - petalx[i / 2] * Math.Cos((Math.PI / 3.0 - num6) * (double)j) - petaly[i / 2] * Math.Sin((Math.PI / 3.0 - num6) * (double)j);
				double num28 = (0.0 - num19) * Math.Sin((Math.PI / 3.0 - num6) * (double)j) + num20 * Math.Cos((Math.PI / 3.0 - num6) * (double)j) + petaly[i / 2] + petalx[i / 2] * Math.Sin((Math.PI / 3.0 - num6) * (double)j) - petaly[i / 2] * Math.Cos((Math.PI / 3.0 - num6) * (double)j);
				wedges[i * 16 + 8 + 4 * (j - 1)] = num27;
				wedges[i * 16 + 9 + 4 * (j - 1)] = num28;
				wedges[i * 16 + 10 + 4 * (j - 1)] = num25;
				wedges[i * 16 + 11 + 4 * (j - 1)] = num26;
				num25 = num27;
				num26 = num28;
			}
			num25 = num19;
			num26 = num20;
			for (int j = 1; j < 4; j++)
			{
				double num29 = num19 * Math.Cos((Math.PI / 3.0 - num6) * (double)j) - num20 * Math.Sin((Math.PI / 3.0 - num6) * (double)j) + petalx[i / 2] - petalx[i / 2] * Math.Cos((Math.PI / 3.0 - num6) * (double)j) + petaly[i / 2] * Math.Sin((Math.PI / 3.0 - num6) * (double)j);
				double num30 = num19 * Math.Sin((Math.PI / 3.0 - num6) * (double)j) + num20 * Math.Cos((Math.PI / 3.0 - num6) * (double)j) + petaly[i / 2] - petalx[i / 2] * Math.Sin((Math.PI / 3.0 - num6) * (double)j) - petaly[i / 2] * Math.Cos((Math.PI / 3.0 - num6) * (double)j);
				wedges[i * 16 + 20 + 4 * (j - 1)] = num25;
				wedges[i * 16 + 21 + 4 * (j - 1)] = num26;
				wedges[i * 16 + 22 + 4 * (j - 1)] = num29;
				wedges[i * 16 + 23 + 4 * (j - 1)] = num30;
				num25 = num29;
				num26 = num30;
			}
			if (i == 0)
			{
				LineLineIntersection(num2, num3, num21, num22, num4, num5, num23, num24, ref p);
				if (p[0] == 1.0)
				{
					initialConvexPoly[0] = p[1];
					initialConvexPoly[1] = p[2];
					initialConvexPoly[2] = wedges[i * 16 + 16];
					initialConvexPoly[3] = wedges[i * 16 + 17];
					initialConvexPoly[4] = wedges[i * 16 + 12];
					initialConvexPoly[5] = wedges[i * 16 + 13];
					initialConvexPoly[6] = wedges[i * 16 + 8];
					initialConvexPoly[7] = wedges[i * 16 + 9];
					initialConvexPoly[8] = num19;
					initialConvexPoly[9] = num20;
					initialConvexPoly[10] = wedges[i * 16 + 22];
					initialConvexPoly[11] = wedges[i * 16 + 23];
					initialConvexPoly[12] = wedges[i * 16 + 26];
					initialConvexPoly[13] = wedges[i * 16 + 27];
					initialConvexPoly[14] = wedges[i * 16 + 30];
					initialConvexPoly[15] = wedges[i * 16 + 31];
				}
			}
			num2 = num4;
			num3 = num5;
			num4 = num9;
			num5 = num10;
		}
		if (numpoints != 0)
		{
			int num31 = (numpoints - 1) / 2 + 1;
			int num32 = 0;
			int num33 = 0;
			int i = 1;
			int numvertices = 8;
			for (int j = 0; j < 32; j += 4)
			{
				num = HalfPlaneIntersection(numvertices, ref initialConvexPoly, wedges[32 * num31 + j], wedges[32 * num31 + 1 + j], wedges[32 * num31 + 2 + j], wedges[32 * num31 + 3 + j]);
				if (num == 0)
				{
					return false;
				}
				numvertices = num;
			}
			for (num33++; num33 < numpoints - 1; num33++)
			{
				for (int j = 0; j < 32; j += 4)
				{
					num = HalfPlaneIntersection(numvertices, ref initialConvexPoly, wedges[32 * (i + num31 * num32) + j], wedges[32 * (i + num31 * num32) + 1 + j], wedges[32 * (i + num31 * num32) + 2 + j], wedges[32 * (i + num31 * num32) + 3 + j]);
					if (num == 0)
					{
						return false;
					}
					numvertices = num;
				}
				i += num32;
				num32 = (num32 + 1) % 2;
			}
			FindPolyCentroid(num, initialConvexPoly, ref newloc);
			if (!behavior.fixedArea)
			{
				return true;
			}
		}
		return false;
	}

	private bool GetWedgeIntersection(int numpoints, double[] points, ref double[] newloc)
	{
		if (2 * numpoints > petalx.Length)
		{
			petalx = new double[2 * numpoints];
			petaly = new double[2 * numpoints];
			petalr = new double[2 * numpoints];
			wedges = new double[2 * numpoints * 20 + 40];
		}
		double[] p = new double[3];
		double[] p2 = new double[3];
		double[] p3 = new double[3];
		double[] p4 = new double[3];
		int num = 0;
		int num2 = 0;
		double num3 = 4.0;
		double num4 = 4.0;
		double num5 = points[2 * numpoints - 4];
		double num6 = points[2 * numpoints - 3];
		double num7 = points[2 * numpoints - 2];
		double num8 = points[2 * numpoints - 1];
		double num9 = behavior.MinAngle * Math.PI / 180.0;
		double num10 = Math.Sin(num9);
		double num11 = Math.Cos(num9);
		double num12 = behavior.MaxAngle * Math.PI / 180.0;
		double num13 = Math.Sin(num12);
		double num14 = Math.Cos(num12);
		double num15;
		double num16;
		if (behavior.goodAngle == 1.0)
		{
			num15 = 0.0;
			num16 = 0.0;
		}
		else
		{
			num15 = 0.5 / Math.Tan(num9);
			num16 = 0.5 / Math.Sin(num9);
		}
		for (int i = 0; i < numpoints * 2; i += 2)
		{
			double num17 = points[i];
			double num18 = points[i + 1];
			double num19 = num7 - num5;
			double num20 = num8 - num6;
			double num21 = Math.Sqrt(num19 * num19 + num20 * num20);
			petalx[i / 2] = num5 + 0.5 * num19 - num15 * num20;
			petaly[i / 2] = num6 + 0.5 * num20 + num15 * num19;
			petalr[i / 2] = num16 * num21;
			petalx[numpoints + i / 2] = petalx[i / 2];
			petaly[numpoints + i / 2] = petaly[i / 2];
			petalr[numpoints + i / 2] = petalr[i / 2];
			double num22 = (num5 + num7) / 2.0;
			double num23 = (num6 + num8) / 2.0;
			double num24 = Math.Sqrt((petalx[i / 2] - num22) * (petalx[i / 2] - num22) + (petaly[i / 2] - num23) * (petaly[i / 2] - num23));
			double num25 = (petalx[i / 2] - num22) / num24;
			double num26 = (petaly[i / 2] - num23) / num24;
			double num27 = petalx[i / 2] + num25 * petalr[i / 2];
			double num28 = petaly[i / 2] + num26 * petalr[i / 2];
			num25 = num7 - num5;
			num26 = num8 - num6;
			double num29 = num7 * num11 - num8 * num10 + num5 - num5 * num11 + num6 * num10;
			double num30 = num7 * num10 + num8 * num11 + num6 - num5 * num10 - num6 * num11;
			wedges[i * 20] = num5;
			wedges[i * 20 + 1] = num6;
			wedges[i * 20 + 2] = num29;
			wedges[i * 20 + 3] = num30;
			num25 = num5 - num7;
			num26 = num6 - num8;
			double num31 = num5 * num11 + num6 * num10 + num7 - num7 * num11 - num8 * num10;
			double num32 = (0.0 - num5) * num10 + num6 * num11 + num8 + num7 * num10 - num8 * num11;
			wedges[i * 20 + 4] = num31;
			wedges[i * 20 + 5] = num32;
			wedges[i * 20 + 6] = num7;
			wedges[i * 20 + 7] = num8;
			num25 = num27 - petalx[i / 2];
			num26 = num28 - petaly[i / 2];
			double num33 = num27;
			double num34 = num28;
			num9 = 2.0 * behavior.MaxAngle + behavior.MinAngle - 180.0;
			if (num9 <= 0.0)
			{
				num2 = 4;
				num3 = 1.0;
				num4 = 1.0;
			}
			else if (num9 <= 5.0)
			{
				num2 = 6;
				num3 = 2.0;
				num4 = 2.0;
			}
			else if (num9 <= 10.0)
			{
				num2 = 8;
				num3 = 3.0;
				num4 = 3.0;
			}
			else
			{
				num2 = 10;
				num3 = 4.0;
				num4 = 4.0;
			}
			num9 = num9 * Math.PI / 180.0;
			for (int j = 1; (double)j < num3; j++)
			{
				if (num3 != 1.0)
				{
					double num35 = num27 * Math.Cos(num9 / (num3 - 1.0) * (double)j) + num28 * Math.Sin(num9 / (num3 - 1.0) * (double)j) + petalx[i / 2] - petalx[i / 2] * Math.Cos(num9 / (num3 - 1.0) * (double)j) - petaly[i / 2] * Math.Sin(num9 / (num3 - 1.0) * (double)j);
					double num36 = (0.0 - num27) * Math.Sin(num9 / (num3 - 1.0) * (double)j) + num28 * Math.Cos(num9 / (num3 - 1.0) * (double)j) + petaly[i / 2] + petalx[i / 2] * Math.Sin(num9 / (num3 - 1.0) * (double)j) - petaly[i / 2] * Math.Cos(num9 / (num3 - 1.0) * (double)j);
					wedges[i * 20 + 8 + 4 * (j - 1)] = num35;
					wedges[i * 20 + 9 + 4 * (j - 1)] = num36;
					wedges[i * 20 + 10 + 4 * (j - 1)] = num33;
					wedges[i * 20 + 11 + 4 * (j - 1)] = num34;
					num33 = num35;
					num34 = num36;
				}
			}
			num25 = num5 - num7;
			num26 = num6 - num8;
			double num37 = num5 * num14 + num6 * num13 + num7 - num7 * num14 - num8 * num13;
			double num38 = (0.0 - num5) * num13 + num6 * num14 + num8 + num7 * num13 - num8 * num14;
			wedges[i * 20 + 20] = num7;
			wedges[i * 20 + 21] = num8;
			wedges[i * 20 + 22] = num37;
			wedges[i * 20 + 23] = num38;
			num33 = num27;
			num34 = num28;
			for (int j = 1; (double)j < num4; j++)
			{
				if (num4 != 1.0)
				{
					double num39 = num27 * Math.Cos(num9 / (num4 - 1.0) * (double)j) - num28 * Math.Sin(num9 / (num4 - 1.0) * (double)j) + petalx[i / 2] - petalx[i / 2] * Math.Cos(num9 / (num4 - 1.0) * (double)j) + petaly[i / 2] * Math.Sin(num9 / (num4 - 1.0) * (double)j);
					double num40 = num27 * Math.Sin(num9 / (num4 - 1.0) * (double)j) + num28 * Math.Cos(num9 / (num4 - 1.0) * (double)j) + petaly[i / 2] - petalx[i / 2] * Math.Sin(num9 / (num4 - 1.0) * (double)j) - petaly[i / 2] * Math.Cos(num9 / (num4 - 1.0) * (double)j);
					wedges[i * 20 + 24 + 4 * (j - 1)] = num33;
					wedges[i * 20 + 25 + 4 * (j - 1)] = num34;
					wedges[i * 20 + 26 + 4 * (j - 1)] = num39;
					wedges[i * 20 + 27 + 4 * (j - 1)] = num40;
					num33 = num39;
					num34 = num40;
				}
			}
			num25 = num7 - num5;
			num26 = num8 - num6;
			double num41 = num7 * num14 - num8 * num13 + num5 - num5 * num14 + num6 * num13;
			double num42 = num7 * num13 + num8 * num14 + num6 - num5 * num13 - num6 * num14;
			wedges[i * 20 + 36] = num41;
			wedges[i * 20 + 37] = num42;
			wedges[i * 20 + 38] = num5;
			wedges[i * 20 + 39] = num6;
			if (i == 0)
			{
				switch (num2)
				{
				case 4:
					LineLineIntersection(num5, num6, num29, num30, num7, num8, num31, num32, ref p);
					LineLineIntersection(num5, num6, num29, num30, num7, num8, num37, num38, ref p2);
					LineLineIntersection(num5, num6, num41, num42, num7, num8, num37, num38, ref p3);
					LineLineIntersection(num5, num6, num41, num42, num7, num8, num31, num32, ref p4);
					if (p[0] == 1.0 && p2[0] == 1.0 && p3[0] == 1.0 && p4[0] == 1.0)
					{
						initialConvexPoly[0] = p[1];
						initialConvexPoly[1] = p[2];
						initialConvexPoly[2] = p2[1];
						initialConvexPoly[3] = p2[2];
						initialConvexPoly[4] = p3[1];
						initialConvexPoly[5] = p3[2];
						initialConvexPoly[6] = p4[1];
						initialConvexPoly[7] = p4[2];
					}
					break;
				case 6:
					LineLineIntersection(num5, num6, num29, num30, num7, num8, num31, num32, ref p);
					LineLineIntersection(num5, num6, num29, num30, num7, num8, num37, num38, ref p2);
					LineLineIntersection(num5, num6, num41, num42, num7, num8, num31, num32, ref p3);
					if (p[0] == 1.0 && p2[0] == 1.0 && p3[0] == 1.0)
					{
						initialConvexPoly[0] = p[1];
						initialConvexPoly[1] = p[2];
						initialConvexPoly[2] = p2[1];
						initialConvexPoly[3] = p2[2];
						initialConvexPoly[4] = wedges[i * 20 + 8];
						initialConvexPoly[5] = wedges[i * 20 + 9];
						initialConvexPoly[6] = num27;
						initialConvexPoly[7] = num28;
						initialConvexPoly[8] = wedges[i * 20 + 26];
						initialConvexPoly[9] = wedges[i * 20 + 27];
						initialConvexPoly[10] = p3[1];
						initialConvexPoly[11] = p3[2];
					}
					break;
				case 8:
					LineLineIntersection(num5, num6, num29, num30, num7, num8, num31, num32, ref p);
					LineLineIntersection(num5, num6, num29, num30, num7, num8, num37, num38, ref p2);
					LineLineIntersection(num5, num6, num41, num42, num7, num8, num31, num32, ref p3);
					if (p[0] == 1.0 && p2[0] == 1.0 && p3[0] == 1.0)
					{
						initialConvexPoly[0] = p[1];
						initialConvexPoly[1] = p[2];
						initialConvexPoly[2] = p2[1];
						initialConvexPoly[3] = p2[2];
						initialConvexPoly[4] = wedges[i * 20 + 12];
						initialConvexPoly[5] = wedges[i * 20 + 13];
						initialConvexPoly[6] = wedges[i * 20 + 8];
						initialConvexPoly[7] = wedges[i * 20 + 9];
						initialConvexPoly[8] = num27;
						initialConvexPoly[9] = num28;
						initialConvexPoly[10] = wedges[i * 20 + 26];
						initialConvexPoly[11] = wedges[i * 20 + 27];
						initialConvexPoly[12] = wedges[i * 20 + 30];
						initialConvexPoly[13] = wedges[i * 20 + 31];
						initialConvexPoly[14] = p3[1];
						initialConvexPoly[15] = p3[2];
					}
					break;
				case 10:
					LineLineIntersection(num5, num6, num29, num30, num7, num8, num31, num32, ref p);
					LineLineIntersection(num5, num6, num29, num30, num7, num8, num37, num38, ref p2);
					LineLineIntersection(num5, num6, num41, num42, num7, num8, num31, num32, ref p3);
					if (p[0] == 1.0 && p2[0] == 1.0 && p3[0] == 1.0)
					{
						initialConvexPoly[0] = p[1];
						initialConvexPoly[1] = p[2];
						initialConvexPoly[2] = p2[1];
						initialConvexPoly[3] = p2[2];
						initialConvexPoly[4] = wedges[i * 20 + 16];
						initialConvexPoly[5] = wedges[i * 20 + 17];
						initialConvexPoly[6] = wedges[i * 20 + 12];
						initialConvexPoly[7] = wedges[i * 20 + 13];
						initialConvexPoly[8] = wedges[i * 20 + 8];
						initialConvexPoly[9] = wedges[i * 20 + 9];
						initialConvexPoly[10] = num27;
						initialConvexPoly[11] = num28;
						initialConvexPoly[12] = wedges[i * 20 + 28];
						initialConvexPoly[13] = wedges[i * 20 + 29];
						initialConvexPoly[14] = wedges[i * 20 + 32];
						initialConvexPoly[15] = wedges[i * 20 + 33];
						initialConvexPoly[16] = wedges[i * 20 + 34];
						initialConvexPoly[17] = wedges[i * 20 + 35];
						initialConvexPoly[18] = p3[1];
						initialConvexPoly[19] = p3[2];
					}
					break;
				}
			}
			num5 = num7;
			num6 = num8;
			num7 = num17;
			num8 = num18;
		}
		if (numpoints != 0)
		{
			int num43 = (numpoints - 1) / 2 + 1;
			int num44 = 0;
			int num45 = 0;
			int i = 1;
			int numvertices = num2;
			for (int j = 0; j < 40; j += 4)
			{
				if ((num2 != 4 || (j != 8 && j != 12 && j != 16 && j != 24 && j != 28 && j != 32)) && (num2 != 6 || (j != 12 && j != 16 && j != 28 && j != 32)) && (num2 != 8 || (j != 16 && j != 32)))
				{
					num = HalfPlaneIntersection(numvertices, ref initialConvexPoly, wedges[40 * num43 + j], wedges[40 * num43 + 1 + j], wedges[40 * num43 + 2 + j], wedges[40 * num43 + 3 + j]);
					if (num == 0)
					{
						return false;
					}
					numvertices = num;
				}
			}
			for (num45++; num45 < numpoints - 1; num45++)
			{
				for (int j = 0; j < 40; j += 4)
				{
					if ((num2 != 4 || (j != 8 && j != 12 && j != 16 && j != 24 && j != 28 && j != 32)) && (num2 != 6 || (j != 12 && j != 16 && j != 28 && j != 32)) && (num2 != 8 || (j != 16 && j != 32)))
					{
						num = HalfPlaneIntersection(numvertices, ref initialConvexPoly, wedges[40 * (i + num43 * num44) + j], wedges[40 * (i + num43 * num44) + 1 + j], wedges[40 * (i + num43 * num44) + 2 + j], wedges[40 * (i + num43 * num44) + 3 + j]);
						if (num == 0)
						{
							return false;
						}
						numvertices = num;
					}
				}
				i += num44;
				num44 = (num44 + 1) % 2;
			}
			FindPolyCentroid(num, initialConvexPoly, ref newloc);
			if (behavior.MaxAngle == 0.0)
			{
				return true;
			}
			int num46 = 0;
			for (int j = 0; j < numpoints * 2 - 2; j += 2)
			{
				if (IsBadTriangleAngle(newloc[0], newloc[1], points[j], points[j + 1], points[j + 2], points[j + 3]))
				{
					num46++;
				}
			}
			if (IsBadTriangleAngle(newloc[0], newloc[1], points[0], points[1], points[numpoints * 2 - 2], points[numpoints * 2 - 1]))
			{
				num46++;
			}
			if (num46 == 0)
			{
				return true;
			}
			int num47 = ((numpoints <= 2) ? 20 : 30);
			for (int k = 0; k < 2 * numpoints; k += 2)
			{
				for (int l = 1; l < num47; l++)
				{
					newloc[0] = 0.0;
					newloc[1] = 0.0;
					for (i = 0; i < 2 * numpoints; i += 2)
					{
						double num48 = 1.0 / (double)numpoints;
						if (i == k)
						{
							newloc[0] = newloc[0] + 0.1 * (double)l * num48 * points[i];
							newloc[1] = newloc[1] + 0.1 * (double)l * num48 * points[i + 1];
						}
						else
						{
							num48 = (1.0 - 0.1 * (double)l * num48) / ((double)numpoints - 1.0);
							newloc[0] = newloc[0] + num48 * points[i];
							newloc[1] = newloc[1] + num48 * points[i + 1];
						}
					}
					num46 = 0;
					for (int j = 0; j < numpoints * 2 - 2; j += 2)
					{
						if (IsBadTriangleAngle(newloc[0], newloc[1], points[j], points[j + 1], points[j + 2], points[j + 3]))
						{
							num46++;
						}
					}
					if (IsBadTriangleAngle(newloc[0], newloc[1], points[0], points[1], points[numpoints * 2 - 2], points[numpoints * 2 - 1]))
					{
						num46++;
					}
					if (num46 == 0)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool ValidPolygonAngles(int numpoints, double[] points)
	{
		for (int i = 0; i < numpoints; i++)
		{
			if (i == numpoints - 1)
			{
				if (IsBadPolygonAngle(points[i * 2], points[i * 2 + 1], points[0], points[1], points[2], points[3]))
				{
					return false;
				}
			}
			else if (i == numpoints - 2)
			{
				if (IsBadPolygonAngle(points[i * 2], points[i * 2 + 1], points[(i + 1) * 2], points[(i + 1) * 2 + 1], points[0], points[1]))
				{
					return false;
				}
			}
			else if (IsBadPolygonAngle(points[i * 2], points[i * 2 + 1], points[(i + 1) * 2], points[(i + 1) * 2 + 1], points[(i + 2) * 2], points[(i + 2) * 2 + 1]))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsBadPolygonAngle(double x1, double y1, double x2, double y2, double x3, double y3)
	{
		double num = x1 - x2;
		double num2 = y1 - y2;
		double num3 = x2 - x3;
		double num4 = y2 - y3;
		double num5 = x3 - x1;
		double num6 = y3 - y1;
		double num7 = num * num + num2 * num2;
		double num8 = num3 * num3 + num4 * num4;
		double num9 = num5 * num5 + num6 * num6;
		if (Math.Acos((num7 + num8 - num9) / (2.0 * Math.Sqrt(num7) * Math.Sqrt(num8))) < 2.0 * Math.Acos(Math.Sqrt(behavior.goodAngle)))
		{
			return true;
		}
		return false;
	}

	private void LineLineIntersection(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, ref double[] p)
	{
		double num = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
		double num2 = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
		double num3 = (x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3);
		if (Math.Abs(num - 0.0) < 1E-50 && Math.Abs(num3 - 0.0) < 1E-50 && Math.Abs(num2 - 0.0) < 1E-50)
		{
			p[0] = 0.0;
			return;
		}
		if (Math.Abs(num - 0.0) < 1E-50)
		{
			p[0] = 0.0;
			return;
		}
		p[0] = 1.0;
		num2 /= num;
		num3 /= num;
		p[1] = x1 + num2 * (x2 - x1);
		p[2] = y1 + num2 * (y2 - y1);
	}

	private int HalfPlaneIntersection(int numvertices, ref double[] convexPoly, double x1, double y1, double x2, double y2)
	{
		double[] array = null;
		int i = 0;
		int num = 0;
		double num2 = x2 - x1;
		double num3 = y2 - y1;
		int num4 = SplitConvexPolygon(numvertices, convexPoly, x1, y1, x2, y2, polys);
		if (num4 == 3)
		{
			i = numvertices;
		}
		else
		{
			for (int j = 0; j < num4; j++)
			{
				double num5 = double.MaxValue;
				double num6 = double.MinValue;
				double num7;
				for (int k = 1; (double)k <= 2.0 * polys[j][0] - 1.0; k += 2)
				{
					num7 = num2 * (polys[j][k + 1] - y1) - num3 * (polys[j][k] - x1);
					num5 = ((num7 < num5) ? num7 : num5);
					num6 = ((num7 > num6) ? num7 : num6);
				}
				num7 = ((Math.Abs(num5) > Math.Abs(num6)) ? num5 : num6);
				if (num7 > 0.0)
				{
					array = polys[j];
					num = 1;
					break;
				}
			}
			if (num == 1)
			{
				for (; (double)i < array[0]; i++)
				{
					convexPoly[2 * i] = array[2 * i + 1];
					convexPoly[2 * i + 1] = array[2 * i + 2];
				}
			}
		}
		return i;
	}

	private int SplitConvexPolygon(int numvertices, double[] convexPoly, double x1, double y1, double x2, double y2, double[][] polys)
	{
		int num = 0;
		double[] p = new double[3];
		int num2 = 0;
		int num3 = 0;
		double num4 = 1E-12;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		for (int i = 0; i < 2 * numvertices; i += 2)
		{
			int num13 = ((i + 2 < 2 * numvertices) ? (i + 2) : 0);
			LineLineSegmentIntersection(x1, y1, x2, y2, convexPoly[i], convexPoly[i + 1], convexPoly[num13], convexPoly[num13 + 1], ref p);
			if (Math.Abs(p[0] - 0.0) <= num4)
			{
				if (num == 1)
				{
					num3++;
					poly2[2 * num3 - 1] = convexPoly[num13];
					poly2[2 * num3] = convexPoly[num13 + 1];
				}
				else
				{
					num2++;
					poly1[2 * num2 - 1] = convexPoly[num13];
					poly1[2 * num2] = convexPoly[num13 + 1];
				}
				num5++;
				continue;
			}
			if (Math.Abs(p[0] - 2.0) <= num4)
			{
				num2++;
				poly1[2 * num2 - 1] = convexPoly[num13];
				poly1[2 * num2] = convexPoly[num13 + 1];
				num6++;
				continue;
			}
			num7++;
			if (Math.Abs(p[1] - convexPoly[num13]) <= num4 && Math.Abs(p[2] - convexPoly[num13 + 1]) <= num4)
			{
				num8++;
				switch (num)
				{
				case 1:
					num3++;
					poly2[2 * num3 - 1] = convexPoly[num13];
					poly2[2 * num3] = convexPoly[num13 + 1];
					num2++;
					poly1[2 * num2 - 1] = convexPoly[num13];
					poly1[2 * num2] = convexPoly[num13 + 1];
					num++;
					break;
				case 0:
					num11++;
					num2++;
					poly1[2 * num2 - 1] = convexPoly[num13];
					poly1[2 * num2] = convexPoly[num13 + 1];
					if (i + 4 < 2 * numvertices)
					{
						int num14 = LinePointLocation(x1, y1, x2, y2, convexPoly[i], convexPoly[i + 1]);
						int num15 = LinePointLocation(x1, y1, x2, y2, convexPoly[i + 4], convexPoly[i + 5]);
						if (num14 != num15 && num14 != 0 && num15 != 0)
						{
							num12++;
							num3++;
							poly2[2 * num3 - 1] = convexPoly[num13];
							poly2[2 * num3] = convexPoly[num13 + 1];
							num++;
						}
					}
					break;
				}
			}
			else if (!(Math.Abs(p[1] - convexPoly[i]) <= num4) || !(Math.Abs(p[2] - convexPoly[i + 1]) <= num4))
			{
				num9++;
				num2++;
				poly1[2 * num2 - 1] = p[1];
				poly1[2 * num2] = p[2];
				num3++;
				poly2[2 * num3 - 1] = p[1];
				poly2[2 * num3] = p[2];
				switch (num)
				{
				case 1:
					num2++;
					poly1[2 * num2 - 1] = convexPoly[num13];
					poly1[2 * num2] = convexPoly[num13 + 1];
					break;
				case 0:
					num3++;
					poly2[2 * num3 - 1] = convexPoly[num13];
					poly2[2 * num3] = convexPoly[num13 + 1];
					break;
				}
				num++;
			}
			else
			{
				num10++;
				if (num == 1)
				{
					num3++;
					poly2[2 * num3 - 1] = convexPoly[num13];
					poly2[2 * num3] = convexPoly[num13 + 1];
				}
				else
				{
					num2++;
					poly1[2 * num2 - 1] = convexPoly[num13];
					poly1[2 * num2] = convexPoly[num13 + 1];
				}
			}
		}
		int result;
		if (num != 0 && num != 2)
		{
			result = 3;
		}
		else
		{
			result = ((num == 0) ? 1 : 2);
			poly1[0] = num2;
			poly2[0] = num3;
			polys[0] = poly1;
			if (num == 2)
			{
				polys[1] = poly2;
			}
		}
		return result;
	}

	private int LinePointLocation(double x1, double y1, double x2, double y2, double x, double y)
	{
		if (Math.Atan((y2 - y1) / (x2 - x1)) * 180.0 / Math.PI == 90.0)
		{
			if (Math.Abs(x1 - x) <= 1E-11)
			{
				return 0;
			}
		}
		else if (Math.Abs(y1 + (y2 - y1) * (x - x1) / (x2 - x1) - y) <= 1E-50)
		{
			return 0;
		}
		double num = (x2 - x1) * (y - y1) - (y2 - y1) * (x - x1);
		if (Math.Abs(num - 0.0) <= 1E-11)
		{
			return 0;
		}
		if (num > 0.0)
		{
			return 1;
		}
		return 2;
	}

	private void LineLineSegmentIntersection(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, ref double[] p)
	{
		double num = 1E-13;
		double num2 = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
		double num3 = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
		double num4 = (x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3);
		if (Math.Abs(num2 - 0.0) < num)
		{
			if (Math.Abs(num4 - 0.0) < num && Math.Abs(num3 - 0.0) < num)
			{
				p[0] = 2.0;
			}
			else
			{
				p[0] = 0.0;
			}
			return;
		}
		num4 /= num2;
		num3 /= num2;
		if (num4 < 0.0 - num || num4 > 1.0 + num)
		{
			p[0] = 0.0;
			return;
		}
		p[0] = 1.0;
		p[1] = x1 + num3 * (x2 - x1);
		p[2] = y1 + num3 * (y2 - y1);
	}

	private void FindPolyCentroid(int numpoints, double[] points, ref double[] centroid)
	{
		centroid[0] = 0.0;
		centroid[1] = 0.0;
		for (int i = 0; i < 2 * numpoints; i += 2)
		{
			centroid[0] = centroid[0] + points[i];
			centroid[1] = centroid[1] + points[i + 1];
		}
		centroid[0] = centroid[0] / (double)numpoints;
		centroid[1] = centroid[1] / (double)numpoints;
	}

	private void CircleLineIntersection(double x1, double y1, double x2, double y2, double x3, double y3, double r, ref double[] p)
	{
		double num = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
		double num2 = 2.0 * ((x2 - x1) * (x1 - x3) + (y2 - y1) * (y1 - y3));
		double num3 = x3 * x3 + y3 * y3 + x1 * x1 + y1 * y1 - 2.0 * (x3 * x1 + y3 * y1) - r * r;
		double num4 = num2 * num2 - 4.0 * num * num3;
		if (num4 < 0.0)
		{
			p[0] = 0.0;
		}
		else if (Math.Abs(num4 - 0.0) < 1E-50)
		{
			p[0] = 1.0;
			double num5 = (0.0 - num2) / (2.0 * num);
			p[1] = x1 + num5 * (x2 - x1);
			p[2] = y1 + num5 * (y2 - y1);
		}
		else if (num4 > 0.0 && !(Math.Abs(num - 0.0) < 1E-50))
		{
			p[0] = 2.0;
			double num5 = (0.0 - num2 + Math.Sqrt(num4)) / (2.0 * num);
			p[1] = x1 + num5 * (x2 - x1);
			p[2] = y1 + num5 * (y2 - y1);
			num5 = (0.0 - num2 - Math.Sqrt(num4)) / (2.0 * num);
			p[3] = x1 + num5 * (x2 - x1);
			p[4] = y1 + num5 * (y2 - y1);
		}
		else
		{
			p[0] = 0.0;
		}
	}

	private bool ChooseCorrectPoint(double x1, double y1, double x2, double y2, double x3, double y3, bool isObtuse)
	{
		double num = (x2 - x3) * (x2 - x3) + (y2 - y3) * (y2 - y3);
		double num2 = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
		if (isObtuse)
		{
			if (num2 >= num)
			{
				return true;
			}
			return false;
		}
		if (num2 < num)
		{
			return true;
		}
		return false;
	}

	private void PointBetweenPoints(double x1, double y1, double x2, double y2, double x, double y, ref double[] p)
	{
		if ((x2 - x) * (x2 - x) + (y2 - y) * (y2 - y) < (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1))
		{
			p[0] = 1.0;
			p[1] = (x - x2) * (x - x2) + (y - y2) * (y - y2);
			p[2] = x;
			p[3] = y;
		}
		else
		{
			p[0] = 0.0;
			p[1] = 0.0;
			p[2] = 0.0;
			p[3] = 0.0;
		}
	}

	private bool IsBadTriangleAngle(double x1, double y1, double x2, double y2, double x3, double y3)
	{
		double num = x1 - x2;
		double num2 = y1 - y2;
		double num3 = x2 - x3;
		double num4 = y2 - y3;
		double num5 = x3 - x1;
		double num6 = y3 - y1;
		double num7 = num * num;
		double num8 = num2 * num2;
		double num9 = num3 * num3;
		double num10 = num4 * num4;
		double num11 = num5 * num5;
		double num12 = num6 * num6;
		double num13 = num7 + num8;
		double num14 = num9 + num10;
		double num15 = num11 + num12;
		double num16;
		if (num13 < num14 && num13 < num15)
		{
			num16 = num3 * num5 + num4 * num6;
			num16 = num16 * num16 / (num14 * num15);
		}
		else if (num14 < num15)
		{
			num16 = num * num5 + num2 * num6;
			num16 = num16 * num16 / (num13 * num15);
		}
		else
		{
			num16 = num * num3 + num2 * num4;
			num16 = num16 * num16 / (num13 * num14);
		}
		double num17 = ((num13 > num14 && num13 > num15) ? ((num14 + num15 - num13) / (2.0 * Math.Sqrt(num14 * num15))) : ((!(num14 > num15)) ? ((num13 + num14 - num15) / (2.0 * Math.Sqrt(num13 * num14))) : ((num13 + num15 - num14) / (2.0 * Math.Sqrt(num13 * num15)))));
		if (num16 > behavior.goodAngle || (behavior.MaxAngle != 0.0 && num17 < behavior.maxGoodAngle))
		{
			return true;
		}
		return false;
	}

	private double MinDistanceToNeighbor(double newlocX, double newlocY, ref Otri searchtri)
	{
		Otri ot = default(Otri);
		LocateResult locateResult = LocateResult.Outside;
		Point point = new Point(newlocX, newlocY);
		Vertex vertex = searchtri.Org();
		Vertex vertex2 = searchtri.Dest();
		if (vertex.x == point.x && vertex.y == point.y)
		{
			locateResult = LocateResult.OnVertex;
			searchtri.Copy(ref ot);
		}
		else if (vertex2.x == point.x && vertex2.y == point.y)
		{
			searchtri.Lnext();
			locateResult = LocateResult.OnVertex;
			searchtri.Copy(ref ot);
		}
		else
		{
			double num = predicates.CounterClockwise(vertex, vertex2, point);
			if (num < 0.0)
			{
				searchtri.Sym();
				searchtri.Copy(ref ot);
				locateResult = mesh.locator.PreciseLocate(point, ref ot, stopatsubsegment: false);
			}
			else if (num == 0.0)
			{
				if (vertex.x < point.x == point.x < vertex2.x && vertex.y < point.y == point.y < vertex2.y)
				{
					locateResult = LocateResult.OnEdge;
					searchtri.Copy(ref ot);
				}
			}
			else
			{
				searchtri.Copy(ref ot);
				locateResult = mesh.locator.PreciseLocate(point, ref ot, stopatsubsegment: false);
			}
		}
		if (locateResult == LocateResult.OnVertex || locateResult == LocateResult.Outside)
		{
			return 0.0;
		}
		Vertex vertex3 = ot.Org();
		Vertex vertex4 = ot.Dest();
		Vertex vertex5 = ot.Apex();
		double num2 = (vertex3.x - point.x) * (vertex3.x - point.x) + (vertex3.y - point.y) * (vertex3.y - point.y);
		double num3 = (vertex4.x - point.x) * (vertex4.x - point.x) + (vertex4.y - point.y) * (vertex4.y - point.y);
		double num4 = (vertex5.x - point.x) * (vertex5.x - point.x) + (vertex5.y - point.y) * (vertex5.y - point.y);
		if (num2 <= num3 && num2 <= num4)
		{
			return num2;
		}
		if (num3 <= num4)
		{
			return num3;
		}
		return num4;
	}
}
