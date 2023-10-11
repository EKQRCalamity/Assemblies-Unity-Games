using UnityEngine;

public class LakePolygonCarveData
{
	public float distSmooth;

	public float minX = float.MaxValue;

	public float maxX = float.MinValue;

	public float minZ = float.MaxValue;

	public float maxZ = float.MinValue;

	public Vector4[,] distances;
}
