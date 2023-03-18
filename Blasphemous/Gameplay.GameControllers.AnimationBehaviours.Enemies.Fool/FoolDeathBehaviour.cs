using Gameplay.GameControllers.Enemies.Fool;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Fool;

public class FoolDeathBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.Fool.Fool Fool { get; set; }

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (Fool == null)
		{
			Fool = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Fool.Fool>();
		}
		Object.Destroy(Fool.gameObject);
	}
}
