using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Hurt;

public class AirHurtBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitent.canClimbCliffLede = false;
		_penitent.cliffLedeClimbingStarted = false;
		if (animator.GetBool("STICK_ON_WALL"))
		{
			_penitent.GetComponentInChildren<WallJump>().enabled = false;
			_penitent.PlatformCharacterController.PlatformCharacterPhysics.Gravity = new Vector3(0f, -9.8f, 0f);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent.Status.Dead)
		{
			_penitent.IsDeadInAir = true;
		}
	}
}
