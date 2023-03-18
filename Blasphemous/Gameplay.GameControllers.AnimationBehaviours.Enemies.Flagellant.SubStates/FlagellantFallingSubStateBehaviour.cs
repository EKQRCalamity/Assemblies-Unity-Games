using Gameplay.GameControllers.Enemies.Flagellant;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Flagellant.SubStates;

public class FlagellantFallingSubStateBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Flagellant.Flagellant _flagellant;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_flagellant == null)
		{
			_flagellant = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Flagellant.Flagellant>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_flagellant.IsFalling = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_flagellant.IsFalling = false;
	}
}
