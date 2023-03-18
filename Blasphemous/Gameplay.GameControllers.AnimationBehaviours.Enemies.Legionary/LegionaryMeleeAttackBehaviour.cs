using Gameplay.GameControllers.Enemies.Legionary;
using Gameplay.GameControllers.Enemies.Legionary.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Legionary;

public class LegionaryMeleeAttackBehaviour : StateMachineBehaviour
{
	protected Gameplay.GameControllers.Enemies.Legionary.Legionary Legionary { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Legionary == null)
		{
			Legionary = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Legionary.Legionary>();
		}
		if ((bool)Legionary)
		{
			Legionary.IsAttacking = true;
			Legionary.Behaviour.ResetHitsCounter();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if ((bool)Legionary)
		{
			StopSpinningAttackAudio();
			Legionary.IsAttacking = false;
			Legionary.Behaviour.LookAtTarget(Legionary.Target.transform.position);
		}
	}

	private void StopSpinningAttackAudio()
	{
		LegionaryAudio componentInChildren = Legionary.GetComponentInChildren<LegionaryAudio>();
		if ((bool)componentInChildren)
		{
			componentInChildren.StopSlideAttack_AUDIO();
		}
	}
}
