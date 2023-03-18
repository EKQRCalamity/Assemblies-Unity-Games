using Gameplay.GameControllers.Bosses.PietyMonster;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.Pietat.Movement;

public class PietyWalkBehaviour : StateMachineBehaviour
{
	private PietyMonster _pietyMonster;

	private bool _isFirstLoop;

	private readonly int _walkToTurn = Animator.StringToHash("WalkToTurnAround");

	private readonly int _walkToIdle1 = Animator.StringToHash("WalkToIdle_1");

	private readonly int _walkToIdle2 = Animator.StringToHash("WalkToIdle_2");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_pietyMonster == null)
		{
			_pietyMonster = animator.GetComponentInParent<PietyMonster>();
		}
		_pietyMonster.CanMove = true;
		_isFirstLoop = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (!_isFirstLoop)
		{
			_isFirstLoop = true;
			_pietyMonster.AnimatorBridge.AllowWalkCameraShake = true;
		}
		bool flag = !animator.GetBool("WALK");
		bool @bool = animator.GetBool("TURN_AROUND");
		if (@bool)
		{
			if (_pietyMonster.CanMove)
			{
				_pietyMonster.CanMove = false;
			}
			animator.Play(_walkToTurn);
		}
		if (stateInfo.normalizedTime <= 0.65f)
		{
			if (flag && !@bool)
			{
				if (_pietyMonster.CanMove)
				{
					_pietyMonster.CanMove = false;
				}
				animator.Play(_walkToIdle1);
			}
		}
		else if (flag && !@bool)
		{
			if (_pietyMonster.CanMove)
			{
				_pietyMonster.CanMove = false;
			}
			animator.Play(_walkToIdle2);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_pietyMonster.CanMove = false;
		_pietyMonster.AnimatorBridge.AllowWalkCameraShake = false;
	}
}
