using System.Linq;
using Framework.EditorScripts.BossesBalance;
using Gameplay.GameControllers.Bosses.BejeweledSaint.IA;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint;

public class BejeweledSaintBalanceImporter : BossBalanceImporter
{
	private const string HANDS_LINE = "HANDS_LINE";

	protected override void ApplyLoadedStats()
	{
		SetHandLineAttackCooldown();
	}

	private void SetHandLineAttackCooldown()
	{
		BejeweledSaintBehaviour component = bossEnemy.GetComponent<BejeweledSaintBehaviour>();
		float cooldown = float.Parse(bossLoadedStats["M.A. Cooldown"].ToString());
		foreach (BejeweledSaintBehaviour.BejewelledAttackConfig item in component.attacksConfig.Where((BejeweledSaintBehaviour.BejewelledAttackConfig attack) => attack.atk.ToString() == "HANDS_LINE"))
		{
			item.cooldown = cooldown;
		}
	}
}
