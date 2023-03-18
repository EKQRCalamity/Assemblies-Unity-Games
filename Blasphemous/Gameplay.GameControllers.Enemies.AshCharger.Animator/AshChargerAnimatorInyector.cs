using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.AshCharger.Animator;

public class AshChargerAnimatorInyector : EnemyAnimatorInyector
{
	public void Attack(bool attack = true)
	{
		base.EntityAnimator.SetBool("ATTACK", attack);
	}

	public void Death()
	{
		base.EntityAnimator.SetTrigger("DEATH");
	}

	public void AnimationEvent_DisableEntity()
	{
		OwnerEntity.gameObject.SetActive(value: false);
	}

	public void AnimationEvent_Attack()
	{
		AshCharger ashCharger = (AshCharger)OwnerEntity;
	}

	public void SetSpeed(float s)
	{
		base.EntityAnimator.speed = s;
	}
}
