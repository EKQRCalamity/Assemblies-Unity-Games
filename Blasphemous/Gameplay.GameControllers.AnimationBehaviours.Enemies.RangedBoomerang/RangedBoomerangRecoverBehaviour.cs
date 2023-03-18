using Framework.Managers;
using Gameplay.GameControllers.Enemies.RangedBoomerang;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.RangedBoomerang;

public class RangedBoomerangRecoverBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.RangedBoomerang.RangedBoomerang RangedBoomerang { get; set; }

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (RangedBoomerang == null)
		{
			RangedBoomerang = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.RangedBoomerang.RangedBoomerang>();
		}
		RangedBoomerang.IsAttacking = false;
		RangedBoomerang.Behaviour.LookAtTarget(Core.Logic.Penitent.transform.position);
	}
}
