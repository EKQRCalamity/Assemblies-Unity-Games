using UnityEngine;

public struct UIAnimation
{
	public Vector3 position;

	public Vector3 rotation;

	public Vector3 scale;

	public float startTime;

	public float duration;

	public UIAnimation(Vector3 position, Vector3 rotation, Vector3 scale, float startTime, float duration)
	{
		this.position = position;
		this.rotation = rotation;
		this.scale = scale;
		this.startTime = startTime;
		this.duration = duration;
	}
}
