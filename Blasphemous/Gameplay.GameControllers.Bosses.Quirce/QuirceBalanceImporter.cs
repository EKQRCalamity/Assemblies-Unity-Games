using Framework.EditorScripts.BossesBalance;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce;

public class QuirceBalanceImporter : BossBalanceImporter
{
	[SerializeField]
	protected GameObject InstantProjectile;

	[SerializeField]
	protected GameObject MovingInstantProjectile;

	[SerializeField]
	protected GameObject FireDash;

	[SerializeField]
	protected GameObject Plunge;

	[SerializeField]
	protected GameObject SplineThrow;

	[SerializeField]
	protected GameObject MeleeDash;

	[SerializeField]
	protected GameObject AreaSummon;

	[SerializeField]
	protected GameObject MultiTeleportPlunge;

	[SerializeField]
	protected GameObject LandingAreaSummon;

	protected override void ApplyLoadedStats()
	{
		SetInstantProjectileDamage();
		SetMovingInstantProjectileDamage();
		SetFireDashDamage();
		SetPlungeDamage();
		SetSplineThrowDamage();
		SetMeleeDashDamage();
		SetAreaSummonDamage();
		SetMultiTeleportPlungeDamage();
		SetLandingAreaSummonDamage();
	}

	private void SetInstantProjectileDamage()
	{
		SetDirectAttackDamage(InstantProjectile, base.GetHeavyAttackDamage);
		SetSpawnerAttackDamage(InstantProjectile, base.GetLightAttackDamage);
	}

	private void SetMovingInstantProjectileDamage()
	{
		SetDirectAttackDamage(MovingInstantProjectile, base.GetLightAttackDamage);
	}

	private void SetFireDashDamage()
	{
		SetDirectAttackDamage(FireDash, base.GetCriticalAttackDamage);
		SetSpawnerAttackDamage(FireDash, base.GetHeavyAttackDamage);
	}

	private void SetPlungeDamage()
	{
		SetDirectAttackDamage(Plunge, base.GetMediumAttackDamage);
		SetSpawnerAttackDamage(Plunge, base.GetMediumAttackDamage);
	}

	private void SetSplineThrowDamage()
	{
		SetProjectileAttackDamage(SplineThrow, base.GetMediumAttackDamage);
	}

	private void SetMeleeDashDamage()
	{
		SetDirectAttackDamage(MeleeDash, base.GetHeavyAttackDamage);
	}

	private void SetAreaSummonDamage()
	{
		SetSpawnerAttackDamage(AreaSummon, base.GetMediumAttackDamage);
	}

	private void SetMultiTeleportPlungeDamage()
	{
		SetDirectAttackDamage(MultiTeleportPlunge, base.GetMediumAttackDamage);
		SetSpawnerAttackDamage(MultiTeleportPlunge, base.GetMediumAttackDamage);
	}

	private void SetLandingAreaSummonDamage()
	{
		SetSpawnerAttackDamage(LandingAreaSummon, base.GetMediumAttackDamage);
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
				Debug.LogError(string.Concat("QuirceBalanceImporter::SetDirectAttackDamage: IDirectAttack not found in attackGO: ", attackGO, "!"));
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
				Debug.LogError(string.Concat("QuirceBalanceImporter::SetProjectileAttackDamage: IProjectileAttack not found in attackGO: ", attackGO, "!"));
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
				Debug.LogError(string.Concat("QuirceBalanceImporter::SetSpawnerAttackDamage: ISpawnerAttack not found in attackGO: ", attackGO, "!"));
			}
		}
	}
}
