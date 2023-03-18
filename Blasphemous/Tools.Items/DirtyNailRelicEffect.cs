using Framework.Inventory;
using Gameplay.GameControllers.Environment.AreaEffects;
using UnityEngine;

namespace Tools.Items;

public class DirtyNailRelicEffect : RelicEffect
{
	public override void OnEquipEffect()
	{
		base.OnEquipEffect();
		EnableMudAreas(enableMudAreas: false);
	}

	public override void OnUnEquipEffect()
	{
		base.OnUnEquipEffect();
		EnableMudAreas();
	}

	private void EnableMudAreas(bool enableMudAreas = true)
	{
		MudAreaEffect[] array = Object.FindObjectsOfType<MudAreaEffect>();
		MudAreaEffect[] array2 = array;
		foreach (MudAreaEffect mudAreaEffect in array2)
		{
			if (!mudAreaEffect.unafectedByRelic)
			{
				mudAreaEffect.EnableEffect(enableMudAreas);
			}
		}
	}
}
