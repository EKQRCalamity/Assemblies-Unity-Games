using Gameplay.GameControllers.Enemies.MeltedLady;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.MeltedLady;

public class InkLadyIdleBehaviour : StateMachineBehaviour
{
	public InkLady InkLady { get; private set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (InkLady == null)
		{
			InkLady = animator.GetComponentInParent<InkLady>();
		}
		if (InkLady.IsAttacking && InkLady.Behaviour.CurrentAttackAmount > 0)
		{
			InkLady.Behaviour.CurrentAttackAmount--;
			InkLady.AnimatorInyector.Attack();
		}
		else
		{
			InkLady.Behaviour.CurrentAttackAmount = InkLady.Behaviour.AttackAmount;
			InkLady.Behaviour.CanTeleport = true;
		}
	}
}
