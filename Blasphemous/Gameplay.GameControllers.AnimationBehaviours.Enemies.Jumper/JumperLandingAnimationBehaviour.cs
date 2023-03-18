using Gameplay.GameControllers.Enemies.Jumper;
using Gameplay.GameControllers.Enemies.Jumper.AI;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Jumper;

public class JumperLandingAnimationBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.Jumper.Jumper Jumper { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Jumper == null)
		{
			Jumper = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Jumper.Jumper>();
		}
		JumperBehaviour jumperBehaviour = (JumperBehaviour)Jumper.EnemyBehaviour;
		jumperBehaviour.StopMovement();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		JumperBehaviour jumperBehaviour = (JumperBehaviour)Jumper.EnemyBehaviour;
		jumperBehaviour.LookAtTarget(Jumper.Target.transform.position);
		jumperBehaviour.IsJumping = false;
	}
}
