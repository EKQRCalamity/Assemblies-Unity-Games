using UnityEngine;

public class RumRunnersLevelMobBossProjectile : BasicProjectile
{
	public override BasicProjectile Create(Vector2 position, float rotation, float speed)
	{
		BasicProjectile basicProjectile = base.Create(position, rotation, speed);
		basicProjectile.CollisionDeath.None();
		basicProjectile.DamagesType.OnlyPlayer();
		return basicProjectile;
	}
}
