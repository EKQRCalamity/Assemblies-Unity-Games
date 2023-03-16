public class ArcadeWeaponRocketPeashot : AbstractArcadeWeapon
{
	private ArcadeWeaponBullet p;

	protected override bool rapidFire => WeaponProperties.ArcadeWeaponRocketPeashot.Basic.rapidFire;

	protected override float rapidFireRate => WeaponProperties.ArcadeWeaponRocketPeashot.Basic.rapidFireRate;

	protected override AbstractProjectile fireBasic()
	{
		if (p != null && !p.dead)
		{
			return null;
		}
		p = base.fireBasic() as ArcadeWeaponBullet;
		p.Speed = WeaponProperties.ArcadeWeaponRocketPeashot.Basic.speed;
		p.Damage = WeaponProperties.ArcadeWeaponRocketPeashot.Basic.damage;
		p.PlayerId = player.id;
		p.DamagesType.PlayerProjectileDefault();
		p.CollisionDeath.PlayerProjectileDefault();
		return p;
	}
}
