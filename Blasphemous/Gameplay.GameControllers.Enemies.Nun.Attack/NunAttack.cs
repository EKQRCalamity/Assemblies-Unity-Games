using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent.Gizmos;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Nun.Attack;

public class NunAttack : EnemyAttack
{
	public DamageArea.DamageType CurrentDamageType;

	private Hit _weaponHit;

	public RootMotionDriver RootMotion { get; set; }

	public AttackArea AttackArea { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		RootMotion = base.EntityOwner.GetComponentInChildren<RootMotionDriver>();
		AttackArea = base.EntityOwner.GetComponentInChildren<AttackArea>();
		base.CurrentEnemyWeapon = GetComponentInChildren<NunWeapon>();
		AttackArea.OnEnter += OnEnterAttackArea;
		_weaponHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = base.EntityOwner.Stats.Strength.Final,
			Force = Force,
			HitSoundId = HitSound,
			Unnavoidable = true
		};
	}

	private void OnEnterAttackArea(object sender, Collider2DParam e)
	{
		if (!base.EntityOwner.Status.Dead)
		{
			_weaponHit.DamageType = CurrentDamageType;
			CurrentWeaponAttack();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		Vector3 position = ((!base.EntityOwner.SpriteRenderer.flipX) ? RootMotion.transform.position : RootMotion.ReversePosition);
		AttackArea.transform.position = position;
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		base.CurrentEnemyWeapon.Attack(_weaponHit);
	}

	private void OnDestroy()
	{
		if ((bool)AttackArea)
		{
			AttackArea.OnEnter -= OnEnterAttackArea;
		}
	}
}
