using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Isidora;

public class HomingBonfireAnimationInyector : EnemyAnimatorInyector
{
	private static readonly int HalfParam = Animator.StringToHash("HALF");

	private static readonly int FullParam = Animator.StringToHash("FULL");

	private static readonly int ExplodeParam = Animator.StringToHash("EXPLODE");

	public void EventAnimation_Attack()
	{
		HomingBonfire homingBonfire = (HomingBonfire)OwnerEntity;
		HomingBonfireBehaviour homingBonfireBehaviour = homingBonfire.EnemyBehaviour as HomingBonfireBehaviour;
		if (homingBonfireBehaviour != null)
		{
			homingBonfireBehaviour.BonfireAttack.FireProjectile();
		}
	}

	public void Dispose()
	{
		if ((bool)OwnerEntity)
		{
			OwnerEntity.gameObject.SetActive(value: false);
		}
	}

	public void SetParamHalf(bool paramValue)
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool(HalfParam, paramValue);
		}
	}

	public void SetParamFull(bool paramValue)
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool(FullParam, paramValue);
		}
	}

	public void SetParamExplode()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger(ExplodeParam);
		}
	}
}
