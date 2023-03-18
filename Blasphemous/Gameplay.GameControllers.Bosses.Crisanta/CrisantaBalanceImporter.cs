using Framework.EditorScripts.BossesBalance;
using Framework.FrameworkCore.Attributes;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Crisanta;

public class CrisantaBalanceImporter : BossBalanceImporter
{
	[SerializeField]
	protected Crisanta Crisanta;

	[SerializeField]
	protected BossAreaSummonAttack ShockWaveAttack;

	[SerializeField]
	protected BossAreaSummonAttack InstaLightningAttack;

	[SerializeField]
	protected CrisantaMeleeAttack MeleeLightAttack;

	[SerializeField]
	protected CrisantaMeleeAttack MeleeHeavyAttack;

	[SerializeField]
	protected BossDashAttack SlashDash;

	[SerializeField]
	protected BossDashAttack DiagonalSlashDash;

	[SerializeField]
	protected BossStraightProjectileAttack ProjectileAttack;

	protected override void ApplyLoadedStats()
	{
		SetShockWaveAttackDamage();
		SetInstaLightningAttackDamage();
		SetMeleeLightAttackDamage();
		SetMeleeHeavyAttackDamage();
		SetSlashDashAttackDamage();
		SetProjectileDamage();
		SetDiagonalSlashDashAttackDamage();
		SetContactDamage();
	}

	public override void SetLifeStat()
	{
		float num = ((!Crisanta.IsCrisantaRedux) ? 1f : 1.5f);
		bossEnemy.Stats.Life = new Life(float.Parse(bossLoadedStats["Life Base"].ToString()) * num, bossEnemy.Stats.LifeUpgrade, float.Parse(bossLoadedStats["Life Base"].ToString()) * num, 1f);
	}

	private void SetShockWaveAttackDamage()
	{
		SetSummonAttackAreaDamage(ShockWaveAttack, base.GetHeavyAttackDamage);
	}

	private void SetInstaLightningAttackDamage()
	{
		SetSummonAttackAreaDamage(InstaLightningAttack, base.GetMediumAttackDamage);
	}

	private void SetMeleeLightAttackDamage()
	{
		SetDirectAttackDamage(MeleeLightAttack, base.GetMediumAttackDamage);
	}

	private void SetMeleeHeavyAttackDamage()
	{
		SetDirectAttackDamage(MeleeHeavyAttack, base.GetHeavyAttackDamage);
	}

	private void SetSlashDashAttackDamage()
	{
		SetDirectAttackDamage(SlashDash, base.GetHeavyAttackDamage);
	}

	private void SetDiagonalSlashDashAttackDamage()
	{
		SetDirectAttackDamage(DiagonalSlashDash, base.GetCriticalAttackDamage);
	}

	private void SetProjectileDamage()
	{
		SetProjectileAttackDamage(ProjectileAttack, base.GetLightAttackDamage);
	}

	private static void SetDirectAttackDamage(IDirectAttack directAttack, int damage)
	{
		directAttack?.SetDamage(damage);
	}

	private void SetSummonAttackAreaDamage(BossAreaSummonAttack attackArea, int damage)
	{
		if (!(attackArea == null))
		{
			attackArea.SpawnedAreaAttackDamage = damage;
		}
	}

	private static void SetProjectileAttackDamage(IProjectileAttack projectileAttack, int damage)
	{
		projectileAttack?.SetProjectileWeaponDamage(damage);
	}

	protected void SetContactDamage()
	{
		EnemyAttack[] componentsInChildren = bossEnemy.GetComponentsInChildren<EnemyAttack>();
		foreach (EnemyAttack enemyAttack in componentsInChildren)
		{
			enemyAttack.ContactDamageAmount = base.GetContactDamage;
		}
	}
}
