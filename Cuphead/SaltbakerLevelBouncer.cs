using System.Collections;
using UnityEngine;

public class SaltbakerLevelBouncer : LevelProperties.Saltbaker.Entity
{
	private const float IDLE_ANIM_LENGTH = 2f / 3f;

	private const float ANIM_TIME_PRE_LAND = 0.1f;

	private const float NORMALIZED_ANIM_TIME_TO_RELAUNCH = 0.55f;

	private const float TARGET_TIME_LAND_A = 27f / 32f;

	private const float TARGET_TIME_LAND_B = 13f / 32f;

	private const float GROUND_TRIGGER_OFFSET = 75f;

	private const float GROUND_POS_OFFSET = 13f;

	[SerializeField]
	private SaltbakerLevelBGSaltHands saltHands;

	[SerializeField]
	private SpriteRenderer shadow;

	[SerializeField]
	private SpriteRenderer pauseShadow;

	[SerializeField]
	private Sprite[] shadowSprites;

	[SerializeField]
	private CollisionChild[] collisionKids;

	[SerializeField]
	private Animator landFXAnimator;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private Vector3 bouncerStartPos;

	private float onGroundY;

	[SerializeField]
	private Collider2D[] colliders;

	private bool isDead;

	private int shadowSprite;

	private int idleHash;

	private float minShadowHeight;

	private float maxShadowHeight;

	private void Start()
	{
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		idleHash = Animator.StringToHash(base.animator.GetLayerName(0) + ".Idle");
		CollisionChild[] array = collisionKids;
		foreach (CollisionChild collisionChild in array)
		{
			collisionChild.OnPlayerCollision += OnCollisionPlayer;
			collisionChild.OnPlayerProjectileCollision += OnCollisionPlayerProjectile;
		}
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public void StartBouncer(Vector3 startPos)
	{
		bouncerStartPos = startPos;
		base.transform.position = startPos;
		saltHands.gameObject.SetActive(value: true);
		StartCoroutine(jump_cr());
	}

	private float TimeToGround(float curYVel, float groundY, float gravity)
	{
		float num = base.transform.position.y - groundY;
		return (curYVel + Mathf.Sqrt(curYVel * curYVel + 2f * gravity * num)) / gravity;
	}

	private IEnumerator jump_cr()
	{
		LevelProperties.Saltbaker.Bouncer p = base.properties.CurrentState.bouncer;
		AnimationHelper animHelper = GetComponent<AnimationHelper>();
		while (!isDead)
		{
			yield return base.animator.WaitForAnimationToEnd(this, "Explode", waitForEndOfFrame: false, waitForStart: false);
			base.transform.position = bouncerStartPos;
			base.animator.Play("Idle");
			saltHands.Play();
			Collider2D[] array = colliders;
			foreach (Collider2D collider2D in array)
			{
				collider2D.enabled = false;
			}
			yield return CupheadTime.WaitForSeconds(this, 3.5f);
			SFX_SALTB_Bouncer_Twirl();
			YieldInstruction wait = new WaitForFixedUpdate();
			Collider2D[] array2 = colliders;
			foreach (Collider2D collider2D2 in array2)
			{
				collider2D2.enabled = true;
			}
			bool goingRight = Rand.Bool();
			float velocityY = 0f;
			float velocityX = 0f;
			float gravity = p.initDropYGravity;
			onGroundY = (float)Level.Current.Ground + GetComponent<Collider2D>().bounds.size.y / 2f + 13f;
			float maxX = (float)Level.Current.Right - GetComponent<Collider2D>().bounds.size.x / 2f;
			minShadowHeight = onGroundY + 75f - p.jumpGravity * (1f / 36f) + p.jumpYSpeed * (1f / 6f);
			maxShadowHeight = onGroundY + p.jumpYSpeed * p.jumpYSpeed / (p.jumpGravity * 2f);
			TransformExtensions.SetPosition(x: PlayerManager.GetNext().transform.position.x, transform: base.transform);
			float timeToGround = TimeToGround(velocityY, onGroundY + 75f, gravity);
			float animTimeOnLand = -1f;
			bool useLandB = false;
			for (int i = 0; i < p.numBounces + 1; i++)
			{
				while (base.transform.position.y > onGroundY + 75f)
				{
					timeToGround -= CupheadTime.FixedDelta;
					if (animTimeOnLand < 0f && base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == idleHash)
					{
						float num = (timeToGround - 0.1f) / (2f / 3f);
						animTimeOnLand = (num + base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime) % 1f;
						float num2 = Mathf.Min(Mathf.Abs(animTimeOnLand - 27f / 32f), Mathf.Abs(27f / 32f - animTimeOnLand));
						float num3 = Mathf.Min(Mathf.Abs(animTimeOnLand - 13f / 32f), Mathf.Abs(13f / 32f - animTimeOnLand));
						useLandB = num2 > num3 && i < p.numBounces;
						float num4 = animTimeOnLand - ((!useLandB) ? (27f / 32f) : (13f / 32f));
						float num5 = num - num4;
						animHelper.Speed = num5 / num;
					}
					if (timeToGround < 0.1f)
					{
						animHelper.Speed = 1f;
						if (i == p.numBounces || isDead)
						{
							base.animator.Play("Explode");
						}
						else
						{
							base.animator.Play((!useLandB) ? "Land_A" : "Land_B");
						}
					}
					velocityY -= gravity * CupheadTime.FixedDelta;
					if (i > 0)
					{
						velocityX = ((!goingRight) ? (0f - p.jumpXSpeed) : p.jumpXSpeed);
					}
					base.transform.AddPosition(velocityX * CupheadTime.FixedDelta, velocityY * CupheadTime.FixedDelta);
					if ((!goingRight && base.transform.position.x < 0f - maxX) || (goingRight && base.transform.position.x > maxX))
					{
						base.transform.SetPosition((!goingRight) ? (0f - maxX) : maxX);
						goingRight = !goingRight;
						if (velocityY < 0f)
						{
							velocityX = 0f;
						}
					}
					yield return wait;
				}
				CupheadLevelCamera.Current.Shake(30f, 0.7f);
				base.transform.SetPosition(base.transform.position.x + velocityX / Mathf.Abs(velocityY) * 75f, onGroundY);
				landFXAnimator.transform.position = base.transform.position;
				landFXAnimator.Play("LandFX");
				while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.55f && !isDead)
				{
					yield return null;
				}
				if (i < p.numBounces && !isDead)
				{
					velocityY = p.jumpYSpeed;
					velocityX = ((!goingRight) ? (0f - p.jumpXSpeed) : p.jumpXSpeed);
					gravity = p.jumpGravity;
					base.transform.position += Vector3.up * 76f;
					base.transform.position += Vector3.right * (velocityX / Mathf.Abs(velocityY) * 75f);
					timeToGround = TimeToGround(velocityY, onGroundY + 75f, gravity);
					animTimeOnLand = -1f;
				}
				if (isDead)
				{
					break;
				}
				yield return wait;
			}
			Collider2D[] array3 = colliders;
			foreach (Collider2D collider2D3 in array3)
			{
				collider2D3.enabled = false;
			}
			if (isDead && !base.animator.GetCurrentAnimatorStateInfo(0).IsName("Explode"))
			{
				base.animator.Play("Explode", 0, 0f);
				base.animator.Update(0f);
			}
		}
	}

