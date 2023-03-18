using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce.Attack;

public class BossSpawnedAreaAttackEndBehaviour : StateMachineBehaviour
{
	private BossSpawnedAreaAttack areaAttack;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (areaAttack == null)
		{
			areaAttack = animator.GetComponentInParent<BossSpawnedAreaAttack>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (areaAttack != null)
		{
			areaAttack.OnEndAnimationFinished();
		}
	}
}
