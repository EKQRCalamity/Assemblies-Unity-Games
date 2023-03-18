using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Effects;

public class GuiltDropRecover : Trait
{
	public GameObject GuiltDropRecoverEffect;

	[EventRef]
	public string EnergyBlastFx;

	protected override void OnStart()
	{
		base.OnStart();
		if ((bool)GuiltDropRecoverEffect)
		{
			PoolManager.Instance.CreatePool(GuiltDropRecoverEffect, 1);
		}
	}

	public void TriggerEffect()
	{
		if ((bool)GuiltDropRecoverEffect)
		{
			PoolManager.Instance.ReuseObject(GuiltDropRecoverEffect, base.EntityOwner.transform.position, Quaternion.identity);
			PlayBlastFx();
			GiveGuiltBonus();
		}
	}

	private void GiveGuiltBonus()
	{
		float num = Core.Logic.Penitent.Stats.Life.CurrentMax * 0.33f;
		if (Core.PenitenceManager.UseStocksOfHealth)
		{
			num = 30f;
		}
		Core.Logic.Penitent.Stats.Life.Current += num;
		float num2 = Core.Logic.Penitent.Stats.Fervour.CurrentMax * 0.5f;
		Core.Logic.Penitent.Stats.Fervour.Current += num2;
		if (Core.GuiltManager.DropTearsAlongWithGuilt)
		{
			Core.Logic.Penitent.Stats.Purge.Current += Core.GuiltManager.DroppedTears;
			Core.GuiltManager.DroppedTears = 0f;
		}
	}

	private void PlayBlastFx()
	{
		if (!string.IsNullOrEmpty(EnergyBlastFx))
		{
			Core.Audio.PlaySfx(EnergyBlastFx);
		}
	}
}
