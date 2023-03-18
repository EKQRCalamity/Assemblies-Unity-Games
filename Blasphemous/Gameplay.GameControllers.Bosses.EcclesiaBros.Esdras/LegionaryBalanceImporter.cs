using Framework.EditorScripts.BossesBalance;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras;

public class LegionaryBalanceImporter : BossBalanceImporter
{
	[SerializeField]
	protected BossAreaSummonAttack LightningSummonAttack;

	[SerializeField]
	protected EsdrasMeleeAttack MeleeSingleSpinAttack;

	[SerializeField]
	protected EsdrasMeleeAttack MeleeHeavyAttack;

	protected override void ApplyLoadedStats()
	{
		SetLightningSummonAttack();
		SetMeleeSingleSpinAttackDamage();
		SetMeleeHeavyAttackDamage();
	}

	private void SetLightningSummonAttack()
	{
		if ((bool)LightningSummonAttack)
		{
			LightningSummonAttack.SpawnedAreaAttackDamage = base.GetHeavyAttackDamage;
		}
	}

	private void SetMeleeHeavyAttackDamage()
	{
		SetLegionaryMeleeAttackDamage(MeleeHeavyAttack, base.GetMediumAttackDamage);
	}

	private void SetMeleeSingleSpinAttackDamage()
	{
		SetLegionaryMeleeAttackDamage(MeleeSingleSpinAttack, base.GetLightAttackDamage);
	}

	private static void SetLegionaryMeleeAttackDamage(IDirectAttack meleeAttack, int meleeAttackDamage)
	{
		meleeAttack?.SetDamage(meleeAttackDamage);
	}
}
