using Framework.Inventory;
using Gameplay.GameControllers.Environment.AreaEffects;
using UnityEngine;

namespace Tools.Items;

public class SilverLungRelicEffect : RelicEffect
{
	public override void OnEquipEffect()
	{
		base.OnEquipEffect();
		EnablePoisonAreas(enablePoisonAreas: false);
	}

	public override void OnUnEquipEffect()
	{
		base.OnUnEquipEffect();
		EnablePoisonAreas();
	}

	private void EnablePoisonAreas(bool enablePoisonAreas = true)
	{
		PoisonAreaEffect[] array = Object.FindObjectsOfType<PoisonAreaEffect>();
		PoisonAreaEffect[] array2 = array;
		foreach (PoisonAreaEffect poisonAreaEffect in array2)
		{
			poisonAreaEffect.EnableEffect(enablePoisonAreas);
		}
	}
}
