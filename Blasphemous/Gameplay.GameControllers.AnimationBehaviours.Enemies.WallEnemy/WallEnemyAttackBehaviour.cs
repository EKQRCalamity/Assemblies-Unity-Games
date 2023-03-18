using Gameplay.GameControllers.Enemies.WallEnemy;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.WallEnemy;

public class WallEnemyAttackBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.WallEnemy.WallEnemy _wallEnemy;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_wallEnemy == null)
		{
			_wallEnemy = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.WallEnemy.WallEnemy>();
		}
		if (!_wallEnemy.IsAttacking)
		{
			_wallEnemy.IsAttacking = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (_wallEnemy.IsAttacking)
		{
			_wallEnemy.IsAttacking = false;
		}
	}
}
