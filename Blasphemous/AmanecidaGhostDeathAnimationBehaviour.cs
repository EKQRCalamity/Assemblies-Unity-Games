using Gameplay.GameControllers.Enemies.Projectiles;
using UnityEngine;

public class AmanecidaGhostDeathAnimationBehaviour : StateMachineBehaviour
{
	private ParriableProjectile parriableProjectile;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (parriableProjectile == null)
		{
			parriableProjectile = animator.GetComponentInParent<ParriableProjectile>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime >= 0.9f)
		{
			parriableProjectile.OnDeathAnimation();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}
}
