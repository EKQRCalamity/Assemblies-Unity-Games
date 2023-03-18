using Framework.EditorScripts.BossesBalance;
using Gameplay.GameControllers.Bosses.PietyMonster;
using Gameplay.GameControllers.Bosses.PietyMonster.Attack;
using UnityEngine;

public class PietyBalanceImporter : BossBalanceImporter
{
	private PietyMonster PietyBoss => (PietyMonster)bossEnemy;

	protected override void ApplyLoadedStats()
	{
		if ((bool)bossEnemy)
		{
			SetStompAttackDamageAmount();
			SetClawAttackDamageAmount();
			SetSmashAttackDamageAmount();
			SetRootsDamage();
			SetSpitAttack();
			SetBushAttack();
		}
	}

	private void SetStompAttackDamageAmount()
	{
		PietyStompAttack stompAttack = PietyBoss.PietyBehaviour.StompAttack;
		if ((bool)stompAttack)
		{
			stompAttack.DamageAmount = float.Parse(bossLoadedStats["Medium Attack"].ToString());
		}
	}

	private void SetClawAttackDamageAmount()
	{
		PietyClawAttack clawAttack = PietyBoss.PietyBehaviour.ClawAttack;
		if ((bool)clawAttack)
		{
			clawAttack.DamageAmount = float.Parse(bossLoadedStats["Medium Attack"].ToString());
		}
	}

	private void SetSmashAttackDamageAmount()
	{
		PietySmashAttack smashAttack = PietyBoss.PietyBehaviour.SmashAttack;
		if ((bool)smashAttack)
		{
			smashAttack.DamageAmount = float.Parse(bossLoadedStats["Heavy Attack"].ToString());
		}
	}

	private void SetRootsDamage()
	{
		PietyRootsManager pietyRootsManager = PietyBoss.PietyRootsManager;
		if ((bool)pietyRootsManager)
		{
			pietyRootsManager.RootDamage = float.Parse(bossLoadedStats["Light Attack"].ToString());
		}
	}

	private void SetSpitAttack()
	{
		PietySpitAttack componentInChildren = PietyBoss.GetComponentInChildren<PietySpitAttack>();
		if ((bool)componentInChildren)
		{
			componentInChildren.SpitDamage = float.Parse(bossLoadedStats["Light Attack"].ToString());
		}
	}

	private void SetBushAttack()
	{
		PietyBushManager pietyBushManager = Object.FindObjectOfType<PietyBushManager>();
		if ((bool)pietyBushManager)
		{
			pietyBushManager.BushDamage = (int)float.Parse(bossLoadedStats["Light Attack"].ToString());
		}
	}
}
