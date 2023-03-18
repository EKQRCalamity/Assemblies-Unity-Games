using Gameplay.GameControllers.Bosses.PietyMonster;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.Pietat.Movement;

public class PietyIdleBehaviour : StateMachineBehaviour
{
	private PietyMonster _pietyMonster;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_pietyMonster == null)
		{
			_pietyMonster = animator.GetComponentInParent<PietyMonster>();
		}
		if (_pietyMonster.PietyBehaviour.StatusChange)
		{
			_pietyMonster.PietyBehaviour.StatusChange = false;
		}
	}
}
