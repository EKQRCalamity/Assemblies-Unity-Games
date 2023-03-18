using Gameplay.GameControllers.Enemies.CrossCrawler;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.CrossCrawler;

public class CrossCrawlerTurnAroundBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.CrossCrawler.CrossCrawler CrossCrawler { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (CrossCrawler == null)
		{
			CrossCrawler = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.CrossCrawler.CrossCrawler>();
		}
		CrossCrawler.Behaviour.TurningAround = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		CrossCrawler.Behaviour.TurningAround = false;
		CrossCrawler.SetOrientation(CrossCrawler.Status.Orientation);
	}
}
