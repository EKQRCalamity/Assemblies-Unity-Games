using Framework.Managers;
using UnityEngine;

namespace Framework.Inventory;

public class QuickHealingBeadEffect : ObjectEffect
{
	[Range(1f, 2f)]
	public float AnimatorSpeed = 1.1f;

	private string SPEED_PARAM = "HEALING_SPEED_MULTIPLIER";

	protected override bool OnApplyEffect()
	{
		Core.Logic.Penitent.Animator.SetFloat(SPEED_PARAM, AnimatorSpeed);
		return true;
	}

	protected override void OnRemoveEffect()
	{
		base.OnRemoveEffect();
		Core.Logic.Penitent.Animator.SetFloat(SPEED_PARAM, 1f);
	}
}
