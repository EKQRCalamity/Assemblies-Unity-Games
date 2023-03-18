using Framework.Managers;
using Gameplay.GameControllers.AnimationBehaviours.Player.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.SubStatesBehaviours;

public class HurtSubStateBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private StartChargingAttackBehaviour _startChargingAttackBehaviour;

	private ThrowBack _throwBack;

	private static readonly int CrouchAttacking = Animator.StringToHash("CROUCH_ATTACKING");

	private static readonly int Throw = Animator.StringToHash("THROW");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE))
		{
			animator.Play("Idle");
		}
		if (!_penitent)
		{
			_penitent = Core.Logic.Penitent;
			_throwBack = _penitent.GetComponentInChildren<ThrowBack>();
		}
		_penitent.Status.IsHurt = true;
		if (_startChargingAttackBehaviour == null)
		{
			_startChargingAttackBehaviour = animator.GetBehaviour<StartChargingAttackBehaviour>();
		}
		_penitent.Audio.StopLoadingChargedAttack();
		if (_penitent.IsChargingAttack)
		{
			_penitent.IsChargingAttack = !_penitent.IsChargingAttack;
		}
		_penitent.IsStickedOnWall = false;
		_penitent.Dash.CrouchAfterDash = false;
		_penitent.Animator.speed = 1f;
		animator.SetBool(CrouchAttacking, value: false);
		_penitent.Audio.StopUseFlask();
		if (!_throwBack.Casting && !animator.GetBool(Throw) && (!Core.LevelManager.currentLevel.LevelName.Equals("D24Z01S01") || Core.Logic.Penitent.CurrentLife != 1f))
		{
			_penitent.Physics.EnablePhysics();
			_penitent.Physics.EnableColliders();
			_penitent.Physics.Enable2DCollision();
			_penitent.GrabLadder.EnableClimbLadderAbility(enable: false);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent.IsDashing)
		{
			_penitent.Dash.StopCast();
		}
		_penitent.GrabLadder.EnableClimbLadderAbility(enable: false);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.Status.IsHurt = false;
		_penitent.IsClimbingCliffLede = false;
		_penitent.IsGrabbingCliffLede = false;
		_penitent.IsGrabbingLadder = false;
		_penitent.GrabLadder.EnableClimbLadderAbility();
	}
}
