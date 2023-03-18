using Gameplay.GameControllers.Enemies.Roller.AI;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Roller.Animator;

public class AxeRollerAnimatorInjector : EnemyAnimatorInyector
{
	public void Attack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("ATTACK");
		}
	}

	public void StopAttack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("STOP_ATTACK");
		}
	}

	public void Damage()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.ResetTrigger("ATTACK");
			base.EntityAnimator.SetTrigger("HURT");
		}
	}

	public void Rolling(bool isRolling)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("ROLLING", isRolling);
			base.EntityAnimator.ResetTrigger("ATTACK");
			base.EntityAnimator.ResetTrigger("HURT");
		}
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
			StopMoving();
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

	public void StartMoving()
	{
		if (!(OwnerEntity == null))
		{
			AxeRoller axeRoller = (AxeRoller)OwnerEntity;
			AxeRollerBehaviour axeRollerBehaviour = (AxeRollerBehaviour)axeRoller.EnemyBehaviour;
			axeRollerBehaviour.Roll();
		}
	}

	public void StopMoving()
	{
		if (!(OwnerEntity == null))
		{
			AxeRoller axeRoller = (AxeRoller)OwnerEntity;
			axeRoller.EnemyBehaviour.StopMovement();
		}
	}

	public void StopAttackIfPenitentIsTooFar()
	{
		if (!(OwnerEntity == null))
		{
			AxeRoller axeRoller = (AxeRoller)OwnerEntity;
			axeRoller.EnemyBehaviour.StopMovement();
		}
	}

	public void LaunchProjectile()
	{
		if (!(OwnerEntity == null))
		{
			AxeRoller axeRoller = (AxeRoller)OwnerEntity;
			axeRoller.AxeAttack.FireProjectile();
			axeRoller.IsAttacking = false;
		}
	}
}
