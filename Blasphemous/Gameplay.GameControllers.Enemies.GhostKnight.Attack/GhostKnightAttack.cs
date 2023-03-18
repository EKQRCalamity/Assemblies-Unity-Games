using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Enemies.GhostKnight.Attack;

public class GhostKnightAttack : EnemyAttack
{
	private AttackArea _attackArea;

	private bool _attacked;

	private GhostKnight _ghostKnight;

	protected override void OnAwake()
	{
		base.OnAwake();
		_ghostKnight = (GhostKnight)base.EntityOwner;
		base.CurrentEnemyWeapon = base.EntityOwner.GetComponentInChildren<Weapon>();
		_attackArea = base.EntityOwner.GetComponentInChildren<AttackArea>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_attackArea.OnStay += AttackAreaOnStay;
		_attackArea.OnExit += AttackAreaOnExit;
	}

	private void AttackAreaOnStay(object sender, Collider2DParam collider2DParam)
	{
		if (_ghostKnight.MotionLerper.IsLerping && !_attacked)
		{
			_attacked = true;
			CurrentWeaponAttack();
		}
	}

	private void AttackAreaOnExit(object sender, Collider2DParam collider2DParam)
	{
		_attacked = false;
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		if (!(base.CurrentEnemyWeapon == null))
		{
			Hit weapondHit = default(Hit);
			float final = _ghostKnight.Stats.Strength.Final;
			weapondHit.AttackingEntity = _ghostKnight.gameObject;
			weapondHit.DamageType = DamageType;
			weapondHit.DamageAmount = final;
			weapondHit.Force = Force;
			weapondHit.HitSoundId = HitSound;
			base.CurrentEnemyWeapon.Attack(weapondHit);
		}
	}
}
