using Gameplay.GameControllers.Enemies.NewFlagellant;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.NewFlagellant;

public class NewFlagellantAttackBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.NewFlagellant.NewFlagellant _NewFlagellant;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_NewFlagellant == null)
		{
			_NewFlagellant = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.NewFlagellant.NewFlagellant>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!(_NewFlagellant == null) && !_NewFlagellant.IsAttacking)
		{
			_NewFlagellant.IsAttacking = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!(_NewFlagellant == null))
		{
			if (_NewFlagellant.IsAttacking)
			{
				_NewFlagellant.IsAttacking = !_NewFlagellant.IsAttacking;
			}
			_NewFlagellant.AnimatorInyector.AttackAnimationFinished();
		}
	}
}
