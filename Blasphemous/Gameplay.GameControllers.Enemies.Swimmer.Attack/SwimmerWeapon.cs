using FMODUnity;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Swimmer.Attack;

public class SwimmerWeapon : Weapon
{
	private Hit _swimmerProjectileHit;

	[EventRef]
	public string HitSoundFx;

	protected AttackArea AttackArea { get; set; }

	protected Rigidbody2D RigidBody { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
		RigidBody = GetComponent<Rigidbody2D>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea.OnEnter += AttackAreaOnEnter;
		_swimmerProjectileHit = new Hit
		{
			DamageType = DamageArea.DamageType.Normal,
			Unnavoidable = false,
			HitSoundId = HitSoundFx
		};
	}

	private void AttackAreaOnEnter(object sender, Collider2DParam e)
	{
		if (e.Collider2DArg.gameObject.CompareTag("Penitent"))
		{
			_swimmerProjectileHit.AttackingEntity = WeaponOwner.gameObject;
			_swimmerProjectileHit.DamageAmount = WeaponOwner.Stats.Strength.Final;
			Attack(_swimmerProjectileHit);
		}
		Destroy();
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		RigidBody.velocity = Vector2.zero;
	}

	public void SetOwner(Entity owner)
	{
		WeaponOwner = owner;
		AttackArea.Entity = owner;
	}
}
