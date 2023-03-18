using System.Linq;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Jumper.Attack;

public class JumperAttack : EnemyAttack
{
	private Hit _jumperHit;

	public AttackArea AttackArea { get; set; }

	public bool TargetInAttackArea { get; private set; }

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea = GetComponentInChildren<AttackArea>();
		AttackArea.OnEnter += OnEnterAttackArea;
		AttackArea.OnExit += OnExitAttackArea;
		_jumperHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = base.EntityOwner.Stats.Strength.Final,
			DamageType = DamageArea.DamageType.Normal,
			HitSoundId = HitSound,
			Unnavoidable = false
		};
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		GameObject[] source = AttackArea.OverlappedEntities();
		GameObject gameObject = source.FirstOrDefault((GameObject overlappedEntity) => overlappedEntity.gameObject.CompareTag("Penitent"));
		if (gameObject != null)
		{
			gameObject.GetComponentInParent<IDamageable>().Damage(_jumperHit);
		}
	}

	private void OnEnterAttackArea(object sender, Collider2DParam e)
	{
		if (!base.EntityOwner.Status.IsGrounded)
		{
			TargetInAttackArea = true;
			CurrentWeaponAttack();
		}
	}

	private void OnExitAttackArea(object sender, Collider2DParam e)
	{
		TargetInAttackArea = false;
	}

	private void OnDestroy()
	{
		if ((bool)AttackArea)
		{
			AttackArea.OnEnter -= OnEnterAttackArea;
			AttackArea.OnExit -= OnExitAttackArea;
		}
	}
}
