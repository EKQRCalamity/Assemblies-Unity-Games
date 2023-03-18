using Framework.Managers;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Jump;

public class LandingBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private CameraManager _cameraManager;

	private bool _setSlidingEffects;

	private readonly int _hardLandingHashAnim = Animator.StringToHash("HardLanding");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		if (_cameraManager == null)
		{
			_cameraManager = Core.Logic.CameraManager;
		}
		_setSlidingEffects = false;
		if (_penitent.IsFallingStunt)
		{
			_penitent.AnimatorInyector.ResetStuntByFall();
			_cameraManager.ProCamera2DShake.ShakeUsingPreset("HardFall");
			_penitent.GetComponentInChildren<Gameplay.GameControllers.Penitent.Abilities.RangeAttack>().enabled = false;
			animator.Play(_hardLandingHashAnim, 0, 0f);
		}
		else
		{
			_penitent.Audio.PlayLandingSound();
		}
		_penitent.GrabLadder.EnableClimbLadderAbility(enable: false);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent.Status.IsGrounded && _penitent.ReachBottonLadder && !_setSlidingEffects && _penitent.IsGrabbingLadder)
		{
			_setSlidingEffects = true;
			Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("LadderLanding");
			_penitent.PenitentMoveAnimations.PlaySlidingLadderLanding();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_penitent.GrabLadder.EnableClimbLadderAbility();
	}
}
