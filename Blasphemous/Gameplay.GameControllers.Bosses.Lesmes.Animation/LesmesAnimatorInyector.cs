using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Bosses.Lesmes.Animation;

public class LesmesAnimatorInyector : EnemyAnimatorInyector
{
	public void Throw()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("THROW");
		}
	}

	public void Dash(bool state)
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("DASH", state);
		}
	}

	public void TeleportIn()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("TELEPORT_IN");
		}
	}

	public void TeleportOut()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("TELEPORT_OUT");
		}
	}

	public void BigDashPreparation()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("PREPARATION");
		}
	}

	public void Plunge(bool state)
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("PLUNGE", state);
		}
	}

	public void Death()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void AttackAnimationEvent()
	{
	}

	public void ResetCoolDownAttack()
	{
		LesmesBehaviour componentInChildren = OwnerEntity.GetComponentInChildren<LesmesBehaviour>();
		if (componentInChildren != null)
		{
			componentInChildren.ResetCoolDown();
		}
	}
}
