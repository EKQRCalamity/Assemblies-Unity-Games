using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.RangeAttack;

public class GroundRangeAttackBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private Gameplay.GameControllers.Penitent.Abilities.RangeAttack _rangeAttack;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_rangeAttack = _penitent.GetComponentInChildren<Gameplay.GameControllers.Penitent.Abilities.RangeAttack>();
		_penitent.PlatformCharacterController.PlatformCharacterPhysics.HSpeed = 0f;
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		_penitent.PenitentAttack.IsRangeAttacking = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		if (_rangeAttack != null)
		{
			_rangeAttack.StopCastRangeAttack();
		}
		_penitent.PenitentAttack.IsRangeAttacking = false;
	}
}
