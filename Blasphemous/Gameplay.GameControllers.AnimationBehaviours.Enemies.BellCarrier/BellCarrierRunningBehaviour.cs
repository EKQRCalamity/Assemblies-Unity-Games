using Gameplay.GameControllers.Enemies.BellCarrier;
using Gameplay.GameControllers.Enemies.BellCarrier.IA;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.BellCarrier;

public class BellCarrierRunningBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.BellCarrier.BellCarrier _bellCarrier;

	private BellCarrierBehaviour _bellCarrierBehaviour;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_bellCarrier == null)
		{
			_bellCarrier = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.BellCarrier.BellCarrier>();
			_bellCarrierBehaviour = _bellCarrier.GetComponentInChildren<BellCarrierBehaviour>();
		}
		_bellCarrier.BodyBarrier.DisableCollider();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (_bellCarrierBehaviour.TurningAround)
		{
			_bellCarrierBehaviour.TurningAround = false;
		}
		if (_bellCarrierBehaviour.stopWhileChasing)
		{
			_bellCarrierBehaviour.stopWhileChasing = !_bellCarrierBehaviour.stopWhileChasing;
		}
		float horizontalInput = ((_bellCarrier.Status.Orientation != 0) ? (-1f) : 1f);
		_bellCarrier.Inputs.HorizontalInput = horizontalInput;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (!_bellCarrier.BodyBarrier.AvoidCollision)
		{
			_bellCarrier.BodyBarrier.AvoidCollision = false;
		}
		_bellCarrier.BodyBarrier.EnableCollider();
	}
}
