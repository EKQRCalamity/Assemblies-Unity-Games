using Gameplay.GameControllers.Enemies.ReekLeader.Attack;
using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.ReekLeader.Animator;

public class ReekLeaderAnimatorInyector : EnemyAnimatorInyector
{
	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void Spawn()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("CAST");
		}
	}

	public void Hurt()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.ResetTrigger("DEATH");
			base.EntityAnimator.ResetTrigger("CAST");
			base.EntityAnimator.SetTrigger("HURT");
		}
	}

	public void Attack()
	{
		if (!(OwnerEntity == null))
		{
			ReekLeader reekLeader = (ReekLeader)OwnerEntity;
			ReekLeaderAttack reekLeaderAttack = (ReekLeaderAttack)reekLeader.EntityAttack;
			reekLeaderAttack.CurrentWeaponAttack();
		}
	}
}
