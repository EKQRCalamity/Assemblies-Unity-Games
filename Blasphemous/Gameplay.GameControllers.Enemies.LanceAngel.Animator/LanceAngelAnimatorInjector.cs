using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.LanceAngel.Animator;

public class LanceAngelAnimatorInjector : EnemyAnimatorInyector
{
	public void AttackReady()
	{
		base.EntityAnimator.SetTrigger("ATTACK_READY");
	}

	public void AttackStart()
	{
		base.EntityAnimator.SetTrigger("ATTACK_START");
		base.EntityAnimator.SetBool("ATTACKING", value: true);
	}

	public void StopAttack()
	{
		base.EntityAnimator.SetBool("ATTACKING", value: false);
	}

	public void Death()
	{
		StopAttack();
		base.EntityAnimator.SetTrigger("DEATH");
	}

	public void AnimationEvent_DisposeEntity()
	{
		OwnerEntity.gameObject.SetActive(value: false);
	}

	public void AnimationEvent_OnStopReposition()
	{
		LanceAngel lanceAngel = (LanceAngel)OwnerEntity;
		lanceAngel.Behaviour.Dash();
	}
}
