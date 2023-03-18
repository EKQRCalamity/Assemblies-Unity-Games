using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Stoners;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Stoner;

public class StonerHurtBehaviour : StateMachineBehaviour
{
	private Stoners _stoner;

	public EntityOrientation CurrentAnimationOrientation;

	private readonly int _hurtLeft = Animator.StringToHash("HurtLeft");

	private readonly int _hurtRight = Animator.StringToHash("HurtRight");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_stoner == null)
		{
			_stoner = animator.GetComponentInParent<Stoners>();
		}
		if (_stoner.Status.Orientation != CurrentAnimationOrientation)
		{
			int properHurtAnimationClip = GetProperHurtAnimationClip(_stoner.Status.Orientation);
			animator.Play(properHurtAnimationClip);
		}
	}

	private int GetProperHurtAnimationClip(EntityOrientation currentEntityOrientation)
	{
		return (currentEntityOrientation != EntityOrientation.Left) ? _hurtRight : _hurtLeft;
	}
}
