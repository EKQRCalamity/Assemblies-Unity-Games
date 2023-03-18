using Gameplay.GameControllers.Enemies.Nun.Attack;
using Gameplay.GameControllers.Enemies.Nun.IA;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Nun.Animator;

public class NunAnimatorInyector : EnemyAnimatorInyector
{
	private static readonly int Turn = UnityEngine.Animator.StringToHash("TURN");

	private static readonly int WalkParam = UnityEngine.Animator.StringToHash("WALK");

	private static readonly int AttackParam = UnityEngine.Animator.StringToHash("ATTACK");

	private static readonly int DeathParam = UnityEngine.Animator.StringToHash("DEATH");

	public void TurnAround()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger(Turn);
		}
	}

	public void Walk()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool(WalkParam, value: true);
		}
	}

	public void Stop()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool(WalkParam, value: false);
		}
	}

	public void Attack()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger(AttackParam);
		}
	}

	public void Death()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger(DeathParam);
		}
	}

	public void ResetCoolDownAttack()
	{
		NunBehaviour componentInChildren = OwnerEntity.GetComponentInChildren<NunBehaviour>();
		if (componentInChildren != null)
		{
			componentInChildren.ResetCoolDown();
		}
	}

	public void SetCurrentDamageType(DamageArea.DamageType damageType)
	{
		NunAttack componentInChildren = OwnerEntity.GetComponentInChildren<NunAttack>();
		if (componentInChildren != null)
		{
			componentInChildren.CurrentDamageType = damageType;
		}
	}

	public void SpawnOilPuddle()
	{
	}
}
