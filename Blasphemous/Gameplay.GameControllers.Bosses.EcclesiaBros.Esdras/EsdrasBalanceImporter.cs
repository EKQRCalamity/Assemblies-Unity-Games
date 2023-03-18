using Framework.EditorScripts.BossesBalance;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras;

public class EsdrasBalanceImporter : BossBalanceImporter
{
	[SerializeField]
	protected EsdrasMeleeAttack MeleeLightAttack;

	[SerializeField]
	protected EsdrasMeleeAttack MeleeHeavyAttack;

	[SerializeField]
	protected EsdrasMeleeAttack MeleeSingleSpinAttack;

	[SerializeField]
	protected BossDashAttack SpinDashAttack;

	[SerializeField]
	protected BossAreaSummonAttack LightningAttack;

	[SerializeField]
	protected BossAreaSummonAttack LightningInstaAttack;

	protected override void ApplyLoadedStats()
	{
		SetMeleeLightAttackDamage();
		SetLightningAttackDamage();
		SetMeleeHeavyAttackDamage();
		SetSpinDashAttack();
		SetMeleeSingleSpinAttackDamage();
		SetInstantLightningAttackDamage();
		SetProjectileDamage();
	}

	private void SetMeleeLightAttackDamage()
	{
		SetEsdrasMeleeAttackDamage(MeleeLightAttack, base.GetLightAttackDamage);
	}

	private void SetMeleeHeavyAttackDamage()
	{
		SetEsdrasMeleeAttackDamage(MeleeHeavyAttack, base.GetCriticalAttackDamage);
	}

	private void SetMeleeSingleSpinAttackDamage()
	{
		SetEsdrasMeleeAttackDamage(MeleeSingleSpinAttack, base.GetHeavyAttackDamage);
	}

	private void SetSpinDashAttack()
	{
		SetEsdrasMeleeAttackDamage(SpinDashAttack, base.GetMediumAttackDamage);
	}

	private void SetLightningAttackDamage()
	{
		if (!(LightningAttack == null))
		{
			LightningAttack.SpawnedAreaAttackDamage = base.GetHeavyAttackDamage;
		}
	}

	private void SetInstantLightningAttackDamage()
	{
		if (!(LightningInstaAttack == null))
		{
			LightningInstaAttack.SpawnedAreaAttackDamage = base.GetHeavyAttackDamage;
		}
	}

	private void SetProjectileDamage()
	{
		BossStraightProjectileAttack componentInChildren = bossEnemy.GetComponentInChildren<BossStraightProjectileAttack>();
		if ((bool)componentInChildren)
		{
			componentInChildren.SetProjectileWeaponDamage(base.GetLightAttackDamage);
		}
	}

	private static void SetEsdrasMeleeAttackDamage(IDirectAttack meleeAttack, int meleeAttackDamage)
	{
		meleeAttack?.SetDamage(meleeAttackDamage);
	}
}
