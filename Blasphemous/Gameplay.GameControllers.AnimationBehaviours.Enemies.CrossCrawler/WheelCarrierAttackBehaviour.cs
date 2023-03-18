using Gameplay.GameControllers.Enemies.WheelCarrier;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.CrossCrawler;

public class WheelCarrierAttackBehaviour : StateMachineBehaviour
{
	protected WheelCarrier WheelCarrier;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (WheelCarrier == null)
		{
			WheelCarrier = animator.GetComponentInParent<WheelCarrier>();
		}
		WheelCarrier.IsAttacking = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		WheelCarrier.Behaviour.ResetCoolDown();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		WheelCarrier.IsAttacking = false;
		WheelCarrier.Behaviour.LookAtTarget(WheelCarrier.Target.transform.position);
	}
}
