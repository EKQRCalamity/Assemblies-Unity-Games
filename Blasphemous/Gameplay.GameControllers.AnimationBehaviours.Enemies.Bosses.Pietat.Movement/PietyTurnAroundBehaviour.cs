using Framework.FrameworkCore;
using Gameplay.GameControllers.Bosses.PietyMonster;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.Pietat.Movement;

public class PietyTurnAroundBehaviour : StateMachineBehaviour
{
	private PietyMonster _pietyMonster;

	private EntityOrientation _pietatCurrentOrientation;

	private EntityOrientation _newEntityOrientation;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_pietyMonster == null)
		{
			_pietyMonster = animator.GetComponentInParent<PietyMonster>();
		}
		if (!_pietyMonster.EnemyBehaviour.TurningAround)
		{
			_pietyMonster.EnemyBehaviour.TurningAround = true;
		}
		if (_pietyMonster.CanMove)
		{
			_pietyMonster.CanMove = false;
		}
		_pietatCurrentOrientation = _pietyMonster.Status.Orientation;
		_newEntityOrientation = ((_pietatCurrentOrientation != EntityOrientation.Left) ? EntityOrientation.Left : EntityOrientation.Right);
		_pietyMonster.SetOrientation(_newEntityOrientation, allowFlipRenderer: false);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (_pietyMonster.EnemyBehaviour.TurningAround)
		{
			_pietyMonster.EnemyBehaviour.TurningAround = false;
		}
		_pietyMonster.SetOrientation(_newEntityOrientation);
		animator.SetBool("TURN_AROUND", value: false);
	}
}
