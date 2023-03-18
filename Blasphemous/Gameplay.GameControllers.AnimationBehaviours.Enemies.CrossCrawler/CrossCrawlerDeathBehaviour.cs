using Gameplay.GameControllers.Enemies.CrossCrawler;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.CrossCrawler;

public class CrossCrawlerDeathBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Gameplay.GameControllers.Enemies.CrossCrawler.CrossCrawler componentInParent = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.CrossCrawler.CrossCrawler>();
		if (componentInParent != null)
		{
			Object.Destroy(componentInParent.gameObject);
		}
	}
}
