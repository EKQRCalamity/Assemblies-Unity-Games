using UnityEngine;

public class RumRunnersLevelGrubPath : MonoBehaviour
{
	public Vector2 start;

	public Vector2 controlPoint;

	public float forceFGSet = 2f;

	public Vector2 GetPoint(float t)
	{
		return Vector2.LerpUnclamped(Vector2.LerpUnclamped(start, controlPoint, t), Vector2.LerpUnclamped(controlPoint, base.transform.position, t), t);
	}
}
