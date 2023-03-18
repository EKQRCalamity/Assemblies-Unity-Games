using Gameplay.GameControllers.Bosses.PietyMonster;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.Pietat;

public class PietatDeathBreathingBehaviour : StateMachineBehaviour
{
	private PietyMonster _pietyMonster;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_pietyMonster == null)
		{
			_pietyMonster = animator.GetComponentInParent<PietyMonster>();
		}
		_pietyMonster.DamageArea.DamageAreaCollider.enabled = false;
		_pietyMonster.BodyBarrier.gameObject.SetActive(value: false);
	}
}
