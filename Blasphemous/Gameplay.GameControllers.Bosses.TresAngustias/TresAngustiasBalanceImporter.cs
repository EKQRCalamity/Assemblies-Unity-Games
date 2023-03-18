using System;
using System.Collections.Generic;
using System.Linq;
using Framework.EditorScripts.BossesBalance;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.TresAngustias;

public class TresAngustiasBalanceImporter : BossBalanceImporter
{
	[Serializable]
	public struct LevelScrollSpeed
	{
		public GameModeManager.GAME_MODES GameMode;

		public float Speed;
	}

	private float maceAttackDamage;

	private int spearAttackDamage;

	[SerializeField]
	private List<LevelScrollSpeed> ScrollSpeeds;

	private TresAngustiasMasterBehaviour BossBehaviour => bossEnemy.EnemyBehaviour as TresAngustiasMasterBehaviour;

	protected override void ApplyLoadedStats()
	{
		maceAttackDamage = int.Parse(bossLoadedStats["Medium Attack"].ToString());
		spearAttackDamage = int.Parse(bossLoadedStats["Light Attack"].ToString());
		SetBeamAttack();
		SetLanceAttack();
		SetMaceAttack();
		SetShieldAttack();
		SetScrollableSpeed();
	}

	private void SetLanceAttack()
	{
		BossBehaviour.singleAnguishLance.Behaviour.spearAttack.SetDamage(spearAttackDamage);
		BossBehaviour.singleAnguishLance.Behaviour.maceAttack.PathFollowingProjectileDamage = maceAttackDamage;
	}

	private void SetMaceAttack()
	{
		BossBehaviour.singleAnguishMace.Behaviour.spearAttack.SetDamage(spearAttackDamage);
		BossBehaviour.singleAnguishMace.Behaviour.maceAttack.PathFollowingProjectileDamage = maceAttackDamage;
	}

	private void SetShieldAttack()
	{
		BossBehaviour.singleAnguishShield.Behaviour.spearAttack.SetDamage(spearAttackDamage);
		BossBehaviour.singleAnguishShield.Behaviour.maceAttack.PathFollowingProjectileDamage = maceAttackDamage;
	}

	private void SetBeamAttack()
	{
		int spawnedAreaAttackDamage = int.Parse(bossLoadedStats["Heavy Attack"].ToString());
		BossAreaSummonAttack componentInChildren = bossEnemy.GetComponentInChildren<BossAreaSummonAttack>();
		if ((bool)componentInChildren)
		{
			componentInChildren.SpawnedAreaAttackDamage = spawnedAreaAttackDamage;
		}
	}

	private void SetScrollableSpeed()
	{
		ScrollableModulesManager scrollableModulesManager = UnityEngine.Object.FindObjectOfType<ScrollableModulesManager>();
		if (!scrollableModulesManager)
		{
			return;
		}
		foreach (LevelScrollSpeed item in ScrollSpeeds.Where((LevelScrollSpeed scrollSpeed) => Core.GameModeManager.IsCurrentMode(scrollSpeed.GameMode)))
		{
			scrollableModulesManager.speed = item.Speed;
		}
	}
}
