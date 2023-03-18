using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Gameplay.GameControllers.Penitent.Abilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Attack;

public class RangeAttackExplosion : Weapon
{
	private Hit _explosionHit;

	public float DamageFactor;

	private AttackArea _attackArea;

	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	protected string BlastSoundFx;

	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	protected string BlastHitSoundFx;

	public RangeAttackBalance RangeAttackBalance;

	protected override void OnAwake()
	{
		base.OnAwake();
		_attackArea = GetComponentInChildren<AttackArea>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_explosionHit = new Hit
		{
			AttackingEntity = WeaponOwner.gameObject,
			DamageType = DamageArea.DamageType.Normal,
			DamageAmount = RangeAttackBalance.GetDamageBySwordLevel * Core.Logic.Penitent.Stats.RangedStrength.Final * DamageFactor,
			HitSoundId = BlastHitSoundFx
		};
		WeaponOwner = Core.Logic.Penitent;
		_attackArea.Entity = WeaponOwner;
		Core.Audio.EventOneShotPanned(BlastSoundFx, base.transform.position);
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public void Attack()
	{
		Attack(_explosionHit);
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		WeaponOwner = Core.Logic.Penitent;
		_attackArea.Entity = WeaponOwner;
		Core.Audio.EventOneShotPanned(BlastSoundFx, base.transform.position);
	}

	public void Dispose()
	{
		Destroy();
	}
}
