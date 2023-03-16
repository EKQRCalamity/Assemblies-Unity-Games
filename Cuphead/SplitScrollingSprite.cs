using UnityEngine;

public class SplitScrollingSprite : ScrollingSprite
{
	[SerializeField]
	private bool ignoreSelfWhenHandlingSplitSprites;

	[SerializeField]
	private Vector2 splitOffset;

	[SerializeField]
	private Sprite[] splitSprites;

	protected override void Start()
	{
		base.Start();
		foreach (SpriteRenderer copyRenderer in base.copyRenderers)
		{
			if (!ignoreSelfWhenHandlingSplitSprites || !(copyRenderer.gameObject == base.gameObject))
			{
				Sprite[] array = splitSprites;
				foreach (Sprite sprite in array)
				{
					GameObject gameObject = new GameObject(sprite.name);
					SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
					spriteRenderer.sprite = sprite;
					spriteRenderer.sortingLayerID = copyRenderer.sortingLayerID;
					spriteRenderer.sortingOrder = copyRenderer.sortingOrder;
					gameObject.transform.SetParent(copyRenderer.transform, worldPositionStays: false);
					gameObject.transform.localPosition = splitOffset;
				}
			}
		}
	}
}
