using Gameplay.GameControllers.Enemies.Acolyte;
using Gameplay.GameControllers.Enemies.Acolyte.Animator;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Acolyte.Hurt;

public class AcolyteDeathEdgeBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Acolyte.Acolyte acolyte;

	private AcolyteAttackAnimations acolyteAttackAnimations;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (acolyte == null)
		{
			acolyte = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Acolyte.Acolyte>();
			acolyteAttackAnimations = acolyte.GetComponent<AcolyteAttackAnimations>();
		}
		if (acolyteAttackAnimations != null)
		{
			acolyte.Audio.PlayDeathOnCliffLede();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime >= 0.95f && acolyte.Stats.Life.Current <= 0f)
		{
			animator.speed = 0f;
		}
	}
}
