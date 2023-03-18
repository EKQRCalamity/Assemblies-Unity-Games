using Framework.EditorScripts.BossesBalance;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Tools.Level.Actionables;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BlindBaby;

public class BlindBabyBalanceImporter : BossBalanceImporter
{
	[SerializeField]
	protected GameObject FallingProjectile;

	[SerializeField]
	protected GameObject BoomerangProjectile;

	[SerializeField]
	protected GameObject BouncingProjectile;

	protected override void ApplyLoadedStats()
	{
		SetFallingProjectileDamage();
		SetBoomerangProjectileDamage();
		SetBouncingProjectileDamage();
		SetContactDamage();
	}

	private void SetFallingProjectileDamage()
	{
		SetProjectileDamage(FallingProjectile, base.GetHeavyAttackDamage);
	}

	private void SetBoomerangProjectileDamage()
	{
		SetProjectileDamage(BoomerangProjectile, base.GetLightAttackDamage);
	}

	private void SetBouncingProjectileDamage()
	{
		SetProjectileDamage(BouncingProjectile, base.GetMediumAttackDamage);
	}

	private void SetContactDamage()
	{
		Transform parent = bossEnemy.transform.parent;
		SimpleDamageArea[] componentsInChildren = parent.GetComponentsInChildren<SimpleDamageArea>();
		SimpleDamageArea[] array = componentsInChildren;
		foreach (SimpleDamageArea simpleDamageArea in array)
		{
			simpleDamageArea.Damage = base.GetContactDamage;
		}
	}

	private static void SetProjectileDamage(GameObject projectile, int projectileDamage)
	{
		if (!(projectile == null))
		{
			projectile.GetComponent<IProjectileAttack>()?.SetProjectileWeaponDamage(projectileDamage);
		}
	}
}
