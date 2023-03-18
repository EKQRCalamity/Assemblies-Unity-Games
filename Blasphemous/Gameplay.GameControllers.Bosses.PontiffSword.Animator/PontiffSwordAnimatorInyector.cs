using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Bosses.PontiffSword.Animator;

public class PontiffSwordAnimatorInyector : EnemyAnimatorInyector
{
	public void Alive(bool alive)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("ALIVE", alive);
		}
	}
}
