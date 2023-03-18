using System.Collections.Generic;
using System.Linq;
using Framework.FrameworkCore.Attributes;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Framework.EditorScripts.BossesBalance;

[RequireComponent(typeof(Enemy))]
public abstract class BossBalanceImporter : MonoBehaviour
{
	protected const string LIFE_BASE_STAT = "Life Base";

	protected const string STRENGTH_BASE_STAT = "Strength";

	protected const string LIGHT_ATTACK = "Light Attack";

	protected const string MEDIUM_ATTACK = "Medium Attack";

	protected const string HEAVY_ATTACK = "Heavy Attack";

	protected const string CRITICAL_ATTACK = "Critical Attack";

	protected const string LIGHT_ATTACK_COOLDOWN = "L.A. Cooldown";

	protected const string MEDIUM_ATTACK_COOLDOWN = "M.A. Cooldown";

	protected const string HEAVY_ATTACK_COOLDOWN = "H.A. Cooldown";

	protected const string VULNERABLE_LAPSE = "Vulnerable Lapse";

	protected const string CONTACT_DAMAGE = "Contact Damage";

	protected const string PURGE_POINTS = "Purge Points";

	protected Enemy bossEnemy;

	protected Dictionary<string, object> bossLoadedStats;

	protected int GetLifeBase => int.Parse(bossLoadedStats["Life Base"].ToString());

	protected int GetLightAttackDamage => int.Parse(bossLoadedStats["Light Attack"].ToString());

	protected int GetMediumAttackDamage => int.Parse(bossLoadedStats["Medium Attack"].ToString());

	protected int GetHeavyAttackDamage => int.Parse(bossLoadedStats["Heavy Attack"].ToString());

	protected int GetCriticalAttackDamage => int.Parse(bossLoadedStats["Critical Attack"].ToString());

	protected int GetPurgePoints => int.Parse(bossLoadedStats["Purge Points"].ToString());

	protected int GetContactDamage => int.Parse(bossLoadedStats["Contact Damage"].ToString());

	private void Awake()
	{
		bossEnemy = GetComponent<Enemy>();
	}

	private void Start()
	{
		LoadStats();
		ApplyLoadedStats();
		OnStart();
	}

	protected virtual void OnStart()
	{
		SetLifeStat();
		SetStrengthStat();
		SetPurgePoints();
	}

	private void LoadStats()
	{
		List<Dictionary<string, object>> bossesBalance = Core.GameModeManager.GetCurrentBossesBalanceChart().BossesBalance;
		using IEnumerator<Dictionary<string, object>> enumerator = bossesBalance.Where((Dictionary<string, object> bossBalanceItem) => bossEnemy.Id.Equals(bossBalanceItem["Id"])).GetEnumerator();
		if (enumerator.MoveNext())
		{
			Dictionary<string, object> current = enumerator.Current;
			bossLoadedStats = current;
		}
	}

	public virtual void SetLifeStat()
	{
		bossEnemy.Stats.Life = new Life(float.Parse(bossLoadedStats["Life Base"].ToString()), bossEnemy.Stats.LifeUpgrade, float.Parse(bossLoadedStats["Life Base"].ToString()), 1f);
	}

	private void SetPurgePoints()
	{
		if ((bool)bossEnemy)
		{
			bossEnemy.purgePointsWhenDead = GetPurgePoints;
		}
	}

	private void SetStrengthStat()
	{
		bossEnemy.Stats.Strength = new Strength(float.Parse(bossLoadedStats["Strength"].ToString()), bossEnemy.Stats.StrengthUpgrade, 1f);
	}

	protected abstract void ApplyLoadedStats();
}
