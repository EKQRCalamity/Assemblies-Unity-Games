using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Crouch;

public class CrouchUpBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private static readonly int JumpOff = Animator.StringToHash("JUMP_OFF");

	private static readonly int CrouchAttacking = Animator.StringToHash("CROUCH_ATTACKING");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitent.PlatformCharacterInput.CancelPlatformDropDown();
		_penitent.Physics.EnableColliders();
		animator.ResetTrigger(JumpOff);
		_penitent.isJumpOffReady = true;
		_penitent.PlatformCharacterController.PlatformCharacterPhysics.VSpeed = 0f;
		_penitent.PlatformCharacterController.InstantVelocity = Vector3.zero;
		_penitent.GrabLadder.IsTopLadderReposition = false;
		_penitent.Dash.CrouchAfterDash = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent.IsCrouchAttacking)
		{
			_penitent.IsCrouchAttacking = !_penitent.IsCrouchAttacking;
		}
		animator.SetBool(CrouchAttacking, value: false);
	}
}
