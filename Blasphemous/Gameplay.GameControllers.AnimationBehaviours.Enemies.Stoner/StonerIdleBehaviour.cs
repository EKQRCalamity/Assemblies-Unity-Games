using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Stoners;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Stoner;

public class StonerIdleBehaviour : StateMachineBehaviour
{
	private Stoners _stoner;

	public EntityOrientation CurrentAnimationOrientation;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_stoner == null)
		{
			_stoner = animator.GetComponentInParent<Stoners>();
		}
	}
}
