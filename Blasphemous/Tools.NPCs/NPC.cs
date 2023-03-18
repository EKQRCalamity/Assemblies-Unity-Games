using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.NPCs.Damage;
using UnityEngine;

namespace Tools.NPCs;

[SelectionBase]
public class NPC : Entity, IDamageable
{
	public bool CanTakeDamage;

	public NpcDamage DamageArea { get; private set; }

	public static event Core.ObjectEvent SCreated;

	public static event Core.SimpleEvent SAttacked;

	protected override void OnAwake()
	{
		base.OnAwake();
		DamageArea = GetComponentInChildren<NpcDamage>();
		if (NPC.SCreated != null)
		{
			NPC.SCreated(base.gameObject);
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		Status.CastShadow = true;
		Status.IsGrounded = true;
		if (!CanTakeDamage && (bool)DamageArea && DamageArea.gameObject != null)
		{
			DamageArea.gameObject.layer = LayerMask.NameToLayer("Default");
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		Status.IsVisibleOnCamera = IsVisible();
	}

	public bool IsVisible()
	{
		return Entity.IsVisibleFrom(base.SpriteRenderer, Camera.main);
	}

	public void Damage(Hit hit)
	{
		if ((bool)DamageArea && CanTakeDamage)
		{
			DamageArea.TakeDamage(hit);
			if (NPC.SAttacked != null)
			{
				NPC.SAttacked();
			}
			if (Status.Dead)
			{
				Kill();
			}
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
