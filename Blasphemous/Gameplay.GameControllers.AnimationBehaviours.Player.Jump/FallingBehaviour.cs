using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Jump;

public class FallingBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitent.cliffLedeClimbingStarted = false;
		_penitent.IsClimbingCliffLede = false;
		_penitent.Physics.EnableColliders();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.startedJumpOff = false;
		_penitent.IsJumpingOff = false;
		_penitent.PlatformCharacterInput.CancelPlatformDropDown();
	}
}
