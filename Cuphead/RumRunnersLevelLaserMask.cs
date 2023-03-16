using UnityEngine;

public class RumRunnersLevelLaserMask : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer[] maskRenderers;

	[SerializeField]
	private SpriteRenderer[] clearRenderers;

	public void Setup(int layerID, int lowestLayerOrder)
	{
		SpriteRenderer[] array = maskRenderers;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.sortingLayerID = layerID;
			spriteRenderer.sortingOrder = lowestLayerOrder - 1;
		}
		SpriteRenderer[] array2 = clearRenderers;
		foreach (SpriteRenderer spriteRenderer2 in array2)
		{
			spriteRenderer2.sortingLayerID = layerID;
			spriteRenderer2.sortingOrder = lowestLayerOrder + 4;
		}
	}
}
