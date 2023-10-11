using UnityEngine;

public struct RaySegment
{
	public Vector3 origin;

	public Vector3 direction;

	public float length;

	public RaySegment(Vector3 start, Vector3 end)
	{
		origin = start;
		Vector3 vector = start - end;
		length = vector.magnitude;
		direction = vector / length;
	}
}
