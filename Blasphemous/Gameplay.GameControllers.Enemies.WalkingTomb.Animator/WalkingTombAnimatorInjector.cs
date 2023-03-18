using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.WalkingTomb.Animator;

public class WalkingTombAnimatorInjector : EnemyAnimatorInyector
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
		WalkingTomb walkingTomb = (WalkingTomb)OwnerEntity;
		walkingTomb.Attack.CurrentWeaponAttack();
	}
}
