using Gameplay.GameControllers.Enemies.ExplodingEnemy;
using Gameplay.GameControllers.Enemies.ExplodingEnemy.AI;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.ExplodingEnemy;

public class ExplodingEnemyHurtBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy ExplodingEnemy { get; private set; }

	public ExplodingEnemyBehaviour ExplodingEnemyBehaviour { get; private set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (ExplodingEnemy == null)
		{
			ExplodingEnemy = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy>();
			ExplodingEnemyBehaviour = ExplodingEnemy.GetComponent<ExplodingEnemyBehaviour>();
		}
		ExplodingEnemyBehaviour.IsHurt = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		ExplodingEnemyBehaviour.IsHurt = false;
	}
}
