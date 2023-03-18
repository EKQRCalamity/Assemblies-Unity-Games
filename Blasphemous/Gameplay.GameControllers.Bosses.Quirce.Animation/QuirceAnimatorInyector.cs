using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Bosses.Quirce.Animation;

public class QuirceAnimatorInyector : EnemyAnimatorInyector
{
	public void Throw()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("THROW");
		}
	}

	public void Spiral(bool on)
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("SPIRAL", on);
		}
	}

	public void Dash(bool state)
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("DASH", state);
		}
	}

	public void Hurt()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("HURT");
		}
	}

	public void ResetTeleport()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.ResetTrigger("TELEPORT_OUT");
		}
	}

	public void ResetHurt()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.ResetTrigger("HURT");
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

	public void SetToss(bool toss)
	{
		base.EntityAnimator.SetBool("TOSS", toss);
	}

	public void BigDashPreparation()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("PREPARATION");
		}
	}

	public void Landing()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("LANDING");
		}
	}

	public void TeleportInSword()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("TELEPORT_IN_TO_SWORD");
		}
	}

	public void TeleportOutSword()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("TELEPORT_OUT_TO_SWORD");
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
		QuirceBehaviour componentInChildren = OwnerEntity.GetComponentInChildren<QuirceBehaviour>();
		if (componentInChildren != null)
		{
			componentInChildren.ResetCoolDown();
		}
	}
}
