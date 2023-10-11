using System.Collections.Generic;
using UnityEngine;

namespace AmplifyImpostors;

public class Triangulator
{
	private List<Vector2> m_points = new List<Vector2>();

	public List<Vector2> Points => m_points;

	public Triangulator(Vector2[] points)
	{
		m_points = new List<Vector2>(points);
	}

	public Triangulator(Vector2[] points, bool invertY = true)
	{
		if (invertY)
		{
			m_points = new List<Vector2>();
			for (int i = 0; i < points.Length; i++)
			{
				m_points.Add(new Vector2(points[i].x, 1f - points[i].y));
			}
		}
		else
		{
			m_points = new List<Vector2>(points);
		}
	}

	public int[] Triangulate()
	{
		List<int> list = new List<int>();
		int count = m_points.Count;
		if (count < 3)
		{
			return list.ToArray();
		}
		int[] array = new int[count];
		if (Area() > 0f)
		{
			for (int i = 0; i < count; i++)
			{
				array[i] = i;
			}
		}
		else
		{
			for (int j = 0; j < count; j++)
			{
				array[j] = count - 1 - j;
			}
		}
		int num = count;
		int num2 = 2 * num;
		int num3 = 0;
		int num4 = num - 1;
		while (num > 2)
		{
			if (num2-- <= 0)
			{
				return list.ToArray();
			}
			int num5 = num4;
			if (num <= num5)
			{
				num5 = 0;
			}
			num4 = num5 + 1;
			if (num <= num4)
			{
				num4 = 0;
			}
			int num6 = num4 + 1;
			if (num <= num6)
			{
				num6 = 0;
			}
			if (Snip(num5, num4, num6, num, array))
			{
				int item = array[num5];
				int item2 = array[num4];
				int item3 = array[num6];
				list.Add(item);
				list.Add(item2);
				list.Add(item3);
				num3++;
				int num7 = num4;
				for (int k = num4 + 1; k < num; k++)
				{
					array[num7] = array[k];
					num7++;
				}
				num--;
				num2 = 2 * num;
			}
		}
		list.Reverse();
		return list.ToArray();
	}

	private float Area()
	{
		int count = m_points.Count;
		float num = 0f;
		int index = count - 1;
		int num2 = 0;
		while (num2 < count)
		{
			Vector2 vector = m_points[index];
			Vector2 vector2 = m_points[num2];
			num += vector.x * vector2.y - vector2.x * vector.y;
			index = num2++;
		}
		return num * 0.5f;
	}

	private bool Snip(int u, int v, int w, int n, int[] V)
	{
		Vector2 v2 = m_points[V[u]];
		Vector2 v3 = m_points[V[v]];
		Vector2 v4 = m_points[V[w]];
		if (Mathf.Epsilon > (v3.x - v2.x) * (v4.y - v2.y) - (v3.y - v2.y) * (v4.x - v2.x))
		{
			return false;
		}
		for (int i = 0; i < n; i++)
		{
			if (i != u && i != v && i != w)
			{
				Vector2 pt = m_points[V[i]];
				if (InsideTriangle(pt, v2, v3, v4))
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool InsideTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
	{
		bool num = pt.Cross(v1, v2) < 0f;
		bool flag = pt.Cross(v2, v3) < 0f;
		bool flag2 = pt.Cross(v3, v1) < 0f;
		if (num == flag)
		{
			return flag == flag2;
		}
		return false;
	}
}
