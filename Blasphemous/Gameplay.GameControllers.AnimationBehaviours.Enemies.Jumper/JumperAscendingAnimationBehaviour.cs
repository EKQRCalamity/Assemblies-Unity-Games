using Gameplay.GameControllers.Enemies.Jumper;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Jumper;

public class JumperAscendingAnimationBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.Jumper.Jumper Jumper { get; private set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Jumper == null)
		{
			Jumper = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Jumper.Jumper>();
		}
		if (Jumper.Attack.TargetInAttackArea)
		{
			Jumper.Attack.CurrentWeaponAttack();
		}
	}
}
