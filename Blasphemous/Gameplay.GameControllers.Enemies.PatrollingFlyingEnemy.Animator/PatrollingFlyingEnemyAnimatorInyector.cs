using Framework.FrameworkCore;
using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.PatrollingFlyingEnemy.Animator;

public class PatrollingFlyingEnemyAnimatorInyector : EnemyAnimatorInyector
{
	[TutorialId]
	public string TutorialId;

	public bool isCherub;

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}
}
