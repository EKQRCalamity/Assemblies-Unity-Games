using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelOilBlob : AbstractProjectile
{
	private static readonly float BlobBHeight = 79f;

	private static readonly float BlobCHeight = 293f;

	private static readonly float SnakeSpawnOffsetX = 130f;

	[SerializeField]
	private BasicProjectile snakePrefab;

	[SerializeField]
	private Effect splatEffect;

	private SpriteRenderer spriteRenderer;

	private Sprite previousSprite;

	private float initialYPosition;

	private float finalYPosition;

	private int frameCounter;

	public FlyingCowboyLevelOilBlob Create(Vector3 position, float finalYPosition, float snakeSpawnX, LevelProperties.FlyingCowboy.SnakeAttack properties, bool playSplatSFX)
	{
		FlyingCowboyLevelOilBlob flyingCowboyLevelOilBlob = base.Create(position) as FlyingCowboyLevelOilBlob;
		flyingCowboyLevelOilBlob.initialYPosition = position.y;
		flyingCowboyLevelOilBlob.finalYPosition = finalYPosition;
		float f = flyingCowboyLevelOilBlob.finalYPosition - flyingCowboyLevelOilBlob.initialYPosition;
		float num = Mathf.Abs(f);
		if (num >= BlobCHeight)
		{
			flyingCowboyLevelOilBlob.animator.Play((!Rand.Bool()) ? "F" : "C");
			flyingCowboyLevelOilBlob.finalYPosition -= Mathf.Sign(f) * BlobCHeight;
		}
		else if (num >= BlobBHeight)
		{
			flyingCowboyLevelOilBlob.animator.Play("B");
			flyingCowboyLevelOilBlob.finalYPosition -= Mathf.Sign(f) * BlobBHeight;
		}
		else
		{
			flyingCowboyLevelOilBlob.animator.Play("A");
		}
		if (flyingCowboyLevelOilBlob.finalYPosition < flyingCowboyLevelOilBlob.initialYPosition)
		{
			Vector3 localScale = flyingCowboyLevelOilBlob.transform.localScale;
			localScale.y *= -1f;
			flyingCowboyLevelOilBlob.transform.localScale = localScale;
		}
		flyingCowboyLevelOilBlob.StartCoroutine(flyingCowboyLevelOilBlob.snakeSpawn_cr(snakeSpawnX, finalYPosition, properties, playSplatSFX));
		return flyingCowboyLevelOilBlob;
	}

	protected override void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void LateUpdate()
	{
		if (spriteRenderer.sprite != previousSprite)
		{
			previousSprite = spriteRenderer.sprite;
			frameCounter++;
		}
		Vector3 position = base.transform.position;
		position.y = Mathf.Lerp(initialYPosition, finalYPosition, (float)frameCounter / 28f);
		base.transform.position = position;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase == CollisionPhase.Enter)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private IEnumerator snakeSpawn_cr(float snakeSpawnX, float snakeSpawnY, LevelProperties.FlyingCowboy.SnakeAttack properties, bool playSplatSFX)
	{
		yield return null;
		yield return null;
		yield return base.animator.WaitForNormalizedTime(this, 1f);
		BasicProjectile snake = snakePrefab.Create(new Vector2(snakeSpawnX + SnakeSpawnOffsetX, snakeSpawnY), 0f, 0f - properties.snakeSpeed);
		snake.animator.Play(0, 0, Random.Range(0f, 1f));
		splatEffect.Create(new Vector2(640f, snakeSpawnY));
		if (playSplatSFX)
		{
			AudioManager.Play("sfx_DLC_Cowgirl_P1_LiquidSplat");
		}
		Object.Destroy(base.gameObject);
	}
}
