using System.Collections.Generic;
using UnityEngine;

public class LexigraphicComparer : IComparer<Vector2>
{
	public static LexigraphicComparer Default = new LexigraphicComparer();

	public int Compare(Vector2 a, Vector2 b)
	{
		int num = Comparer<float>.Default.Compare(a.x, b.x);
		if (num == 0)
		{
			return Comparer<float>.Default.Compare(a.y, b.y);
		}
		return num;
	}
}
