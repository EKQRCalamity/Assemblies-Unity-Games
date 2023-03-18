using UnityEngine;

public class IsidoraResetSpeedBehaviour : StateMachineBehaviour
{
	private IsidoraAnimationEventsController isidoraAnimationEventsController;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (isidoraAnimationEventsController == null)
		{
			isidoraAnimationEventsController = animator.GetComponent<IsidoraAnimationEventsController>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		isidoraAnimationEventsController.Animation_CheckFlagAndResetSpeed();
	}
}
