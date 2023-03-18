using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;

namespace Framework.Inventory;

public class PrayerShieldEffect : ObjectEffect_Stat
{
	private Penitent _owner;

	protected override bool OnApplyEffect()
	{
		_owner = Core.Logic.Penitent;
		ShieldSystemPrayer shieldPrayer = _owner.GetComponentInChildren<PrayerUse>().shieldPrayer;
		shieldPrayer.InstantiateShield();
		return base.OnApplyEffect();
	}

	protected override void OnRemoveEffect()
	{
		_owner = Core.Logic.Penitent;
		PrayerUse componentInChildren = _owner.GetComponentInChildren<PrayerUse>();
		if ((bool)componentInChildren && (bool)componentInChildren.shieldPrayer)
		{
			componentInChildren.shieldPrayer.DisposeShield();
		}
		base.OnRemoveEffect();
	}
}
