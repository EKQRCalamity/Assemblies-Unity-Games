using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;

namespace Gameplay.GameControllers.Enemies.WallEnemy.Attack;

public class WallEnemyAttack : EnemyAttack
{
	private Hit _wallEnemyHit;

	public AttackArea AttackArea { get; set; }

	public WallEnemyWeapon Weapon { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.CurrentEnemyWeapon = GetComponentInChildren<WallEnemyWeapon>();
		Weapon = (WallEnemyWeapon)base.CurrentEnemyWeapon;
		_wallEnemyHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = base.EntityOwner.Stats.Strength.Final,
			Force = Force,
			DamageType = DamageType,
			HitSoundId = HitSound,
			ThrowbackDirByOwnerPosition = true,
			Unnavoidable = false
		};
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		base.CurrentEnemyWeapon.Attack(_wallEnemyHit);
	}
}
