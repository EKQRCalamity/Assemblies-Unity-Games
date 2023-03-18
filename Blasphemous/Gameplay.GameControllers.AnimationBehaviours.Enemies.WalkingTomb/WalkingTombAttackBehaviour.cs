using Gameplay.GameControllers.Enemies.WalkingTomb;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.WalkingTomb;

public class WalkingTombAttackBehaviour : StateMachineBehaviour
{
	protected Gameplay.GameControllers.Enemies.WalkingTomb.WalkingTomb WalkingTomb { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (WalkingTomb == null)
		{
			WalkingTomb = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.WalkingTomb.WalkingTomb>();
		}
		WalkingTomb.IsAttacking = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		WalkingTomb.IsAttacking = false;
	}
}
