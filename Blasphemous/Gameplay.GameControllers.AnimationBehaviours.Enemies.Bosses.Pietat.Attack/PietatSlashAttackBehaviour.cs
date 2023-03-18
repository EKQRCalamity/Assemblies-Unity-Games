using Gameplay.GameControllers.Bosses.PietyMonster;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.Pietat.Attack;

public class PietatSlashAttackBehaviour : StateMachineBehaviour
{
	private PietyMonster _pietyMonster;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_pietyMonster == null)
		{
			_pietyMonster = animator.GetComponentInParent<PietyMonster>();
		}
		if (!_pietyMonster.IsAttacking)
		{
			_pietyMonster.IsAttacking = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (_pietyMonster.PietyBehaviour.Attacking)
		{
			_pietyMonster.PietyBehaviour.Attacking = false;
		}
		if (_pietyMonster.IsAttacking)
		{
			_pietyMonster.IsAttacking = false;
		}
		animator.ResetTrigger("SLASH_ATTACK");
	}
}
