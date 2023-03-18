using Gameplay.GameControllers.Enemies.RangedBoomerang;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.RangedBoomerang;

public class RangedBoomerangAttackBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.RangedBoomerang.RangedBoomerang RangedBoomerang { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (RangedBoomerang == null)
		{
			RangedBoomerang = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.RangedBoomerang.RangedBoomerang>();
		}
		RangedBoomerang.IsAttacking = true;
	}
}
