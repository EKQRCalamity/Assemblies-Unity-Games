using UnityEngine;

public class WeaponCrackshot : AbstractLevelWeapon
{
	private const float Y_POS = 20f;

	private const float ROTATION_OFFSET = 3f;

	private float[] yPositions = new float[4] { 0f, 20f, 40f, 20f };

	private int currentY;

	[SerializeField]
	private PatternString variantString;

	private bool useBComet;

	private WeaponCrackshotExProjectile activeEX;

	protected override bool rapidFire => true;

	protected override float rapidFireRate => WeaponProperties.LevelWeaponCrackshot.Basic.fireRate;

	protected override AbstractProjectile fireBasic()
	{
		WeaponCrackshotProjectile weaponCrackshotProjectile = base.fireBasic() as WeaponCrackshotProjectile;
		weaponCrackshotProjectile.Speed = WeaponProperties.LevelWeaponCrackshot.Basic.initialSpeed;
		weaponCrackshotProjectile.Damage = WeaponProperties.LevelWeaponCrackshot.Basic.initialDamage;
		weaponCrackshotProjectile.PlayerId = player.id;
		weaponCrackshotProjectile.DamagesType.PlayerProjectileDefault();
		weaponCrackshotProjectile.CollisionDeath.PlayerProjectileDefault();
		weaponCrackshotProjectile.maxAngleRange = ((!WeaponProperties.LevelWeaponCrackshot.Basic.enableMaxAngle) ? 180f : WeaponProperties.LevelWeaponCrackshot.Basic.maxAngle);
		weaponCrackshotProjectile.variant = variantString.PopInt();
		weaponCrackshotProjectile.useBComet = useBComet;
		useBComet = !useBComet;
		float y = yPositions[currentY];
		currentY++;
		if (currentY >= yPositions.Length)
		{
			currentY = 0;
		}
		weaponCrackshotProjectile.transform.AddPosition(0f, y);
		return weaponCrackshotProjectile;
	}

	protected override AbstractProjectile fireEx()
	{
		if ((bool)activeEX)
		{
			activeEX.GetReplaced();
		}
		WeaponCrackshotExProjectile weaponCrackshotExProjectile = base.fireEx() as WeaponCrackshotExProjectile;
		weaponCrackshotExProjectile.Damage = WeaponProperties.LevelWeaponCrackshot.Ex.collideDamage;
		weaponCrackshotExProjectile.DamageRate = 0f;
		weaponCrackshotExProjectile.PlayerId = player.id;
		MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Ex);
		meterScoreTracker.Add(weaponCrackshotExProjectile);
		activeEX = weaponCrackshotExProjectile;
		return weaponCrackshotExProjectile;
	}

	public override void BeginBasic()
	{
		AudioManager.Play("player_weapon_crackshot_shoot_start");
		emitAudioFromObject.Add("player_weapon_crackshot_shoot_start");
		variantString = new PatternString("0,1,0,2,1,2,0,1,2");
		base.BeginBasic();
	}

	public override void EndBasic()
	{
		ActivateCooldown();
		base.EndBasic();
	}
}
