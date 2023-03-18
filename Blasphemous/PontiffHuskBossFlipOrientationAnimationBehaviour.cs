using UnityEngine;

public class PontiffHuskBossFlipOrientationAnimationBehaviour : StateMachineBehaviour
{
	private PontiffHuskBossAnimationEventsController eventsController;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (eventsController == null)
		{
			eventsController = animator.GetComponent<PontiffHuskBossAnimationEventsController>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		eventsController.Animation_FlipOrientation();
	}
}
