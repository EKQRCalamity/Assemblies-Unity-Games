using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.FlyingPortrait;

public class FlyingPortraitAttackBehaviour : StateMachineBehaviour
{
	private Enemy _flyingPortrait;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_flyingPortrait == null)
		{
			_flyingPortrait = animator.GetComponentInParent<Enemy>();
		}
		_flyingPortrait.IsAttacking = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_flyingPortrait.IsAttacking = false;
	}
}
