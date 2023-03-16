using UnityEngine;

public class WeaponCrackshotExProjectileChild : BasicProjectile
{
	protected override void Start()
	{
		base.Start();
		base.animator.SetBool("IsB", Rand.Bool());
		base.animator.Play((!Rand.Bool()) ? "CometStartA" : "CometStartB");
		damageDealer.isDLCWeapon = true;
	}

	protected override void Die()
	{
		base.Die();
		if (base.animator.GetCurrentAnimatorStateInfo(0).IsTag("Comet"))
		{
			base.animator.Play((!Rand.Bool()) ? "ImpactCometB" : "ImpactCometA");
		}
		else
		{
			base.animator.Play((!Rand.Bool()) ? "ImpactSmallB" : "ImpactSmallA");
		}
	}

	private void OnEffectComplete()
	{
		Object.Destroy(base.gameObject);
	}
}
