using System;
using TriangleNet.Geometry;
using TriangleNet.Tools;

namespace TriangleNet;

public class RobustPredicates : IPredicates
{
	private static readonly object creationLock;

	private static RobustPredicates _default;

	private static double epsilon;

	private static double splitter;

	private static double resulterrbound;

	private static double ccwerrboundA;

	private static double ccwerrboundB;

	private static double ccwerrboundC;

	private static double iccerrboundA;

	private static double iccerrboundB;

	private static double iccerrboundC;

	private double[] fin1;

	private double[] fin2;

	private double[] abdet;

	private double[] axbc;

	private double[] axxbc;

	private double[] aybc;

	private double[] ayybc;

	private double[] adet;

	private double[] bxca;

	private double[] bxxca;

	private double[] byca;

	private double[] byyca;

	private double[] bdet;

	private double[] cxab;

	private double[] cxxab;

	private double[] cyab;

	private double[] cyyab;

	private double[] cdet;

	private double[] temp8;

	private double[] temp16a;

	private double[] temp16b;

	private double[] temp16c;

	private double[] temp32a;

	private double[] temp32b;

	private double[] temp48;

	private double[] temp64;

	public static RobustPredicates Default
	{
		get
		{
			if (_default == null)
			{
				lock (creationLock)
				{
					if (_default == null)
					{
						_default = new RobustPredicates();
					}
				}
			}
			return _default;
		}
	}

	static RobustPredicates()
	{
		creationLock = new object();
		bool flag = true;
		double num = 0.5;
		epsilon = 1.0;
		splitter = 1.0;
		double num2 = 1.0;
		double num3;
		do
		{
			num3 = num2;
			epsilon *= num;
			if (flag)
			{
				splitter *= 2.0;
			}
			flag = !flag;
			num2 = 1.0 + epsilon;
		}
		while (num2 != 1.0 && num2 != num3);
		splitter += 1.0;
		resulterrbound = (3.0 + 8.0 * epsilon) * epsilon;
		ccwerrboundA = (3.0 + 16.0 * epsilon) * epsilon;
		ccwerrboundB = (2.0 + 12.0 * epsilon) * epsilon;
		ccwerrboundC = (9.0 + 64.0 * epsilon) * epsilon * epsilon;
		iccerrboundA = (10.0 + 96.0 * epsilon) * epsilon;
		iccerrboundB = (4.0 + 48.0 * epsilon) * epsilon;
		iccerrboundC = (44.0 + 576.0 * epsilon) * epsilon * epsilon;
	}

	public RobustPredicates()
	{
		AllocateWorkspace();
	}

	public double CounterClockwise(Point pa, Point pb, Point pc)
	{
		Statistic.CounterClockwiseCount++;
		double num = (pa.x - pc.x) * (pb.y - pc.y);
		double num2 = (pa.y - pc.y) * (pb.x - pc.x);
		double num3 = num - num2;
		if (Behavior.NoExact)
		{
			return num3;
		}
		double num4;
		if (num > 0.0)
		{
			if (num2 <= 0.0)
			{
				return num3;
			}
			num4 = num + num2;
		}
		else
		{
			if (!(num < 0.0))
			{
				return num3;
			}
			if (num2 >= 0.0)
			{
				return num3;
			}
			num4 = 0.0 - num - num2;
		}
		double num5 = ccwerrboundA * num4;
		if (num3 >= num5 || 0.0 - num3 >= num5)
		{
			return num3;
		}
		Statistic.CounterClockwiseAdaptCount++;
		return CounterClockwiseAdapt(pa, pb, pc, num4);
	}

	public double InCircle(Point pa, Point pb, Point pc, Point pd)
	{
		Statistic.InCircleCount++;
		double num = pa.x - pd.x;
		double num2 = pb.x - pd.x;
		double num3 = pc.x - pd.x;
		double num4 = pa.y - pd.y;
		double num5 = pb.y - pd.y;
		double num6 = pc.y - pd.y;
		double num7 = num2 * num6;
		double num8 = num3 * num5;
		double num9 = num * num + num4 * num4;
		double num10 = num3 * num4;
		double num11 = num * num6;
		double num12 = num2 * num2 + num5 * num5;
		double num13 = num * num5;
		double num14 = num2 * num4;
		double num15 = num3 * num3 + num6 * num6;
		double num16 = num9 * (num7 - num8) + num12 * (num10 - num11) + num15 * (num13 - num14);
		if (Behavior.NoExact)
		{
			return num16;
		}
		double num17 = (Math.Abs(num7) + Math.Abs(num8)) * num9 + (Math.Abs(num10) + Math.Abs(num11)) * num12 + (Math.Abs(num13) + Math.Abs(num14)) * num15;
		double num18 = iccerrboundA * num17;
		if (num16 > num18 || 0.0 - num16 > num18)
		{
			return num16;
		}
		Statistic.InCircleAdaptCount++;
		return InCircleAdapt(pa, pb, pc, pd, num17);
	}

	public double NonRegular(Point pa, Point pb, Point pc, Point pd)
	{
		return InCircle(pa, pb, pc, pd);
	}

	public Point FindCircumcenter(Point org, Point dest, Point apex, ref double xi, ref double eta, double offconstant)
	{
		Statistic.CircumcenterCount++;
		double num = dest.x - org.x;
		double num2 = dest.y - org.y;
		double num3 = apex.x - org.x;
		double num4 = apex.y - org.y;
		double num5 = num * num + num2 * num2;
		double num6 = num3 * num3 + num4 * num4;
		double num7 = (dest.x - apex.x) * (dest.x - apex.x) + (dest.y - apex.y) * (dest.y - apex.y);
		double num8;
		if (Behavior.NoExact)
		{
			num8 = 0.5 / (num * num4 - num3 * num2);
		}
		else
		{
			num8 = 0.5 / CounterClockwise(dest, apex, org);
			Statistic.CounterClockwiseCount--;
		}
		double num9 = (num4 * num5 - num2 * num6) * num8;
		double num10 = (num * num6 - num3 * num5) * num8;
		if (num5 < num6 && num5 < num7)
		{
			if (offconstant > 0.0)
			{
				double num11 = 0.5 * num - offconstant * num2;
				double num12 = 0.5 * num2 + offconstant * num;
				if (num11 * num11 + num12 * num12 < num9 * num9 + num10 * num10)
				{
					num9 = num11;
					num10 = num12;
				}
			}
		}
		else if (num6 < num7)
		{
			if (offconstant > 0.0)
			{
				double num11 = 0.5 * num3 + offconstant * num4;
				double num12 = 0.5 * num4 - offconstant * num3;
				if (num11 * num11 + num12 * num12 < num9 * num9 + num10 * num10)
				{
					num9 = num11;
					num10 = num12;
				}
			}
		}
		else if (offconstant > 0.0)
		{
			double num11 = 0.5 * (apex.x - dest.x) - offconstant * (apex.y - dest.y);
			double num12 = 0.5 * (apex.y - dest.y) + offconstant * (apex.x - dest.x);
			if (num11 * num11 + num12 * num12 < (num9 - num) * (num9 - num) + (num10 - num2) * (num10 - num2))
			{
				num9 = num + num11;
				num10 = num2 + num12;
			}
		}
		xi = (num4 * num9 - num3 * num10) * (2.0 * num8);
		eta = (num * num10 - num2 * num9) * (2.0 * num8);
		return new Point(org.x + num9, org.y + num10);
	}

	public Point FindCircumcenter(Point org, Point dest, Point apex, ref double xi, ref double eta)
	{
		Statistic.CircumcenterCount++;
		double num = dest.x - org.x;
		double num2 = dest.y - org.y;
		double num3 = apex.x - org.x;
		double num4 = apex.y - org.y;
		double num5 = num * num + num2 * num2;
		double num6 = num3 * num3 + num4 * num4;
		double num7;
		if (Behavior.NoExact)
		{
			num7 = 0.5 / (num * num4 - num3 * num2);
		}
		else
		{
			num7 = 0.5 / CounterClockwise(dest, apex, org);
			Statistic.CounterClockwiseCount--;
		}
		double num8 = (num4 * num5 - num2 * num6) * num7;
		double num9 = (num * num6 - num3 * num5) * num7;
		xi = (num4 * num8 - num3 * num9) * (2.0 * num7);
		eta = (num * num9 - num2 * num8) * (2.0 * num7);
		return new Point(org.x + num8, org.y + num9);
	}

