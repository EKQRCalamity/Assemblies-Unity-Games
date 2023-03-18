using Gameplay.GameControllers.Effects.Player.Dust;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Effects.Player;

public class StepDustBehaviour : StateMachineBehaviour
{
	private StepDust _stepDust;

	private StepDustSpawner _stepDustSpawner;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_stepDust == null)
		{
			_stepDust = animator.gameObject.GetComponent<StepDust>();
		}
		if (_stepDustSpawner == null)
		{
			_stepDustSpawner = _stepDust.Owner.GetComponentInChildren<StepDustSpawner>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_stepDustSpawner != null)
		{
			_stepDustSpawner.StoreStepDust(_stepDust);
		}
	}
}
