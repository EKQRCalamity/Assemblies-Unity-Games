using System.Collections.Generic;
using UnityEngine;

public class Vector3NormalEqualityComparerApproximate : IEqualityComparer<Vector3>
{
	public static Vector3NormalEqualityComparerApproximate Default = new Vector3NormalEqualityComparerApproximate();

	private float _epsilon;

	public Vector3NormalEqualityComparerApproximate()
	{
		_epsilon = MathUtil.CosOneDegree;
	}

	public bool Equals(Vector3 x, Vector3 y)
	{
		return Vector3.Dot(x, y) > _epsilon;
	}

	public int GetHashCode(Vector3 obj)
	{
		return obj.GetHashCode();
	}
}
