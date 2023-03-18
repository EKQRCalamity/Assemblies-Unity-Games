using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint.Animation;

public class BejeweledSaintAnimatorInyector : EnemyAnimatorInyector
{
	public Animator ArmAnimator;

	public void HeadDamage()
	{
		if (!(OwnerEntity.Animator == null))
		{
			OwnerEntity.Animator.SetTrigger("HURT");
		}
	}

	public void BasicStaffAttack()
	{
		if (!(ArmAnimator == null))
		{
			ArmAnimator.SetTrigger("BASIC_ATTACK");
		}
	}

	public void DoSweep(bool isSweep)
	{
		if (!(ArmAnimator == null))
		{
			ArmAnimator.SetBool("IS_SWEEPING", isSweep);
		}
	}

	public void SetCackle(bool cackle)
	{
		OwnerEntity.Animator.SetBool("CACKLE", cackle);
	}

	public void ResetAttack()
	{
		ArmAnimator.ResetTrigger("BASIC_ATTACK");
		ArmAnimator.SetBool("IS_SWEEPING", value: false);
	}
}
