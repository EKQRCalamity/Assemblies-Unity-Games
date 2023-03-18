using Gameplay.GameControllers.Enemies.PontiffHusk;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.PontiffHusk;

public class PontiffHuskDeathBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.PontiffHusk.PontiffHuskRanged _pontiffHuskRanged;

	private PontiffHuskMelee _pontiffHuskMelee;

	private bool _destroy;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_pontiffHuskRanged == null)
		{
			_pontiffHuskRanged = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.PontiffHusk.PontiffHuskRanged>();
		}
		if (_pontiffHuskMelee == null)
		{
			_pontiffHuskMelee = animator.GetComponentInParent<PontiffHuskMelee>();
		}
		_destroy = false;
		if (_pontiffHuskRanged != null)
		{
			Gameplay.GameControllers.Enemies.PontiffHusk.PontiffHuskRanged pontiffHuskRanged = _pontiffHuskRanged;
			if (pontiffHuskRanged.MotionLerper.IsLerping)
			{
				pontiffHuskRanged.MotionLerper.StopLerping();
			}
			pontiffHuskRanged.Audio.StopFloating();
			pontiffHuskRanged.Audio.StopChargeAttack();
			pontiffHuskRanged.Audio.StopAttack();
			return;
		}
		PontiffHuskMelee pontiffHuskMelee = _pontiffHuskMelee;
		if (pontiffHuskMelee.AttackArea != null)
		{
			pontiffHuskMelee.AttackArea.WeaponCollider.enabled = false;
		}
		if (pontiffHuskMelee.MotionLerper.IsLerping)
		{
			pontiffHuskMelee.MotionLerper.StopLerping();
		}
		pontiffHuskMelee.Audio.StopFloating();
		pontiffHuskMelee.Audio.StopChargeAttack();
		pontiffHuskMelee.Audio.StopAttack();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime >= 0.95f && !_destroy)
		{
			_destroy = true;
			if (_pontiffHuskRanged != null)
			{
				_pontiffHuskRanged.gameObject.SetActive(value: false);
			}
			if (_pontiffHuskMelee != null)
			{
				_pontiffHuskMelee.gameObject.SetActive(value: false);
			}
		}
	}
}
