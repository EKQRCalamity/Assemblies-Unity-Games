using Gameplay.GameControllers.Bosses.Isidora;
using Sirenix.OdinInspector;
using UnityEngine;

public class IsidoraMeleeAttackAnimationBehaviour : StateMachineBehaviour
{
	public float percentageStart;

	public float percentageEnd;

	public bool alwaysActive;

	public IsidoraBehaviour.ISIDORA_WEAPONS weaponToUse;

	public bool flipCollider;

	[ShowIf("flipCollider", true)]
	public float percentageToFlipCollider;

	private IsidoraAnimationEventsController isidoraAnimationEventsController;

	private bool started;

	private bool finished;

	private bool flipped;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		started = false;
		finished = false;
		flipped = false;
		if (isidoraAnimationEventsController == null)
		{
			isidoraAnimationEventsController = animator.GetComponent<IsidoraAnimationEventsController>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!started && stateInfo.normalizedTime >= percentageStart)
		{
			isidoraAnimationEventsController.Animation_SetWeapon(weaponToUse);
			isidoraAnimationEventsController.Animation_OnMeleeAttackStarts();
			started = true;
		}
		if (flipCollider && !flipped && stateInfo.normalizedTime >= percentageToFlipCollider)
		{
			isidoraAnimationEventsController.Animation_FlipCollider();
			flipped = true;
		}
		if (!finished && stateInfo.normalizedTime >= percentageEnd && !alwaysActive)
		{
			isidoraAnimationEventsController.Animation_OnMeleeAttackFinished();
			finished = true;
			if (flipped)
			{
				isidoraAnimationEventsController.Animation_FlipCollider();
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		isidoraAnimationEventsController.Animation_OnMeleeAttackFinished();
		if (flipped)
		{
			isidoraAnimationEventsController.Animation_FlipCollider();
		}
	}
}
