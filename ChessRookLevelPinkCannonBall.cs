using System.Collections;
using UnityEngine;

public class ChessRookLevelPinkCannonBall : AbstractProjectile
{
	private LevelProperties.ChessRook.PinkCannonBall properties;

	private Collider2D coll;

	private Vector3 newRoot;

	private float apexHeight;

	private float targetDistance;

	private float gravity;

	private float distAddition;

	private float heightAddition;

	private bool gotParried;

	private bool playerOnLeft;

	private bool playerOnBottom;

	[SerializeField]
	private Collider2D parryColl;

	[SerializeField]
	private SpriteRenderer shadow;

	[SerializeField]
	private Sprite[] shadowSprites;

	[SerializeField]
	private SpriteRenderer topExplosion;

	[SerializeField]
	private SpriteRenderer rotatingExplosion;

	[SerializeField]
	private SpriteRenderer bigExplosion;

	[SerializeField]
	private Effect sinkFX;

	private bool sinking;

	[SerializeField]
	private float maxShadowDistance = 750f;

	public override float ParryMeterMultiplier => 0f;

	public bool finishedOriginalArc { get; private set; }

	public override void OnLevelEnd()
	{
	}

	protected override void Start()
	{
		base.Start();
		coll = GetComponent<Collider2D>();
	}

	public ChessRookLevelPinkCannonBall Create(Vector3 position, float apexHeight, float targetDistance, LevelProperties.ChessRook.PinkCannonBall properties)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = position;
		this.properties = properties;
		this.apexHeight = apexHeight;
		this.targetDistance = targetDistance;
		gravity = properties.pinkGravity;
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

	private IEnumerator move_cr()
	{
		AudioManager.Play("sfx_dlc_kog_rook_ghosthead_launch");
		emitAudioFromObject.Add("sfx_dlc_kog_rook_ghosthead_launch");
		newRoot = new Vector3(y: Level.Current.Ground, x: base.transform.position.x - targetDistance);
		float x = newRoot.x - base.transform.position.x;
		float apexDist = apexHeight;
		float toSqrRootForviY = Mathf.Abs(2f * gravity * apexDist);
		float viY = Mathf.Sqrt(toSqrRootForviY);
		float timeToApex = Mathf.Abs(viY / gravity);
		float toSqrtForTimeToG = Mathf.Abs(2f * (base.transform.position.y + apexDist) / gravity);
		float timeToGround = Mathf.Sqrt(toSqrtForTimeToG);
		float viX = x / (timeToApex + timeToGround);
		bool stillMoving2 = true;
		if (finishedOriginalArc && !playerOnBottom)
		{
			viY = properties.velocityAfterSlam;
			gravity = properties.gravityAfterSlam;
			yield return null;
		}
		Vector3 speed = new Vector3(viX, viY);
		while (stillMoving2)
		{
			speed += new Vector3(0f, gravity * CupheadTime.FixedDelta);
			base.transform.Translate(speed * CupheadTime.FixedDelta);
			yield return new WaitForFixedUpdate();
			if (gotParried)
			{
				stillMoving2 = false;
				break;
			}
			if (base.transform.position.y < (float)(Level.Current.Ground + 120))
			{
				Sink(speed.x);
			}
			if (base.transform.position.y < (float)(Level.Current.Ground + 40))
			{
				coll.enabled = false;
			}
			if (base.transform.position.y < (float)(Level.Current.Ground - 40))
			{
				Die();
			}
		}
		if (gotParried)
		{
			Bounce();
		}
		yield return null;
	}

	private void Sink(float speedX)
	{
		if (!sinking)
		{
			sinking = true;
			parryColl.enabled = false;
			sinkFX.Create(new Vector3(base.transform.position.x + speedX / 9f, Level.Current.Ground - 40));
			AudioManager.Play("sfx_dlc_kog_rook_ghosthead_hitground_explode");
			emitAudioFromObject.Add("sfx_dlc_kog_rook_ghosthead_hitground_explode");
		}
	}

	public void Explosion()
	{
		StopAllCoroutines();
		parryColl.enabled = false;
		coll.enabled = false;
		rotatingExplosion.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
		topExplosion.flipX = false;
		base.animator.Play("Explode");
		AudioManager.Play("sfx_dlc_kog_rook_ghosthead_hitsrook");
		emitAudioFromObject.Add("sfx_dlc_kog_rook_ghosthead_hitsrook");
	}

	protected override void Die()
	{
		this.Recycle();
	}

	private void Bounce()
	{
		gravity = properties.pinkReactionGravity;
		apexHeight = properties.bounceUpApexHeight + heightAddition;
		targetDistance = ((!playerOnLeft) ? (properties.bounceUpTargetDist + distAddition) : (0f - properties.bounceUpTargetDist - distAddition));
		if (!finishedOriginalArc)
		{
			finishedOriginalArc = true;
		}
		gotParried = false;
		Move();
	}

	public void GotParried(AbstractPlayerController player)
	{
		playerOnLeft = player.transform.position.x < base.transform.position.x;
		playerOnBottom = true;
		Vector3 vector = player.center - base.transform.position;
		float num = MathUtils.DirectionToAngle(vector);
		if (num < 0f)
		{
			num = 360f + num;
		}
		if (num >= 180f - properties.goodQuadrantClemencyLeft && num <= 270f + properties.goodQuadrantClemencyBottom)
		{
			base.animator.SetTrigger("Parried");
			GetComponent<SpriteRenderer>().sortingOrder = 1;
			float num2 = Mathf.InverseLerp(270f + properties.goodQuadrantClemencyBottom, 180f, num);
			distAddition = properties.distanceAddition.GetFloatAt(num2);
			heightAddition = properties.heightAddition.GetFloatAt(1f - num2);
		}
		else if (num > 270f + properties.goodQuadrantClemencyBottom)
		{
			float num3 = Mathf.InverseLerp(270f + properties.goodQuadrantClemencyBottom, 360f, num);
			distAddition = properties.distanceAdditionBack.GetFloatAt(num3);
			heightAddition = properties.heightAdditionBack.GetFloatAt(1f - num3);
		}
		else
		{
			if (playerOnLeft)
			{
				base.animator.SetTrigger("Parried");
			}
			float i = Mathf.InverseLerp(180f, 0f, num);
			distAddition = ((!playerOnLeft) ? properties.distanceAdditionBack.GetFloatAt(i) : properties.distanceAddition.GetFloatAt(i));
			heightAddition = 0f;
			GetComponent<SpriteRenderer>().sortingOrder = 1;
			playerOnBottom = false;
		}
		gotParried = true;
		StartCoroutine(collider_off_cr());
	}

	private IEnumerator collider_off_cr()
	{
		parryColl.enabled = false;
		coll.enabled = false;
		yield return CupheadTime.WaitForSeconds(this, properties.bounceCollisionOffTimer);
		parryColl.enabled = true;
		coll.enabled = true;
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
		if (Level.Current.Ending)
		{
			coll.enabled = false;
			parryColl.enabled = false;
		}
	}
}
