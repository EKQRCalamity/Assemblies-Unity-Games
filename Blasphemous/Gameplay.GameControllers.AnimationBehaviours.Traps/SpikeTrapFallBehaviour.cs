using Gameplay.GameControllers.Environment.Traps.SpikesTrap;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Traps;

public class SpikeTrapFallBehaviour : StateMachineBehaviour
{
	private SpikeTrap spikeTrap;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (spikeTrap == null)
		{
			spikeTrap = animator.GetComponent<SpikeTrap>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (spikeTrap.SpikeTrapCollider.enabled)
		{
			spikeTrap.SpikeTrapCollider.enabled = false;
		}
	}
}
