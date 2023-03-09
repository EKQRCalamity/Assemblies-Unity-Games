using UnityEngine;

public class LevelCharacterShadow : AbstractPausableComponent
{
	[Range(1f, 1000f)]
	[SerializeField]
	private int maxDistance = 250;

	[SerializeField]
	private Transform root;

	[SerializeField]
	private Sprite[] shadowSprites;

	[SerializeField]
	private bool isBGLayer;

	private Transform shadow;

	private SpriteRenderer spriteRenderer;

	private readonly int groundMask = 1048576;

	private void Start()
	{
		shadow = new GameObject(base.gameObject.name + "_Shadow").transform;
		spriteRenderer = shadow.gameObject.AddComponent<SpriteRenderer>();
		shadow.position = new Vector3(base.transform.position.x, Level.Current.Ground, 0f);
		spriteRenderer.sprite = shadowSprites[0];
		if (isBGLayer)
		{
			spriteRenderer.sortingLayerName = SpriteLayer.Background.ToString();
			spriteRenderer.sortingOrder = 100;
		}
	}

	private void Update()
	{
		Vector3 position = shadow.position;
		position.x = base.transform.position.x;
		Collider2D component = root.GetComponent<Collider2D>();
		RaycastHit2D raycastHit2D = Physics2D.BoxCast(root.transform.position, new Vector2(component.bounds.size.x, 1f), 0f, Vector2.down, maxDistance, groundMask);
		if (raycastHit2D.collider == null)
		{
			spriteRenderer.enabled = false;
			return;
		}
		LevelPlatform component2 = raycastHit2D.collider.gameObject.GetComponent<LevelPlatform>();
		if (component2 != null && !component2.AllowShadows)
		{
			spriteRenderer.enabled = false;
			return;
		}
		position.y = raycastHit2D.point.y;
		shadow.position = position;
		SetSprite();
	}

	private void SetSprite()
	{
		int num = (int)(Mathf.Abs(base.transform.position.y - shadow.position.y) / (float)maxDistance * (float)shadowSprites.Length);
		if (num < 0 || num >= shadowSprites.Length)
		{
			spriteRenderer.enabled = false;
			return;
		}
		spriteRenderer.enabled = true;
		spriteRenderer.sprite = shadowSprites[num];
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (shadow != null)
		{
			Object.Destroy(shadow.gameObject);
		}
	}
}
