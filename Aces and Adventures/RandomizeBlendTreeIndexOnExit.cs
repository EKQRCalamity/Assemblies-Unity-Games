using UnityEngine;

public class RandomizeBlendTreeIndexOnExit : StateMachineBehaviour
{
	public string parameter;

	public int minIndex;

	public int maxIndex;

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetFloat(parameter, Random.Range(minIndex, maxIndex + 1));
	}
}
