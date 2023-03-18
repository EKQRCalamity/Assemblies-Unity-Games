using Gameplay.GameControllers.Enemies.Nun;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Nun;

public class NunDeathBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Gameplay.GameControllers.Enemies.Nun.Nun componentInParent = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Nun.Nun>();
		if (componentInParent != null)
		{
			Object.Destroy(componentInParent.gameObject);
		}
	}
}
