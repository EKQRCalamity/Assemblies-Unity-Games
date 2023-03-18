using System;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Legionary.Animator;

public class LegionaryAnimator : EnemyAnimatorInyector
{
	protected Legionary Legionary { get; private set; }

	public event Action<LegionaryAnimator> OnHeavyAttackLightningSummon;

	public event Action<LegionaryAnimator, Vector2> OnSpinProjectilePoint;

	protected override void OnStart()
	{
		base.OnStart();
		Legionary = (Legionary)OwnerEntity;
	}

	public void Walk(bool walk = true)
	{
		base.EntityAnimator.SetBool("WALK", walk);
		if (walk)
		{
			base.EntityAnimator.SetBool("RUN", value: false);
		}
	}

	public void Run(bool run = true)
	{
		base.EntityAnimator.SetBool("RUN", run);
		if (run)
		{
			base.EntityAnimator.SetBool("WALK", value: false);
		}
	}

	public void Parry()
	{
		base.EntityAnimator.SetTrigger("PARRY");
	}

	public void SpinAttack()
	{
		base.EntityAnimator.SetTrigger("SPIN_ATTACK");
	}

	public void Hurt()
	{
		if (base.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
		{
			PlayDamageAnim();
		}
		else
		{
			base.EntityAnimator.SetTrigger("HURT");
		}
	}

	public void Death()
	{
		Walk(walk: false);
		Run(run: false);
		base.EntityAnimator.SetTrigger("DEATH");
	}

	private void PlayDamageAnim()
	{
		base.EntityAnimator.Play("Hurt", 0, 0f);
	}

	public void LightningSummon()
	{
		base.EntityAnimator.SetTrigger("SUMMON_ATTACK");
	}

	public void LightAttack()
	{
		base.EntityAnimator.SetTrigger("LIGHT_ATTACK");
	}

	public void AnimationEvent_SpinProjectilePointRight()
	{
		if (this.OnSpinProjectilePoint != null)
		{
			this.OnSpinProjectilePoint(this, Vector2.right);
		}
	}

	public void AnimationEvent_SpinProjectilePointLeft()
	{
		if (this.OnSpinProjectilePoint != null)
		{
			this.OnSpinProjectilePoint(this, Vector2.left);
		}
	}

	public void AnimationEvent_LightAttackStarts()
	{
		Legionary.LightAttack.DealsDamage = true;
	}

	public void AnimationEvent_LightAttackEnds()
	{
		Legionary.LightAttack.DealsDamage = false;
	}

	public void AnimationEvent_HeavyAttack()
	{
		Legionary.SpinAttack.CurrentWeaponAttack();
	}

	public void AnimationEvent_TauntImpact()
	{
		Legionary.Behaviour.LightningSummonAttack();
	}

	public void AnimationEvent_SpinAttackStarts()
	{
		Legionary.SpinAttack.DealsDamage = true;
	}

	public void AnimationEvent_SpinAttackEnds()
	{
		Legionary.SpinAttack.DealsDamage = false;
	}

	public void AnimationEvent_StopLerping()
	{
		Legionary.MotionLerper.StopLerping();
	}

	public void AnimationEvent_OpenToAttacks()
	{
		Legionary.CanTakeDamage = true;
	}

	public void AnimationEvent_CloseAttackWindow()
	{
		Legionary.CanTakeDamage = false;
	}

	public void AnimationEvent_SetShieldedOn()
	{
		Legionary.IsGuarding = true;
	}

	public void AnimationEvent_SetShieldedOff()
	{
		Legionary.IsGuarding = false;
	}

	public void AnimationEvent_DisableEntity()
	{
		Legionary.gameObject.SetActive(value: false);
	}

	public void AnimationEvent_LightScreenShake()
	{
		Vector2 vector = ((OwnerEntity.Status.Orientation != 0) ? Vector2.left : Vector2.right);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.15f, vector * 0.5f, 10, 0.01f, 0f, default(Vector3), 0.01f);
	}

	public void AnimationEvent_HeavySecondScreenShake()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.3f, Vector3.up * 3f, 60, 0.01f, 0f, default(Vector3), 0.01f);
	}

	public void AnimationEvent_HeavyFirstScreenShake()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.7f, Vector3.down * 2f, 30, 0.01f, 0f, default(Vector3), 0.001f);
	}

	public void AnimationEvent_AttackDisplacement()
	{
		Vector3 dir = ((Legionary.Status.Orientation != 0) ? (-base.transform.right) : base.transform.right);
		Legionary.MotionLerper.distanceToMove = 1.5f;
		Legionary.MotionLerper.TimeTakenDuringLerp = 0.2f;
		Legionary.MotionLerper.StartLerping(dir);
	}

	public void AnimationEvent_SpinAttackDisplacement()
	{
		Vector3 dir = ((Legionary.Status.Orientation != 0) ? (-base.transform.right) : base.transform.right);
		Legionary.MotionLerper.distanceToMove = 4.5f;
		Legionary.MotionLerper.TimeTakenDuringLerp = 1.75f;
		Legionary.MotionLerper.StartLerping(dir);
	}
}
