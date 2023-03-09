using UnityEngine;

public class PlatformingLevelEnemyShadow : AbstractCollidableObject
{
	[Range(1f, 1000f)]
	[SerializeField]
	private int maxDistance = 250;

	[SerializeField]
	private Sprite[] shadowSprites;

	private Transform shadow;

	private SpriteRenderer spriteRenderer;

	private PlatformingLevelGroundMovementEnemy enemy;

	private BoxCollider2D boxCollider;

	private readonly int groundMask = 1048576;

	private void Start()
	{
		shadow = new GameObject(base.gameObject.name + "_Shadow").transform;
		spriteRenderer = shadow.gameObject.AddComponent<SpriteRenderer>();
		shadow.position = new Vector3(base.transform.position.x, Level.Current.Ground, 0f);
		spriteRenderer.sprite = shadowSprites[0];
		enemy = GetComponent<PlatformingLevelGroundMovementEnemy>();
		boxCollider = enemy.GetComponent<BoxCollider2D>();
	}

	private void Update()
	{
		if (enemy.Grounded || enemy.Dead)
		{
			spriteRenderer.enabled = false;
			return;
		}
		spriteRenderer.enabled = true;
		Vector3 position = shadow.position;
		position.x = base.transform.position.x;
		RaycastHit2D raycastHit2D = Physics2D.BoxCast(base.transform.position, new Vector2(boxCollider.size.x, 1f), 0f, Vector2.down, maxDistance, groundMask);
		if (raycastHit2D.collider == null)
		{
			spriteRenderer.enabled = false;
			return;
		}
		LevelPlatform component = raycastHit2D.collider.gameObject.GetComponent<LevelPlatform>();
		if (component != null && !component.AllowShadows)
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

	public Vector3 ShadowPosition()
	{
		return shadow.position;
	}
}
