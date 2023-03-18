using UnityEngine;

namespace Tools.Util;

public class RandomFrame : StateMachineBehaviour
{
	private int previousState;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		previousState = stateInfo.fullPathHash;
		if (stateInfo.fullPathHash != previousState)
		{
			animator.Play(stateInfo.fullPathHash, 0, Random.Range(0f, 1f));
		}
	}
}
