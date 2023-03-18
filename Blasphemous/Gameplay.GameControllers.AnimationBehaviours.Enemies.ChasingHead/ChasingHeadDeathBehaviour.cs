using Gameplay.GameControllers.Enemies.ChasingHead;
using Gameplay.GameControllers.Enemies.HeadThrower.AI;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.ChasingHead;

public class ChasingHeadDeathBehaviour : StateMachineBehaviour
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
		if (_chasingHead.OwnHeadThrower != null)
		{
			_chasingHead.OwnHeadThrower.GetComponentInChildren<HeadThrowerBehaviour>().RemoveSpawnedHeadFromList(_chasingHead.gameObject);
		}
		Object.Destroy(animator.transform.root.gameObject);
	}
}
