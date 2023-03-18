using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Generic;

public class GenericEnemyAttackBehaviour : StateMachineBehaviour
{
	private Enemy GenericEnemy;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (GenericEnemy == null)
		{
			GenericEnemy = animator.GetComponentInParent<Enemy>();
		}
		GenericEnemy.IsAttacking = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		GenericEnemy.IsAttacking = false;
	}
}
