using Sirenix.OdinInspector;
using UnityEngine;

public class SnakeActivateCollisionAnimationBehaviour : StateMachineBehaviour
{
	public enum TONGUE_COLLIDER
	{
		IDLE,
		OPEN_MOUTH
	}

	[EnumToggleButtons]
	public TONGUE_COLLIDER colliderType;

	public bool collisionOnEnter;

	public bool collisionOnExit;

	public bool useAnimationPercentage;

	[ShowIf("useAnimationPercentage", true)]
	[Range(0f, 1f)]
	public float percentageToUse;

	[ShowIf("useAnimationPercentage", true)]
	public bool collisionOnPercentage;

	private bool collisionOnPercentageUsed;

	private SnakeAnimationEventsController eventsController;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (eventsController == null)
		{
			eventsController = animator.GetComponent<SnakeAnimationEventsController>();
		}
		if (!(eventsController == null))
		{
			collisionOnPercentageUsed = false;
			if (colliderType == TONGUE_COLLIDER.OPEN_MOUTH)
			{
				eventsController.DoActivateCollisionsOpenMouth(collisionOnEnter);
			}
			else
			{
				eventsController.DoActivateCollisionsIdle(collisionOnEnter);
			}
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (eventsController == null)
		{
			eventsController = animator.GetComponent<SnakeAnimationEventsController>();
		}
		if (!(eventsController == null) && useAnimationPercentage && !collisionOnPercentageUsed && stateInfo.normalizedTime >= percentageToUse)
		{
			if (colliderType == TONGUE_COLLIDER.OPEN_MOUTH)
			{
				eventsController.DoActivateCollisionsOpenMouth(collisionOnPercentage);
			}
			else
			{
				eventsController.DoActivateCollisionsIdle(collisionOnPercentage);
			}
			collisionOnPercentageUsed = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (eventsController == null)
		{
			eventsController = animator.GetComponent<SnakeAnimationEventsController>();
		}
		if (!(eventsController == null))
		{
			if (colliderType == TONGUE_COLLIDER.OPEN_MOUTH)
			{
				eventsController.DoActivateCollisionsOpenMouth(collisionOnExit);
			}
			else
			{
				eventsController.DoActivateCollisionsIdle(collisionOnExit);
			}
		}
	}
}
