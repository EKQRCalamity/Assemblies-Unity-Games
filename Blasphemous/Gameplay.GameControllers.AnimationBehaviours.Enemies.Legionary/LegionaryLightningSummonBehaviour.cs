using Gameplay.GameControllers.Enemies.Legionary;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Legionary;

public class LegionaryLightningSummonBehaviour : StateMachineBehaviour
{
	protected Gameplay.GameControllers.Enemies.Legionary.Legionary Legionary { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Legionary == null)
		{
			Legionary = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Legionary.Legionary>();
		}
		Legionary.IsAttacking = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Legionary.IsAttacking = false;
		Legionary.Behaviour.ResetHitsCounter();
	}
}
