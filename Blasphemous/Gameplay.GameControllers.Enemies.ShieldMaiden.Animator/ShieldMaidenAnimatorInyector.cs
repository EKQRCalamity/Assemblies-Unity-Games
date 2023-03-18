using Gameplay.GameControllers.Enemies.ShieldMaiden.Attack;
using Gameplay.GameControllers.Enemies.ShieldMaiden.IA;
using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.ShieldMaiden.Animator;

public class ShieldMaidenAnimatorInyector : EnemyAnimatorInyector
{
	public ShieldMaidenAttack attack;

	public void Walk()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("WALK", value: true);
		}
	}

	public void Parry()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("PARRY", value: true);
		}
	}

	public void ParryRecover()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("PARRY", value: false);
		}
	}

	public void Stop()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("WALK", value: false);
		}
	}

	public void Recover()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("RECOVER");
		}
	}

	public void Attack()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("ATTACK");
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
		ShieldMaiden shieldMaiden = (ShieldMaiden)OwnerEntity;
		shieldMaiden.Attack.CurrentWeaponAttack();
	}

	public void ActivateShieldAnimationEvent()
	{
		ShieldMaiden shieldMaiden = (ShieldMaiden)OwnerEntity;
		shieldMaiden.Behaviour.ToggleShield(active: true);
	}

	public void StopAll()
	{
		base.EntityAnimator.Play("Idle");
	}

	public void ResetCoolDownAttack()
	{
		ShieldMaidenBehaviour componentInChildren = OwnerEntity.GetComponentInChildren<ShieldMaidenBehaviour>();
		if (componentInChildren != null)
		{
			componentInChildren.ResetCoolDown();
		}
	}
}
