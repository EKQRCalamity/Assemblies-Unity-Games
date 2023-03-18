using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Gizmos;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.ClimbClifLede;

public class HangOnCliffLedeBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private RootMotionDriver rootMotionDriver;

	private readonly int _hurtInTheAirAnimHash = Animator.StringToHash("Hurt In The Air");

	private bool alreadySetCanClimbCliffLede;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		if (rootMotionDriver == null)
		{
			rootMotionDriver = _penitent.GetComponentInChildren<RootMotionDriver>();
		}
		alreadySetCanClimbCliffLede = false;
		animator.ResetTrigger("FORWARD_JUMP");
		animator.ResetTrigger("JUMP");
		_penitent.IsClimbingCliffLede = true;
		_penitent.canClimbCliffLede = false;
		_penitent.PlatformCharacterInput.CancelJump();
		_penitent.CanLowerCliff = false;
		_penitent.AnimatorInyector.ResetStuntByFall();
		_penitent.Physics.Enable2DCollision(enable: false);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.cliffLedeClimbingStarted = true;
		SetHangOnOrientation();
		if (!alreadySetCanClimbCliffLede && stateInfo.normalizedTime >= 0.2f)
		{
			alreadySetCanClimbCliffLede = true;
			_penitent.canClimbCliffLede = true;
		}
		_penitent.PlatformCharacterController.PlatformCharacterPhysics.VSpeed = 0f;
		_penitent.PlatformCharacterController.InstantVelocity = Vector3.zero;
		_penitent.Physics.EnablePhysics(enable: false);
		if (stateInfo.normalizedTime <= 0.1f)
		{
			_penitent.transform.position = _penitent.RootTargetPosition;
		}
		if (_penitent.Status.Dead)
		{
			animator.Play(_hurtInTheAirAnimHash);
		}
		else if (_penitent.PlatformCharacterInput.Rewired.GetButtonDown(65) && !Core.Input.InputBlocked)
		{
			HangOffCliff();
		}
		else if (stateInfo.normalizedTime >= 0.5f && _penitent.PlatformCharacterInput.isJoystickDown && !Core.Input.InputBlocked)
		{
			HangOffCliff();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.Physics.Enable2DCollision();
		_penitent.PlatformCharacterController.PlatformCharacterPhysics.Acceleration = Vector3.zero;
		_penitent.PlatformCharacterController.InstantVelocity = Vector3.zero;
		_penitent.PlatformCharacterController.PlatformCharacterPhysics.VSpeed = 0f;
		_penitent.Animator.ResetTrigger("AIR_ATTACK");
	}

	private void HangOffCliff()
	{
		if (!_penitent.CanLowerCliff)
		{
			_penitent.Physics.EnablePhysics();
			_penitent.CanLowerCliff = true;
			HangOffDisplacement();
		}
	}

	private void SetHangOnOrientation()
	{
		if (_penitent.Status.Orientation != _penitent.CliffLedeOrientation)
		{
			_penitent.SetOrientation(_penitent.CliffLedeOrientation);
		}
		SetMotionDriverPosition(_penitent.CliffLedeOrientation);
	}

	private void SetMotionDriverPosition(EntityOrientation playerOrientation)
	{
		_penitent.RootMotionDrive = ((playerOrientation != 0) ? rootMotionDriver.ReversePosition : rootMotionDriver.transform.position);
	}

	private void HangOffDisplacement()
	{
		float num = ((_penitent.Status.Orientation != EntityOrientation.Left) ? (-0.5f) : 0.5f);
		float endValue = _penitent.transform.position.x + num;
		_penitent.transform.DOMoveX(endValue, 0.1f).SetEase(Ease.OutSine);
	}
}
