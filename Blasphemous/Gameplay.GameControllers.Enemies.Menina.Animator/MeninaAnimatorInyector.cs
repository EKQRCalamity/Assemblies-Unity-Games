using System;
using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.Menina.Animator;

public class MeninaAnimatorInyector : EnemyAnimatorInyector
{
	public event Action OnStepFinished;

	public void NotifyOnStepFinished()
	{
		if (this.OnStepFinished != null)
		{
			this.OnStepFinished();
		}
	}

	public void Attack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("ATTACK");
		}
	}

	public void ResetAttackTrigger()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.ResetTrigger("ATTACK");
		}
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void Step(bool forward = true)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool((!forward) ? "STEP_BCK" : "STEP_FWD", value: true);
			if (base.EntityAnimator.GetBool("STEP_FWD"))
			{
				base.EntityAnimator.SetBool("STEP_BCK", value: false);
			}
			if (base.EntityAnimator.GetBool("STEP_BCK"))
			{
				base.EntityAnimator.SetBool("STEP_FWD", value: false);
			}
		}
	}

	public void Stop()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("STEP_FWD", value: false);
			base.EntityAnimator.SetBool("STEP_BCK", value: false);
		}
	}

	public void AttackEvent()
	{
		Menina menina = (Menina)OwnerEntity;
		menina.Attack.CurrentWeaponAttack();
		ResetAttackTrigger();
	}

	public void AnimatorEvent_Dispose()
	{
		OwnerEntity.gameObject.SetActive(value: false);
	}
}
