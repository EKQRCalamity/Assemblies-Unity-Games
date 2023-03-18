using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.ElderBrother;

public class ElderBrotherAnimatorInyector : EnemyAnimatorInyector
{
	public GameObject blockCollider;

	public void AnimEvent_ActivateBarrier()
	{
		blockCollider.SetActive(value: true);
	}

	public void AnimEvent_DeactivateBarrier()
	{
		blockCollider.SetActive(value: false);
	}

	public void BigSmashPreparation()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("PREPARATION");
		}
	}

	public void Smash()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("SMASH");
		}
	}

	public void Death()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void SetMidAir(bool midAir)
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("MID-AIR", midAir);
		}
	}
}
