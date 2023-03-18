using Gameplay.GameControllers.Bosses.PietyMonster.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.Pietat.PietyRoot;

public class PietyRootRiseFallBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Bosses.PietyMonster.Attack.PietyRoot _pietyRoot;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_pietyRoot == null)
		{
			_pietyRoot = animator.GetComponent<Gameplay.GameControllers.Bosses.PietyMonster.Attack.PietyRoot>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (stateInfo.normalizedTime > 0.9f && _pietyRoot.gameObject.activeSelf)
		{
			_pietyRoot.Manager.StoreRoot(_pietyRoot.gameObject);
			_pietyRoot.gameObject.SetActive(value: false);
		}
	}
}
