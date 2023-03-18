using Gameplay.GameControllers.Enemies.ExplodingEnemy.Attack;
using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.ExplodingEnemy.Animator;

public class ExplodingEnemyAnimatorInyector : EnemyAnimatorInyector
{
	public bool IsExploding => base.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("Explode");

	public void Damage()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("HURT");
		}
	}

	public void Walk()
	{
		if (!(base.EntityAnimator == null) && !base.EntityAnimator.GetBool("WALK"))
		{
			base.EntityAnimator.SetBool("WALK", value: true);
		}
	}

	public void StopWalk()
	{
		if (!(base.EntityAnimator == null) && base.EntityAnimator.GetBool("WALK"))
		{
			base.EntityAnimator.SetBool("WALK", value: false);
		}
	}

	public void ChargeExplosion()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("CHARGE_EXPLOSION");
		}
	}

	public void TurnAround()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("TURN");
		}
	}

	public void Vanish()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("VANISH");
		}
	}

	public void DisableExplosionAttack()
	{
		ExplodingEnemy explodingEnemy = (ExplodingEnemy)OwnerEntity;
		ExplodingEnemyAttack explodingEnemyAttack = (ExplodingEnemyAttack)explodingEnemy.EntityAttack;
		explodingEnemyAttack.HasExplode = true;
	}
}
