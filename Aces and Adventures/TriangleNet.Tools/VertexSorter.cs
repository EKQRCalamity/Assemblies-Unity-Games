using System;
using TriangleNet.Geometry;

namespace TriangleNet.Tools;

public class VertexSorter
{
	private const int RANDOM_SEED = 57113;

	private Random rand;

	private Vertex[] points;

	private VertexSorter(Vertex[] points, int seed)
	{
		this.points = points;
		rand = new Random(seed);
	}

	public static void Sort(Vertex[] array, int seed = 57113)
	{
		new VertexSorter(array, seed).QuickSort(0, array.Length - 1);
	}

	public static void Alternate(Vertex[] array, int length, int seed = 57113)
	{
		VertexSorter vertexSorter = new VertexSorter(array, seed);
		int num = length >> 1;
		if (length - num >= 2)
		{
			if (num >= 2)
			{
				vertexSorter.AlternateAxes(0, num - 1, 1);
			}
			vertexSorter.AlternateAxes(num, length - 1, 1);
		}
	}

	private void QuickSort(int left, int right)
	{
		int num = left;
		int num2 = right;
		int num3 = right - left + 1;
		Vertex[] array = points;
		if (num3 < 32)
		{
			for (int i = left + 1; i <= right; i++)
			{
				Vertex vertex = array[i];
				int num4 = i - 1;
				while (num4 >= left && (array[num4].x > vertex.x || (array[num4].x == vertex.x && array[num4].y > vertex.y)))
				{
					array[num4 + 1] = array[num4];
					num4--;
				}
				array[num4 + 1] = vertex;
			}
			return;
		}
		int num5 = rand.Next(left, right);
		double x = array[num5].x;
		double y = array[num5].y;
		left--;
		right++;
		while (left < right)
		{
			do
			{
				left++;
			}
			while (left <= right && (array[left].x < x || (array[left].x == x && array[left].y < y)));
			do
			{
				right--;
			}
			while (left <= right && (array[right].x > x || (array[right].x == x && array[right].y > y)));
			if (left < right)
			{
				Vertex vertex2 = array[left];
				array[left] = array[right];
				array[right] = vertex2;
			}
		}
		if (left > num)
		{
			QuickSort(num, left);
		}
		if (num2 > right + 1)
		{
			QuickSort(right + 1, num2);
		}
	}

	private void AlternateAxes(int left, int right, int axis)
	{
		int num = right - left + 1;
		int num2 = num >> 1;
		if (num <= 3)
		{
			axis = 0;
		}
		if (axis == 0)
		{
			VertexMedianX(left, right, left + num2);
		}
		else
		{
			VertexMedianY(left, right, left + num2);
		}
		if (num - num2 >= 2)
		{
			if (num2 >= 2)
			{
				AlternateAxes(left, left + num2 - 1, 1 - axis);
			}
			AlternateAxes(left + num2, right, 1 - axis);
		}
	}

	private void VertexMedianX(int left, int right, int median)
	{
		int num = right - left + 1;
		int left2 = left;
		int right2 = right;
		Vertex[] array = points;
		if (num == 2)
		{
			if (array[left].x > array[right].x || (array[left].x == array[right].x && array[left].y > array[right].y))
			{
				Vertex vertex = array[right];
				array[right] = array[left];
				array[left] = vertex;
			}
			return;
		}
		int num2 = rand.Next(left, right);
		double x = array[num2].x;
		double y = array[num2].y;
		left--;
		right++;
		while (left < right)
		{
			do
			{
				left++;
			}
			while (left <= right && (array[left].x < x || (array[left].x == x && array[left].y < y)));
			do
			{
				right--;
			}
			while (left <= right && (array[right].x > x || (array[right].x == x && array[right].y > y)));
			if (left < right)
			{
				Vertex vertex = array[left];
				array[left] = array[right];
				array[right] = vertex;
			}
		}
		if (left > median)
		{
			VertexMedianX(left2, left - 1, median);
		}
		if (right < median - 1)
		{
			VertexMedianX(right + 1, right2, median);
		}
	}

	private void VertexMedianY(int left, int right, int median)
	{
		int num = right - left + 1;
		int left2 = left;
		int right2 = right;
		Vertex[] array = points;
		if (num == 2)
		{
			if (array[left].y > array[right].y || (array[left].y == array[right].y && array[left].x > array[right].x))
			{
				Vertex vertex = array[right];
				array[right] = array[left];
				array[left] = vertex;
			}
			return;
		}
		int num2 = rand.Next(left, right);
		double y = array[num2].y;
		double x = array[num2].x;
		left--;
		right++;
		while (left < right)
		{
			do
			{
				left++;
			}
			while (left <= right && (array[left].y < y || (array[left].y == y && array[left].x < x)));
			do
			{
				right--;
			}
			while (left <= right && (array[right].y > y || (array[right].y == y && array[right].x > x)));
			if (left < right)
			{
				Vertex vertex = array[left];
				array[left] = array[right];
				array[right] = vertex;
			}
		}
		if (left > median)
		{
			VertexMedianY(left2, left - 1, median);
		}
		if (right < median - 1)
		{
			VertexMedianY(right + 1, right2, median);
		}
	}
}