	public void EndBouncer()
	{
		StartCoroutine(end_bouncer_cr());
	}

	private IEnumerator end_bouncer_cr()
	{
		isDead = true;
		yield return base.animator.WaitForAnimationToStart(this, "Off");
		Collider2D[] array = colliders;
		foreach (Collider2D collider2D in array)
		{
			collider2D.enabled = false;
		}
		yield return CupheadTime.WaitForSeconds(this, 1f);
		CollisionChild[] array2 = collisionKids;
		foreach (CollisionChild collisionChild in array2)
		{
			collisionChild.OnPlayerCollision -= OnCollisionPlayer;
			collisionChild.OnPlayerProjectileCollision -= OnCollisionPlayerProjectile;
		}
		damageReceiver.OnDamageTaken -= OnDamageTaken;
		Object.Destroy(base.gameObject);
	}

	private void LateUpdate()
	{
		if (base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == idleHash)
		{
			shadow.sprite = shadowSprites[(int)(Mathf.InverseLerp(maxShadowHeight, minShadowHeight, base.transform.position.y) * (float)(shadowSprites.Length - 1))];
		}
		shadow.transform.position = new Vector3(base.transform.position.x, onGroundY);
	}

	public override void OnPause()
	{
		base.OnPause();
		pauseShadow.sprite = shadow.sprite;
		pauseShadow.transform.position = new Vector3(shadow.transform.position.x, onGroundY);
		shadow.enabled = false;
	}

	public override void OnUnpause()
	{
		base.OnUnpause();
		pauseShadow.sprite = null;
		shadow.enabled = true;
	}

	private void AnimationEvent_SFX_SALTB_Bouncer_Bounce()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p3_bouncer_bounce");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p3_bouncer_bounce");
	}

	private void AnimationEvent_SFX_SALTB_Bouncer_Death()
	{
		AudioManager.Stop("sfx_dlc_saltbaker_p3_bouncer_twirl");
		AudioManager.Play("sfx_dlc_saltbaker_p3_bouncer_death");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p3_bouncer_death");
	}

	private void SFX_SALTB_Bouncer_Twirl()
	{
		AudioManager.PlayLoop("sfx_dlc_saltbaker_p3_bouncer_twirl");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p3_bouncer_twirl");
	}
}
