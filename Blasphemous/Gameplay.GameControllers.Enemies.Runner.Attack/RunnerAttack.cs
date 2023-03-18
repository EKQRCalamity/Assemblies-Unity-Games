using System;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Runner.Attack;

public class RunnerAttack : EnemyAttack
{
	private float contactDamageCoolDown = 1f;

	private IDamageable damageableTarget;

	private bool onSuscribeEvents;

	public ContactDamage ContactDamage { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		ContactDamage = base.EntityOwner.GetComponentInChildren<ContactDamage>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		ContactDamage contactDamage = ContactDamage;
		contactDamage.OnContactDamage = (Core.GenericEvent)Delegate.Combine(contactDamage.OnContactDamage, new Core.GenericEvent(OnContactDamage));
		base.EntityOwner.OnDeath += OnDeath;
		onSuscribeEvents = true;
	}

	private void OnContactDamage(UnityEngine.Object param)
	{
		damageableTarget = (IDamageable)param;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!base.EntityOwner || base.EntityOwner.Dead)
		{
			return;
		}
		if (ContactDamage.IsTargetOverlapped && damageableTarget != null)
		{
			contactDamageCoolDown -= Time.deltaTime;
			if (contactDamageCoolDown <= 0f)
			{
				contactDamageCoolDown = 1f;
				ContactAttack(damageableTarget);
			}
		}
		else
		{
			contactDamageCoolDown = 1f;
		}
	}

	private void OnDeath()
	{
		UnsubscribeEvents();
	}

	private void OnDestroy()
	{
		UnsubscribeEvents();
	}

	private void UnsubscribeEvents()
	{
		if (onSuscribeEvents)
		{
			onSuscribeEvents = false;
			if ((bool)ContactDamage)
			{
				ContactDamage contactDamage = ContactDamage;
				contactDamage.OnContactDamage = (Core.GenericEvent)Delegate.Remove(contactDamage.OnContactDamage, new Core.GenericEvent(OnContactDamage));
			}
			if ((bool)base.EntityOwner)
			{
				base.EntityOwner.OnDeath -= OnDeath;
			}
		}
	}
}
