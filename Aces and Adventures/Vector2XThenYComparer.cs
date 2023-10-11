using System.Collections.Generic;
using UnityEngine;

public class Vector2XThenYComparer : IComparer<Vector2>
{
	public static Vector2XThenYComparer Ascending = new Vector2XThenYComparer(1);

	public static Vector2XThenYComparer Descending = new Vector2XThenYComparer(-1);

	private readonly int _sign;

	private Vector2XThenYComparer(int sign)
	{
		_sign = sign;
	}

	public int Compare(Vector2 a, Vector2 b)
	{
		int num = a.x.CompareTo(b.x);
		return ((num != 0) ? num : a.y.CompareTo(b.y)) * _sign;
	}
}
