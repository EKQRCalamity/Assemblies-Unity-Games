using Gameplay.GameControllers.Enemies.PontiffHusk;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.PontiffHusk;

public class PontiffHuskIdleBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.PontiffHusk.PontiffHuskRanged _PontiffHuskRanged;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_PontiffHuskRanged == null)
		{
			_PontiffHuskRanged = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.PontiffHusk.PontiffHuskRanged>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!_PontiffHuskRanged)
		{
			return;
		}
		if (_PontiffHuskRanged.Behaviour.Asleep)
		{
			_PontiffHuskRanged.Audio.StopFloating();
			return;
		}
		if (_PontiffHuskRanged.IsAttacking)
		{
			_PontiffHuskRanged.Audio.StopFloating();
		}
		if (!_PontiffHuskRanged.Behaviour.IsAppear)
		{
			_PontiffHuskRanged.Audio.StopFloating();
		}
		if (_PontiffHuskRanged.Behaviour.IsAppear && _PontiffHuskRanged.IsAttacking)
		{
		}
	}
}
