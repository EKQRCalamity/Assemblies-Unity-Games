using UnityEngine;

public class ChessQueenLevelEgg : AbstractProjectile
{
	private const float GROUND_OFFSET = 15f;

	private Vector2 velocity;

	private float gravity;

	private bool isDead;

	private float delay;

	[SerializeField]
	private Animator anim;

	[SerializeField]
	private Collider2D coll;

	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private SpriteRenderer explosionRend;

	public override float ParryMeterMultiplier => 0f;

	public ChessQueenLevelEgg Create(Vector3 position, Vector3 velocity, float gravity, float delay)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = position;
		this.velocity = velocity;
		this.gravity = gravity;
		this.delay = delay;
		coll.enabled = false;
		rend.flipX = Rand.Bool();
		anim.Play(Random.Range(0, 12).ToString());
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

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!isDead)
		{
			if (base.lifetime > delay)
			{
				rend.sortingLayerName = "Projectiles";
				rend.sortingOrder = 0;
				rend.color = Color.white;
				coll.enabled = true;
			}
			else
			{
				rend.color = Color.Lerp(new Color(0.7f, 0.7f, 0.7f, 1f), Color.white, base.lifetime / delay);
			}
			base.transform.AddPosition(velocity.x * CupheadTime.FixedDelta, velocity.y * CupheadTime.FixedDelta);
			velocity.y -= gravity * CupheadTime.FixedDelta;
			if (base.transform.position.y < (float)Level.Current.Ground + 15f)
			{
				HitGround();
			}
		}
	}

	protected void HitGround()
	{
		StopAllCoroutines();
		base.transform.position = new Vector3(base.transform.position.x, (float)Level.Current.Ground + 15f);
		isDead = true;
		coll.enabled = false;
		explosionRend.flipX = Rand.Bool();
		anim.Play((!Rand.Bool()) ? "ExplodeB" : "ExplodeA", 1, 0f);
		anim.Update(0f);
	}
}
