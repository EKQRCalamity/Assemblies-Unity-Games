using Gameplay.GameControllers.Enemies.ExplodingEnemy;
using Gameplay.GameControllers.Enemies.ExplodingEnemy.AI;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.ExplodingEnemy;

public class ExplodingEnemyVanishBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy ExplodingEnemy { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (ExplodingEnemy == null)
		{
			ExplodingEnemy = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy>();
		}
		ExplodingEnemyBehaviour explodingEnemyBehaviour = (ExplodingEnemyBehaviour)ExplodingEnemy.EnemyBehaviour;
		explodingEnemyBehaviour.IsMelting = true;
		ExplodingEnemy.Status.CastShadow = false;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (ExplodingEnemy.IsSummoned && !ExplodingEnemy.ReekLeader.Status.Dead)
		{
			ExplodingEnemy.ReekLeader.Behaviour.ReekSpawner.DisposeReek(ExplodingEnemy.gameObject);
		}
		Object.Destroy(ExplodingEnemy.gameObject);
	}
}
