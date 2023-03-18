using Framework.EditorScripts.BossesBalance;
using Framework.FrameworkCore.Attributes;
using Gameplay.GameControllers.Bosses.PontiffSword;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffGiant;

public class PontiffGiantBalanceImporter : BossBalanceImporter
{
	[SerializeField]
	protected GameObject Beam;

	[SerializeField]
	protected GameObject LightningAttackInstant;

	[SerializeField]
	protected GameObject ToxicProjectile;

	[SerializeField]
	protected GameObject MagicBullets;

	[SerializeField]
	protected GameObject MagicShockwave;

	[SerializeField]
	protected GameObject MachinegunShooter;

	[SerializeField]
	protected Gameplay.GameControllers.Bosses.PontiffSword.PontiffSword PontiffSword;

	[SerializeField]
	protected GameObject SwordMeleeAttack;

	[DetailedInfoBox("Info", "This value is used to set the PontiffSword's max health value", InfoMessageType.Info, null)]
	public float SwordHealthPercentage = 0.19f;

	[DetailedInfoBox("Info", "This value is used to set the PontiffSword's attack value by using the PontiffGiant LightAttackDamage and multiplying that value by this coefficient.", InfoMessageType.Info, null)]
	public float SwordDamagePercentage = 0.5f;

	protected override void ApplyLoadedStats()
	{
		SetBeamDamage();
		SetLightningAttackInstantDamage();
		SetToxicProjectileDamage();
		SetMagicBulletsDamage();
		SetMagicShockwaveDamage();
		SetMachinegunShooterDamage();
		SetPontiffSwordLife();
		SetSwordMeleeAttackDamage();
	}

	private void SetBeamDamage()
	{
		SetSpawnerAttackDamage(Beam, base.GetCriticalAttackDamage);
	}

	private void SetLightningAttackInstantDamage()
	{
		SetSpawnerAttackDamage(LightningAttackInstant, base.GetLightAttackDamage);
	}

	private void SetToxicProjectileDamage()
	{
		SetProjectileAttackDamage(ToxicProjectile, base.GetLightAttackDamage);
	}

	private void SetMagicBulletsDamage()
	{
		SetProjectileAttackDamage(MagicBullets, base.GetMediumAttackDamage);
	}

	private void SetMagicShockwaveDamage()
	{
		SetSpawnerAttackDamage(MagicShockwave, base.GetMediumAttackDamage);
	}

	private void SetMachinegunShooterDamage()
	{
		SetProjectileAttackDamage(MachinegunShooter, base.GetHeavyAttackDamage);
	}

	private void SetPontiffSwordLife()
	{
		SetEnemyLife(PontiffSword, (int)((float)base.GetLifeBase * SwordHealthPercentage));
	}

	private void SetSwordMeleeAttackDamage()
	{
		SetDirectAttackDamage(SwordMeleeAttack, (int)((float)base.GetLightAttackDamage * SwordDamagePercentage));
	}

	private void SetDirectAttackDamage(GameObject attackGO, int damage)
	{
		if (!(attackGO == null))
		{
			IDirectAttack component = attackGO.GetComponent<IDirectAttack>();
			if (component != null)
			{
				component.SetDamage(damage);
			}
			else
			{
				Debug.LogError(string.Concat("PontiffGiantBalanceImporter::SetDirectAttackDamage: IDirectAttack not found in attackGO: ", attackGO, "!"));
			}
		}
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
				Debug.LogError(string.Concat("PontiffGiantBalanceImporter::SetProjectileAttackDamage: IProjectileAttack not found in attackGO: ", attackGO, "!"));
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
				Debug.LogError(string.Concat("PontiffGiantBalanceImporter::SetSpawnerAttackDamage: ISpawnerAttack not found in attackGO: ", attackGO, "!"));
			}
		}
	}

	private void SetEnemyLife(Enemy enemy, int life)
	{
		if (!(enemy == null))
		{
			enemy.Stats.Life = new Life(life, enemy.Stats.LifeUpgrade, life, 1f);
		}
	}
}
