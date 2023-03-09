using UnityEngine;

public class PlaneWeaponChaliceBomb : AbstractPlaneWeapon
{
	private bool isA;

	protected override bool rapidFire => WeaponProperties.PlaneWeaponChaliceBomb.Basic.rapidFire;

	protected override float rapidFireRate => WeaponProperties.PlaneWeaponChaliceBomb.Basic.rapidFireRate;

	protected override AbstractProjectile fireBasic()
	{
		PlaneWeaponChaliceBombProjectile planeWeaponChaliceBombProjectile = base.fireBasic() as PlaneWeaponChaliceBombProjectile;
		planeWeaponChaliceBombProjectile.transform.Rotate(new Vector3(0f, 0f, Random.Range(0f - WeaponProperties.PlaneWeaponChaliceBomb.Basic.angleRange, WeaponProperties.PlaneWeaponChaliceBomb.Basic.angleRange)));
		planeWeaponChaliceBombProjectile.velocity = WeaponProperties.PlaneWeaponChaliceBomb.Basic.speed * MathUtils.AngleToDirection(planeWeaponChaliceBombProjectile.transform.rotation.eulerAngles.z);
		planeWeaponChaliceBombProjectile.gravity = WeaponProperties.PlaneWeaponChaliceBomb.Basic.gravity;
		planeWeaponChaliceBombProjectile.Damage = WeaponProperties.PlaneWeaponChaliceBomb.Basic.damage;
		planeWeaponChaliceBombProjectile.size = WeaponProperties.PlaneWeaponChaliceBomb.Basic.size;
		planeWeaponChaliceBombProjectile.damageExplosion = WeaponProperties.PlaneWeaponChaliceBomb.Basic.damageExplosion;
		planeWeaponChaliceBombProjectile.explosionSize = WeaponProperties.PlaneWeaponChaliceBomb.Basic.sizeExplosion;
		planeWeaponChaliceBombProjectile.PlayerId = player.id;
		planeWeaponChaliceBombProjectile.SetAnimation(isA);
		isA = !isA;
		return planeWeaponChaliceBombProjectile;
	}

	protected override AbstractProjectile fireEx()
	{
		PlaneWeaponChaliceBombExProjectile planeWeaponChaliceBombExProjectile = base.fireEx() as PlaneWeaponChaliceBombExProjectile;
		planeWeaponChaliceBombExProjectile.FreezeTime = WeaponProperties.PlaneWeaponChaliceBomb.Ex.freezeTime;
		planeWeaponChaliceBombExProjectile.Damage = WeaponProperties.PlaneWeaponChaliceBomb.Ex.damage;
		planeWeaponChaliceBombExProjectile.DamageRate = WeaponProperties.PlaneWeaponChaliceBomb.Ex.damageRate;
		planeWeaponChaliceBombExProjectile.DamageRateIncrease = WeaponProperties.PlaneWeaponChaliceBomb.Ex.damageRateIncrease;
		planeWeaponChaliceBombExProjectile.Gravity = WeaponProperties.PlaneWeaponChaliceBomb.Ex.gravity;
		planeWeaponChaliceBombExProjectile.Velocity = WeaponProperties.PlaneWeaponChaliceBomb.Ex.startSpeed * Vector3.right;
		planeWeaponChaliceBombExProjectile.PlayerId = player.id;
		MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Ex);
		meterScoreTracker.Add(planeWeaponChaliceBombExProjectile);
		return planeWeaponChaliceBombExProjectile;
	}
}
