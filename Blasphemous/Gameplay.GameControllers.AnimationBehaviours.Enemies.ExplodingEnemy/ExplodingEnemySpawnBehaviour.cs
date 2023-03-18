using Gameplay.GameControllers.Enemies.ExplodingEnemy;
using Gameplay.GameControllers.Enemies.ExplodingEnemy.AI;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.ExplodingEnemy;

public class ExplodingEnemySpawnBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy ExplodingEnemy { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (ExplodingEnemy == null)
		{
			ExplodingEnemy = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy>();
		}
		ExplodingEnemyBehaviour explodingEnemyBehaviour = (ExplodingEnemyBehaviour)ExplodingEnemy.EnemyBehaviour;
		explodingEnemyBehaviour.IsMelting = true;
		ExplodingEnemy.EntityAttack.transform.gameObject.SetActive(value: false);
		ExplodingEnemy.Audio.Appear();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		ExplodingEnemyBehaviour explodingEnemyBehaviour = (ExplodingEnemyBehaviour)ExplodingEnemy.EnemyBehaviour;
		explodingEnemyBehaviour.IsMelting = false;
		ExplodingEnemy.Status.CastShadow = true;
		ExplodingEnemy.EntityAttack.transform.gameObject.SetActive(value: true);
	}
}