	private int FastExpansionSumZeroElim(int elen, double[] e, int flen, double[] f, double[] h)
	{
		double num = e[0];
		double num2 = f[0];
		int num3;
		int num4 = (num3 = 0);
		double num5;
		if (num2 > num == num2 > 0.0 - num)
		{
			num5 = num;
			num = e[++num4];
		}
		else
		{
			num5 = num2;
			num2 = f[++num3];
		}
		int num6 = 0;
		if (num4 < elen && num3 < flen)
		{
			double num7;
			double num9;
			if (num2 > num == num2 > 0.0 - num)
			{
				num7 = num + num5;
				double num8 = num7 - num;
				num9 = num5 - num8;
				num = e[++num4];
			}
			else
			{
				num7 = num2 + num5;
				double num8 = num7 - num2;
				num9 = num5 - num8;
				num2 = f[++num3];
			}
			num5 = num7;
			if (num9 != 0.0)
			{
				h[num6++] = num9;
			}
			while (num4 < elen && num3 < flen)
			{
				if (num2 > num == num2 > 0.0 - num)
				{
					num7 = num5 + num;
					double num8 = num7 - num5;
					double num10 = num7 - num8;
					double num11 = num - num8;
					num9 = num5 - num10 + num11;
					num = e[++num4];
				}
				else
				{
					num7 = num5 + num2;
					double num8 = num7 - num5;
					double num10 = num7 - num8;
					double num11 = num2 - num8;
					num9 = num5 - num10 + num11;
					num2 = f[++num3];
				}
				num5 = num7;
				if (num9 != 0.0)
				{
					h[num6++] = num9;
				}
			}
		}
		while (num4 < elen)
		{
			double num7 = num5 + num;
			double num8 = num7 - num5;
			double num10 = num7 - num8;
			double num11 = num - num8;
			double num9 = num5 - num10 + num11;
			num = e[++num4];
			num5 = num7;
			if (num9 != 0.0)
			{
				h[num6++] = num9;
			}
		}
		while (num3 < flen)
		{
			double num7 = num5 + num2;
			double num8 = num7 - num5;
			double num10 = num7 - num8;
			double num11 = num2 - num8;
			double num9 = num5 - num10 + num11;
			num2 = f[++num3];
			num5 = num7;
			if (num9 != 0.0)
			{
				h[num6++] = num9;
			}
		}
		if (num5 != 0.0 || num6 == 0)
		{
			h[num6++] = num5;
		}
		return num6;
	}

	private int ScaleExpansionZeroElim(int elen, double[] e, double b, double[] h)
	{
		double num = splitter * b;
		double num2 = num - b;
		double num3 = num - num2;
		double num4 = b - num3;
		double num5 = e[0] * b;
		double num6 = splitter * e[0];
		num2 = num6 - e[0];
		double num7 = num6 - num2;
		double num8 = e[0] - num7;
		double num9 = num5 - num7 * num3 - num8 * num3 - num7 * num4;
		double num10 = num8 * num4 - num9;
		int num11 = 0;
		if (num10 != 0.0)
		{
			h[num11++] = num10;
		}
		for (int i = 1; i < elen; i++)
		{
			double num12 = e[i];
			double num13 = num12 * b;
			double num14 = splitter * num12;
			num2 = num14 - num12;
			num7 = num14 - num2;
			num8 = num12 - num7;
			num9 = num13 - num7 * num3 - num8 * num3 - num7 * num4;
			double num15 = num8 * num4 - num9;
			double num16 = num5 + num15;
			double num17 = num16 - num5;
			double num18 = num16 - num17;
			double num19 = num15 - num17;
			num10 = num5 - num18 + num19;
			if (num10 != 0.0)
			{
				h[num11++] = num10;
			}
			num5 = num13 + num16;
			num17 = num5 - num13;
			num10 = num16 - num17;
			if (num10 != 0.0)
			{
				h[num11++] = num10;
			}
		}
		if (num5 != 0.0 || num11 == 0)
		{
			h[num11++] = num5;
		}
		return num11;
	}

	private double Estimate(int elen, double[] e)
	{
		double num = e[0];
		for (int i = 1; i < elen; i++)
		{
			num += e[i];
		}
		return num;
	}

	private double CounterClockwiseAdapt(Point pa, Point pb, Point pc, double detsum)
	{
		double[] array = new double[5];
		double[] array2 = new double[5];
		double[] array3 = new double[8];
		double[] array4 = new double[12];
		double[] array5 = new double[16];
		double num = pa.x - pc.x;
		double num2 = pb.x - pc.x;
		double num3 = pa.y - pc.y;
		double num4 = pb.y - pc.y;
		double num5 = num * num4;
		double num6 = splitter * num;
		double num7 = num6 - num;
		double num8 = num6 - num7;
		double num9 = num - num8;
		double num10 = splitter * num4;
		num7 = num10 - num4;
		double num11 = num10 - num7;
		double num12 = num4 - num11;
		double num13 = num5 - num8 * num11 - num9 * num11 - num8 * num12;
		double num14 = num9 * num12 - num13;
		double num15 = num3 * num2;
		double num16 = splitter * num3;
		num7 = num16 - num3;
		num8 = num16 - num7;
		num9 = num3 - num8;
		double num17 = splitter * num2;
		num7 = num17 - num2;
		num11 = num17 - num7;
		num12 = num2 - num11;
		num13 = num15 - num8 * num11 - num9 * num11 - num8 * num12;
		double num18 = num9 * num12 - num13;
		double num19 = num14 - num18;
		double num20 = num14 - num19;
		double num21 = num19 + num20;
		double num22 = num20 - num18;
		double num23 = num14 - num21;
		array[0] = num23 + num22;
		double num24 = num5 + num19;
		num20 = num24 - num5;
		num21 = num24 - num20;
		num22 = num19 - num20;
		num23 = num5 - num21;
		double num25 = num23 + num22;
		num19 = num25 - num15;
		num20 = num25 - num19;
		num21 = num19 + num20;
		num22 = num20 - num15;
		num23 = num25 - num21;
		array[1] = num23 + num22;
		double num26 = num24 + num19;
		num20 = num26 - num24;
		num21 = num26 - num20;
		num22 = num19 - num20;
		num23 = num24 - num21;
		array[2] = num23 + num22;
		array[3] = num26;
		double num27 = Estimate(4, array);
		double num28 = ccwerrboundB * detsum;
		if (num27 >= num28 || 0.0 - num27 >= num28)
		{
			return num27;
		}
		num20 = pa.x - num;
		num21 = num + num20;
		num22 = num20 - pc.x;
		num23 = pa.x - num21;
		double num29 = num23 + num22;
		num20 = pb.x - num2;
		num21 = num2 + num20;
		num22 = num20 - pc.x;
		num23 = pb.x - num21;
		double num30 = num23 + num22;
		num20 = pa.y - num3;
		num21 = num3 + num20;
		num22 = num20 - pc.y;
		num23 = pa.y - num21;
		double num31 = num23 + num22;
		num20 = pb.y - num4;
		num21 = num4 + num20;
		num22 = num20 - pc.y;
		num23 = pb.y - num21;
		double num32 = num23 + num22;
		if (num29 == 0.0 && num31 == 0.0 && num30 == 0.0 && num32 == 0.0)
		{
			return num27;
		}
		num28 = ccwerrboundC * detsum + resulterrbound * ((num27 >= 0.0) ? num27 : (0.0 - num27));
		num27 += num * num32 + num4 * num29 - (num3 * num30 + num2 * num31);
		if (num27 >= num28 || 0.0 - num27 >= num28)
		{
			return num27;
		}
		double num33 = num29 * num4;
		double num34 = splitter * num29;
		num7 = num34 - num29;
		num8 = num34 - num7;
		num9 = num29 - num8;
		double num35 = splitter * num4;
		num7 = num35 - num4;
		num11 = num35 - num7;
		num12 = num4 - num11;
		num13 = num33 - num8 * num11 - num9 * num11 - num8 * num12;
		double num36 = num9 * num12 - num13;
		double num37 = num31 * num2;
		double num38 = splitter * num31;
		num7 = num38 - num31;
		num8 = num38 - num7;
		num9 = num31 - num8;
		double num39 = splitter * num2;
		num7 = num39 - num2;
		num11 = num39 - num7;
		num12 = num2 - num11;
		num13 = num37 - num8 * num11 - num9 * num11 - num8 * num12;
		double num40 = num9 * num12 - num13;
		num19 = num36 - num40;
		num20 = num36 - num19;
		num21 = num19 + num20;
		num22 = num20 - num40;
		num23 = num36 - num21;
		array2[0] = num23 + num22;
		num24 = num33 + num19;
		num20 = num24 - num33;
		num21 = num24 - num20;
		num22 = num19 - num20;
		num23 = num33 - num21;
		double num41 = num23 + num22;
		num19 = num41 - num37;
		num20 = num41 - num19;
		num21 = num19 + num20;
		num22 = num20 - num37;
		num23 = num41 - num21;
		array2[1] = num23 + num22;
		double num42 = num24 + num19;
		num20 = num42 - num24;
		num21 = num42 - num20;
		num22 = num19 - num20;
		num23 = num24 - num21;
		array2[2] = num23 + num22;
		array2[3] = num42;
		int elen = FastExpansionSumZeroElim(4, array, 4, array2, array3);
		num33 = num * num32;
		double num43 = splitter * num;
		num7 = num43 - num;
		num8 = num43 - num7;
		num9 = num - num8;
		double num44 = splitter * num32;
		num7 = num44 - num32;
		num11 = num44 - num7;
		num12 = num32 - num11;
		num13 = num33 - num8 * num11 - num9 * num11 - num8 * num12;
		double num45 = num9 * num12 - num13;
		num37 = num3 * num30;
		double num46 = splitter * num3;
		num7 = num46 - num3;
		num8 = num46 - num7;
		num9 = num3 - num8;
		double num47 = splitter * num30;
		num7 = num47 - num30;
		num11 = num47 - num7;
		num12 = num30 - num11;
		num13 = num37 - num8 * num11 - num9 * num11 - num8 * num12;
		num40 = num9 * num12 - num13;
		num19 = num45 - num40;
		num20 = num45 - num19;
		num21 = num19 + num20;
		num22 = num20 - num40;
		num23 = num45 - num21;
		array2[0] = num23 + num22;
		num24 = num33 + num19;
		num20 = num24 - num33;
		num21 = num24 - num20;
		num22 = num19 - num20;
		num23 = num33 - num21;
		double num48 = num23 + num22;
		num19 = num48 - num37;
		num20 = num48 - num19;
		num21 = num19 + num20;
		num22 = num20 - num37;
		num23 = num48 - num21;
		array2[1] = num23 + num22;
		num42 = num24 + num19;
		num20 = num42 - num24;
		num21 = num42 - num20;
		num22 = num19 - num20;
		num23 = num24 - num21;
		array2[2] = num23 + num22;
		array2[3] = num42;
		int elen2 = FastExpansionSumZeroElim(elen, array3, 4, array2, array4);
		num33 = num29 * num32;
		double num49 = splitter * num29;
		num7 = num49 - num29;
		num8 = num49 - num7;
		num9 = num29 - num8;
		double num50 = splitter * num32;
		num7 = num50 - num32;
		num11 = num50 - num7;
		num12 = num32 - num11;
		num13 = num33 - num8 * num11 - num9 * num11 - num8 * num12;
		double num51 = num9 * num12 - num13;
		num37 = num31 * num30;
		double num52 = splitter * num31;
		num7 = num52 - num31;
		num8 = num52 - num7;
		num9 = num31 - num8;
		double num53 = splitter * num30;
		num7 = num53 - num30;
		num11 = num53 - num7;
		num12 = num30 - num11;
		num13 = num37 - num8 * num11 - num9 * num11 - num8 * num12;
		num40 = num9 * num12 - num13;
		num19 = num51 - num40;
		num20 = num51 - num19;
		num21 = num19 + num20;
		num22 = num20 - num40;
		num23 = num51 - num21;
		array2[0] = num23 + num22;
		num24 = num33 + num19;
		num20 = num24 - num33;
		num21 = num24 - num20;
		num22 = num19 - num20;
		num23 = num33 - num21;
		double num54 = num23 + num22;
		num19 = num54 - num37;
		num20 = num54 - num19;
		num21 = num19 + num20;
		num22 = num20 - num37;
		num23 = num54 - num21;
		array2[1] = num23 + num22;
		num42 = num24 + num19;
		num20 = num42 - num24;
		num21 = num42 - num20;
		num22 = num19 - num20;
		num23 = num24 - num21;
		array2[2] = num23 + num22;
		array2[3] = num42;
		int num55 = FastExpansionSumZeroElim(elen2, array4, 4, array2, array5);
		return array5[num55 - 1];
	}

