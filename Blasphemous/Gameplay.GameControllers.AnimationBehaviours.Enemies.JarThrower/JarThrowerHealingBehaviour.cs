using Gameplay.GameControllers.Enemies.JarThrower;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.JarThrower;

public class JarThrowerHealingBehaviour : StateMachineBehaviour
{
	protected Gameplay.GameControllers.Enemies.JarThrower.JarThrower JarThrower { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (JarThrower == null)
		{
			JarThrower = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.JarThrower.JarThrower>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		JarThrower.Behaviour.IsHealing = false;
	}
}
