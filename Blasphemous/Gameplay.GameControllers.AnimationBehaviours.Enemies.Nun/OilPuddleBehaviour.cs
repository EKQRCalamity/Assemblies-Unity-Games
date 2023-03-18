using Gameplay.GameControllers.Enemies.Nun.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Nun;

public class OilPuddleBehaviour : StateMachineBehaviour
{
	private OilPuddle _oilPuddle;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_oilPuddle == null)
		{
			_oilPuddle = animator.GetComponentInParent<OilPuddle>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_oilPuddle.Dissappear();
	}
}
