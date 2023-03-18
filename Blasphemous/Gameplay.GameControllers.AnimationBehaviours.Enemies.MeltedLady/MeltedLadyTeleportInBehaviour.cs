using Gameplay.GameControllers.Enemies.MeltedLady;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.MeltedLady;

public class MeltedLadyTeleportInBehaviour : StateMachineBehaviour
{
	public FloatingLady MeltedLady { get; private set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (MeltedLady == null)
		{
			MeltedLady = animator.GetComponentInParent<FloatingLady>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		MeltedLady.IsAttacking = !MeltedLady.Behaviour.IsInOrigin;
		MeltedLady.DamageArea.DamageAreaCollider.enabled = true;
		MeltedLady.DamageByContact = true;
		if (MeltedLady.Status.IsHurt)
		{
			MeltedLady.Status.IsHurt = false;
			MeltedLady.IsAttacking = true;
			MeltedLady.Behaviour.ResetAttackCounter();
		}
	}
}
