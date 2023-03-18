using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Bishop;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bishop;

public class BishopTurnBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Bishop.Bishop _bishop;

	private EntityOrientation _prevOrientation;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_bishop == null)
		{
			_bishop = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Bishop.Bishop>();
		}
		_prevOrientation = _bishop.Status.Orientation;
		_bishop.EnemyBehaviour.TurningAround = true;
		EntityOrientation orientation = ((_prevOrientation != EntityOrientation.Left) ? EntityOrientation.Left : EntityOrientation.Right);
		_bishop.SetOrientation(orientation, allowFlipRenderer: false);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_bishop.SetOrientation(_bishop.Status.Orientation);
		_bishop.EnemyBehaviour.TurningAround = false;
	}
}
