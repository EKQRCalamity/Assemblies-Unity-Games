using System.Collections.Generic;
using UnityEngine;

public class ClockwiseComparerVector3 : IComparer<Vector3>
{
	private Vector3 center;

	public static PoolHandle<ClockwiseComparerVector3> Use(Vector3 center)
	{
		PoolHandle<ClockwiseComparerVector3> poolHandle = Pools.Use<ClockwiseComparerVector3>();
		poolHandle.value.SetData(center);
		return poolHandle;
	}

	private ClockwiseComparerVector3 SetData(Vector3 center)
	{
		this.center = center;
		return this;
	}

	public int Compare(Vector3 a, Vector3 b)
	{
		if (a.x - center.x >= 0f && b.x - center.x < 0f)
		{
			return -1;
		}
		if (a.x - center.x < 0f && b.x - center.x >= 0f)
		{
			return 1;
		}
		if (a.x - center.x == 0f && b.x - center.x == 0f)
		{
			if (a.y - center.y >= 0f || b.y - center.y >= 0f)
			{
				return MathUtil.Compare(a.y, b.y);
			}
			return MathUtil.Compare(b.y, a.y);
		}
		float num = (a.x - center.x) * (b.y - center.y) - (b.x - center.x) * (a.y - center.y);
		if (num < 0f)
		{
			return -1;
		}
		if (num > 0f)
		{
			return 1;
		}
		float a2 = (a.x - center.x) * (a.x - center.x) + (a.y - center.y) * (a.y - center.y);
		float b2 = (b.x - center.x) * (b.x - center.x) + (b.y - center.y) * (b.y - center.y);
		return MathUtil.Compare(a2, b2);
	}
}
