using UnityEngine;

public static class BoundsUtilities
{
	public static Bounds CalculateBounds(Vector2 size, Vector2 offset, Transform transform)
	{
		Vector2 vector = size * 0.5f;
		Vector3 position = new Vector2(0f - vector.x, vector.y) + offset;
		Vector3 position2 = new Vector2(vector.x, vector.y) + offset;
		Vector3 position3 = new Vector2(0f - vector.x, 0f - vector.y) + offset;
		Vector3 position4 = new Vector2(vector.x, 0f - vector.y) + offset;
		position = transform.TransformPoint(position);
		position2 = transform.TransformPoint(position2);
		position3 = transform.TransformPoint(position3);
		position4 = transform.TransformPoint(position4);
		float x = Mathf.Min(Mathf.Min(Mathf.Min(position.x, position2.x), position3.x), position4.x);
		float y = Mathf.Min(Mathf.Min(Mathf.Min(position.y, position2.y), position3.y), position4.y);
		float x2 = Mathf.Max(Mathf.Max(Mathf.Max(position.x, position2.x), position3.x), position4.x);
		float y2 = Mathf.Max(Mathf.Max(Mathf.Max(position.y, position2.y), position3.y), position4.y);
		float num = Mathf.Min(Mathf.Min(Mathf.Min(position.z, position2.z), position3.z), position4.z);
		float num2 = Mathf.Max(Mathf.Max(Mathf.Max(position.z, position2.z), position3.z), position4.z);
		Bounds result = default(Bounds);
		result.SetMinMax(new Vector3(x, y), new Vector3(x2, y2));
		return result;
	}
}
