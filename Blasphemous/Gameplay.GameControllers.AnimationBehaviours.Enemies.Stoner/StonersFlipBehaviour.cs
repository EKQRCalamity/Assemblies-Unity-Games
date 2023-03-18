using Gameplay.GameControllers.Enemies.Stoners;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Stoner;

public class StonersFlipBehaviour : StateMachineBehaviour
{
	private Stoners _stoners;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_stoners == null)
		{
			_stoners = animator.GetComponentInParent<Stoners>();
		}
		_stoners.AnimatorInyector.AllowOrientation(allow: false);
	}
}
