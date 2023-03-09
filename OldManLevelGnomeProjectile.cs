using System.Collections;
using UnityEngine;

public class OldManLevelGnomeProjectile : AbstractProjectile
{
	private const float OFFSET_TO_HIT_BONE = 50f;

	private const float OFFSET_TO_PLAY_ANTICIPATION_SOUND = 200f;

	private const float OFFSET_TO_HIT_LEG = 200f;

	private Vector3 speed;

	private float gravity;

	private bool spawnParryable;

	private OldManLevelStomachPlatform target;

	private bool bouncingOffscreen;

	[SerializeField]
	private float bounceModifier = 0.5f;

	[SerializeField]
	private SpriteRenderer underwaterSprite;

	private bool triedHit;

	private bool splashed;

	private AnimationHelper animHelper;

	private bool playedAnticipationSound;

	public bool IsFlying { get; private set; }

	public virtual OldManLevelGnomeProjectile Init(Vector3 position, Vector3 speed, float gravity, bool spawnParryable, bool parryable, OldManLevelStomachPlatform target)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = position;
		base.transform.localScale = new Vector3((!MathUtils.RandomBool()) ? 1 : (-1), 1f);
		this.speed = speed;
		this.gravity = gravity;
		this.spawnParryable = spawnParryable;
		IsFlying = true;
		SetParryable(parryable);
		this.target = target;
		animHelper = GetComponent<AnimationHelper>();
		animHelper.Speed = 1f;
		base.animator.Play(spawnParryable ? "Bone" : ((!parryable) ? "Chicken" : "ChickenPink"));
		base.animator.Update(0f);
		GetComponent<Collider2D>().enabled = true;
		bouncingOffscreen = false;
		triedHit = false;
		underwaterSprite.color = Color.white;
		playedAnticipationSound = false;
		return this;
	}

	public override void OnParry(AbstractPlayerController player)
	{
		target.CancelAnticipation();
		base.OnParry(player);
	}

	public override void OnLevelEnd()
	{
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		speed += new Vector3(0f, gravity * CupheadTime.FixedDelta);
		if (bouncingOffscreen)
		{
			speed += new Vector3(0f, gravity * CupheadTime.FixedDelta);
		}
		base.transform.Translate(speed * CupheadTime.FixedDelta);
		if (bouncingOffscreen)
		{
			if (!splashed && base.transform.position.y < target.main.splashHandler.transform.position.y)
			{
				target.main.splashHandler.SplashIn(base.transform.position.x);
				speed *= 0.5f;
				gravity *= 0.5f;
				animHelper.Speed = 0.5f;
				splashed = true;
			}
			if (base.transform.position.y < -560f)
			{
				this.Recycle();
			}
			if (splashed)
			{
				underwaterSprite.color = new Color(1f, 1f, 1f, (1f - Mathf.InverseLerp(target.main.splashHandler.transform.position.y, target.main.splashHandler.transform.position.y - 140f, base.transform.position.y)) * 0.5f);
			}
		}
		if (spawnParryable && base.transform.position.y < target.transform.position.y + 200f && !playedAnticipationSound)
		{
			SFX_PreBoneHit();
			playedAnticipationSound = true;
		}
		if (base.transform.position.y < target.transform.position.y + ((!spawnParryable) ? 200f : 50f) && !triedHit)
		{
			HitTarget();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private void HitTarget()
	{
		triedHit = true;
		if (target.isActivated)
		{
			AbstractPlayerController[] componentsInChildren = target.GetComponentsInChildren<AbstractPlayerController>();
			foreach (AbstractPlayerController abstractPlayerController in componentsInChildren)
			{
				if (!(abstractPlayerController == null))
				{
					abstractPlayerController.transform.parent = null;
				}
			}
			target.DeactivatePlatform(spawnParryable);
			IsFlying = false;
			if (spawnParryable)
			{
				bouncingOffscreen = true;
				speed.y = (0f - speed.y) * bounceModifier;
				speed.x *= 2f;
				base.transform.localScale = new Vector3(0f - base.transform.localScale.x, 1f);
				GetComponent<Collider2D>().enabled = false;
			}
			else
			{
				StartCoroutine(wait_for_eat());
			}
		}
		else
		{
			bouncingOffscreen = true;
		}
	}

	private IEnumerator wait_for_eat()
	{
		Animator anim = target.GetComponent<Animator>();
		yield return anim.WaitForAnimationToStart(this, "Eat");
		while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 11f / 58f)
		{
			yield return null;
		}
		GetComponent<Collider2D>().enabled = false;
		this.Recycle();
	}

	private void SFX_PreBoneHit()
	{
		AudioManager.Play("sfx_dlc_omm_p3_dinobells_prebonehit");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_dinobells_prebonehit");
	}
}
