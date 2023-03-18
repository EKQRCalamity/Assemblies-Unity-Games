using Framework.Managers;
using Gameplay.GameControllers.AnimationBehaviours.Player.Attack;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Run;

public class IdleAnimatonBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private StartChargingAttackBehaviour _startChargingAttackBehaviour;

	private static readonly int CrouchAttacking = Animator.StringToHash("CROUCH_ATTACKING");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		if (_startChargingAttackBehaviour == null)
		{
			_startChargingAttackBehaviour = animator.GetBehaviour<StartChargingAttackBehaviour>();
		}
		_penitent.Audio.StopLoadingChargedAttack();
		_penitent.IsGrabbingLadder = false;
		_penitent.Audio.AudioManager.StopSfx("PenitentLoadingChargedAttack");
		_penitent.Dash.StandUpAfterDash = false;
		_penitent.Dash.CrouchAfterDash = false;
		animator.SetBool(CrouchAttacking, value: false);
		if (_penitent.IsFallingStunt)
		{
			_penitent.IsFallingStunt = !_penitent.IsFallingStunt;
		}
		if (_penitent.IsChargingAttack)
		{
			_penitent.IsChargingAttack = !_penitent.IsChargingAttack;
		}
		_penitent.Status.IsIdle = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if ((bool)_penitent)
		{
			_penitent.Status.IsIdle = false;
		}
	}
}
