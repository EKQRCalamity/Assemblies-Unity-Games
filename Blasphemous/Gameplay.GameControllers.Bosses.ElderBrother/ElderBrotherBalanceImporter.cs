using Framework.EditorScripts.BossesBalance;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.ElderBrother;

public class ElderBrotherBalanceImporter : BossBalanceImporter
{
	[SerializeField]
	protected BossAreaSummonAttack CorpsesSummonAttack;

	[SerializeField]
	protected BossAreaSummonAttack MaceSummonAttack;

	protected override void ApplyLoadedStats()
	{
		SetJumpAttackDamage();
		SetCorpseAreaAttackDamage();
		SetMaceAttackDamage();
		SetContactDamage();
	}

	private void SetJumpAttackDamage()
	{
		BossJumpAttack componentInChildren = bossEnemy.GetComponentInChildren<BossJumpAttack>();
		if ((bool)componentInChildren)
		{
			componentInChildren.SetDamage(base.GetLightAttackDamage);
		}
	}

	private void SetCorpseAreaAttackDamage()
	{
		if (!(CorpsesSummonAttack == null))
		{
			CorpsesSummonAttack.SpawnedAreaAttackDamage = base.GetMediumAttackDamage;
		}
	}

	private void SetMaceAttackDamage()
	{
		if (!(MaceSummonAttack == null))
		{
			MaceSummonAttack.SpawnedAreaAttackDamage = base.GetMediumAttackDamage;
		}
	}

	private void SetContactDamage()
	{
		EnemyAttack[] componentsInChildren = bossEnemy.GetComponentsInChildren<EnemyAttack>();
		foreach (EnemyAttack enemyAttack in componentsInChildren)
		{
			enemyAttack.ContactDamageAmount = base.GetContactDamage;
		}
	}
}
