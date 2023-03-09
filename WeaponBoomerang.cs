using UnityEngine;

public class WeaponBoomerang : AbstractLevelWeapon
{
	private int distanceIndex;

	private Vector2[] distances;

	protected override bool rapidFire => true;

	protected override float rapidFireRate => WeaponProperties.LevelWeaponBoomerang.Basic.fireRate;

	protected override void Awake()
	{
		base.Awake();
		string[] array = WeaponProperties.LevelWeaponBoomerang.Basic.xDistanceString.Split(',');
		string[] array2 = WeaponProperties.LevelWeaponBoomerang.Basic.yDistanceString.Split(',');
		distances = new Vector2[Mathf.Min(array.Length, array2.Length)];
		for (int i = 0; i < distances.Length; i++)
		{
			Parser.FloatTryParse(array[i], out distances[i].x);
			Parser.FloatTryParse(array2[i], out distances[i].y);
		}
		distanceIndex = Random.Range(0, distances.Length);
	}

	public override void BeginBasic()
	{
		BeginBasicCheckAttenuation("player_weapon_boomerang", "player_weapon_boomerang_p2");
		base.BeginBasic();
	}

	protected override AbstractProjectile fireBasic()
	{
		BasicSoundOneShot("player_weapon_boomerang", "player_weapon_boomerang_p2");
		WeaponBoomerangProjectile weaponBoomerangProjectile = base.fireBasic() as WeaponBoomerangProjectile;
		weaponBoomerangProjectile.Speed = WeaponProperties.LevelWeaponBoomerang.Basic.speed;
		weaponBoomerangProjectile.Damage = WeaponProperties.LevelWeaponBoomerang.Basic.damage;
		weaponBoomerangProjectile.PlayerId = player.id;
		weaponBoomerangProjectile.DamagesType.PlayerProjectileDefault();
		weaponBoomerangProjectile.CollisionDeath.PlayerProjectileDefault();
		weaponBoomerangProjectile.CollisionDeath.Other = false;
		weaponBoomerangProjectile.player = player;
		distanceIndex = (distanceIndex + 1) % distances.Length;
		weaponBoomerangProjectile.forwardDistance = distances[distanceIndex].x;
		weaponBoomerangProjectile.lateralDistance = distances[distanceIndex].y;
		return weaponBoomerangProjectile;
	}

	public override void EndBasic()
	{
		base.EndBasic();
		EndBasicCheckAttenuation("player_weapon_boomerang", "player_weapon_boomerang_p2");
	}

	protected override AbstractProjectile fireEx()
	{
		WeaponBoomerangProjectile weaponBoomerangProjectile = base.fireEx() as WeaponBoomerangProjectile;
		weaponBoomerangProjectile.Speed = WeaponProperties.LevelWeaponBoomerang.Ex.speed;
		weaponBoomerangProjectile.Damage = WeaponProperties.LevelWeaponBoomerang.Ex.damage;
		weaponBoomerangProjectile.maxDamage = WeaponProperties.LevelWeaponBoomerang.Ex.maxDamage * PlayerManager.DamageMultiplier;
		weaponBoomerangProjectile.PlayerId = player.id;
		weaponBoomerangProjectile.hitFreezeTime = WeaponProperties.LevelWeaponBoomerang.Ex.hitFreezeTime;
		weaponBoomerangProjectile.DamageRate = WeaponProperties.LevelWeaponBoomerang.Ex.damageRate + weaponBoomerangProjectile.hitFreezeTime;
		weaponBoomerangProjectile.DamagesType.PlayerProjectileDefault();
		weaponBoomerangProjectile.forwardDistance = WeaponProperties.LevelWeaponBoomerang.Ex.xDistance;
		weaponBoomerangProjectile.lateralDistance = WeaponProperties.LevelWeaponBoomerang.Ex.yDistance;
		weaponBoomerangProjectile.player = player;
		weaponBoomerangProjectile.CollisionDeath.Other = false;
		MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Ex);
		meterScoreTracker.Add(weaponBoomerangProjectile);
		return weaponBoomerangProjectile;
	}
}
