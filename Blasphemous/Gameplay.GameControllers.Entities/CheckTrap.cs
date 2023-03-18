using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

[RequireComponent(typeof(Collider2D))]
public class CheckTrap : Trait
{
	public const string PenitentKilled = "PENITENT_KILLED";

	public const string SpikesTrap = "SPIKES";

	public const string AbyssTrap = "ABYSS";

	private const float ColliderOverlapVOffset = 0.15f;

	public bool AvoidTrapDamage { get; set; }

	public bool DeathBySpike { get; private set; }

	public bool DeathByFall { get; private set; }

	private BoxCollider2D FloorCollider { get; set; }

	private bool IsOwnerThrown => Core.Logic.Penitent.ThrowBack.IsThrown;

	protected override void OnAwake()
	{
		base.OnAwake();
		DeathBySpike = false;
		DeathByFall = false;
		FloorCollider = GetComponent<BoxCollider2D>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("SpikeTrap"))
		{
			if (OverlapTrapCollider(other))
			{
				DeathBySpike = true;
				SpikeTrapDamage();
				Core.Events.LaunchEvent("PENITENT_KILLED", "SPIKES");
			}
		}
		else if (other.gameObject.CompareTag("AbyssTrap") && !AvoidTrapDamage)
		{
			DeathByAbyssTrap();
		}
		else if (other.gameObject.CompareTag("UnnavoidableTrap"))
		{
			DeathByAbyssTrap();
		}
		else if (other.gameObject.CompareTag("HWTrap"))
		{
			DeathByHWTrap();
		}
	}

	private void DeathByAbyssTrap()
	{
		AbyssTrapDamage();
		DeathByFall = true;
		Core.Events.LaunchEvent("PENITENT_KILLED", "ABYSS");
	}

	private void DeathByHWTrap()
	{
		Hit hit = default(Hit);
		hit.DamageAmount = Core.Logic.Penitent.Stats.Life.Current;
		hit.AttackingEntity = Core.Logic.Penitent.gameObject;
		hit.DamageType = DamageArea.DamageType.Heavy;
		Hit hit2 = hit;
		Core.Logic.Penitent.DamageArea.TakeDamage(hit2, force: true);
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("SpikeTrap"))
		{
			if (OverlapTrapCollider(other) && !DeathBySpike)
			{
				DeathBySpike = true;
				SpikeTrapDamage();
				Core.Events.LaunchEvent("PENITENT_KILLED", "SPIKES");
			}
		}
		else if (other.gameObject.CompareTag("HWTrap"))
		{
			DeathByHWTrap();
		}
	}

	private void SpikeTrapDamage()
	{
		if (!(base.EntityOwner == null))
		{
			base.EntityOwner.IsImpaled = true;
			if (base.EntityOwner.Stats.Life.Current > 0f)
			{
				base.EntityOwner.Stats.Life.Current = 0f;
			}
			base.EntityOwner.KillInstanteneously();
			PlatformCharacterController componentInChildren = base.EntityOwner.GetComponentInChildren<PlatformCharacterController>();
			if ((bool)componentInChildren)
			{
				componentInChildren.enabled = false;
			}
		}
	}

	private bool OverlapTrapCollider(Collider2D trapCollider)
	{
		return FloorCollider.bounds.min.y + 0.15f <= trapCollider.bounds.max.y && !IsOwnerThrown;
	}

	private void AbyssTrapDamage()
	{
		if (!(base.EntityOwner == null))
		{
			if (base.EntityOwner.Stats.Life.Current > 0f)
			{
				base.EntityOwner.Stats.Life.Current = 0f;
			}
			base.EntityOwner.KillInstanteneously();
			PlatformCharacterController componentInChildren = base.EntityOwner.GetComponentInChildren<PlatformCharacterController>();
			if ((bool)componentInChildren)
			{
				componentInChildren.enabled = false;
			}
			if (base.EntityOwner.GetType() == typeof(Gameplay.GameControllers.Penitent.Penitent))
			{
				Core.Logic.CameraManager.ProCamera2D.FollowVertical = false;
			}
		}
	}
}
