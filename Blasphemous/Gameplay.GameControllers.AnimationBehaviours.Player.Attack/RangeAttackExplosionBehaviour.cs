using Gameplay.GameControllers.Penitent.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Attack;

public class RangeAttackExplosionBehaviour : StateMachineBehaviour
{
	private RangeAttackExplosion _rangeAttackExplosion;

	private bool attack;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_rangeAttackExplosion == null)
		{
			_rangeAttackExplosion = animator.GetComponent<RangeAttackExplosion>();
		}
		attack = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if ((double)stateInfo.length > 0.5 && !attack)
		{
			attack = true;
			_rangeAttackExplosion.Attack();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_rangeAttackExplosion.Dispose();
	}
}
