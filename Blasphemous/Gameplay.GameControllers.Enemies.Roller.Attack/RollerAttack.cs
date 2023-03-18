using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Projectiles;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Roller.Attack;

public class RollerAttack : EnemyAttack
{
	public BossStraightProjectileAttack launcher;

	public float LaunchHeight = 2f;

	private IProjectileAttack ProjectileAttack { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		ProjectileAttack = GetComponent<BossStraightProjectileAttack>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		ProjectileAttack.SetProjectileWeaponDamage((int)base.EntityOwner.Stats.Strength.Final);
	}

	public void FireProjectile()
	{
		Vector2 dir = ((base.EntityOwner.Status.Orientation != 0) ? Vector2.left : Vector2.right);
		AcceleratedProjectile acceleratedProjectile = launcher.Shoot(dir) as AcceleratedProjectile;
		acceleratedProjectile.SetBouncebackData(launcher.projectileSource, Vector2.zero, 10);
	}
}
