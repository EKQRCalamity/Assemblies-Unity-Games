using Gameplay.GameControllers.Enemies.MeltedLady;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.MeltedLady;

public class MeltedLadyIdleBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.MeltedLady.MeltedLady MeltedLady { get; private set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (MeltedLady == null)
		{
			MeltedLady = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.MeltedLady.MeltedLady>();
		}
		if (MeltedLady.IsAttacking && MeltedLady.Behaviour.CurrentAttackAmount > 0)
		{
			MeltedLady.Behaviour.CurrentAttackAmount--;
			MeltedLady.AnimatorInyector.Attack();
		}
		else
		{
			MeltedLady.Behaviour.CurrentAttackAmount = MeltedLady.Behaviour.AttackAmount;
			MeltedLady.Behaviour.CanTeleport = true;
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
	}
}
