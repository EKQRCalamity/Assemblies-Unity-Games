using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Bosses.TresAngustias.Animator;

public class TresAngustiasMasterAnimatorInyector : EnemyAnimatorInyector
{
	public void Disappear()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DISAPPEAR");
		}
	}

	public void Divide()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DIVIDE");
		}
	}

	public void Merge()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("MERGE");
		}
	}
}