	private double InCircleAdapt(Point pa, Point pb, Point pc, Point pd, double permanent)
	{
		double[] array = new double[4];
		double[] array2 = new double[4];
		double[] array3 = new double[4];
		double[] array4 = new double[4];
		double[] array5 = new double[4];
		double[] array6 = new double[4];
		double[] array7 = new double[5];
		double[] array8 = new double[5];
		double[] array9 = new double[8];
		double[] array10 = new double[8];
		double[] array11 = new double[8];
		double[] array12 = new double[8];
		double[] array13 = new double[8];
		double[] array14 = new double[8];
		double[] array15 = new double[8];
		double[] array16 = new double[8];
		double[] array17 = new double[8];
		double[] array18 = new double[8];
		double[] array19 = new double[8];
		double[] array20 = new double[8];
		double[] array21 = new double[8];
		double[] array22 = new double[8];
		double[] array23 = new double[8];
		double[] array24 = new double[8];
		double[] array25 = new double[8];
		double[] array26 = new double[8];
		int elen = 0;
		int elen2 = 0;
		int elen3 = 0;
		int elen4 = 0;
		int elen5 = 0;
		int elen6 = 0;
		double[] array27 = new double[16];
		double[] array28 = new double[16];
		double[] array29 = new double[16];
		double[] array30 = new double[16];
		double[] array31 = new double[16];
		double[] array32 = new double[16];
		double[] array33 = new double[8];
		double[] array34 = new double[8];
		double[] array35 = new double[8];
		double[] array36 = new double[8];
		double[] array37 = new double[8];
		double[] array38 = new double[8];
		double[] array39 = new double[8];
		double[] array40 = new double[8];
		double[] array41 = new double[8];
		double[] array42 = new double[4];
		double[] array43 = new double[4];
		double[] array44 = new double[4];
		double num = pa.x - pd.x;
		double num2 = pb.x - pd.x;
		double num3 = pc.x - pd.x;
		double num4 = pa.y - pd.y;
		double num5 = pb.y - pd.y;
		double num6 = pc.y - pd.y;
		num = pa.x - pd.x;
		num2 = pb.x - pd.x;
		num3 = pc.x - pd.x;
		num4 = pa.y - pd.y;
		num5 = pb.y - pd.y;
		num6 = pc.y - pd.y;
		double num7 = num2 * num6;
		double num8 = splitter * num2;
		double num9 = num8 - num2;
		double num10 = num8 - num9;
		double num11 = num2 - num10;
		double num12 = splitter * num6;
		num9 = num12 - num6;
		double num13 = num12 - num9;
		double num14 = num6 - num13;
		double num15 = num7 - num10 * num13 - num11 * num13 - num10 * num14;
		double num16 = num11 * num14 - num15;
		double num17 = num3 * num5;
		double num18 = splitter * num3;
		num9 = num18 - num3;
		num10 = num18 - num9;
		num11 = num3 - num10;
		double num19 = splitter * num5;
		num9 = num19 - num5;
		num13 = num19 - num9;
		num14 = num5 - num13;
		num15 = num17 - num10 * num13 - num11 * num13 - num10 * num14;
		double num20 = num11 * num14 - num15;
		double num21 = num16 - num20;
		double num22 = num16 - num21;
		double num23 = num21 + num22;
		double num24 = num22 - num20;
		double num25 = num16 - num23;
		array[0] = num25 + num24;
		double num26 = num7 + num21;
		num22 = num26 - num7;
		num23 = num26 - num22;
		num24 = num21 - num22;
		num25 = num7 - num23;
		double num27 = num25 + num24;
		num21 = num27 - num17;
		num22 = num27 - num21;
		num23 = num21 + num22;
		num24 = num22 - num17;
		num25 = num27 - num23;
		array[1] = num25 + num24;
		double num28 = num26 + num21;
		num22 = num28 - num26;
		num23 = num28 - num22;
		num24 = num21 - num22;
		num25 = num26 - num23;
		array[2] = num25 + num24;
		array[3] = num28;
		int elen7 = ScaleExpansionZeroElim(4, array, num, axbc);
		int elen8 = ScaleExpansionZeroElim(elen7, axbc, num, axxbc);
		int elen9 = ScaleExpansionZeroElim(4, array, num4, aybc);
		int flen = ScaleExpansionZeroElim(elen9, aybc, num4, ayybc);
		int elen10 = FastExpansionSumZeroElim(elen8, axxbc, flen, ayybc, adet);
		double num29 = num3 * num4;
		double num30 = splitter * num3;
		num9 = num30 - num3;
		num10 = num30 - num9;
		num11 = num3 - num10;
		double num31 = splitter * num4;
		num9 = num31 - num4;
		num13 = num31 - num9;
		num14 = num4 - num13;
		num15 = num29 - num10 * num13 - num11 * num13 - num10 * num14;
		double num32 = num11 * num14 - num15;
		double num33 = num * num6;
		double num34 = splitter * num;
		num9 = num34 - num;
		num10 = num34 - num9;
		num11 = num - num10;
		double num35 = splitter * num6;
		num9 = num35 - num6;
		num13 = num35 - num9;
		num14 = num6 - num13;
		num15 = num33 - num10 * num13 - num11 * num13 - num10 * num14;
		double num36 = num11 * num14 - num15;
		num21 = num32 - num36;
		num22 = num32 - num21;
		num23 = num21 + num22;
		num24 = num22 - num36;
		num25 = num32 - num23;
		array2[0] = num25 + num24;
		num26 = num29 + num21;
		num22 = num26 - num29;
		num23 = num26 - num22;
		num24 = num21 - num22;
		num25 = num29 - num23;
		num27 = num25 + num24;
		num21 = num27 - num33;
		num22 = num27 - num21;
		num23 = num21 + num22;
		num24 = num22 - num33;
		num25 = num27 - num23;
		array2[1] = num25 + num24;
		double num37 = num26 + num21;
		num22 = num37 - num26;
		num23 = num37 - num22;
		num24 = num21 - num22;
		num25 = num26 - num23;
		array2[2] = num25 + num24;
		array2[3] = num37;
		int elen11 = ScaleExpansionZeroElim(4, array2, num2, bxca);
		int elen12 = ScaleExpansionZeroElim(elen11, bxca, num2, bxxca);
		int elen13 = ScaleExpansionZeroElim(4, array2, num5, byca);
		int flen2 = ScaleExpansionZeroElim(elen13, byca, num5, byyca);
		int flen3 = FastExpansionSumZeroElim(elen12, bxxca, flen2, byyca, bdet);
		double num38 = num * num5;
		double num39 = splitter * num;
		num9 = num39 - num;
		num10 = num39 - num9;
		num11 = num - num10;
		double num40 = splitter * num5;
		num9 = num40 - num5;
		num13 = num40 - num9;
		num14 = num5 - num13;
		num15 = num38 - num10 * num13 - num11 * num13 - num10 * num14;
		double num41 = num11 * num14 - num15;
		double num42 = num2 * num4;
		double num43 = splitter * num2;
		num9 = num43 - num2;
		num10 = num43 - num9;
		num11 = num2 - num10;
		double num44 = splitter * num4;
		num9 = num44 - num4;
		num13 = num44 - num9;
		num14 = num4 - num13;
		num15 = num42 - num10 * num13 - num11 * num13 - num10 * num14;
		double num45 = num11 * num14 - num15;
		num21 = num41 - num45;
		num22 = num41 - num21;
		num23 = num21 + num22;
		num24 = num22 - num45;
		num25 = num41 - num23;
		array3[0] = num25 + num24;
		num26 = num38 + num21;
		num22 = num26 - num38;
		num23 = num26 - num22;
		num24 = num21 - num22;
		num25 = num38 - num23;
		num27 = num25 + num24;
		num21 = num27 - num42;
		num22 = num27 - num21;
		num23 = num21 + num22;
		num24 = num22 - num42;
		num25 = num27 - num23;
		array3[1] = num25 + num24;
		double num46 = num26 + num21;
		num22 = num46 - num26;
		num23 = num46 - num22;
		num24 = num21 - num22;
		num25 = num26 - num23;
		array3[2] = num25 + num24;
		array3[3] = num46;
		int elen14 = ScaleExpansionZeroElim(4, array3, num3, cxab);
		int elen15 = ScaleExpansionZeroElim(elen14, cxab, num3, cxxab);
		int elen16 = ScaleExpansionZeroElim(4, array3, num6, cyab);
		int flen4 = ScaleExpansionZeroElim(elen16, cyab, num6, cyyab);
		int flen5 = FastExpansionSumZeroElim(elen15, cxxab, flen4, cyyab, cdet);
		int elen17 = FastExpansionSumZeroElim(elen10, adet, flen3, bdet, abdet);
		int num47 = FastExpansionSumZeroElim(elen17, abdet, flen5, cdet, fin1);
		double num48 = Estimate(num47, fin1);
		double num49 = iccerrboundB * permanent;
		if (num48 >= num49 || 0.0 - num48 >= num49)
		{
			return num48;
		}
		num22 = pa.x - num;
		num23 = num + num22;
		num24 = num22 - pd.x;
		num25 = pa.x - num23;
		double num50 = num25 + num24;
		num22 = pa.y - num4;
		num23 = num4 + num22;
		num24 = num22 - pd.y;
		num25 = pa.y - num23;
		double num51 = num25 + num24;
		num22 = pb.x - num2;
		num23 = num2 + num22;
		num24 = num22 - pd.x;
		num25 = pb.x - num23;
		double num52 = num25 + num24;
		num22 = pb.y - num5;
		num23 = num5 + num22;
		num24 = num22 - pd.y;
		num25 = pb.y - num23;
		double num53 = num25 + num24;
		num22 = pc.x - num3;
		num23 = num3 + num22;
		num24 = num22 - pd.x;
		num25 = pc.x - num23;
		double num54 = num25 + num24;
		num22 = pc.y - num6;
		num23 = num6 + num22;
		num24 = num22 - pd.y;
		num25 = pc.y - num23;
		double num55 = num25 + num24;
		if (num50 == 0.0 && num52 == 0.0 && num54 == 0.0 && num51 == 0.0 && num53 == 0.0 && num55 == 0.0)
		{
			return num48;
		}
		num49 = iccerrboundC * permanent + resulterrbound * ((num48 >= 0.0) ? num48 : (0.0 - num48));
		num48 += (num * num + num4 * num4) * (num2 * num55 + num6 * num52 - (num5 * num54 + num3 * num53)) + 2.0 * (num * num50 + num4 * num51) * (num2 * num6 - num5 * num3) + ((num2 * num2 + num5 * num5) * (num3 * num51 + num4 * num54 - (num6 * num50 + num * num55)) + 2.0 * (num2 * num52 + num5 * num53) * (num3 * num4 - num6 * num)) + ((num3 * num3 + num6 * num6) * (num * num53 + num5 * num50 - (num4 * num52 + num2 * num51)) + 2.0 * (num3 * num54 + num6 * num55) * (num * num5 - num4 * num2));
		if (num48 >= num49 || 0.0 - num48 >= num49)
		{
			return num48;
		}
		double[] array45 = fin1;
		double[] array46 = fin2;
		if (num52 != 0.0 || num53 != 0.0 || num54 != 0.0 || num55 != 0.0)
		{
			double num56 = num * num;
			double num57 = splitter * num;
			num9 = num57 - num;
			num10 = num57 - num9;
			num11 = num - num10;
			num15 = num56 - num10 * num10 - (num10 + num10) * num11;
			double num58 = num11 * num11 - num15;
			double num59 = num4 * num4;
			double num60 = splitter * num4;
			num9 = num60 - num4;
			num10 = num60 - num9;
			num11 = num4 - num10;
			num15 = num59 - num10 * num10 - (num10 + num10) * num11;
			double num61 = num11 * num11 - num15;
			num21 = num58 + num61;
			num22 = num21 - num58;
			num23 = num21 - num22;
			num24 = num61 - num22;
			num25 = num58 - num23;
			array4[0] = num25 + num24;
			num26 = num56 + num21;
			num22 = num26 - num56;
			num23 = num26 - num22;
			num24 = num21 - num22;
			num25 = num56 - num23;
			num27 = num25 + num24;
			num21 = num27 + num59;
			num22 = num21 - num27;
			num23 = num21 - num22;
			num24 = num59 - num22;
			num25 = num27 - num23;
			array4[1] = num25 + num24;
			double num62 = num26 + num21;
			num22 = num62 - num26;
			num23 = num62 - num22;
			num24 = num21 - num22;
			num25 = num26 - num23;
			array4[2] = num25 + num24;
			array4[3] = num62;
		}
		if (num54 != 0.0 || num55 != 0.0 || num50 != 0.0 || num51 != 0.0)
		{
			double num63 = num2 * num2;
			double num64 = splitter * num2;
			num9 = num64 - num2;
			num10 = num64 - num9;
			num11 = num2 - num10;
			num15 = num63 - num10 * num10 - (num10 + num10) * num11;
			double num65 = num11 * num11 - num15;
			double num66 = num5 * num5;
			double num67 = splitter * num5;
			num9 = num67 - num5;
			num10 = num67 - num9;
			num11 = num5 - num10;
			num15 = num66 - num10 * num10 - (num10 + num10) * num11;
			double num68 = num11 * num11 - num15;
			num21 = num65 + num68;
			num22 = num21 - num65;
			num23 = num21 - num22;
			num24 = num68 - num22;
			num25 = num65 - num23;
			array5[0] = num25 + num24;
			num26 = num63 + num21;
			num22 = num26 - num63;
			num23 = num26 - num22;
			num24 = num21 - num22;
			num25 = num63 - num23;
			num27 = num25 + num24;
			num21 = num27 + num66;
			num22 = num21 - num27;
			num23 = num21 - num22;
			num24 = num66 - num22;
			num25 = num27 - num23;
			array5[1] = num25 + num24;
			double num69 = num26 + num21;
			num22 = num69 - num26;
			num23 = num69 - num22;
			num24 = num21 - num22;
			num25 = num26 - num23;
			array5[2] = num25 + num24;
			array5[3] = num69;
		}
		if (num50 != 0.0 || num51 != 0.0 || num52 != 0.0 || num53 != 0.0)
		{
			double num70 = num3 * num3;
			double num71 = splitter * num3;
			num9 = num71 - num3;
			num10 = num71 - num9;
			num11 = num3 - num10;
			num15 = num70 - num10 * num10 - (num10 + num10) * num11;
			double num72 = num11 * num11 - num15;
			double num73 = num6 * num6;
			double num74 = splitter * num6;
			num9 = num74 - num6;
			num10 = num74 - num9;
			num11 = num6 - num10;
			num15 = num73 - num10 * num10 - (num10 + num10) * num11;
			double num75 = num11 * num11 - num15;
			num21 = num72 + num75;
			num22 = num21 - num72;
			num23 = num21 - num22;
			num24 = num75 - num22;
			num25 = num72 - num23;
			array6[0] = num25 + num24;
			num26 = num70 + num21;
			num22 = num26 - num70;
			num23 = num26 - num22;
			num24 = num21 - num22;
			num25 = num70 - num23;
			num27 = num25 + num24;
			num21 = num27 + num73;
			num22 = num21 - num27;
			num23 = num21 - num22;
			num24 = num73 - num22;
			num25 = num27 - num23;
			array6[1] = num25 + num24;
			double num76 = num26 + num21;
			num22 = num76 - num26;
			num23 = num76 - num22;
			num24 = num21 - num22;
			num25 = num26 - num23;
			array6[2] = num25 + num24;
			array6[3] = num76;
		}
		if (num50 != 0.0)
		{
			elen = ScaleExpansionZeroElim(4, array, num50, array21);
			int elen18 = ScaleExpansionZeroElim(elen, array21, 2.0 * num, temp16a);
			int elen19 = ScaleExpansionZeroElim(4, array6, num50, array10);
			int flen6 = ScaleExpansionZeroElim(elen19, array10, num5, temp16b);
			int elen20 = ScaleExpansionZeroElim(4, array5, num50, array9);
			int elen21 = ScaleExpansionZeroElim(elen20, array9, 0.0 - num6, temp16c);
			int flen7 = FastExpansionSumZeroElim(elen18, temp16a, flen6, temp16b, temp32a);
			int flen8 = FastExpansionSumZeroElim(elen21, temp16c, flen7, temp32a, temp48);
			num47 = FastExpansionSumZeroElim(num47, array45, flen8, temp48, array46);
			double[] array47 = array45;
			array45 = array46;
			array46 = array47;
		}
		if (num51 != 0.0)
		{
			elen2 = ScaleExpansionZeroElim(4, array, num51, array22);
			int elen18 = ScaleExpansionZeroElim(elen2, array22, 2.0 * num4, temp16a);
			int elen22 = ScaleExpansionZeroElim(4, array5, num51, array11);
			int flen6 = ScaleExpansionZeroElim(elen22, array11, num3, temp16b);
			int elen23 = ScaleExpansionZeroElim(4, array6, num51, array12);
			int elen21 = ScaleExpansionZeroElim(elen23, array12, 0.0 - num2, temp16c);
			int flen7 = FastExpansionSumZeroElim(elen18, temp16a, flen6, temp16b, temp32a);
			int flen8 = FastExpansionSumZeroElim(elen21, temp16c, flen7, temp32a, temp48);
			num47 = FastExpansionSumZeroElim(num47, array45, flen8, temp48, array46);
			double[] array48 = array45;
			array45 = array46;
			array46 = array48;
		}
		if (num52 != 0.0)
		{
			elen3 = ScaleExpansionZeroElim(4, array2, num52, array23);
			int elen18 = ScaleExpansionZeroElim(elen3, array23, 2.0 * num2, temp16a);
			int elen24 = ScaleExpansionZeroElim(4, array4, num52, array13);
			int flen6 = ScaleExpansionZeroElim(elen24, array13, num6, temp16b);
			int elen25 = ScaleExpansionZeroElim(4, array6, num52, array14);
			int elen21 = ScaleExpansionZeroElim(elen25, array14, 0.0 - num4, temp16c);
			int flen7 = FastExpansionSumZeroElim(elen18, temp16a, flen6, temp16b, temp32a);
			int flen8 = FastExpansionSumZeroElim(elen21, temp16c, flen7, temp32a, temp48);
			num47 = FastExpansionSumZeroElim(num47, array45, flen8, temp48, array46);
			double[] array49 = array45;
			array45 = array46;
			array46 = array49;
		}
		if (num53 != 0.0)
		{
			elen4 = ScaleExpansionZeroElim(4, array2, num53, array24);
			int elen18 = ScaleExpansionZeroElim(elen4, array24, 2.0 * num5, temp16a);
			int elen26 = ScaleExpansionZeroElim(4, array6, num53, array16);
			int flen6 = ScaleExpansionZeroElim(elen26, array16, num, temp16b);
			int elen27 = ScaleExpansionZeroElim(4, array4, num53, array15);
			int elen21 = ScaleExpansionZeroElim(elen27, array15, 0.0 - num3, temp16c);
			int flen7 = FastExpansionSumZeroElim(elen18, temp16a, flen6, temp16b, temp32a);
			int flen8 = FastExpansionSumZeroElim(elen21, temp16c, flen7, temp32a, temp48);
			num47 = FastExpansionSumZeroElim(num47, array45, flen8, temp48, array46);
			double[] array50 = array45;
			array45 = array46;
			array46 = array50;
		}
		if (num54 != 0.0)
		{
			elen5 = ScaleExpansionZeroElim(4, array3, num54, array25);
			int elen18 = ScaleExpansionZeroElim(elen5, array25, 2.0 * num3, temp16a);
			int elen28 = ScaleExpansionZeroElim(4, array5, num54, array18);
			int flen6 = ScaleExpansionZeroElim(elen28, array18, num4, temp16b);
			int elen29 = ScaleExpansionZeroElim(4, array4, num54, array17);
			int elen21 = ScaleExpansionZeroElim(elen29, array17, 0.0 - num5, temp16c);
			int flen7 = FastExpansionSumZeroElim(elen18, temp16a, flen6, temp16b, temp32a);
			int flen8 = FastExpansionSumZeroElim(elen21, temp16c, flen7, temp32a, temp48);
			num47 = FastExpansionSumZeroElim(num47, array45, flen8, temp48, array46);
			double[] array51 = array45;
			array45 = array46;
			array46 = array51;
		}
		if (num55 != 0.0)
		{
			elen6 = ScaleExpansionZeroElim(4, array3, num55, array26);
			int elen18 = ScaleExpansionZeroElim(elen6, array26, 2.0 * num6, temp16a);
			int elen30 = ScaleExpansionZeroElim(4, array4, num55, array19);
			int flen6 = ScaleExpansionZeroElim(elen30, array19, num2, temp16b);
			int elen31 = ScaleExpansionZeroElim(4, array5, num55, array20);
			int elen21 = ScaleExpansionZeroElim(elen31, array20, 0.0 - num, temp16c);
			int flen7 = FastExpansionSumZeroElim(elen18, temp16a, flen6, temp16b, temp32a);
			int flen8 = FastExpansionSumZeroElim(elen21, temp16c, flen7, temp32a, temp48);
			num47 = FastExpansionSumZeroElim(num47, array45, flen8, temp48, array46);
			double[] array52 = array45;
			array45 = array46;
			array46 = array52;
		}
		if (num50 != 0.0 || num51 != 0.0)
		{
			int elen32;
			int elen33;
			if (num52 != 0.0 || num53 != 0.0 || num54 != 0.0 || num55 != 0.0)
			{
				double num77 = num52 * num6;
				double num78 = splitter * num52;
				num9 = num78 - num52;
				num10 = num78 - num9;
				num11 = num52 - num10;
				double num79 = splitter * num6;
				num9 = num79 - num6;
				num13 = num79 - num9;
				num14 = num6 - num13;
				num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
				double num80 = num11 * num14 - num15;
				double num81 = num2 * num55;
				double num82 = splitter * num2;
				num9 = num82 - num2;
				num10 = num82 - num9;
				num11 = num2 - num10;
				double num83 = splitter * num55;
				num9 = num83 - num55;
				num13 = num83 - num9;
				num14 = num55 - num13;
				num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
				double num84 = num11 * num14 - num15;
				num21 = num80 + num84;
				num22 = num21 - num80;
				num23 = num21 - num22;
				num24 = num84 - num22;
				num25 = num80 - num23;
				array7[0] = num25 + num24;
				num26 = num77 + num21;
				num22 = num26 - num77;
				num23 = num26 - num22;
				num24 = num21 - num22;
				num25 = num77 - num23;
				num27 = num25 + num24;
				num21 = num27 + num81;
				num22 = num21 - num27;
				num23 = num21 - num22;
				num24 = num81 - num22;
				num25 = num27 - num23;
				array7[1] = num25 + num24;
				double num85 = num26 + num21;
				num22 = num85 - num26;
				num23 = num85 - num22;
				num24 = num21 - num22;
				num25 = num26 - num23;
				array7[2] = num25 + num24;
				array7[3] = num85;
				double num86 = 0.0 - num5;
				num77 = num54 * num86;
				double num87 = splitter * num54;
				num9 = num87 - num54;
				num10 = num87 - num9;
				num11 = num54 - num10;
				double num88 = splitter * num86;
				num9 = num88 - num86;
				num13 = num88 - num9;
				num14 = num86 - num13;
				num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
				num80 = num11 * num14 - num15;
				num86 = 0.0 - num53;
				num81 = num3 * num86;
				double num89 = splitter * num3;
				num9 = num89 - num3;
				num10 = num89 - num9;
				num11 = num3 - num10;
				double num90 = splitter * num86;
				num9 = num90 - num86;
				num13 = num90 - num9;
				num14 = num86 - num13;
				num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
				num84 = num11 * num14 - num15;
				num21 = num80 + num84;
				num22 = num21 - num80;
				num23 = num21 - num22;
				num24 = num84 - num22;
				num25 = num80 - num23;
				array8[0] = num25 + num24;
				num26 = num77 + num21;
				num22 = num26 - num77;
				num23 = num26 - num22;
				num24 = num21 - num22;
				num25 = num77 - num23;
				num27 = num25 + num24;
				num21 = num27 + num81;
				num22 = num21 - num27;
				num23 = num21 - num22;
				num24 = num81 - num22;
				num25 = num27 - num23;
				array8[1] = num25 + num24;
				double num91 = num26 + num21;
				num22 = num91 - num26;
				num23 = num91 - num22;
				num24 = num21 - num22;
				num25 = num26 - num23;
				array8[2] = num25 + num24;
				array8[3] = num91;
				elen32 = FastExpansionSumZeroElim(4, array7, 4, array8, array40);
				num77 = num52 * num55;
				double num92 = splitter * num52;
				num9 = num92 - num52;
				num10 = num92 - num9;
				num11 = num52 - num10;
				double num93 = splitter * num55;
				num9 = num93 - num55;
				num13 = num93 - num9;
				num14 = num55 - num13;
				num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
				num80 = num11 * num14 - num15;
				num81 = num54 * num53;
				double num94 = splitter * num54;
				num9 = num94 - num54;
				num10 = num94 - num9;
				num11 = num54 - num10;
				double num95 = splitter * num53;
				num9 = num95 - num53;
				num13 = num95 - num9;
				num14 = num53 - num13;
				num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
				num84 = num11 * num14 - num15;
				num21 = num80 - num84;
				num22 = num80 - num21;
				num23 = num21 + num22;
				num24 = num22 - num84;
				num25 = num80 - num23;
				array43[0] = num25 + num24;
				num26 = num77 + num21;
				num22 = num26 - num77;
				num23 = num26 - num22;
				num24 = num21 - num22;
				num25 = num77 - num23;
				num27 = num25 + num24;
				num21 = num27 - num81;
				num22 = num27 - num21;
				num23 = num21 + num22;
				num24 = num22 - num81;
				num25 = num27 - num23;
				array43[1] = num25 + num24;
				double num96 = num26 + num21;
				num22 = num96 - num26;
				num23 = num96 - num22;
				num24 = num21 - num22;
				num25 = num26 - num23;
				array43[2] = num25 + num24;
				array43[3] = num96;
				elen33 = 4;
			}
			else
			{
				array40[0] = 0.0;
				elen32 = 1;
				array43[0] = 0.0;
				elen33 = 1;
			}
			if (num50 != 0.0)
			{
				int elen18 = ScaleExpansionZeroElim(elen, array21, num50, temp16a);
				int elen34 = ScaleExpansionZeroElim(elen32, array40, num50, array27);
				int flen7 = ScaleExpansionZeroElim(elen34, array27, 2.0 * num, temp32a);
				int flen8 = FastExpansionSumZeroElim(elen18, temp16a, flen7, temp32a, temp48);
				num47 = FastExpansionSumZeroElim(num47, array45, flen8, temp48, array46);
				double[] array53 = array45;
				array45 = array46;
				array46 = array53;
				if (num53 != 0.0)
				{
					int elen35 = ScaleExpansionZeroElim(4, array6, num50, temp8);
					elen18 = ScaleExpansionZeroElim(elen35, temp8, num53, temp16a);
					num47 = FastExpansionSumZeroElim(num47, array45, elen18, temp16a, array46);
					double[] array54 = array45;
					array45 = array46;
					array46 = array54;
				}
				if (num55 != 0.0)
				{
					int elen35 = ScaleExpansionZeroElim(4, array5, 0.0 - num50, temp8);
					elen18 = ScaleExpansionZeroElim(elen35, temp8, num55, temp16a);
					num47 = FastExpansionSumZeroElim(num47, array45, elen18, temp16a, array46);
					double[] array55 = array45;
					array45 = array46;
					array46 = array55;
				}
				flen7 = ScaleExpansionZeroElim(elen34, array27, num50, temp32a);
				int elen36 = ScaleExpansionZeroElim(elen33, array43, num50, array33);
				elen18 = ScaleExpansionZeroElim(elen36, array33, 2.0 * num, temp16a);
				int flen6 = ScaleExpansionZeroElim(elen36, array33, num50, temp16b);
				int flen9 = FastExpansionSumZeroElim(elen18, temp16a, flen6, temp16b, temp32b);
				int flen10 = FastExpansionSumZeroElim(flen7, temp32a, flen9, temp32b, temp64);
				num47 = FastExpansionSumZeroElim(num47, array45, flen10, temp64, array46);
				double[] array56 = array45;
				array45 = array46;
				array46 = array56;
			}
			if (num51 != 0.0)
			{
				int elen18 = ScaleExpansionZeroElim(elen2, array22, num51, temp16a);
				int elen37 = ScaleExpansionZeroElim(elen32, array40, num51, array28);
				int flen7 = ScaleExpansionZeroElim(elen37, array28, 2.0 * num4, temp32a);
				int flen8 = FastExpansionSumZeroElim(elen18, temp16a, flen7, temp32a, temp48);
				num47 = FastExpansionSumZeroElim(num47, array45, flen8, temp48, array46);
				double[] array57 = array45;
				array45 = array46;
				array46 = array57;
				flen7 = ScaleExpansionZeroElim(elen37, array28, num51, temp32a);
				int elen38 = ScaleExpansionZeroElim(elen33, array43, num51, array34);
				elen18 = ScaleExpansionZeroElim(elen38, array34, 2.0 * num4, temp16a);
				int flen6 = ScaleExpansionZeroElim(elen38, array34, num51, temp16b);
				int flen9 = FastExpansionSumZeroElim(elen18, temp16a, flen6, temp16b, temp32b);
				int flen10 = FastExpansionSumZeroElim(flen7, temp32a, flen9, temp32b, temp64);
				num47 = FastExpansionSumZeroElim(num47, array45, flen10, temp64, array46);
				double[] array58 = array45;
				array45 = array46;
				array46 = array58;
			}
		}
		if (num52 != 0.0 || num53 != 0.0)
		{
			int elen39;
			int elen40;
			if (num54 != 0.0 || num55 != 0.0 || num50 != 0.0 || num51 != 0.0)
			{
				double num77 = num54 * num4;
				double num97 = splitter * num54;
				num9 = num97 - num54;
				num10 = num97 - num9;
				num11 = num54 - num10;
				double num98 = splitter * num4;
				num9 = num98 - num4;
				num13 = num98 - num9;
				num14 = num4 - num13;
				num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
				double num80 = num11 * num14 - num15;
				double num81 = num3 * num51;
				double num99 = splitter * num3;
				num9 = num99 - num3;
				num10 = num99 - num9;
				num11 = num3 - num10;
				double num100 = splitter * num51;
				num9 = num100 - num51;
				num13 = num100 - num9;
				num14 = num51 - num13;
				num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
				double num84 = num11 * num14 - num15;
				num21 = num80 + num84;
				num22 = num21 - num80;
				num23 = num21 - num22;
				num24 = num84 - num22;
				num25 = num80 - num23;
				array7[0] = num25 + num24;
				num26 = num77 + num21;
				num22 = num26 - num77;
				num23 = num26 - num22;
				num24 = num21 - num22;
				num25 = num77 - num23;
				num27 = num25 + num24;
				num21 = num27 + num81;
				num22 = num21 - num27;
				num23 = num21 - num22;
				num24 = num81 - num22;
				num25 = num27 - num23;
				array7[1] = num25 + num24;
				double num85 = num26 + num21;
				num22 = num85 - num26;
				num23 = num85 - num22;
				num24 = num21 - num22;
				num25 = num26 - num23;
				array7[2] = num25 + num24;
				array7[3] = num85;
				double num86 = 0.0 - num6;
				num77 = num50 * num86;
				double num101 = splitter * num50;
				num9 = num101 - num50;
				num10 = num101 - num9;
				num11 = num50 - num10;
				double num102 = splitter * num86;
				num9 = num102 - num86;
				num13 = num102 - num9;
				num14 = num86 - num13;
				num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
				num80 = num11 * num14 - num15;
				num86 = 0.0 - num55;
				num81 = num * num86;
				double num103 = splitter * num;
				num9 = num103 - num;
				num10 = num103 - num9;
				num11 = num - num10;
				double num104 = splitter * num86;
				num9 = num104 - num86;
				num13 = num104 - num9;
				num14 = num86 - num13;
				num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
				num84 = num11 * num14 - num15;
				num21 = num80 + num84;
				num22 = num21 - num80;
				num23 = num21 - num22;
				num24 = num84 - num22;
				num25 = num80 - num23;
				array8[0] = num25 + num24;
				num26 = num77 + num21;
				num22 = num26 - num77;
				num23 = num26 - num22;
				num24 = num21 - num22;
				num25 = num77 - num23;
				num27 = num25 + num24;
				num21 = num27 + num81;
				num22 = num21 - num27;
				num23 = num21 - num22;
				num24 = num81 - num22;
				num25 = num27 - num23;
				array8[1] = num25 + num24;
				double num91 = num26 + num21;
				num22 = num91 - num26;
				num23 = num91 - num22;
				num24 = num21 - num22;
				num25 = num26 - num23;
				array8[2] = num25 + num24;
				array8[3] = num91;
				elen39 = FastExpansionSumZeroElim(4, array7, 4, array8, array41);
				num77 = num54 * num51;
				double num105 = splitter * num54;
				num9 = num105 - num54;
				num10 = num105 - num9;
				num11 = num54 - num10;
				double num106 = splitter * num51;
				num9 = num106 - num51;
				num13 = num106 - num9;
				num14 = num51 - num13;
				num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
				num80 = num11 * num14 - num15;
				num81 = num50 * num55;
				double num107 = splitter * num50;
				num9 = num107 - num50;
				num10 = num107 - num9;
				num11 = num50 - num10;
				double num108 = splitter * num55;
				num9 = num108 - num55;
				num13 = num108 - num9;
				num14 = num55 - num13;
				num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
				num84 = num11 * num14 - num15;
				num21 = num80 - num84;
				num22 = num80 - num21;
				num23 = num21 + num22;
				num24 = num22 - num84;
				num25 = num80 - num23;
				array44[0] = num25 + num24;
				num26 = num77 + num21;
				num22 = num26 - num77;
				num23 = num26 - num22;
				num24 = num21 - num22;
				num25 = num77 - num23;
				num27 = num25 + num24;
				num21 = num27 - num81;
				num22 = num27 - num21;
				num23 = num21 + num22;
				num24 = num22 - num81;
				num25 = num27 - num23;
				array44[1] = num25 + num24;
				double num109 = num26 + num21;
				num22 = num109 - num26;
				num23 = num109 - num22;
				num24 = num21 - num22;
				num25 = num26 - num23;
				array44[2] = num25 + num24;
				array44[3] = num109;
				elen40 = 4;
			}
			else
			{
				array41[0] = 0.0;
				elen39 = 1;
				array44[0] = 0.0;
				elen40 = 1;
			}
			if (num52 != 0.0)
			{
				int elen18 = ScaleExpansionZeroElim(elen3, array23, num52, temp16a);
				int elen41 = ScaleExpansionZeroElim(elen39, array41, num52, array29);
				int flen7 = ScaleExpansionZeroElim(elen41, array29, 2.0 * num2, temp32a);
				int flen8 = FastExpansionSumZeroElim(elen18, temp16a, flen7, temp32a, temp48);
				num47 = FastExpansionSumZeroElim(num47, array45, flen8, temp48, array46);
				double[] array59 = array45;
				array45 = array46;
				array46 = array59;
				if (num55 != 0.0)
				{
					int elen35 = ScaleExpansionZeroElim(4, array4, num52, temp8);
					elen18 = ScaleExpansionZeroElim(elen35, temp8, num55, temp16a);
					num47 = FastExpansionSumZeroElim(num47, array45, elen18, temp16a, array46);
					double[] array60 = array45;
					array45 = array46;
					array46 = array60;
				}
				if (num51 != 0.0)
				{
					int elen35 = ScaleExpansionZeroElim(4, array6, 0.0 - num52, temp8);
					elen18 = ScaleExpansionZeroElim(elen35, temp8, num51, temp16a);
					num47 = FastExpansionSumZeroElim(num47, array45, elen18, temp16a, array46);
					double[] array61 = array45;
					array45 = array46;
					array46 = array61;
				}
				flen7 = ScaleExpansionZeroElim(elen41, array29, num52, temp32a);
				int elen42 = ScaleExpansionZeroElim(elen40, array44, num52, array35);
				elen18 = ScaleExpansionZeroElim(elen42, array35, 2.0 * num2, temp16a);
				int flen6 = ScaleExpansionZeroElim(elen42, array35, num52, temp16b);
				int flen9 = FastExpansionSumZeroElim(elen18, temp16a, flen6, temp16b, temp32b);
				int flen10 = FastExpansionSumZeroElim(flen7, temp32a, flen9, temp32b, temp64);
				num47 = FastExpansionSumZeroElim(num47, array45, flen10, temp64, array46);
				double[] array62 = array45;
				array45 = array46;
				array46 = array62;
			}
			if (num53 != 0.0)
			{
				int elen18 = ScaleExpansionZeroElim(elen4, array24, num53, temp16a);
				int elen43 = ScaleExpansionZeroElim(elen39, array41, num53, array30);
				int flen7 = ScaleExpansionZeroElim(elen43, array30, 2.0 * num5, temp32a);
				int flen8 = FastExpansionSumZeroElim(elen18, temp16a, flen7, temp32a, temp48);
				num47 = FastExpansionSumZeroElim(num47, array45, flen8, temp48, array46);
				double[] array63 = array45;
				array45 = array46;
				array46 = array63;
				flen7 = ScaleExpansionZeroElim(elen43, array30, num53, temp32a);
				int elen44 = ScaleExpansionZeroElim(elen40, array44, num53, array36);
				elen18 = ScaleExpansionZeroElim(elen44, array36, 2.0 * num5, temp16a);
				int flen6 = ScaleExpansionZeroElim(elen44, array36, num53, temp16b);
				int flen9 = FastExpansionSumZeroElim(elen18, temp16a, flen6, temp16b, temp32b);
				int flen10 = FastExpansionSumZeroElim(flen7, temp32a, flen9, temp32b, temp64);
				num47 = FastExpansionSumZeroElim(num47, array45, flen10, temp64, array46);
				double[] array64 = array45;
				array45 = array46;
				array46 = array64;
			}
		}
		if (num54 != 0.0 || num55 != 0.0)
		{
			int elen45;
			int elen46;
			if (num50 != 0.0 || num51 != 0.0 || num52 != 0.0 || num53 != 0.0)
			{
				double num77 = num50 * num5;
				double num110 = splitter * num50;
				num9 = num110 - num50;
				num10 = num110 - num9;
				num11 = num50 - num10;
				double num111 = splitter * num5;
				num9 = num111 - num5;
				num13 = num111 - num9;
				num14 = num5 - num13;
				num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
				double num80 = num11 * num14 - num15;
				double num81 = num * num53;
				double num112 = splitter * num;
				num9 = num112 - num;
				num10 = num112 - num9;
				num11 = num - num10;
				double num113 = splitter * num53;
				num9 = num113 - num53;
				num13 = num113 - num9;
				num14 = num53 - num13;
				num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
				double num84 = num11 * num14 - num15;
				num21 = num80 + num84;
				num22 = num21 - num80;
				num23 = num21 - num22;
				num24 = num84 - num22;
				num25 = num80 - num23;
				array7[0] = num25 + num24;
				num26 = num77 + num21;
				num22 = num26 - num77;
				num23 = num26 - num22;
				num24 = num21 - num22;
				num25 = num77 - num23;
				num27 = num25 + num24;
				num21 = num27 + num81;
				num22 = num21 - num27;
				num23 = num21 - num22;
				num24 = num81 - num22;
				num25 = num27 - num23;
				array7[1] = num25 + num24;
				double num85 = num26 + num21;
				num22 = num85 - num26;
				num23 = num85 - num22;
				num24 = num21 - num22;
				num25 = num26 - num23;
				array7[2] = num25 + num24;
				array7[3] = num85;
				double num86 = 0.0 - num4;
				num77 = num52 * num86;
				double num114 = splitter * num52;
				num9 = num114 - num52;
				num10 = num114 - num9;
				num11 = num52 - num10;
				double num115 = splitter * num86;
				num9 = num115 - num86;
				num13 = num115 - num9;
				num14 = num86 - num13;
				num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
				num80 = num11 * num14 - num15;
				num86 = 0.0 - num51;
				num81 = num2 * num86;
				double num116 = splitter * num2;
				num9 = num116 - num2;
				num10 = num116 - num9;
				num11 = num2 - num10;
				double num117 = splitter * num86;
				num9 = num117 - num86;
				num13 = num117 - num9;
				num14 = num86 - num13;
				num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
				num84 = num11 * num14 - num15;
				num21 = num80 + num84;
				num22 = num21 - num80;
				num23 = num21 - num22;
				num24 = num84 - num22;
				num25 = num80 - num23;
				array8[0] = num25 + num24;
				num26 = num77 + num21;
				num22 = num26 - num77;
				num23 = num26 - num22;
				num24 = num21 - num22;
				num25 = num77 - num23;
				num27 = num25 + num24;
				num21 = num27 + num81;
				num22 = num21 - num27;
				num23 = num21 - num22;
				num24 = num81 - num22;
				num25 = num27 - num23;
				array8[1] = num25 + num24;
				double num91 = num26 + num21;
				num22 = num91 - num26;
				num23 = num91 - num22;
				num24 = num21 - num22;
				num25 = num26 - num23;
				array8[2] = num25 + num24;
				array8[3] = num91;
				elen45 = FastExpansionSumZeroElim(4, array7, 4, array8, array39);
				num77 = num50 * num53;
				double num118 = splitter * num50;
				num9 = num118 - num50;
				num10 = num118 - num9;
				num11 = num50 - num10;
				double num119 = splitter * num53;
				num9 = num119 - num53;
				num13 = num119 - num9;
				num14 = num53 - num13;
				num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
				num80 = num11 * num14 - num15;
				num81 = num52 * num51;
				double num120 = splitter * num52;
				num9 = num120 - num52;
				num10 = num120 - num9;
				num11 = num52 - num10;
				double num121 = splitter * num51;
				num9 = num121 - num51;
				num13 = num121 - num9;
				num14 = num51 - num13;
				num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
				num84 = num11 * num14 - num15;
				num21 = num80 - num84;
				num22 = num80 - num21;
				num23 = num21 + num22;
				num24 = num22 - num84;
				num25 = num80 - num23;
				array42[0] = num25 + num24;
				num26 = num77 + num21;
				num22 = num26 - num77;
				num23 = num26 - num22;
				num24 = num21 - num22;
				num25 = num77 - num23;
				num27 = num25 + num24;
				num21 = num27 - num81;
				num22 = num27 - num21;
				num23 = num21 + num22;
				num24 = num22 - num81;
				num25 = num27 - num23;
				array42[1] = num25 + num24;
				double num122 = num26 + num21;
				num22 = num122 - num26;
				num23 = num122 - num22;
				num24 = num21 - num22;
				num25 = num26 - num23;
				array42[2] = num25 + num24;
				array42[3] = num122;
				elen46 = 4;
			}
			else
			{
				array39[0] = 0.0;
				elen45 = 1;
				array42[0] = 0.0;
				elen46 = 1;
			}
			if (num54 != 0.0)
			{
				int elen18 = ScaleExpansionZeroElim(elen5, array25, num54, temp16a);
				int elen47 = ScaleExpansionZeroElim(elen45, array39, num54, array31);
				int flen7 = ScaleExpansionZeroElim(elen47, array31, 2.0 * num3, temp32a);
				int flen8 = FastExpansionSumZeroElim(elen18, temp16a, flen7, temp32a, temp48);
				num47 = FastExpansionSumZeroElim(num47, array45, flen8, temp48, array46);
				double[] array65 = array45;
				array45 = array46;
				array46 = array65;
				if (num51 != 0.0)
				{
					int elen35 = ScaleExpansionZeroElim(4, array5, num54, temp8);
					elen18 = ScaleExpansionZeroElim(elen35, temp8, num51, temp16a);
					num47 = FastExpansionSumZeroElim(num47, array45, elen18, temp16a, array46);
					double[] array66 = array45;
					array45 = array46;
					array46 = array66;
				}
				if (num53 != 0.0)
				{
					int elen35 = ScaleExpansionZeroElim(4, array4, 0.0 - num54, temp8);
					elen18 = ScaleExpansionZeroElim(elen35, temp8, num53, temp16a);
					num47 = FastExpansionSumZeroElim(num47, array45, elen18, temp16a, array46);
					double[] array67 = array45;
					array45 = array46;
					array46 = array67;
				}
				flen7 = ScaleExpansionZeroElim(elen47, array31, num54, temp32a);
				int elen48 = ScaleExpansionZeroElim(elen46, array42, num54, array37);
				elen18 = ScaleExpansionZeroElim(elen48, array37, 2.0 * num3, temp16a);
				int flen6 = ScaleExpansionZeroElim(elen48, array37, num54, temp16b);
				int flen9 = FastExpansionSumZeroElim(elen18, temp16a, flen6, temp16b, temp32b);
				int flen10 = FastExpansionSumZeroElim(flen7, temp32a, flen9, temp32b, temp64);
				num47 = FastExpansionSumZeroElim(num47, array45, flen10, temp64, array46);
				double[] array68 = array45;
				array45 = array46;
				array46 = array68;
			}
			if (num55 != 0.0)
			{
				int elen18 = ScaleExpansionZeroElim(elen6, array26, num55, temp16a);
				int elen49 = ScaleExpansionZeroElim(elen45, array39, num55, array32);
				int flen7 = ScaleExpansionZeroElim(elen49, array32, 2.0 * num6, temp32a);
				int flen8 = FastExpansionSumZeroElim(elen18, temp16a, flen7, temp32a, temp48);
				num47 = FastExpansionSumZeroElim(num47, array45, flen8, temp48, array46);
				double[] array69 = array45;
				array45 = array46;
				array46 = array69;
				flen7 = ScaleExpansionZeroElim(elen49, array32, num55, temp32a);
				int elen50 = ScaleExpansionZeroElim(elen46, array42, num55, array38);
				elen18 = ScaleExpansionZeroElim(elen50, array38, 2.0 * num6, temp16a);
				int flen6 = ScaleExpansionZeroElim(elen50, array38, num55, temp16b);
				int flen9 = FastExpansionSumZeroElim(elen18, temp16a, flen6, temp16b, temp32b);
				int flen10 = FastExpansionSumZeroElim(flen7, temp32a, flen9, temp32b, temp64);
				num47 = FastExpansionSumZeroElim(num47, array45, flen10, temp64, array46);
				double[] array70 = array45;
				array45 = array46;
				array46 = array70;
			}
		}
		return array45[num47 - 1];
	}

	private void AllocateWorkspace()
	{
		fin1 = new double[1152];
		fin2 = new double[1152];
		abdet = new double[64];
		axbc = new double[8];
		axxbc = new double[16];
		aybc = new double[8];
		ayybc = new double[16];
		adet = new double[32];
		bxca = new double[8];
		bxxca = new double[16];
		byca = new double[8];
		byyca = new double[16];
		bdet = new double[32];
		cxab = new double[8];
		cxxab = new double[16];
		cyab = new double[8];
		cyyab = new double[16];
		cdet = new double[32];
		temp8 = new double[8];
		temp16a = new double[16];
		temp16b = new double[16];
		temp16c = new double[16];
		temp32a = new double[32];
		temp32b = new double[32];
		temp48 = new double[48];
		temp64 = new double[64];
	}

	private void ClearWorkspace()
	{
	}
}
