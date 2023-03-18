using Gameplay.GameControllers.Enemies.ExplodingEnemy;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.ExplodingEnemy;

public class ExplodingEnemyTurnAround : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy ExplodingEnemy { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (ExplodingEnemy == null)
		{
			ExplodingEnemy = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy>();
		}
		ExplodingEnemy.EnemyBehaviour.TurningAround = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		ExplodingEnemy.SetOrientation(ExplodingEnemy.Status.Orientation);
		ExplodingEnemy.EnemyBehaviour.TurningAround = false;
	}
}
