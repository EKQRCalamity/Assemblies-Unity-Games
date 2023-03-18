using System.Linq;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ReekLeader.Attack;

public class ReekLeaderAttack : EnemyAttack
{
	private Hit _reekHit;

	public AttackArea AttackArea { get; private set; }

	public bool IsTargetReachable { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea = GetComponentInChildren<AttackArea>();
		AttackArea.OnEnter += OnEnterAttackArea;
		AttackArea.OnExit += OnExitAttackArea;
		_reekHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = base.EntityOwner.Stats.Strength.Final,
			DamageType = DamageType,
			Force = 0f,
			HitSoundId = HitSound,
			Unnavoidable = false
		};
	}

	private void OnEnterAttackArea(object sender, Collider2DParam e)
	{
		IsTargetReachable = true;
	}

	private void OnExitAttackArea(object sender, Collider2DParam e)
	{
		IsTargetReachable = false;
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		if (!(AttackArea == null) && IsTargetReachable)
		{
			GameObject[] source = AttackArea.OverlappedEntities();
			GameObject gameObject = source.FirstOrDefault((GameObject target) => target.CompareTag("Penitent"));
			if (gameObject != null)
			{
				gameObject.GetComponentInParent<IDamageable>().Damage(_reekHit);
			}
		}
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
