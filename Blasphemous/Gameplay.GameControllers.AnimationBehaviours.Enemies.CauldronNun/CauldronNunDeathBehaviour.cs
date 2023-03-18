using Gameplay.GameControllers.Enemies.CauldronNun;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.CauldronNun;

public class CauldronNunDeathBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.CauldronNun.CauldronNun CauldronNun { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (CauldronNun == null)
		{
			CauldronNun = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.CauldronNun.CauldronNun>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Object.Destroy(CauldronNun.gameObject);
	}
}
