using Gameplay.GameControllers.Enemies.BellCarrier;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.BellCarrier;

public class BellCarrierIdleBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.BellCarrier.BellCarrier _bellCarrier;

	private readonly int _turnAround = Animator.StringToHash("TurnAround");

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
		if (!_bellCarrier.BellCarrierBehaviour.TargetInLine && animator.GetBool("CHASING"))
		{
			animator.Play(_turnAround);
		}
	}
}
