using Gameplay.GameControllers.Enemies.PontiffHusk;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.PontiffHusk;

public class PontiffHuskHurtBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.PontiffHusk.PontiffHuskRanged _PontiffHuskRanged;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_PontiffHuskRanged == null)
		{
			_PontiffHuskRanged = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.PontiffHusk.PontiffHuskRanged>();
		}
		if (!_PontiffHuskRanged.Status.IsHurt)
		{
			_PontiffHuskRanged.Status.IsHurt = true;
		}
		_PontiffHuskRanged.Audio.StopChargeAttack();
		_PontiffHuskRanged.Audio.StopAttack(allowFade: false);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_PontiffHuskRanged.Status.IsHurt)
		{
			_PontiffHuskRanged.Status.IsHurt = !_PontiffHuskRanged.Status.IsHurt;
		}
	}
}
