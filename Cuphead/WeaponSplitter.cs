public class WeaponSplitter : AbstractLevelWeapon
{
	private const float ROTATION_OFFSET = 3f;

	protected override bool rapidFire => true;

	protected override float rapidFireRate => WeaponProperties.LevelWeaponSplitter.Basic.fireRate;

	protected override AbstractProjectile fireBasic()
	{
		WeaponSplitterProjectile weaponSplitterProjectile = base.fireBasic() as WeaponSplitterProjectile;
		weaponSplitterProjectile.Speed = WeaponProperties.LevelWeaponSplitter.Basic.speed;
		weaponSplitterProjectile.Damage = WeaponProperties.LevelWeaponSplitter.Basic.bulletDamage;
		weaponSplitterProjectile.isMain = true;
		weaponSplitterProjectile.nextDistance = WeaponProperties.LevelWeaponSplitter.Basic.splitDistanceA;
		weaponSplitterProjectile.PlayerId = player.id;
		weaponSplitterProjectile.DamagesType.PlayerProjectileDefault();
		weaponSplitterProjectile.CollisionDeath.PlayerProjectileDefault();
		return weaponSplitterProjectile;
	}

	protected override AbstractProjectile fireEx()
	{
		WeaponPeashotExProjectile weaponPeashotExProjectile = base.fireEx() as WeaponPeashotExProjectile;
		weaponPeashotExProjectile.moveSpeed = WeaponProperties.LevelWeaponPeashot.Ex.speed;
		weaponPeashotExProjectile.Damage = WeaponProperties.LevelWeaponPeashot.Ex.damage;
		weaponPeashotExProjectile.hitFreezeTime = WeaponProperties.LevelWeaponPeashot.Ex.freezeTime;
		weaponPeashotExProjectile.DamageRate = weaponPeashotExProjectile.hitFreezeTime + WeaponProperties.LevelWeaponPeashot.Ex.damageDistance / weaponPeashotExProjectile.moveSpeed;
		weaponPeashotExProjectile.maxDamage = WeaponProperties.LevelWeaponPeashot.Ex.maxDamage;
		weaponPeashotExProjectile.PlayerId = player.id;
		MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Ex);
		meterScoreTracker.Add(weaponPeashotExProjectile);
		return weaponPeashotExProjectile;
	}

	public override void BeginBasic()
	{
		OneShotCooldown("player_default_fire_start");
		BasicSoundLoop("player_default_fire_loop", "player_default_fire_loop_p2");
		base.BeginBasic();
	}

	public override void EndBasic()
	{
		ActivateCooldown();
		base.EndBasic();
		StopLoopSound("player_default_fire_loop", "player_default_fire_loop_p2");
	}
}
