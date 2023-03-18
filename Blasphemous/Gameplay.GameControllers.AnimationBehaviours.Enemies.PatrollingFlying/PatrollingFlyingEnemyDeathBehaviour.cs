using Gameplay.GameControllers.Enemies.PatrollingFlyingEnemy;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.PatrollingFlying;

public class PatrollingFlyingEnemyDeathBehaviour : StateMachineBehaviour
{
	private PatrollingFlyingEnemy _PatrollingFlyingEnemy;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_PatrollingFlyingEnemy == null)
		{
			_PatrollingFlyingEnemy = animator.GetComponentInParent<PatrollingFlyingEnemy>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if ((double)stateInfo.normalizedTime > 0.9)
		{
			Object.Destroy(_PatrollingFlyingEnemy.gameObject);
		}
	}
}
