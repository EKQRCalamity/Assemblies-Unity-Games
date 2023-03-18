using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.BellGhost;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.BellGhost;

public class BellGhostTurnAroundBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.BellGhost.BellGhost _bellGhost;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_bellGhost == null)
		{
			_bellGhost = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.BellGhost.BellGhost>();
		}
		if (!_bellGhost.Behaviour.TurningAround)
		{
			_bellGhost.Behaviour.TurningAround = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (_bellGhost.Behaviour.TurningAround)
		{
			_bellGhost.Behaviour.TurningAround = false;
		}
		EntityOrientation orientation = _bellGhost.Status.Orientation;
		_bellGhost.SetOrientation(orientation);
	}
}
