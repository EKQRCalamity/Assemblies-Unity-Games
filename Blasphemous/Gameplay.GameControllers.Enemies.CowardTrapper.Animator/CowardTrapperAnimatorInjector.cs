using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.CowardTrapper.Animator;

public class CowardTrapperAnimatorInjector : EnemyAnimatorInyector
{
	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void Hurt()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("HURT");
		}
	}

	public void Scared()
	{
		if (!(base.EntityAnimator == null))
		{
			CowardTrapper cowardTrapper = (CowardTrapper)OwnerEntity;
			cowardTrapper.Behaviour.IsRunAway = true;
			base.EntityAnimator.SetTrigger("RUN");
		}
	}

	public void Run()
	{
		CowardTrapper cowardTrapper = (CowardTrapper)OwnerEntity;
		cowardTrapper.Behaviour.StartRun();
		base.EntityAnimator.SetBool("RUNNING", value: true);
	}

	public void StopRun()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("RUNNING", value: false);
		}
	}

	public void Dispose()
	{
		OwnerEntity.gameObject.SetActive(value: false);
	}
}
