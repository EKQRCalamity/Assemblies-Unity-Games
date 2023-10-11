using System.Collections.Generic;
using UnityEngine;

public class Vector3EqualityComparerApproximate : IEqualityComparer<Vector3>
{
	public static Vector3EqualityComparerApproximate Default = new Vector3EqualityComparerApproximate();

	private float _epsilon;

	public Vector3EqualityComparerApproximate(float epsilon = 0.001f)
	{
		_epsilon = epsilon * epsilon;
	}

	public bool Equals(Vector3 x, Vector3 y)
	{
		return (x - y).sqrMagnitude < _epsilon;
	}

	public int GetHashCode(Vector3 obj)
	{
		return obj.GetHashCode();
	}
}
