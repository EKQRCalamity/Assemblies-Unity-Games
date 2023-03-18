using Gameplay.GameControllers.Enemies.Runner;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Runner;

public class RunnerScreamBehaviour : StateMachineBehaviour
{
	protected Gameplay.GameControllers.Enemies.Runner.Runner Runner { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Runner == null)
		{
			Runner = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Runner.Runner>();
		}
		Runner.Behaviour.IsScreaming = true;
		Runner.Behaviour.Stop();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Runner.Behaviour.IsScreaming = false;
	}
}
