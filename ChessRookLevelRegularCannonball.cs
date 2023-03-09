using System.Collections;
using UnityEngine;

public class ChessRookLevelRegularCannonball : AbstractProjectile
{
	private float apexTime;

	private float apexHeight;

	private float targetDistance;

	private float gravity;

	[SerializeField]
	private Collider2D coll;

	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private SpriteRenderer shadow;

	[SerializeField]
	private Sprite[] shadowSprites;

	[SerializeField]
	private float maxShadowDistance = 750f;

	protected override void Start()
	{
		base.Start();
	}

	public ChessRookLevelRegularCannonball Create(Vector3 position, float apexHeight, float targetDistance, LevelProperties.ChessRook.RegularCannonBall properties)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = position;
		this.apexHeight = apexHeight;
		this.targetDistance = targetDistance;
		gravity = properties.cannonGravity;
		Move();
		return this;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void Move()
	{
		StartCoroutine(move_cr());
	}

	protected override void Die()
	{
		this.Recycle();
	}

	private IEnumerator move_cr()
	{
		AudioManager.Play("sfx_dlc_kog_rook_ghosthead_launch");
		emitAudioFromObject.Add("sfx_dlc_kog_rook_ghosthead_launch");
		float x = new Vector3(y: Level.Current.Ground, x: base.transform.position.x - targetDistance).x - base.transform.position.x;
		float apexDist = apexHeight;
		float toSqrRootForviY = Mathf.Abs(2f * gravity * apexDist);
		float viY = Mathf.Sqrt(toSqrRootForviY);
		float timeToApex = Mathf.Abs(viY / gravity);
		float toSqrtForTimeToG = Mathf.Abs(2f * (base.transform.position.y + apexDist) / gravity);
		float timeToGround = Mathf.Sqrt(toSqrtForTimeToG);
		float viX = x / (timeToApex + timeToGround);
		Vector3 speed = new Vector3(viX, viY);
		bool stillMoving2 = true;
		while (stillMoving2)
		{
			speed += new Vector3(0f, gravity * CupheadTime.FixedDelta);
			base.transform.Translate(speed * CupheadTime.FixedDelta);
			yield return new WaitForFixedUpdate();
			if (base.transform.position.y < (float)(Level.Current.Ground + 40))
			{
				stillMoving2 = false;
				break;
			}
		}
		base.animator.Play("Explode", 1, 0f);
		AudioManager.Play("sfx_dlc_kog_rook_ghosthead_hitground_explode");
		emitAudioFromObject.Add("sfx_dlc_kog_rook_ghosthead_hitground_explode");
		rend.flipX = Rand.Bool();
		coll.enabled = false;
	}

	private void LateUpdate()
	{
		shadow.transform.position = new Vector3(base.transform.position.x, Level.Current.Ground);
		int num = (int)(Mathf.Abs(base.transform.position.y - (float)Level.Current.Ground) / maxShadowDistance * (float)shadowSprites.Length);
		shadow.enabled = coll.enabled && num >= 0 && num < shadowSprites.Length;
		if (shadow.enabled)
		{
			shadow.sprite = shadowSprites[num];
		}
	}
}
