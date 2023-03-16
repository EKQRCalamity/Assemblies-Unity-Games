using System.Collections.Generic;

public class WeaponExploder : AbstractLevelWeapon
{
	public List<WeaponArcProjectile> projectilesOnGround = new List<WeaponArcProjectile>();

	protected override bool rapidFire => true;

	protected override float rapidFireRate => WeaponProperties.LevelWeaponExploder.Basic.fireRate;

	protected override AbstractProjectile fireBasic()
	{
		WeaponExploderProjectile weaponExploderProjectile = base.fireBasic() as WeaponExploderProjectile;
		weaponExploderProjectile.Speed = WeaponProperties.LevelWeaponExploder.Basic.speed;
		weaponExploderProjectile.PlayerId = player.id;
		weaponExploderProjectile.DamagesType.SetAll(b: false);
		weaponExploderProjectile.CollisionDeath.PlayerProjectileDefault();
		weaponExploderProjectile.weapon = this;
		weaponExploderProjectile.minMaxSpeed = WeaponProperties.LevelWeaponExploder.Basic.easeSpeed;
		weaponExploderProjectile.easeTime = WeaponProperties.LevelWeaponExploder.Basic.easeTime;
		if (WeaponProperties.LevelWeaponExploder.Basic.easing)
		{
			weaponExploderProjectile.EaseSpeed();
		}
		return weaponExploderProjectile;
	}

	protected override AbstractProjectile fireEx()
	{
		WeaponExploderProjectile weaponExploderProjectile = base.fireEx() as WeaponExploderProjectile;
		weaponExploderProjectile.Speed = WeaponProperties.LevelWeaponExploder.Ex.speed;
		weaponExploderProjectile.Damage = WeaponProperties.LevelWeaponExploder.Ex.damage;
		weaponExploderProjectile.explodeRadius = WeaponProperties.LevelWeaponExploder.Ex.explodeRadius;
		weaponExploderProjectile.PlayerId = player.id;
		weaponExploderProjectile.DamagesType.SetAll(b: false);
		weaponExploderProjectile.CollisionDeath.PlayerProjectileDefault();
		MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Ex);
		meterScoreTracker.Add(weaponExploderProjectile);
		return weaponExploderProjectile;
	}
}
