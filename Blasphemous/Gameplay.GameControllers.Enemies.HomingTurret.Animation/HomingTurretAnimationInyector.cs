using Gameplay.GameControllers.Enemies.HomingTurret.AI;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.HomingTurret.Animation;

public class HomingTurretAnimationInyector : EnemyAnimatorInyector
{
	private static readonly int DeathParam = Animator.StringToHash("DEAD");

	private static readonly int Charge = Animator.StringToHash("CHARGE");

	private static readonly int DamageParam = Animator.StringToHash("DAMAGE");

	private static readonly int SpawnParam = Animator.StringToHash("SPAWN");

	private static readonly int HoldParam = Animator.StringToHash("HOLD");

	public void Death()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool(DeathParam, value: true);
		}
	}

	public void Damage()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger(DamageParam);
		}
	}

	public void ChargeAttack()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool(HoldParam, value: true);
			base.EntityAnimator.SetTrigger(Charge);
		}
	}

	public void ReleaseAttack()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool(HoldParam, value: false);
		}
	}

	public void EventAnimation_Attack()
	{
		HomingTurret homingTurret = (HomingTurret)OwnerEntity;
		HomingTurretBehaviour homingTurretBehaviour = homingTurret.EnemyBehaviour as HomingTurretBehaviour;
		if (homingTurretBehaviour != null)
		{
			homingTurretBehaviour.TurretAttack.FireProjectileToPenitent();
		}
	}

	public void Spawn()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger(SpawnParam);
		}
	}

	public void Dispose()
	{
		if ((bool)OwnerEntity)
		{
			OwnerEntity.gameObject.SetActive(value: false);
		}
	}
}
