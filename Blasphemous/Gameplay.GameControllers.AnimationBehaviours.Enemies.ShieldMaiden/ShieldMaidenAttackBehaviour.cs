using Gameplay.GameControllers.Enemies.ShieldMaiden;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.ShieldMaiden;

public class ShieldMaidenAttackBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.ShieldMaiden.ShieldMaiden ShieldMaiden { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (ShieldMaiden == null)
		{
			ShieldMaiden = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.ShieldMaiden.ShieldMaiden>();
		}
		ShieldMaiden.Behaviour.ToggleShield(active: false);
		ShieldMaiden.IsAttacking = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		ShieldMaiden.IsAttacking = false;
	}
}
