using UnityEngine;

namespace AmplifyImpostors;

public static class BoundsEx
{
	public static Bounds Transform(this Bounds bounds, Matrix4x4 matrix)
	{
		Vector3 center = matrix.MultiplyPoint3x4(bounds.center);
		Vector3 extents = bounds.extents;
		Vector3 vector = matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f));
		Vector3 vector2 = matrix.MultiplyVector(new Vector3(0f, extents.y, 0f));
		Vector3 vector3 = matrix.MultiplyVector(new Vector3(0f, 0f, extents.z));
		extents.x = Mathf.Abs(vector.x) + Mathf.Abs(vector2.x) + Mathf.Abs(vector3.x);
		extents.y = Mathf.Abs(vector.y) + Mathf.Abs(vector2.y) + Mathf.Abs(vector3.y);
		extents.z = Mathf.Abs(vector.z) + Mathf.Abs(vector2.z) + Mathf.Abs(vector3.z);
		Bounds result = default(Bounds);
		result.center = center;
		result.extents = extents;
		return result;
	}
}
