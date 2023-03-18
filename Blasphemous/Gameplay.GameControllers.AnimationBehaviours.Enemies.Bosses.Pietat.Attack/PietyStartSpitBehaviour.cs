using Gameplay.GameControllers.Bosses.PietyMonster;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.Pietat.Attack;

public class PietyStartSpitBehaviour : StateMachineBehaviour
{
	private PietyMonster _piety;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_piety == null)
		{
			_piety = animator.GetComponentInParent<PietyMonster>();
		}
		_piety.PietyBehaviour.SpitAttack.RefillSpitPositions();
	}
}
