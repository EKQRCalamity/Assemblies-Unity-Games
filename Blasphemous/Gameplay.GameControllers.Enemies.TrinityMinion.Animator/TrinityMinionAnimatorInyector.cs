using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.TrinityMinion.Animator;

public class TrinityMinionAnimatorInyector : EnemyAnimatorInyector
{
	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}
}
