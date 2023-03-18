using FMODUnity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BurntFace.Rosary;

public class BurntFaceBeamAttack : Weapon, IDirectAttack
{
	private Hit _beamHit;

	[EventRef]
	public string DivineBeamFx;

	public float tickCounter;

	public int damage = 30;

	private const float TIME_BETWEEN_TICKS = 0.15f;

	public Entity BeamAttackOwner;

	[EventRef]
	public string BeamAttackSoundFx;

	public AttackArea AttackArea { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		WeaponOwner = ((!(BeamAttackOwner == null)) ? BeamAttackOwner : Object.FindObjectOfType<BurntFace>());
		AttackArea = GetComponentInChildren<AttackArea>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea.Entity = WeaponOwner;
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
			DamageElement = DamageArea.DamageElement.Magic,
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
