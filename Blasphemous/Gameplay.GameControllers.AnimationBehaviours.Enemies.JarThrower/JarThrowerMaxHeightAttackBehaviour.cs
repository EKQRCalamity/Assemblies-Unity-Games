using Gameplay.GameControllers.Enemies.JarThrower;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.JarThrower;

public class JarThrowerMaxHeightAttackBehaviour : StateMachineBehaviour
{
	protected Gameplay.GameControllers.Enemies.JarThrower.JarThrower JarThrower;

	protected float DefaultGravityScale;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (JarThrower == null)
		{
			JarThrower = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.JarThrower.JarThrower>();
		}
		JarThrower.Controller.PlatformCharacterPhysics.VSpeed = 0f;
		DefaultGravityScale = JarThrower.Controller.PlatformCharacterPhysics.GravityScale;
		JarThrower.Controller.PlatformCharacterPhysics.GravityScale = 0f;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		JarThrower.Controller.PlatformCharacterPhysics.GravityScale = DefaultGravityScale;
	}
}
