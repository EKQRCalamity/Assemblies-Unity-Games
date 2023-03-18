using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.SubStatesBehaviours;

public class ChargeAttackSubStateBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = ((!(_penitent.PenitentAttack.AttackSpeed > 1f)) ? 1f : _penitent.PenitentAttack.AttackSpeed);
		if (_penitent.IsCrouchAttacking)
		{
			_penitent.IsCrouchAttacking = !_penitent.IsCrouchAttacking;
			animator.SetBool("CROUCH_ATTACKING", value: false);
		}
		if (!_penitent.IsChargingAttack)
		{
			_penitent.IsChargingAttack = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		animator.speed = 1f;
	}
}
