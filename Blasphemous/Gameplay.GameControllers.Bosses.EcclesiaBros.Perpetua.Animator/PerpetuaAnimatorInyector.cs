using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Perpetua.Animator;

public class PerpetuaAnimatorInyector : EnemyAnimatorInyector
{
	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void Vanish()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("VANISH");
		}
	}

	public void Appear()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("APPEAR");
		}
	}

	public void Spell()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("SPELL");
		}
	}

	public void ChargeDash(bool activate)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("CHARGE", activate);
		}
	}

	public void Dash()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DASH");
		}
	}

	public void Flap(bool activate)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("FLAP", activate);
		}
	}

	public void AnimationEvent_Dispose()
	{
		OwnerEntity.gameObject.SetActive(value: false);
	}
}
