using Framework.Managers;
using UnityEngine;

namespace Framework.Inventory;

public class QuickAreaTransformBeadEffect : ObjectEffect
{
	[Range(1f, 2f)]
	public float AuraTransformAnimationSpeed = 1.5f;

	protected override bool OnApplyEffect()
	{
		SetAuraTransformSpeed(AuraTransformAnimationSpeed);
		return true;
	}

	protected override void OnRemoveEffect()
	{
		base.OnRemoveEffect();
		SetAuraTransformSpeed(1f);
	}

	private static void SetAuraTransformSpeed(float animationSpeed)
	{
		if (!(animationSpeed < 0f))
		{
			Animator animator = Core.Logic.Penitent.Animator;
			if ((bool)animator)
			{
				animator.SetFloat("PRAYER_SPEED_MULTIPLIER", animationSpeed);
			}
		}
	}
}
