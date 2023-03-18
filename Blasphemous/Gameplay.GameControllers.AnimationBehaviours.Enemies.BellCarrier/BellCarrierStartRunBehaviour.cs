using Gameplay.GameControllers.Enemies.BellCarrier;
using Gameplay.GameControllers.Enemies.BellCarrier.IA;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.BellCarrier;

public class BellCarrierStartRunBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.BellCarrier.BellCarrier _bellCarrier;

	private BellCarrierBehaviour _bellCarrierBehaviour;

	[Range(0f, 1f)]
	public float NormalizedTimeBeforeRunning;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (!(_bellCarrier != null))
		{
			_bellCarrier = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.BellCarrier.BellCarrier>();
			_bellCarrierBehaviour = _bellCarrier.GetComponentInChildren<BellCarrierBehaviour>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (stateInfo.normalizedTime >= NormalizedTimeBeforeRunning && _bellCarrierBehaviour.IsChasing && !_bellCarrierBehaviour.IsBlocked)
		{
			_bellCarrierBehaviour.StartMovement();
		}
	}
}
