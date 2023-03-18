using Gameplay.GameControllers.Enemies.WaxCrawler;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.WaxCrawler;

public class WaxCrawlerWalkBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.WaxCrawler.WaxCrawler _waxCrawler;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_waxCrawler == null)
		{
			_waxCrawler = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.WaxCrawler.WaxCrawler>();
		}
		_waxCrawler.Audio.Walk();
		_waxCrawler.DamageArea.DamageAreaCollider.enabled = true;
		_waxCrawler.CanBeAttacked = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_waxCrawler.CanBeAttacked = false;
	}
}
