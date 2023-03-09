using UnityEngine;

public class FlyingCowboyLevelOverlayToggle : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 1f)]
	private float overlayProbability;

	[SerializeField]
	private SpriteRenderer[] overlayRenderers;

	private void Start()
	{
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		SpriteRenderer[] array = overlayRenderers;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			if (Random.value < overlayProbability)
			{
				spriteRenderer.enabled = true;
				spriteRenderer.sortingOrder = component.sortingOrder + 1;
			}
			else
			{
				spriteRenderer.enabled = false;
			}
		}
	}
}
