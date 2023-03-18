using Gameplay.GameControllers.Enemies.BellGhost;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.BellGhost;

public class BellGhostHurtBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.BellGhost.BellGhost _bellGhost;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_bellGhost == null)
		{
			_bellGhost = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.BellGhost.BellGhost>();
		}
		if (!_bellGhost.Status.IsHurt)
		{
			_bellGhost.Status.IsHurt = true;
		}
		_bellGhost.Audio.StopChargeAttack();
		_bellGhost.Audio.StopAttack(allowFade: false);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_bellGhost.Status.IsHurt)
		{
			_bellGhost.Status.IsHurt = !_bellGhost.Status.IsHurt;
		}
		_bellGhost.Behaviour.IsBackToOrigin = false;
	}
}
