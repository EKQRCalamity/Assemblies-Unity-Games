using Gameplay.GameControllers.Enemies.ShieldMaiden;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.ShieldMaiden;

public class ShieldMaidenDeathBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Gameplay.GameControllers.Enemies.ShieldMaiden.ShieldMaiden componentInParent = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.ShieldMaiden.ShieldMaiden>();
		if (componentInParent != null)
		{
			Object.Destroy(componentInParent.gameObject);
		}
	}
}
