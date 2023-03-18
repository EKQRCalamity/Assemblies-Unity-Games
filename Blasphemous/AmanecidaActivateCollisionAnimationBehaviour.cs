using Sirenix.OdinInspector;
using UnityEngine;

public class AmanecidaActivateCollisionAnimationBehaviour : StateMachineBehaviour
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

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<AmanecidasAnimationEventsController>().DoActivateCollisions(collisionOnEnter);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (useAnimationPercentage && !collisionOnPercentageUsed && stateInfo.normalizedTime >= percentageToUse)
		{
			animator.GetComponent<AmanecidasAnimationEventsController>().DoActivateCollisions(collisionOnPercentage);
			collisionOnPercentageUsed = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<AmanecidasAnimationEventsController>().DoActivateCollisions(collisionOnExit);
	}
}
