using UnityEngine;

public class SimpleTimedDestruction : MonoBehaviour
{
	public float TTL = 1f;

	private void Update()
	{
		TTL -= Time.deltaTime;
		if (TTL < 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
