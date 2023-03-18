using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Stoners;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Stoner;

public class StonersDeathBehaviour : StateMachineBehaviour
{
	private Stoners _stoner;

	public EntityOrientation CurrentAnimationOrientation;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (!(_stoner != null))
		{
			_stoner = animator.GetComponentInParent<Stoners>();
		}
	}

	public static string GetProperDeathAnimationClip(EntityOrientation currentEntityOrientation)
	{
		return (currentEntityOrientation != EntityOrientation.Left) ? "DeathRight" : "DeathLeft";
	}
}
