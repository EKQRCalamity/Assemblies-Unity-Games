using System.Collections;
using Framework.Managers;
using Framework.Util;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.Pietat.ThornProjectile;

public class PietyProjectileDestroyBehaviour : StateMachineBehaviour
{
	private GameObject _projectile;

	private SpriteRenderer _renderer;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		_projectile = animator.transform.root.gameObject;
		_renderer = animator.GetComponent<SpriteRenderer>();
		if (!_renderer.enabled)
		{
			Singleton<Core>.Instance.StartCoroutine(InvokeDestruction());
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (stateInfo.normalizedTime > 0.95f && _renderer.enabled)
		{
			_renderer.enabled = false;
			Singleton<Core>.Instance.StartCoroutine(InvokeDestruction());
		}
	}

	private void DestroyProjectile()
	{
		if (_projectile != null)
		{
			Object.Destroy(_projectile);
		}
	}

	private IEnumerator InvokeDestruction()
	{
		yield return new WaitForSeconds(3f);
		DestroyProjectile();
	}
}
