using Gameplay.GameControllers.Enemies.Bishop.AI;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Bishop.Animation;

public class BishopAnimatorInyector : EnemyAnimatorInyector
{
	private static readonly int Turn = Animator.StringToHash("TURN");

	private static readonly int AttackParam = Animator.StringToHash("ATTACK");

	private static readonly int ChasingParam = Animator.StringToHash("CHASING");

	private static readonly int Hurt = Animator.StringToHash("HURT");

	public void TurnAround()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger(Turn);
		}
	}

	public void Chasing(bool isChasing)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.ResetTrigger(AttackParam);
			base.EntityAnimator.SetBool(ChasingParam, isChasing);
		}
	}

	public void Attack()
	{
		if (!(base.EntityAnimator == null) && !OwnerEntity.Status.Dead)
		{
			base.EntityAnimator.SetTrigger(AttackParam);
		}
	}

	public void Damage()
	{
		if (!(base.EntityAnimator == null) && !OwnerEntity.Status.Dead)
		{
			base.EntityAnimator.ResetTrigger(Turn);
			base.EntityAnimator.ResetTrigger(AttackParam);
			base.EntityAnimator.SetTrigger(Hurt);
		}
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.ResetTrigger(Hurt);
			base.EntityAnimator.ResetTrigger(AttackParam);
			base.EntityAnimator.Play("Death");
		}
	}

	public void Idle()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.ResetTrigger(Hurt);
			base.EntityAnimator.ResetTrigger(AttackParam);
			base.EntityAnimator.SetBool(ChasingParam, value: false);
		}
	}

	public void SpearAttack()
	{
		Bishop bishop = (Bishop)OwnerEntity;
		BishopBehaviour componentInChildren = bishop.GetComponentInChildren<BishopBehaviour>();
		if ((bool)componentInChildren && !componentInChildren.IsExecuted)
		{
			bishop.Attack.CurrentWeaponAttack();
		}
	}
}
