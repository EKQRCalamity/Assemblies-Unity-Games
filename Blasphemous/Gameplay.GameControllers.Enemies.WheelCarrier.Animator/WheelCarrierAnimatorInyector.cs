using Gameplay.GameControllers.Enemies.WheelCarrier.IA;
using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.WheelCarrier.Animator;

public class WheelCarrierAnimatorInyector : EnemyAnimatorInyector
{
	private WheelCarrier _wheelCarrier;

	protected override void OnStart()
	{
		base.OnStart();
		_wheelCarrier = GetComponentInParent<WheelCarrier>();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		base.EntityAnimator.SetBool("IS_PARRIED", _wheelCarrier.Behaviour.GotParry);
	}

	public void Walk()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("WALK", value: true);
		}
	}

	public void Stop()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("WALK", value: false);
		}
	}

	public void Attack()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("ATTACK");
		}
	}

	public void Death()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void ParryReaction()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.Play("Parry");
		}
	}

	public void ResetCoolDownAttack()
	{
		WheelCarrierBehaviour componentInChildren = OwnerEntity.GetComponentInChildren<WheelCarrierBehaviour>();
		if (componentInChildren != null)
		{
			componentInChildren.ResetCoolDown();
		}
	}

	public void DisposeEnemy()
	{
		_wheelCarrier.gameObject.SetActive(value: false);
	}

	public void SetVulnerableTrue()
	{
		_wheelCarrier.Behaviour.StartVulnerablePeriod();
	}

	public void AttackAnimationEvent()
	{
		WheelCarrier wheelCarrier = (WheelCarrier)OwnerEntity;
		wheelCarrier.Attack.CurrentWeaponAttack();
	}

	public void PlayAttack()
	{
		_wheelCarrier.Audio.PlayAttack();
	}

	public void PlayDeathAnimation()
	{
		_wheelCarrier.Audio.PlayDeath();
	}
}
