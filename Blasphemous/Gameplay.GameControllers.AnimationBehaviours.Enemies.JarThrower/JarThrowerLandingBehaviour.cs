using Gameplay.GameControllers.Enemies.JarThrower;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.JarThrower;

public class JarThrowerLandingBehaviour : StateMachineBehaviour
{
	protected Gameplay.GameControllers.Enemies.JarThrower.JarThrower Jarthrower;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Jarthrower == null)
		{
			Jarthrower = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.JarThrower.JarThrower>();
		}
		Jarthrower.IsRunLanding = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Jarthrower.IsRunLanding = false;
	}
}
