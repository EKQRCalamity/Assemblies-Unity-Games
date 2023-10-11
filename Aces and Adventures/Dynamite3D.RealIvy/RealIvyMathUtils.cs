using UnityEngine;

namespace Dynamite3D.RealIvy;

public static class RealIvyMathUtils
{
	public struct Segment
	{
		public Vector2 a;

		public Vector2 b;
	}

	public static float DistanceBetweenPointAndSegmentSS(Vector2 point, Vector2 a, Vector2 b)
	{
		float num = 0f;
		float num2 = (point.x - a.x) * (b.x - a.x) + (point.y - a.y) * (b.y - a.y);
		num2 /= (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y);
		if (num2 < 0f)
		{
			return (point - a).sqrMagnitude;
		}
		if (num2 >= 0f && num2 <= 1f)
		{
			Vector2 vector = new Vector2(a.x + num2 * (b.x - a.x), a.y + num2 * (b.y - a.y));
			return (point - vector).sqrMagnitude;
		}
		return (point - b).sqrMagnitude;
	}
}
