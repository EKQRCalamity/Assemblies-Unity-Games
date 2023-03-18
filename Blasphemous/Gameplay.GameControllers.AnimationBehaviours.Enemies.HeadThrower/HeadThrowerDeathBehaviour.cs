using Gameplay.GameControllers.Enemies.HeadThrower;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.HeadThrower;

public class HeadThrowerDeathBehaviour : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		animator.GetComponentInParent<Gameplay.GameControllers.Enemies.HeadThrower.HeadThrower>().Audio.PlayDeath();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Enemy componentInParent = animator.GetComponentInParent<Enemy>();
		if (componentInParent != null)
		{
			Object.Destroy(componentInParent.gameObject);
		}
	}
}
