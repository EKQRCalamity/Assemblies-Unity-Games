using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.SingleAnguish.Animator;

public class SingleAnguishAnimatorInyector : EnemyAnimatorInyector
{
	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}
}
