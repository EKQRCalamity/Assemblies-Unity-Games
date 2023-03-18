using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ViciousDasher.Animator;

public class ViciousDasherAnimatorInyector : EnemyAnimatorInyector
{
	private static readonly int Parried = UnityEngine.Animator.StringToHash("IS_PARRIED");

	private static readonly int Dash1 = UnityEngine.Animator.StringToHash("DASH");

	private static readonly int Attack1 = UnityEngine.Animator.StringToHash("ATTACK");

	private static readonly int Death1 = UnityEngine.Animator.StringToHash("DEATH");

	public void Dash()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger(Dash1);
		}
	}

	public void ResetDash()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.ResetTrigger(Dash1);
		}
	}

	public void Attack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool(Attack1, value: true);
		}
	}

	public void StopAttack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool(Attack1, value: false);
		}
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			IsParried(isParried: false);
			base.EntityAnimator.SetTrigger(Death1);
		}
	}

	public void Dispose()
	{
		OwnerEntity.gameObject.SetActive(value: false);
	}

	public void TriggerDash()
	{
		ViciousDasher viciousDasher = (ViciousDasher)OwnerEntity;
		viciousDasher.ViciousDasherBehaviour.Dash();
	}

	public void AttackEvent()
	{
		ViciousDasher viciousDasher = (ViciousDasher)OwnerEntity;
		viciousDasher.Attack.CurrentWeaponAttack();
	}

	public void IsParried(bool isParried)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool(Parried, isParried);
		}
	}
}
