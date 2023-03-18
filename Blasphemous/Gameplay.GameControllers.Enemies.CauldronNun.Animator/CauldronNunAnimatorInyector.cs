using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.CauldronNun.Animator;

public class CauldronNunAnimatorInyector : EnemyAnimatorInyector
{
	private CauldronNun CauldronNun { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		CauldronNun = (CauldronNun)OwnerEntity;
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
			if ((bool)CauldronNun)
			{
				CauldronNun.Audio.PlayDeath();
			}
		}
	}

	public void PullChain()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("CAST");
			CauldronNun.Audio.PlayCall();
		}
	}

	public void Hurt()
	{
	}

	public void RingAnimationEvent()
	{
		if (!(CauldronNun == null))
		{
			CauldronNun.Behaviour.TriggerAllTraps();
		}
	}
}
