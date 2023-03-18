using Gameplay.GameControllers.Enemies.TrinityMinion;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.TrinityMinion;

public class TrinityMinionDeathBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.TrinityMinion.TrinityMinion _TrinityMinion;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_TrinityMinion == null)
		{
			_TrinityMinion = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.TrinityMinion.TrinityMinion>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if ((double)stateInfo.normalizedTime > 0.9)
		{
			Object.Destroy(_TrinityMinion.gameObject);
		}
	}
}
