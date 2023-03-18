using System.Linq;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.NPCs.Damage;

public class NpcDamage : DamageArea
{
	protected override void OnStart()
	{
		base.OnStart();
		if ((bool)base.OwnerEntity)
		{
			base.OwnerEntity.OnDeath += OwnerEntityOnDeath;
		}
	}

	public override void TakeDamage(Gameplay.GameControllers.Entities.Hit hit, bool force = false)
	{
		base.TakeDamage(hit, force);
		base.OwnerEntity.Damage(hit.DamageAmount, hit.HitSoundId);
		if (!base.OwnerEntity.Status.Dead)
		{
			PlayHurtAnim();
		}
	}

	private void OwnerEntityOnDeath()
	{
		PlayDeathAnim();
		if (base.DamageAreaCollider.enabled)
		{
			base.DamageAreaCollider.enabled = false;
		}
	}

	public void PlayHurtAnim()
	{
		Animator animator = base.OwnerEntity.Animator;
		if (!(animator == null) && !animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
		{
			animator.Play("Hurt");
		}
	}

	public void PlayDeathAnim()
	{
		Animator animator = base.OwnerEntity.Animator;
		if (!(animator == null) && ContainsParam("DEATH"))
		{
			animator.SetTrigger("DEATH");
		}
	}

	public bool ContainsParam(string paramName)
	{
		return base.OwnerEntity.Animator.parameters.Any((AnimatorControllerParameter param) => param.name.Equals(paramName));
	}

	private void OnDestroy()
	{
		if ((bool)base.OwnerEntity)
		{
			base.OwnerEntity.OnDeath -= OwnerEntityOnDeath;
		}
	}
}
