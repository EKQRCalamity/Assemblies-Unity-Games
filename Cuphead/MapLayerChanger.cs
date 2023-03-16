using UnityEngine;

public class MapLayerChanger : AbstractMonoBehaviour
{
	[SerializeField]
	private int sortingOrder;

	private void OnTriggerEnter2D(Collider2D collider)
	{
		SpriteRenderer[] componentsInChildren = collider.GetComponentsInChildren<SpriteRenderer>();
		SpriteRenderer[] array = componentsInChildren;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.sortingOrder = sortingOrder;
		}
	}

	private void OnTriggerStay2D(Collider2D collider)
	{
		SpriteRenderer[] componentsInChildren = collider.GetComponentsInChildren<SpriteRenderer>();
		SpriteRenderer[] array = componentsInChildren;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.sortingOrder = sortingOrder;
		}
	}
}
