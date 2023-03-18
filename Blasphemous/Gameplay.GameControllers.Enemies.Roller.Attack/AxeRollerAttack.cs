using Framework.Managers;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Projectiles;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Roller.Attack;

public class AxeRollerAttack : EnemyAttack
{
	public BossCurvedProjectileAttack launcher;

	public float LaunchHeight = 2f;

	public float MaxProjectileDistance;

	private IProjectileAttack ProjectileAttack { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		ProjectileAttack = GetComponent<BossCurvedProjectileAttack>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		ProjectileAttack.SetProjectileWeaponDamage((int)base.EntityOwner.Stats.Strength.Final);
	}

	public void FireProjectile()
	{
		Vector2 target = ClampTarget(Core.Logic.Penitent.GetPosition());
		CurvedProjectile curvedProjectile = launcher.Shoot(target);
	}

	private Vector2 ClampTarget(Vector2 penitentPosition)
	{
		Vector2 result = penitentPosition;
		if (Vector2.Distance(base.transform.position, penitentPosition) > MaxProjectileDistance)
		{
			Vector2 vector = (penitentPosition - (Vector2)base.transform.position).normalized * MaxProjectileDistance;
			result = vector + (Vector2)base.transform.position;
		}
		return result;
	}
}
