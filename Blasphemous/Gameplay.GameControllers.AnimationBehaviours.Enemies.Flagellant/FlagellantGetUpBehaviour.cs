using Gameplay.GameControllers.Enemies.Flagellant;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Flagellant;

public class FlagellantGetUpBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Flagellant.Flagellant _flagellant;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_flagellant == null)
		{
			_flagellant = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Flagellant.Flagellant>();
		}
		if (!_flagellant.Status.IsHurt)
		{
			_flagellant.Status.IsHurt = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_flagellant.Status.IsHurt)
		{
			_flagellant.Status.IsHurt = !_flagellant.Status.IsHurt;
		}
	}
}
