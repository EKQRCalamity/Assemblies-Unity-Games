using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Runner;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Runner;

public class RunnerTurnAroundBehaviour : StateMachineBehaviour
{
	private bool _turned;

	private bool _throwScream;

	public Gameplay.GameControllers.Enemies.Runner.Runner Runner { get; private set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Runner == null)
		{
			Runner = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Runner.Runner>();
		}
		Runner.Behaviour.TurningAround = true;
		_turned = false;
		_throwScream = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (stateInfo.normalizedTime > 0.5f)
		{
			SetOrientation();
		}
		if (stateInfo.normalizedTime > 0.9f)
		{
			ThrowScream();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Runner.Behaviour.TurningAround = false;
		animator.SetBool("TURNING", value: false);
		Runner.SpriteRenderer.flipX = Runner.Status.Orientation == EntityOrientation.Left;
	}

	private void SetOrientation()
	{
		if (!_turned)
		{
			_turned = true;
			EntityOrientation orientation = Runner.Status.Orientation;
			Runner.SetOrientation((orientation != EntityOrientation.Left) ? EntityOrientation.Left : EntityOrientation.Right, allowFlipRenderer: false);
		}
	}

	private void ThrowScream()
	{
		if (!_throwScream)
		{
			_throwScream = true;
			if (Random.value <= 0.3f)
			{
				_throwScream = true;
				Runner.Behaviour.Scream();
			}
		}
	}
}
