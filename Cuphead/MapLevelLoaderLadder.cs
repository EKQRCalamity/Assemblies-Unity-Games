using UnityEngine;

public class MapLevelLoaderLadder : MapLevelLoader
{
	[SerializeField]
	private SpriteRenderer shadowRenderer;

	[SerializeField]
	private SpriteRenderer[] smokeRenderers;

	public void EnableShadow(bool enabled)
	{
		shadowRenderer.enabled = enabled;
	}

	private void animationEvent_DownStarted()
	{
		int num = Random.Range(0, smokeRenderers.Length);
		for (int i = 0; i < smokeRenderers.Length; i++)
		{
			smokeRenderers[i].enabled = i == num;
		}
	}
}
