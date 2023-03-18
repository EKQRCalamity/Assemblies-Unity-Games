using Gameplay.GameControllers.Enemies.Legionary;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Legionary;

public class LegionaryHurtBehaviour : StateMachineBehaviour
{
	protected Gameplay.GameControllers.Enemies.Legionary.Legionary Legionary { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Legionary == null)
		{
			Legionary = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Legionary.Legionary>();
		}
		Legionary.Behaviour.IsHurt = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Legionary.Behaviour.IsHurt = false;
	}
}
