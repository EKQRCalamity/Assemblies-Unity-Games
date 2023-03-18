using Gameplay.GameControllers.Enemies.ChasingHead;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.ChasingHead;

public class ChasingHeadHurtBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.ChasingHead.ChasingHead _chasingHead;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_chasingHead == null)
		{
			_chasingHead = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.ChasingHead.ChasingHead>();
		}
		_chasingHead.Status.IsHurt = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_chasingHead.Status.IsHurt = false;
	}
}
