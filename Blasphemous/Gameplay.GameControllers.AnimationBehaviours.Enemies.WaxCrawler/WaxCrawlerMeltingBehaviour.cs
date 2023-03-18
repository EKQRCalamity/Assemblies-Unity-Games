using Gameplay.GameControllers.Enemies.WaxCrawler;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.WaxCrawler;

public class WaxCrawlerMeltingBehaviour : StateMachineBehaviour
{
	private bool _goToOrigin;

	private Gameplay.GameControllers.Enemies.WaxCrawler.WaxCrawler _waxCrawler;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_waxCrawler == null)
		{
			_waxCrawler = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.WaxCrawler.WaxCrawler>();
		}
		_goToOrigin = false;
		_waxCrawler.Behaviour.Melting = true;
		bool flag = animator.GetCurrentAnimatorStateInfo(0).IsName("Appear");
		_waxCrawler.DamageArea.DamageAreaCollider.enabled = flag;
		_waxCrawler.CanBeAttacked = flag;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (stateInfo.normalizedTime > 0.95f && animator.GetCurrentAnimatorStateInfo(0).IsName("Disappear") && !_goToOrigin)
		{
			_waxCrawler.Behaviour.Asleep();
			_goToOrigin = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (!(_waxCrawler == null))
		{
			_waxCrawler.Behaviour.Melting = false;
		}
	}
}
