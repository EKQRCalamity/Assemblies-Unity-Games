using UnityEngine;

public class WeaponFirecracker : AbstractLevelWeapon
{
	public bool isTypeB;

	private GameObject dummyObject;

	private string[] explosionAngles;

	private int explosionAngleIndex;

	protected override bool rapidFire => true;

	protected override float rapidFireRate => WeaponProperties.LevelWeaponFirecracker.Basic.fireRate;

	private void Start()
	{
		CreateDummyObject();
		explosionAngles = WeaponProperties.LevelWeaponFirecrackerB.Basic.explosionAngleString.Split(',');
	}

	protected override AbstractProjectile fireBasic()
	{
		WeaponFirecrackerProjectile weaponFirecrackerProjectile = base.fireBasic() as WeaponFirecrackerProjectile;
		if (isTypeB)
		{
			weaponFirecrackerProjectile.explosionRadiusSize = WeaponProperties.LevelWeaponFirecrackerB.Basic.explosionsRadiusSize;
			float result = 0f;
			Parser.FloatTryParse(explosionAngles[explosionAngleIndex], out result);
			weaponFirecrackerProjectile.explosionAngle = result;
			explosionAngleIndex = (explosionAngleIndex + 1) % explosionAngles.Length;
			weaponFirecrackerProjectile.Speed = WeaponProperties.LevelWeaponFirecrackerB.Basic.bulletSpeed;
			weaponFirecrackerProjectile.Damage = WeaponProperties.LevelWeaponFirecrackerB.Basic.explosionDamage;
			weaponFirecrackerProjectile.bulletLife = WeaponProperties.LevelWeaponFirecrackerB.Basic.bulletLife;
			weaponFirecrackerProjectile.explosionSize = WeaponProperties.LevelWeaponFirecrackerB.Basic.explosionSize;
			weaponFirecrackerProjectile.explosionDuration = WeaponProperties.LevelWeaponFirecrackerB.Basic.explosionDuration;
		}
		else
		{
			weaponFirecrackerProjectile.Speed = WeaponProperties.LevelWeaponFirecracker.Basic.bulletSpeed;
			weaponFirecrackerProjectile.Damage = WeaponProperties.LevelWeaponFirecracker.Basic.explosionDamage;
			weaponFirecrackerProjectile.bulletLife = WeaponProperties.LevelWeaponFirecracker.Basic.bulletLife;
			weaponFirecrackerProjectile.explosionSize = WeaponProperties.LevelWeaponFirecracker.Basic.explosionSize;
			weaponFirecrackerProjectile.explosionDuration = WeaponProperties.LevelWeaponFirecracker.Basic.explosionDuration;
		}
		weaponFirecrackerProjectile.collider.enabled = false;
		weaponFirecrackerProjectile.PlayerId = player.id;
		weaponFirecrackerProjectile.DamagesType.PlayerProjectileDefault();
		weaponFirecrackerProjectile.CollisionDeath.PlayerProjectileDefault();
		dummyObject.transform.eulerAngles = player.transform.eulerAngles;
		dummyObject.transform.localScale = player.transform.localScale;
		weaponFirecrackerProjectile.transform.parent = dummyObject.transform;
		weaponFirecrackerProjectile.SetupFirecracker(dummyObject.transform, player, isTypeB);
		return weaponFirecrackerProjectile;
	}

	protected override AbstractProjectile fireEx()
	{
		WeaponFirecrackerEXProjectile weaponFirecrackerEXProjectile = base.fireEx() as WeaponFirecrackerEXProjectile;
		if (isTypeB)
		{
			weaponFirecrackerEXProjectile.Speed = WeaponProperties.LevelWeaponFirecrackerB.Ex.exSpeed;
			weaponFirecrackerEXProjectile.bulletLife = WeaponProperties.LevelWeaponFirecrackerB.Ex.exLife;
			weaponFirecrackerEXProjectile.explosionSize = WeaponProperties.LevelWeaponFirecrackerB.Ex.explosionRadius;
			weaponFirecrackerEXProjectile.DamageRate = WeaponProperties.LevelWeaponFirecrackerB.Ex.damageRate;
			weaponFirecrackerEXProjectile.Damage = WeaponProperties.LevelWeaponFirecrackerB.Ex.explosionDamage;
			weaponFirecrackerEXProjectile.explosionDuration = WeaponProperties.LevelWeaponFirecrackerB.Ex.explosionTime;
		}
		else
		{
			weaponFirecrackerEXProjectile.Speed = WeaponProperties.LevelWeaponFirecracker.Ex.exSpeed;
			weaponFirecrackerEXProjectile.bulletLife = WeaponProperties.LevelWeaponFirecracker.Ex.exLife;
			weaponFirecrackerEXProjectile.explosionSize = WeaponProperties.LevelWeaponFirecracker.Ex.explosionRadius;
			weaponFirecrackerEXProjectile.DamageRate = WeaponProperties.LevelWeaponFirecracker.Ex.damageRate;
			weaponFirecrackerEXProjectile.Damage = WeaponProperties.LevelWeaponFirecracker.Ex.explosionDamage;
			weaponFirecrackerEXProjectile.explosionDuration = WeaponProperties.LevelWeaponFirecracker.Ex.explosionTime;
		}
		MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Ex);
		meterScoreTracker.Add(weaponFirecrackerEXProjectile);
		return weaponFirecrackerEXProjectile;
	}

	private void Update()
	{
		dummyObject.transform.position = player.transform.position;
	}

	private void CreateDummyObject()
	{
		dummyObject = new GameObject();
		dummyObject.name = "FirecrackerDummyObj";
	}
}
