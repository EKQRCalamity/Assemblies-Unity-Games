using Gameplay.GameControllers.Enemies.WallEnemy;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.WallEnemy;

public class WallEnemyDeathBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.WallEnemy.WallEnemy _wallEnemy;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_wallEnemy == null)
		{
			_wallEnemy = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.WallEnemy.WallEnemy>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Object.Destroy(_wallEnemy.gameObject);
	}
}
