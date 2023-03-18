using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint.Attack;

public class BejeweledDivineBeam : Weapon
{
	private Hit _beamHit;

	[EventRef]
	public string DivineBeamFx;

	[EventRef]
	public string DivineBeamHitFx;

	public AttackArea AttackArea { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		WeaponOwner = Object.FindObjectOfType<BejeweledSaintHead>();
		AttackArea = GetComponentInChildren<AttackArea>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea.Entity = WeaponOwner;
		_beamHit = new Hit
		{
			AttackingEntity = base.transform.gameObject,
			DamageAmount = WeaponOwner.Stats.Strength.Final * 0.8f,
			DamageType = DamageArea.DamageType.Normal,
			Force = 0f,
			Unnavoidable = true,
			HitSoundId = DivineBeamHitFx
		};
	}

	public void FireAttack()
	{
		Attack(_beamHit);
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public void Dispose()
	{
		Destroy();
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		Core.Audio.PlaySfx(DivineBeamFx);
	}
}
