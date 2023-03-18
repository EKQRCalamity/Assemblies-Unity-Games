using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.ClimbLadder;

public class GrabLadderBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private readonly int _ladderGoingUpAnimHash = Animator.StringToHash("ladder_going_up");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitent.IsGrabbingLadder = true;
		_penitent.GrabLadder.SetClimbingSpeed(0f);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!_penitent.Status.IsGrounded)
		{
			animator.Play(_ladderGoingUpAnimHash);
		}
		if (stateInfo.normalizedTime >= 0.95f)
		{
			animator.Play(_ladderGoingUpAnimHash);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.IsClimbingLadder = true;
		_penitent.GrabLadder.SetClimbingSpeed(2.25f);
	}
}
