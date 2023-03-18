using Gameplay.GameControllers.Bosses.BurntFace;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.BurntFace;

public class BurntFaceDeathBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Gameplay.GameControllers.Bosses.BurntFace.BurntFace componentInParent = animator.GetComponentInParent<Gameplay.GameControllers.Bosses.BurntFace.BurntFace>();
		if (componentInParent != null)
		{
			Object.Destroy(componentInParent.gameObject);
		}
	}
}
