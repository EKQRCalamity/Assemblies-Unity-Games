using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.RangeAttack;

public class MidAirRangeAttackBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private Gameplay.GameControllers.Penitent.Abilities.RangeAttack _rangeAttack;

	private Vector3 _accumulatedVelocity;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_rangeAttack = _penitent.GetComponentInChildren<Gameplay.GameControllers.Penitent.Abilities.RangeAttack>();
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		Vector3 velocity = _penitent.PlatformCharacterController.PlatformCharacterPhysics.Velocity;
		_accumulatedVelocity = new Vector3(velocity.x, 0f, velocity.z);
		_penitent.PlatformCharacterController.PlatformCharacterPhysics.Gravity = Vector3.zero;
		_penitent.PenitentAttack.IsRangeAttacking = true;
		_penitent.AnimatorInyector.ResetStuntByFall();
		_penitent.PlatformCharacterController.PlatformCharacterPhysics.VSpeed = 0f;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		_penitent.PlatformCharacterController.PlatformCharacterPhysics.Velocity = Vector3.zero;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_penitent.PlatformCharacterController.PlatformCharacterPhysics.Gravity = new Vector3(0f, -9.8f, 0f);
		_penitent.PlatformCharacterController.PlatformCharacterPhysics.Velocity = _accumulatedVelocity;
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		_penitent.PenitentAttack.IsRangeAttacking = false;
		if (_rangeAttack != null)
		{
			_rangeAttack.StopCastRangeAttack();
		}
	}
}
