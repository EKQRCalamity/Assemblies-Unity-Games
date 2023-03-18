using Gameplay.GameControllers.Enemies.ExplodingEnemy;
using Gameplay.GameControllers.Enemies.ExplodingEnemy.AI;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.ExplodingEnemy;

public class ExplodingEnemyExplosionBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy ExplodingEnemy { get; set; }

	private ExplodingEnemyBehaviour ExplodingEnemyBehaviour { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (ExplodingEnemy == null)
		{
			ExplodingEnemy = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy>();
			ExplodingEnemyBehaviour = ExplodingEnemy.GetComponent<ExplodingEnemyBehaviour>();
		}
		ExplodingEnemy.EntityAttack.CurrentWeaponAttack();
		ExplodingEnemyBehaviour.IsExploding = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		ExplodingEnemyBehaviour.IsExploding = false;
		if (ExplodingEnemy.IsSummoned)
		{
			ExplodingEnemy.ReekLeader.Behaviour.ReekSpawner.DisposeReek(ExplodingEnemy.gameObject);
		}
		Object.Destroy(ExplodingEnemy.gameObject);
	}
}
