using Gameplay.GameControllers.Enemies.AshCharger;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.AshCharger;

public class AshChargerDeathBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Gameplay.GameControllers.Enemies.AshCharger.AshCharger componentInParent = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.AshCharger.AshCharger>();
		if (componentInParent != null)
		{
			Object.Destroy(componentInParent.gameObject);
		}
	}
}
