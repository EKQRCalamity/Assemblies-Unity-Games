using Gameplay.GameControllers.Enemies.BellCarrier;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.BellCarrier;

public class BellCarrierDeathBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.BellCarrier.BellCarrier _bellCarrier;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_bellCarrier == null)
		{
			_bellCarrier = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.BellCarrier.BellCarrier>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if ((double)stateInfo.normalizedTime > 0.9)
		{
			Object.Destroy(_bellCarrier.gameObject);
		}
	}
}
