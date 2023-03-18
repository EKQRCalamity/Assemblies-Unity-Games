using UnityEngine;

public class AmanecidaMeleeAttackAnimationBehaviour : StateMachineBehaviour
{
	public float percentageStart;

	public float percentageEnd;

	public bool alwaysActive;

	private AmanecidasAnimationEventsController amanecidasAnimationEventsController;

	private bool started;

	private bool finished;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		started = false;
		finished = false;
		if (amanecidasAnimationEventsController == null)
		{
			amanecidasAnimationEventsController = animator.GetComponent<AmanecidasAnimationEventsController>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!started && stateInfo.normalizedTime >= percentageStart)
		{
			amanecidasAnimationEventsController.AnimationEvent_MeleeAttackStart();
			started = true;
		}
		if (!finished && stateInfo.normalizedTime >= percentageEnd && !alwaysActive)
		{
			amanecidasAnimationEventsController.AnimationEvent_MeleeAttackFinished();
			finished = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		amanecidasAnimationEventsController.AnimationEvent_MeleeAttackFinished();
	}
}
