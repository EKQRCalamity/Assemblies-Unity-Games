using Gameplay.GameControllers.Enemies.ReekLeader;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.ReekLeader;

public class ReekLeaderDeathBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.ReekLeader.ReekLeader ReekLeader { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (ReekLeader == null)
		{
			ReekLeader = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.ReekLeader.ReekLeader>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Object.Destroy(ReekLeader.gameObject);
	}
}
