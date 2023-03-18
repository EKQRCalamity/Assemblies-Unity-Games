using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Crouch;

public class CrouchDownBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	[Range(0f, 1f)]
	public float allowJumpOffReadyLenght;

	private static readonly int CrouchAttacking = Animator.StringToHash("CROUCH_ATTACKING");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		if (_penitent.IsFallingStunt)
		{
			animator.Play("Landing");
		}
		_penitent.BeginCrouch = true;
		if (_penitent.IsCrouchAttacking)
		{
			_penitent.IsCrouchAttacking = !_penitent.IsCrouchAttacking;
		}
		float timeGrounded = _penitent.AnimatorInyector.TimeGrounded;
		if (timeGrounded < 0.05f)
		{
			_penitent.Audio.PlayLandingForward();
		}
		if (_penitent.StepOnLadder)
		{
			_penitent.GrabLadder.SetClimbingSpeed(0f);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetBool(CrouchAttacking, value: false);
		if (stateInfo.normalizedTime >= stateInfo.length * allowJumpOffReadyLenght)
		{
			_penitent.isJumpOffReady = true;
		}
		if (_penitent.PlatformCharacterInput.Attack && !_penitent.IsCrouchAttacking)
		{
			animator.SetBool(CrouchAttacking, value: true);
			animator.Play("Crouch Attack");
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.BeginCrouch = false;
		_penitent.isJumpOffReady = false;
		if (_penitent.GrabLadder.GetClimbingSpeed() < 2.25f)
		{
			_penitent.GrabLadder.SetClimbingSpeed(2.25f);
		}
	}
}
