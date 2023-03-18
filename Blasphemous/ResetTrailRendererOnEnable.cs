using UnityEngine;

public class ResetTrailRendererOnEnable : MonoBehaviour
{
	private void OnEnable()
	{
		Clean();
	}

	private void OnDisable()
	{
		Clean();
	}

	public void Clean()
	{
		TrailRenderer component = GetComponent<TrailRenderer>();
		if ((bool)component)
		{
			component.Clear();
		}
	}
}
