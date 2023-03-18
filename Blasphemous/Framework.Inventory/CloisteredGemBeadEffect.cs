using System;
using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Animator;
using Gameplay.GameControllers.Penitent.Attack;
using UnityEngine;

namespace Framework.Inventory;

public class CloisteredGemBeadEffect : ObjectEffect
{
	public int DamageAmount;

	public int MaxUses = 3;

	private Penitent penitent;

	private CloisteredGemProjectileAttack projectileAttack;

	private Vector2 direction;

	private Vector2 offset;

	private Vector3 rotation;

	private int currentUses;

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	protected override bool OnApplyEffect()
	{
		penitent = Core.Logic.Penitent;
		if (!projectileAttack)
		{
			projectileAttack = penitent.GetComponentInChildren<CloisteredGemProjectileAttack>();
		}
		projectileAttack.enabled = true;
		AnimatorInyector animatorInyector = penitent.AnimatorInyector;
		animatorInyector.OnAttack = (Core.SimpleEvent)Delegate.Remove(animatorInyector.OnAttack, new Core.SimpleEvent(OnAttack));
		AnimatorInyector animatorInyector2 = penitent.AnimatorInyector;
		animatorInyector2.OnAttack = (Core.SimpleEvent)Delegate.Combine(animatorInyector2.OnAttack, new Core.SimpleEvent(OnAttack));
		currentUses = 0;
		return true;
	}

	private void SetProjectileDirection()
	{
		if (penitent.IsCrouched)
		{
			direction = Vector2.right * ((penitent.Status.Orientation == EntityOrientation.Right) ? 1 : (-1)) + Vector2.down * 0.33f;
			offset = ((penitent.Status.Orientation != 0) ? new Vector2(-2f, -0.33f) : new Vector2(2f, -0.33f));
			rotation = new Vector3(0f, 0f, (penitent.Status.Orientation != 0) ? 30f : (-30f));
		}
		else if (penitent.PlatformCharacterInput.isJoystickUp)
		{
			direction = Vector2.up + Vector2.right * ((penitent.Status.Orientation != 0) ? (-0.001f) : 0.001f);
			offset = ((penitent.Status.Orientation != 0) ? new Vector2(-0.3f, 2.5f) : new Vector2(0.3f, 2.5f));
			rotation = new Vector3(0f, 180f, (penitent.Status.Orientation != 0) ? 90f : (-90f));
		}
		else
		{
			direction = Vector2.right * ((penitent.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
			offset = ((penitent.Status.Orientation != 0) ? new Vector2(-1.8f, 1.2f) : new Vector2(1.8f, 1.2f));
			rotation = new Vector3(0f, 0f, 0f);
		}
	}

	private void OnAttack()
	{
		SetProjectileDirection();
		StartCoroutine(WaitAndShoot(0.2f));
		if (currentUses >= MaxUses)
		{
			StopEffect();
		}
	}

	private IEnumerator WaitAndShoot(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		if (!penitent.Status.Dead && (bool)projectileAttack)
		{
			currentUses++;
			projectileAttack.Shoot(direction, offset, rotation, DamageAmount);
		}
	}

	protected override void OnRemoveEffect()
	{
		if ((bool)penitent)
		{
			AnimatorInyector animatorInyector = penitent.AnimatorInyector;
			animatorInyector.OnAttack = (Core.SimpleEvent)Delegate.Remove(animatorInyector.OnAttack, new Core.SimpleEvent(OnAttack));
		}
		projectileAttack = null;
		base.OnRemoveEffect();
	}

	private void StopEffect()
	{
		currentUses = 0;
		InvObj.RemoveAllObjectEffets();
	}
}
