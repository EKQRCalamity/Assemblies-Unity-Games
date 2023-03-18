using Framework.Managers;
using Gameplay.GameControllers.Enemies.BellGhost;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.BellGhost;

public class BellGhostVariantBAttackBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.BellGhost.BellGhost BellGhost { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (BellGhost == null)
		{
			BellGhost = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.BellGhost.BellGhost>();
		}
		BellGhost.IsAttacking = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		BellGhost.IsAttacking = false;
		BellGhost.Behaviour.LookAtTarget(Core.Logic.Penitent.transform.position);
	}
}
