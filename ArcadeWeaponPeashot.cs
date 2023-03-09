public class ArcadeWeaponPeashot : AbstractArcadeWeapon
{
	private const float Y_POS = 20f;

	private const float ROTATION_OFFSET = 3f;

	private ArcadeWeaponBullet p;

	protected override bool rapidFire => WeaponProperties.ArcadeWeaponPeashot.Basic.rapidFire;

	protected override float rapidFireRate => WeaponProperties.ArcadeWeaponPeashot.Basic.rapidFireRate;

	protected override AbstractProjectile fireBasic()
	{
		if (p != null && !p.dead)
		{
			return null;
		}
		p = base.fireBasic() as ArcadeWeaponBullet;
		p.Speed = WeaponProperties.ArcadeWeaponPeashot.Basic.speed;
		p.Damage = WeaponProperties.ArcadeWeaponPeashot.Basic.damage;
		p.PlayerId = player.id;
		p.DamagesType.PlayerProjectileDefault();
		p.CollisionDeath.PlayerProjectileDefault();
		return p;
	}

	protected override AbstractProjectile fireEx()
	{
		return null;
	}
}
