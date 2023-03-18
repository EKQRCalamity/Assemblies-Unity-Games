using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.ClimbClifLede;

public class ClimbCliffLedeBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.ResetTrigger("CLIMB_CLIFF_LEDGE");
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitent.IsGrabbingCliffLede = false;
		_penitent.Physics.EnableColliders(enable: false);
		_penitent.transform.position = GetRootMotionCliffPosition(_penitent.RootMotionDrive);
		_penitent.Status.Unattacable = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.cliffLedeClimbingStarted = false;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.Status.Unattacable = false;
		_penitent.IsClimbingCliffLede = false;
		_penitent.Physics.EnablePhysics();
		_penitent.Physics.EnableColliders();
	}

	private Vector3 GetRootMotionCliffPosition(Vector3 rootMotion)
	{
		Vector3 result = new Vector3(rootMotion.x, _penitent.GrabCliffLede.CliffColliderBoundaries.max.y - 1f / 32f, rootMotion.z);
		return result;
	}
}
