using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;

namespace Framework.Inventory;

public class PrayerAlliedCherubEffect : ObjectEffect_Stat
{
	private Penitent _owner;

	protected override bool OnApplyEffect()
	{
		_owner = Core.Logic.Penitent;
		PrayerUse componentInChildren = _owner.GetComponentInChildren<PrayerUse>();
		if (!componentInChildren)
		{
			return base.OnApplyEffect();
		}
		AlliedCherubPrayer cherubPrayer = componentInChildren.cherubPrayer;
		if ((bool)cherubPrayer)
		{
			cherubPrayer.InstantiateCherubs();
		}
		return base.OnApplyEffect();
	}

	protected override void OnRemoveEffect()
	{
		_owner = Core.Logic.Penitent;
		PrayerUse componentInChildren = _owner.GetComponentInChildren<PrayerUse>();
		if (!componentInChildren)
		{
			base.OnRemoveEffect();
		}
		AlliedCherubPrayer cherubPrayer = componentInChildren.cherubPrayer;
		if ((bool)cherubPrayer)
		{
			cherubPrayer.DisposeCherubs();
		}
		base.OnRemoveEffect();
	}
}
