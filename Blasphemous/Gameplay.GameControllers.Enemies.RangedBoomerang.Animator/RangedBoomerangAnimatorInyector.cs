using Gameplay.GameControllers.Enemies.RangedBoomerang.Attack;
using Gameplay.GameControllers.Enemies.RangedBoomerang.IA;
using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.RangedBoomerang.Animator;

public class RangedBoomerangAnimatorInyector : EnemyAnimatorInyector
{
	public RangedBoomerangAttack attack;

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
			base.EntityAnimator.SetBool("DEATH", value: true);
		}
	}

	public void AttackAnimationEvent()
	{
		RangedBoomerang rangedBoomerang = (RangedBoomerang)OwnerEntity;
		rangedBoomerang.Attack.CurrentWeaponAttack();
	}

	public void DisposeEnemy()
	{
		OwnerEntity.gameObject.SetActive(value: false);
	}

	public void ResetCoolDownAttack()
	{
		RangedBoomerangBehaviour componentInChildren = OwnerEntity.GetComponentInChildren<RangedBoomerangBehaviour>();
		if (componentInChildren != null)
		{
			componentInChildren.ResetCoolDown();
		}
	}
}
