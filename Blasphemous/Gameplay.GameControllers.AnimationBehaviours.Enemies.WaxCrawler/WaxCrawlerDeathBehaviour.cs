using Gameplay.GameControllers.Enemies.WaxCrawler;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.WaxCrawler;

public class WaxCrawlerDeathBehaviour : StateMachineBehaviour
{
	private bool _isDestroy;

	private Gameplay.GameControllers.Enemies.WaxCrawler.WaxCrawler _waxCrawler;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_waxCrawler == null)
		{
			_waxCrawler = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.WaxCrawler.WaxCrawler>();
		}
		_waxCrawler.Status.IsHurt = true;
		_isDestroy = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (stateInfo.normalizedTime >= 0.9f && !_isDestroy)
		{
			_isDestroy = true;
			Object.Destroy(_waxCrawler.gameObject);
		}
	}
}
