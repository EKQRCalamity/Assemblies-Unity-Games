using Gameplay.GameControllers.Bosses.Quirce;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Quirce;

public class QuirceDeathBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Gameplay.GameControllers.Bosses.Quirce.Quirce componentInParent = animator.GetComponentInParent<Gameplay.GameControllers.Bosses.Quirce.Quirce>();
		if (componentInParent != null)
		{
			Object.Destroy(componentInParent.gameObject);
		}
	}
}
