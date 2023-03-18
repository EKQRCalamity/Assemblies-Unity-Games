using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

[RequireComponent(typeof(Collider2D))]
public class ContactDamage : Trait
{
	public Core.GenericEvent OnContactDamage;

	public LayerMask DamageableLayers;

	protected Collider2D ContactDamageCollider;

	protected Attack EntityAttack;

	public bool IsTargetOverlapped { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		ContactDamageCollider = GetComponent<Collider2D>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		if ((bool)base.EntityOwner)
		{
			EntityAttack = base.EntityOwner.EntityAttack;
		}
		CenterContactDamageCollider();
	}

	public void EntityContactDamage(IDamageable damageable)
	{
		if (!(EntityAttack == null))
		{
			EntityAttack.ContactAttack(damageable);
			if (OnContactDamage != null)
			{
				OnContactDamage(damageable as Object);
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if ((DamageableLayers.value & (1 << other.gameObject.layer)) <= 0)
		{
			return;
		}
		IsTargetOverlapped = true;
		Enemy enemy = base.EntityOwner as Enemy;
		if (!(enemy == null))
		{
			IDamageable componentInParent = other.GetComponentInParent<IDamageable>();
			if (!enemy.Status.Dead && !enemy.IsStunt && enemy.DamageByContact)
			{
				EntityContactDamage(componentInParent);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if ((DamageableLayers.value & (1 << other.gameObject.layer)) > 0)
		{
			IsTargetOverlapped = false;
		}
	}

	protected void CenterContactDamageCollider()
	{
		Vector2 offset = ContactDamageCollider.offset;
		offset.x = 0f;
		ContactDamageCollider.offset = offset;
	}
}
