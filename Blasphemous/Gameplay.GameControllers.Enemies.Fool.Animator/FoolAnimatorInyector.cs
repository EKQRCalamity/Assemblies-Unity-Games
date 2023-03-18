using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.Fool.Animator;

public class FoolAnimatorInyector : EnemyAnimatorInyector
{
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

	public void TurnAround()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("TURN");
		}
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}
}
