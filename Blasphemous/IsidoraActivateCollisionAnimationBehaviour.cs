using Sirenix.OdinInspector;
using UnityEngine;

public class IsidoraActivateCollisionAnimationBehaviour : StateMachineBehaviour
{
	public bool collisionOnEnter;

	public bool collisionOnExit;

	public bool useAnimationPercentage;

	[ShowIf("useAnimationPercentage", true)]
	[Range(0f, 1f)]
	public float percentageToUse;

	[ShowIf("useAnimationPercentage", true)]
	public bool collisionOnPercentage;

	private bool collisionOnPercentageUsed;

	private IsidoraAnimationEventsController eventsController;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (eventsController == null)
		{
			eventsController = animator.GetComponent<IsidoraAnimationEventsController>();
		}
		collisionOnPercentageUsed = false;
		eventsController.DoActivateCollisions(collisionOnEnter);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (eventsController == null)
		{
			eventsController = animator.GetComponent<IsidoraAnimationEventsController>();
		}
		if (useAnimationPercentage && !collisionOnPercentageUsed && stateInfo.normalizedTime >= percentageToUse)
		{
			eventsController.DoActivateCollisions(collisionOnPercentage);
			collisionOnPercentageUsed = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		eventsController.DoActivateCollisions(collisionOnExit);
	}
}
