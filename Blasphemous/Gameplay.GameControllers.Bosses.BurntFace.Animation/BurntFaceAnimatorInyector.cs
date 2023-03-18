using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Bosses.BurntFace.Animation;

public class BurntFaceAnimatorInyector : EnemyAnimatorInyector
{
	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void SetHidingLevel(int lvl)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetInteger("HIDING_LEVEL", lvl);
		}
	}

	public void PoisonBreath()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("POISON");
		}
	}
}
