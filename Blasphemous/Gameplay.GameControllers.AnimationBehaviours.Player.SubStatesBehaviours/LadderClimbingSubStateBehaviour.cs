using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.SubStatesBehaviours;

public class LadderClimbingSubStateBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		if (_penitent.IsFallingStunt)
		{
			_penitent.IsFallingStunt = !_penitent.IsFallingStunt;
		}
		_penitent.PlatformCharacterController.PlatformCharacterPhysics.VSpeed = 0f;
		_penitent.PlatformCharacterController.InstantVelocity = Vector3.zero;
		animator.SetBool("AIR_ATTACKING", value: false);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.DamageArea.IncludeEnemyLayer(include: false);
		_penitent.PlatformCharacterController.PlatformCharacterPhysics.Acceleration = Vector3.zero;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.DamageArea.IncludeEnemyLayer();
	}
}
