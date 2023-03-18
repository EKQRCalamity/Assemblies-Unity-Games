using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Stoners;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Stoner;

public class StonersAttackBehaviour : StateMachineBehaviour
{
	private Stoners _stoner;

	public EntityOrientation CurrentAnimationOrientation;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_stoner == null)
		{
			_stoner = animator.GetComponentInParent<Stoners>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		_stoner.AnimatorInyector.AllowOrientation(CurrentAnimationOrientation != _stoner.Status.Orientation);
	}
}
