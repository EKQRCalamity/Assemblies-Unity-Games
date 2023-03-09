using UnityEngine;

public class FlyingCowboyLevelOverlayScrollingSprite : ScrollingSprite
{
	[SerializeField]
	[Range(0f, 1f)]
	private float overlayProbability;

	[SerializeField]
	private SpriteRenderer[] overlayRenderers;

	private SpriteRenderer leftRenderer;

	private SpriteRenderer rightRenderer;

	private SpriteRenderer[] rightOverlayRenderers;

	private bool[] leftOverlaysEnabled;

	private bool[] rightOverlaysEnabled;

	protected override void Start()
	{
		base.Start();
		leftRenderer = GetComponent<SpriteRenderer>();
		rightRenderer = base.copyRenderers.Find((SpriteRenderer renderer) => renderer.transform.position.x > leftRenderer.transform.position.x);
		rightOverlayRenderers = new SpriteRenderer[overlayRenderers.Length];
		for (int i = 0; i < overlayRenderers.Length; i++)
		{
			SpriteRenderer spriteRenderer = overlayRenderers[i];
			GameObject gameObject = new GameObject(spriteRenderer.gameObject.name);
			SpriteRenderer spriteRenderer2 = gameObject.AddComponent<SpriteRenderer>();
			spriteRenderer2.sprite = spriteRenderer.sprite;
			spriteRenderer2.sortingLayerID = spriteRenderer.sortingLayerID;
			spriteRenderer2.sortingOrder = spriteRenderer.sortingOrder;
			spriteRenderer2.enabled = false;
			gameObject.transform.SetParent(rightRenderer.transform, worldPositionStays: false);
			rightOverlayRenderers[i] = spriteRenderer2;
		}
		leftOverlaysEnabled = new bool[overlayRenderers.Length];
		rightOverlaysEnabled = new bool[overlayRenderers.Length];
	}

	protected override void onLoop()
	{
		base.onLoop();
		bool[] array = leftOverlaysEnabled;
		leftOverlaysEnabled = rightOverlaysEnabled;
		rightOverlaysEnabled = array;
		for (int i = 0; i < rightOverlaysEnabled.Length; i++)
		{
			rightOverlaysEnabled[i] = Random.value < overlayProbability;
		}
		toggleOverlays(overlayRenderers, leftOverlaysEnabled);
		toggleOverlays(rightOverlayRenderers, rightOverlaysEnabled);
	}

	private static void toggleOverlays(SpriteRenderer[] overlayRenderers, bool[] activeStatus)
	{
		for (int i = 0; i < overlayRenderers.Length; i++)
		{
			overlayRenderers[i].enabled = activeStatus[i];
		}
	}
}
