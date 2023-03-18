using Gameplay.GameControllers.Enemies.GoldenCorpse;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.GoldenCorpse;

public class GoldenCorpseAwakenBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.GoldenCorpse.GoldenCorpse GoldenCorpse { get; set; }

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (GoldenCorpse == null)
		{
			GoldenCorpse = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.GoldenCorpse.GoldenCorpse>();
		}
		GoldenCorpse.Behaviour.OnAwakeAnimationFinished();
	}
}
