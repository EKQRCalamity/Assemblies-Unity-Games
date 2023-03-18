using Framework.Inventory;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Items;

public class BloodPenitenceBeadEffect : ObjectEffect
{
	[SerializeField]
	[BoxGroup("Values", true, false, 0)]
	private float regenFactorIncrease;

	protected override bool OnApplyEffect()
	{
		Core.PenitenceManager.AddFlasksPassiveHealthRegen(regenFactorIncrease);
		return true;
	}

	protected override void OnRemoveEffect()
	{
		Core.PenitenceManager.AddFlasksPassiveHealthRegen(0f - regenFactorIncrease);
	}
}
