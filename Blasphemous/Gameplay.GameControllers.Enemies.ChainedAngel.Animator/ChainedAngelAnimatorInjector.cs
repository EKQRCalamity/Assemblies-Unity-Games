using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.ChainedAngel.Animator;

public class ChainedAngelAnimatorInjector : EnemyAnimatorInyector
{
	public void Death()
	{
		base.EntityAnimator.SetTrigger("DEATH");
	}

	public void AnimationEvent_Dispose()
	{
		OwnerEntity.transform.parent.gameObject.SetActive(value: false);
	}
}
