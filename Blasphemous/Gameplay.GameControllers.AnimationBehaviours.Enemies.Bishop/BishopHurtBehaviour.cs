using Gameplay.GameControllers.Enemies.Bishop;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bishop;

public class BishopHurtBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.Bishop.Bishop Bishop { get; private set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Bishop == null)
		{
			Bishop = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Bishop.Bishop>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		animator.ResetTrigger("HURT");
		Bishop.EnemyBehaviour.StartBehaviour();
	}
}
