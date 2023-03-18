using System;
using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.NewFlagellant.Animator;

public class NewFlagellantAnimatorInyector : EnemyAnimatorInyector
{
	public event Action<NewFlagellantAnimatorInyector> OnAttackAnimationFinished;

	public void AttackAnimationFinished()
	{
		if (this.OnAttackAnimationFinished != null)
		{
			this.OnAttackAnimationFinished(this);
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}

	public void Falling()
	{
		base.EntityAnimator.SetTrigger("FALL");
	}

	public void Landing()
	{
		base.EntityAnimator.SetTrigger("LAND");
	}

	public void Dash()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DASH");
		}
	}

	public void Hurt()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.Play("HURT", 0, 0f);
		}
	}

	public void ResetDash()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.ResetTrigger("DASH");
		}
	}

	public void Run(bool run)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("RUN", run);
		}
	}

	public void Walk(bool walk)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("WALK", walk);
		}
	}

	public void Attack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("ATTACK", value: true);
		}
	}

	public void FastAttack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("FASTATTACK");
		}
	}

	public void StopAttack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("ATTACK", value: false);
		}
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			IsParried(isParried: false);
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void Dispose()
	{
		OwnerEntity.gameObject.SetActive(value: false);
	}

	public void TriggerDash()
	{
		NewFlagellant newFlagellant = (NewFlagellant)OwnerEntity;
		newFlagellant.NewFlagellantBehaviour.Dash();
	}

	public void AnimationEvent_HeavyAttack()
	{
		NewFlagellant newFlagellant = (NewFlagellant)OwnerEntity;
		newFlagellant.Attack.CurrentWeaponAttack();
	}

	public void AnimationEvent_FastAttack()
	{
		NewFlagellant newFlagellant = (NewFlagellant)OwnerEntity;
		newFlagellant.FastAttack.CurrentWeaponAttack();
	}

	public void AnimationEvent_AttackDisplacement()
	{
		NewFlagellant newFlagellant = (NewFlagellant)OwnerEntity;
		newFlagellant.NewFlagellantBehaviour.AttackDisplacement();
	}

	public void IsParried(bool isParried)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("IS_PARRIED", isParried);
		}
	}
}
