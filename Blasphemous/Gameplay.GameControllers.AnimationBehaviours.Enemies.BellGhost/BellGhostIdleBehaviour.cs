using Gameplay.GameControllers.Enemies.BellGhost;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.BellGhost;

public class BellGhostIdleBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.BellGhost.BellGhost _bellGhost;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_bellGhost == null)
		{
			_bellGhost = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.BellGhost.BellGhost>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!_bellGhost)
		{
			return;
		}
		if (_bellGhost.Behaviour.Asleep)
		{
			_bellGhost.Audio.StopFloating();
			return;
		}
		if (_bellGhost.IsAttacking)
		{
			_bellGhost.Audio.StopFloating();
		}
		if (!_bellGhost.Behaviour.IsAppear)
		{
			_bellGhost.Audio.StopFloating();
		}
		if (_bellGhost.Behaviour.IsAppear && !_bellGhost.IsAttacking)
		{
			_bellGhost.Audio.PlayFloating();
		}
	}
}
