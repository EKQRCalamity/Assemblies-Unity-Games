using Gameplay.GameControllers.Enemies.Flagellant;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Flagellant;

public class FlagellantDeathBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Flagellant.Flagellant flagellant;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (flagellant == null)
		{
			flagellant = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Flagellant.Flagellant>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime >= 0.5f)
		{
			EntityShadow componentInChildren = flagellant.GetComponentInChildren<EntityShadow>();
			if (componentInChildren != null)
			{
				componentInChildren.RemoveBlobShadow();
			}
		}
		if (stateInfo.normalizedTime >= 0.95f)
		{
			Object.Destroy(flagellant.gameObject);
		}
	}
}
