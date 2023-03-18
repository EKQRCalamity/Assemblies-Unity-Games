using Sirenix.OdinInspector;
using UnityEngine;

public class PontiffHuskBossActivateCollisionAnimationBehaviour : StateMachineBehaviour
{
	public bool collisionOnEnter;

	public bool collisionOnExit;

	public bool useAnimationPercentage;

	[ShowIf("useAnimationPercentage", true)]
	[Range(0f, 1f)]
	public float percentageToUse;

	private bool collisionOnPercentageUsed;

	private PontiffHuskBossAnimationEventsController eventsController;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (eventsController == null)
		{
			eventsController = animator.GetComponent<PontiffHuskBossAnimationEventsController>();
		}
		collisionOnPercentageUsed = false;
		eventsController.Animation_DoActivateCollisions(collisionOnEnter);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (eventsController == null)
		{
			eventsController = animator.GetComponent<PontiffHuskBossAnimationEventsController>();
		}
		if (useAnimationPercentage && !collisionOnPercentageUsed && stateInfo.normalizedTime >= percentageToUse)
		{
			eventsController.Animation_DoActivateCollisions(collisionOnExit);
			collisionOnPercentageUsed = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		eventsController.Animation_DoActivateCollisions(collisionOnExit);
	}
}
