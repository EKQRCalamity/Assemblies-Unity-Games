using FMOD.Studio;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.ClimbLadder;

public class LadderSlidingBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private bool _isSlidingLoopSound;

	private bool _setMaxSpeed;

	private EventInstance _slidingSoundEvent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_setMaxSpeed = false;
		_isSlidingLoopSound = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent.IsLadderSliding && !_setMaxSpeed)
		{
			_setMaxSpeed = true;
			_penitent.GrabLadder.SetClimbingSpeed(6.75f);
		}
		if (!_isSlidingLoopSound)
		{
			_isSlidingLoopSound = true;
			_penitent.Audio.SlidingLadder(out _slidingSoundEvent);
		}
		if (_penitent.ReachBottonLadder)
		{
			_isSlidingLoopSound = false;
			_penitent.GrabLadder.EnableClimbLadderAbility(enable: false);
			animator.Play("Idle");
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_isSlidingLoopSound)
		{
			_isSlidingLoopSound = !_isSlidingLoopSound;
		}
		_penitent.Audio.StopSlidingLadder(_slidingSoundEvent);
		_penitent.GrabLadder.EnableClimbLadderAbility();
	}

	private void TriggerLadderLandingFxs()
	{
		if (!(_penitent.PlatformCharacterController.GroundDist >= 0.5f))
		{
			Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("LadderLanding");
			_penitent.PenitentMoveAnimations.PlaySlidingLadderLanding();
		}
	}
}
