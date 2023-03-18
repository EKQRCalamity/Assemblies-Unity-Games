using Framework.EditorScripts.BossesBalance;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffOldman;

public class PontiffOldmanBalanceImporter : BossBalanceImporter
{
	[SerializeField]
	protected GameObject MachinegunShooter;

	[SerializeField]
	protected GameObject LightningAttack;

	[SerializeField]
	protected GameObject LightningAttackInstant;

	[SerializeField]
	protected GameObject MagicProjectile;

	[SerializeField]
	protected GameObject ToxicProjectile;

	protected override void ApplyLoadedStats()
	{
		SetMachinegunShooterDamage();
		SetLightningAttackDamage();
		SetLightningAttackInstantDamage();
		SetMagicProjectileDamage();
		SetToxicProjectileDamage();
	}

	private void SetMachinegunShooterDamage()
	{
		SetProjectileAttackDamage(MachinegunShooter, base.GetHeavyAttackDamage);
	}

	private void SetLightningAttackDamage()
	{
		SetSpawnerAttackDamage(LightningAttack, base.GetLightAttackDamage);
	}

	private void SetLightningAttackInstantDamage()
	{
		SetSpawnerAttackDamage(LightningAttackInstant, base.GetLightAttackDamage);
	}

	private void SetMagicProjectileDamage()
	{
		SetProjectileAttackDamage(MagicProjectile, base.GetCriticalAttackDamage);
	}

	private void SetToxicProjectileDamage()
	{
		SetProjectileAttackDamage(ToxicProjectile, base.GetMediumAttackDamage);
	}

	private void SetProjectileAttackDamage(GameObject attackGO, int damage)
	{
		if (!(attackGO == null))
		{
			IProjectileAttack component = attackGO.GetComponent<IProjectileAttack>();
			if (component != null)
			{
				component.SetProjectileWeaponDamage(damage);
			}
			else
			{
				Debug.LogError(string.Concat("PontiffOldmanBalanceImporter::SetProjectileAttackDamage: IProjectileAttack not found in attackGO: ", attackGO, "!"));
			}
		}
	}

	private static void SetSpawnerAttackDamage(GameObject attackGO, int damage)
	{
		if (!(attackGO == null))
		{
			ISpawnerAttack component = attackGO.GetComponent<ISpawnerAttack>();
			if (component != null)
			{
				component.SetSpawnsDamage(damage);
			}
			else
			{
				Debug.LogError(string.Concat("PontiffOldmanBalanceImporter::SetSpawnerAttackDamage: ISpawnerAttack not found in attackGO: ", attackGO, "!"));
			}
		}
	}
}
