using Framework.EditorScripts.BossesBalance;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Perpetua;

public class PerpetuaBalanceImporter : BossBalanceImporter
{
	[SerializeField]
	protected BossDashAttack DashAttack;

	[SerializeField]
	protected BossAreaSummonAttack SummonAttack;

	[SerializeField]
	protected BossInstantProjectileAttack ProjectileAttack;

	protected override void ApplyLoadedStats()
	{
		SetBossDashAttackDamage();
		SetSummonAttackDamage();
		SetProjectileAttackDamage();
	}

	private void SetBossDashAttackDamage()
	{
		if ((bool)DashAttack)
		{
			DashAttack.SetDamage(base.GetLightAttackDamage);
		}
	}

	private void SetSummonAttackDamage()
	{
		if ((bool)SummonAttack)
		{
			SummonAttack.SetSpawnsDamage(base.GetHeavyAttackDamage);
		}
	}

	private void SetProjectileAttackDamage()
	{
		if ((bool)ProjectileAttack)
		{
			ProjectileAttack.SetDamage(base.GetHeavyAttackDamage);
		}
	}
}
