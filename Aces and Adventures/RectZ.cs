using UnityEngine;

public struct RectZ
{
	public Rect rect;

	public float z;

	public RectZ(Rect rect, float z)
	{
		this.rect = rect;
		this.z = z;
	}

	public PoolStructListHandle<Vector3> Corners()
	{
		PoolStructListHandle<Vector3> poolStructListHandle = Pools.UseStructList<Vector3>();
		poolStructListHandle.Add(new Vector3(rect.xMin, rect.yMin, z));
		poolStructListHandle.Add(new Vector3(rect.xMin, rect.yMax, z));
		poolStructListHandle.Add(new Vector3(rect.xMax, rect.yMax, z));
		poolStructListHandle.Add(new Vector3(rect.xMax, rect.yMin, z));
		return poolStructListHandle;
	}

	public Rect3D ToRect3D()
	{
		return new Rect3D(new Vector3(rect.xMin, rect.yMin, z), new Vector3(rect.xMin, rect.yMax, z), new Vector3(rect.xMax, rect.yMax, z));
	}

	public static implicit operator Rect3D(RectZ rectZ)
	{
		return rectZ.ToRect3D();
	}
}
