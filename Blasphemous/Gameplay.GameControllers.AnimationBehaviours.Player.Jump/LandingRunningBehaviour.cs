using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Jump;

public class LandingRunningBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private bool _setSlidingEffect;

	private readonly int _landingAnim = Animator.StringToHash("Landing");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		if (_penitent.IsFallingStunt)
		{
			animator.Play(_landingAnim);
		}
		else
		{
			_penitent.PenitentMoveAnimations.PlayLandingRunning();
		}
		if (_penitent.PlatformCharacterController.CurrentClimbingCollider != null)
		{
			_penitent.PlatformCharacterController.CurrentClimbingCollider = null;
		}
		_penitent.GrabLadder.EnableClimbLadderAbility(enable: false);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_penitent.GrabLadder.EnableClimbLadderAbility();
	}
}
