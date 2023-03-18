using Gameplay.GameControllers.Bosses.BejeweledSaint.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.BejeweledBoss;

public class BsSmashHandBehaviour : StateMachineBehaviour
{
	private BejeweledSmashHand _smashHand;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_smashHand == null)
		{
			_smashHand = animator.GetComponent<BejeweledSmashHand>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_smashHand.IsRaised = false;
		_smashHand.Disappear();
	}
}
