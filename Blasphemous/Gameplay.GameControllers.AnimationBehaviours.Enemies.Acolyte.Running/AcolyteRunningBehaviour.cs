using Gameplay.GameControllers.Enemies.Acolyte;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Acolyte.Running;

public class AcolyteRunningBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Acolyte.Acolyte _acolyte;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_acolyte == null)
		{
			_acolyte = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Acolyte.Acolyte>();
		}
		if (_acolyte.IsAttacking)
		{
			_acolyte.IsAttacking = false;
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!(_acolyte == null) && !_acolyte.IsChasing)
		{
			_acolyte.IsChasing = true;
		}
	}
}
