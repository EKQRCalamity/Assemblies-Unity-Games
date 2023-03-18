using Gameplay.GameControllers.Bosses.ElderBrother;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.ElderBrother;

public class ElderBrotherDeathBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Gameplay.GameControllers.Bosses.ElderBrother.ElderBrother componentInParent = animator.GetComponentInParent<Gameplay.GameControllers.Bosses.ElderBrother.ElderBrother>();
		if (componentInParent != null)
		{
			Object.Destroy(componentInParent.gameObject);
		}
	}
}
