using UnityEngine;

public class PulseScale : MonoBehaviour
{
	public float minScale = 1f;

	public float maxScale = 3f;

	public float speed;

	private Vector2 initialScale;

	private void Awake()
	{
		initialScale = base.transform.localScale;
	}

	private void Update()
	{
		float t = (1f + Mathf.Sin(Time.time * speed)) / 2f;
		base.transform.localScale = Vector2.Lerp(initialScale * minScale, initialScale * maxScale, t);
	}
}
