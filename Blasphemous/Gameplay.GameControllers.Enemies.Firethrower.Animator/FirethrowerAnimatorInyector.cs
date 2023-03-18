using Gameplay.GameControllers.Enemies.Firethrower.Attack;
using Gameplay.GameControllers.Enemies.Firethrower.IA;
using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.Firethrower.Animator;

public class FirethrowerAnimatorInyector : EnemyAnimatorInyector
{
	public FirethrowerAttack attack;

	public void TurnAround()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("TURN");
		}
	}

	public void Walk()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("WALK", value: true);
		}
	}

	public void Stop()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("WALK", value: false);
		}
	}

	public void Charge()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("CHARGE");
		}
	}

	public void Attack()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("ATTACK", value: true);
		}
	}

	public void StopAttack()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("ATTACK", value: false);
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
		Firethrower firethrower = (Firethrower)OwnerEntity;
		firethrower.Attack.CurrentWeaponAttack();
	}

	public void FireStartAnimationEvent()
	{
		attack.SetFireLevel(FirethrowerAttack.FIRE_LEVEL.START);
	}

	public void FireGrowingtAnimationEvent()
	{
		attack.SetFireLevel(FirethrowerAttack.FIRE_LEVEL.GROWING);
	}

	public void FireMainAnimationEvent()
	{
		attack.SetFireLevel(FirethrowerAttack.FIRE_LEVEL.LOOP);
	}

	public void AttackEndAnimationEvent()
	{
		attack.SetFireLevel(FirethrowerAttack.FIRE_LEVEL.NONE);
	}

	public void ResetCoolDownAttack()
	{
		FirethrowerBehaviour componentInChildren = OwnerEntity.GetComponentInChildren<FirethrowerBehaviour>();
		if (componentInChildren != null)
		{
			componentInChildren.ResetCoolDown();
		}
	}

	public void Dispose()
	{
		attack.SetFireLevel(FirethrowerAttack.FIRE_LEVEL.NONE);
		OwnerEntity.gameObject.SetActive(value: false);
	}
}
