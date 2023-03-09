using System.Collections;
using UnityEngine;

public class OldManLevelGnomeClimber : AbstractProjectile
{
	private const float START_Y = -165f;

	private const float CLIMB_X_OFFSET = 120f;

	private const float TOP_Y_OFFSET = 60f;

	private const float SMASH_Y_OFFSET = 100f;

	[SerializeField]
	private Effect deathPuff;

	[SerializeField]
	private Effect[] deathParts;

	[SerializeField]
	private SpriteDeathPartsDLC hat;

	[SerializeField]
	private Effect smashEffect;

	[SerializeField]
	private Transform smashRoot;

	private LevelProperties.OldMan.ClimberGnomes properties;

	private Transform smashPos;

	[SerializeField]
	private DamageReceiver damageReceiver;

	[SerializeField]
	private new Rigidbody2D rigidbody;

	private bool smashFXA;

	private float hp;

	public virtual OldManLevelGnomeClimber Init(float startXPosition, float facing, Transform smashPos, LevelProperties.OldMan.ClimberGnomes properties)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = new Vector3(startXPosition, -165f);
		this.smashPos = smashPos;
		this.properties = properties;
		base.transform.SetScale(facing);
		smashFXA = Rand.Bool();
		if (!properties.canDestroy)
		{
			rigidbody.simulated = false;
		}
		hp = properties.health;
		damageReceiver.OnDamageTaken += OnDamageTaken;
		StartMoving();
		return this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		WORKAROUND_NullifyFields();
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		hp -= info.damage;
		if (hp <= 0f)
		{
			Level.Current.RegisterMinionKilled();
			Die();
		}
	}

	protected override void Die()
	{
		deathPuff.Create(base.transform.position);
		deathParts[0].Create(base.transform.position);
		deathParts[1].Create(base.transform.position);
		SpriteDeathParts spriteDeathParts = hat.CreatePart(base.transform.position);
		spriteDeathParts.animator.Play("_Teal");
		AudioManager.Play("sfx_dlc_omm_gnome_death");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_death");
		this.Recycle();
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
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

	private void StartMoving()
	{
		StartCoroutine(move_up_cr());
	}

	private IEnumerator move_up_cr()
	{
		base.animator.SetBool("DualSmash", properties.dualSmash);
		YieldInstruction wait = new WaitForFixedUpdate();
		float speed = properties.climbSpeed;
		yield return base.animator.WaitForAnimationToEnd(this, "Appear");
		while (smashPos != null && base.transform.position.y < smashPos.position.y + 60f)
		{
			base.transform.AddPosition(0f, speed * CupheadTime.FixedDelta);
			yield return wait;
		}
		base.transform.parent = smashPos;
		base.animator.SetTrigger("ReachedTop");
		yield return base.animator.WaitForAnimationToEnd(this, "ReachedTop");
		if (smashPos != null)
		{
			base.transform.SetPosition(smashPos.position.x, smashPos.position.y + 100f);
		}
		yield return CupheadTime.WaitForSeconds(this, properties.preAttackDelay);
		base.animator.Play("Anticipation");
		yield return base.animator.WaitForAnimationToEnd(this, "Anticipation");
		yield return CupheadTime.WaitForSeconds(this, properties.attackDelay);
		base.animator.SetTrigger("Attack");
		yield return AnimatorExtensions.WaitForAnimationToEnd(name: (!properties.dualSmash) ? "Vanish" : "Vanish_Flipped", animator: base.animator, parent: this);
		this.Recycle();
		yield return null;
	}

	private void AniEvent_SpawnEffect(AnimationEvent ev)
	{
		Effect effect = smashEffect.Create(base.transform.position + new Vector3(smashRoot.localPosition.x * base.transform.localScale.x * ev.floatParameter, smashRoot.localPosition.y));
		effect.transform.SetScale(base.transform.localScale.x * ev.floatParameter);
		effect.GetComponent<Animator>().Play((!smashFXA) ? "B" : "A");
		smashFXA = !smashFXA;
	}

	private void AnimationEvent_SFX_OMM_Gnome_ClimberHammer()
	{
		AudioManager.Play("sfx_dlc_omm_gnome_climber_attack");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_climber_attack");
	}

	private void AnimationEvent_SFX_OMM_Gnome_ClimberHammerVocal()
	{
		AudioManager.Play("sfx_dlc_omm_gnome_climber_attackvocal");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_climber_attackvocal");
	}

	private void WORKAROUND_NullifyFields()
	{
		deathPuff = null;
		deathParts = null;
		hat = null;
		smashEffect = null;
		smashRoot = null;
		smashPos = null;
		rigidbody = null;
	}
}
