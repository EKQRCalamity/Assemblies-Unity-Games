using Gameplay.GameControllers.Enemies.PontiffHusk;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.PontiffHuskRanged;

public class PontiffHuskRangedIdleToShootBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.PontiffHusk.PontiffHuskRanged PontiffHuskRanged { get; set; }

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (PontiffHuskRanged == null)
		{
			PontiffHuskRanged = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.PontiffHusk.PontiffHuskRanged>();
		}
		PontiffHuskRanged.IsAttacking = true;
	}
}
