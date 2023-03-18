using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.ChimeRinger.Animator;

public class ChimeRingerAnimatorInyector : EnemyAnimatorInyector
{
	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
			if (!(OwnerEntity == null))
			{
				ChimeRinger chimeRinger = (ChimeRinger)OwnerEntity;
				chimeRinger.Audio.PlayDeath();
			}
		}
	}

	public void Ring()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("CAST");
		}
	}

	public void Hurt()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.ResetTrigger("DEATH");
			base.EntityAnimator.ResetTrigger("CAST");
			base.EntityAnimator.SetTrigger("HURT");
		}
	}

	public void RingAnimationEvent()
	{
		if (!(OwnerEntity == null))
		{
			ChimeRinger chimeRinger = (ChimeRinger)OwnerEntity;
			chimeRinger.Behaviour.TriggerAllTraps();
			chimeRinger.Audio.PlayCall();
		}
	}
}
