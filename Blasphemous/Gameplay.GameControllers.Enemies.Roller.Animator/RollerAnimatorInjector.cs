using Gameplay.GameControllers.Enemies.Roller.Attack;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Roller.Animator;

public class RollerAnimatorInjector : EnemyAnimatorInyector
{
	public void Attack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("ATTACK");
		}
	}

	public void Rolling(bool isRolling)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("ROLLING", isRolling);
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

	public void DestroyEnemy()
	{
		if (!(OwnerEntity == null))
		{
			Object.Destroy(OwnerEntity.gameObject);
			base.EntityAnimator.ResetTrigger("ATTACK");
		}
	}

	public void LaunchProjectile()
	{
		RollerAttack rollerAttack = (RollerAttack)OwnerEntity.EntityAttack;
		rollerAttack.FireProjectile();
	}
}
