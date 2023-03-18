using System;
using FMODUnity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Snake;

public class SnakeBeamAttack : Weapon, IDirectAttack
{
	private Hit _beamHit;

	public float tickCounter;

	public int damage = 30;

	private const float TIME_BETWEEN_TICKS = 0.15f;

	public Entity BeamAttackOwner;

	[EventRef]
	public string BeamAttackSoundFx;

	public AttackArea AttackArea { get; private set; }

	public event Action<SnakeBeamAttack> OnBeamHit;

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		CreateHit();
	}

	public void FireAttack()
	{
		Attack(_beamHit);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		tickCounter += Time.deltaTime;
		if (tickCounter > 0.15f)
		{
			tickCounter = 0f;
			FireAttack();
		}
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
		if (this.OnBeamHit != null)
		{
			this.OnBeamHit(this);
		}
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
	}

	public void CreateHit()
	{
		_beamHit = new Hit
		{
			AttackingEntity = base.transform.gameObject,
			DamageAmount = damage,
			DamageType = DamageArea.DamageType.Normal,
			DamageElement = DamageArea.DamageElement.Lightning,
			Force = 0f,
			Unnavoidable = true,
			HitSoundId = BeamAttackSoundFx
		};
	}

	public void SetDamage(int damage)
	{
		if (damage >= 0)
		{
			this.damage = damage;
			CreateHit();
		}
	}
}
