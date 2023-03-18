using Gameplay.GameControllers.Enemies.BellCarrier;
using Gameplay.GameControllers.Enemies.BellCarrier.IA;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.BellCarrier;

public class BellCarrierTurnAroundBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.BellCarrier.BellCarrier _bellCarrier;

	private BellCarrierBehaviour _bellCarrierBehaviour;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (!(_bellCarrier != null))
		{
			_bellCarrier = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.BellCarrier.BellCarrier>();
			_bellCarrierBehaviour = _bellCarrier.GetComponentInChildren<BellCarrierBehaviour>();
			_bellCarrier.AnimatorInyector.ResetTurnAround();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_bellCarrier.EnemyBehaviour.ReverseOrientation();
		if (_bellCarrierBehaviour.TurningAround)
		{
			_bellCarrierBehaviour.TurningAround = false;
		}
		if (_bellCarrier.BellCarrierBehaviour.WallHit)
		{
			_bellCarrier.BellCarrierBehaviour.WallHit = false;
		}
		if (_bellCarrierBehaviour.IsChasing)
		{
			_bellCarrierBehaviour.ResetTimeChasing();
		}
	}
}
