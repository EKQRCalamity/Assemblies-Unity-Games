using System;
using Framework.Managers;
using UnityEngine;

namespace Framework.Inventory;

public class HardLandingBeadEffect : ObjectEffect_Stat
{
	private float _prevAnimatorSpeed = 1f;

	[SerializeField]
	protected string AccAnimation;

	[Range(1f, 10f)]
	public float AnimatorNormalizedSpeed = 1.25f;

	private Animator PenitentAnimator { get; set; }

	protected override bool OnApplyEffect()
	{
		PenitentAnimator = Core.Logic.Penitent.Animator;
		return base.OnApplyEffect();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!(PenitentAnimator == null) && PenitentAnimator.GetCurrentAnimatorStateInfo(0).IsName(AccAnimation) && Math.Abs(PenitentAnimator.speed - AnimatorNormalizedSpeed) > Mathf.Epsilon)
		{
			PenitentAnimator.speed = AnimatorNormalizedSpeed;
		}
	}

	protected override void OnRemoveEffect()
	{
		base.OnRemoveEffect();
		if (PenitentAnimator != null)
		{
			PenitentAnimator.speed = _prevAnimatorSpeed;
		}
	}
}
