using Gameplay.GameControllers.Bosses.Snake;
using UnityEngine;

public class SnakeMeleeAttackAnimationBehaviour : StateMachineBehaviour
{
	public float percentageStart;

	public float percentageEnd;

	public bool alwaysActive;

	public SnakeBehaviour.SNAKE_WEAPONS weaponToUse;

	private SnakeAnimationEventsController eventsController;

	private bool started;

	private bool finished;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (eventsController == null)
		{
			eventsController = animator.GetComponent<SnakeAnimationEventsController>();
		}
		if (!(eventsController == null))
		{
			started = false;
			finished = false;
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (eventsController == null)
		{
			eventsController = animator.GetComponent<SnakeAnimationEventsController>();
		}
		if (!(eventsController == null))
		{
			if (!started && stateInfo.normalizedTime >= percentageStart)
			{
				eventsController.Animation_SetWeapon(weaponToUse);
				eventsController.Animation_OnMeleeAttackStarts();
				started = true;
			}
			if (!finished && stateInfo.normalizedTime >= percentageEnd && !alwaysActive)
			{
				eventsController.Animation_OnMeleeAttackFinished();
				finished = true;
			}
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
			eventsController.Animation_OnMeleeAttackFinished();
		}
	}
}
