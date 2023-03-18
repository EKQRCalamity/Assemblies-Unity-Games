using System.Collections;
using Framework.FrameworkCore;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.Traits;

public class EnemyHitStun : Trait
{
	private Animator entityAnimator;

	private Coroutine hitStunCoroutine;

	public bool onHitStun;

	public float hitStunAmount = 0.25f;

	protected override void OnTraitEnable()
	{
		base.OnTraitEnable();
		if ((bool)base.EntityOwner)
		{
			entityAnimator = base.EntityOwner.Animator;
			base.EntityOwner.OnDamaged -= EntityOwner_OnDamaged;
			base.EntityOwner.OnDamaged += EntityOwner_OnDamaged;
		}
	}

	private void EntityOwner_OnDamaged()
	{
		OnDamage();
	}

	public void OnDamage()
	{
		if (hitStunCoroutine != null)
		{
			StopCoroutine(hitStunCoroutine);
		}
		hitStunCoroutine = StartCoroutine(ActivateHitStun(hitStunAmount));
	}

	private IEnumerator ActivateHitStun(float seconds)
	{
		onHitStun = true;
		SetAnimatorSpeed(0f);
		yield return new WaitForSeconds(seconds);
		onHitStun = false;
		SetAnimatorSpeed(1f);
	}

	private void SetAnimatorSpeed(float spd)
	{
		entityAnimator.speed = spd;
	}
}
