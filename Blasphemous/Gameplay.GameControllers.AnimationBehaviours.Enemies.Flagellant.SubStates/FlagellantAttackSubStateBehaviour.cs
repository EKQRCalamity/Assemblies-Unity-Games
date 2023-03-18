using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Flagellant.SubStates;

public class FlagellantAttackSubStateBehaviour : StateMachineBehaviour
{
	private Entity _flagellant;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_flagellant == null)
		{
			_flagellant = animator.GetComponentInParent<Entity>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_flagellant.EntityAttack.IsWeaponBlowingUp = false;
	}
}
