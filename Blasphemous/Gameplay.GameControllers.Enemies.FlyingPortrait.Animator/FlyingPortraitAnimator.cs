using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.FlyingPortrait.Animator;

public class FlyingPortraitAnimator : EnemyAnimatorInyector
{
	protected override void OnUpdate()
	{
		base.OnUpdate();
		FlyingPortrait flyingPortrait = (FlyingPortrait)OwnerEntity;
		base.EntityAnimator.SetBool("IS_PARRIED", flyingPortrait.Behaviour.GotParry);
	}

	public void UnHang()
	{
		base.EntityAnimator.SetTrigger("RELEASE");
	}

	public void Attack()
	{
		base.EntityAnimator.SetTrigger("ATTACK");
	}

	public void Alive()
	{
		base.EntityAnimator.Play("StunToIdle");
	}

	public void ResetAttack()
	{
		base.EntityAnimator.ResetTrigger("ATTACK");
	}

	public void AwakeAfterUnHang()
	{
		FlyingPortrait flyingPortrait = (FlyingPortrait)OwnerEntity;
		flyingPortrait.Behaviour.IsAwake = true;
		flyingPortrait.DamageArea.DamageAreaCollider.enabled = true;
		flyingPortrait.DamageByContact = true;
	}

	public void ParryReaction()
	{
		base.EntityAnimator.Play("Parry");
	}

	public void Death()
	{
		base.EntityAnimator.SetTrigger("DEATH");
	}

	public void WeaponAttack()
	{
		FlyingPortrait flyingPortrait = (FlyingPortrait)OwnerEntity;
		flyingPortrait.Attack.CurrentWeaponAttack();
	}

	public void Dispose()
	{
		OwnerEntity.gameObject.SetActive(value: false);
	}
}
