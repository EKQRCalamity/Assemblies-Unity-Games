using Gameplay.GameControllers.Penitent.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Attack;

public class DefaultSwordSlashBehaviour : StateMachineBehaviour
{
	private SwordAnimatorInyector AnimatorInyector;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (AnimatorInyector == null)
		{
			AnimatorInyector = animator.GetComponent<SwordAnimatorInyector>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		AnimatorInyector.ResetParameters();
	}
}
