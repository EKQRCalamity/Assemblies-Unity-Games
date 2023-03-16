using UnityEngine;

public class WeaponUpshot : AbstractLevelWeapon
{
	private const int NUM_OF_BULLETS = 3;

	private int[] xOffset = new int[6] { -1, 1, 0, 1, -1, 0 };

	private int xIndex;

	private int animationCycleCount;

	protected override bool rapidFire => true;

	protected override float rapidFireRate => WeaponProperties.LevelWeaponUpshot.Basic.fireRate;

	protected override AbstractProjectile fireBasic()
	{
		animationCycleCount++;
		for (int i = 0; i < 3; i++)
		{
			WeaponUpshotProjectile weaponUpshotProjectile = ((i != 0) ? (fireBasicNoEffect() as WeaponUpshotProjectile) : (base.fireBasic() as WeaponUpshotProjectile));
			if (i == 1)
			{
				weaponUpshotProjectile.GetComponent<SpriteRenderer>().sortingOrder = 1;
			}
			weaponUpshotProjectile.Damage = WeaponProperties.LevelWeaponUpshot.Basic.damage;
			weaponUpshotProjectile.PlayerId = player.id;
			weaponUpshotProjectile.DamagesType.PlayerProjectileDefault();
			weaponUpshotProjectile.CollisionDeath.PlayerProjectileDefault();
			weaponUpshotProjectile.CollisionDeath.Other = false;
			weaponUpshotProjectile.xSpeed = WeaponProperties.LevelWeaponUpshot.Basic.xSpeed[i];
			weaponUpshotProjectile.ySpeedMinMax = WeaponProperties.LevelWeaponUpshot.Basic.ySpeed[i];
			weaponUpshotProjectile.timeToArc = WeaponProperties.LevelWeaponUpshot.Basic.timeToMaxSpeed[i];
			weaponUpshotProjectile.animator.Play(((animationCycleCount + i) % 3).ToString(), 0, Random.Range(0f, 1f));
		}
		return null;
	}

	protected override AbstractProjectile fireEx()
	{
		WeaponUpshotExProjectile weaponUpshotExProjectile = base.fireEx() as WeaponUpshotExProjectile;
		weaponUpshotExProjectile.Damage = WeaponProperties.LevelWeaponUpshot.Ex.damage;
		weaponUpshotExProjectile.DamageRate = WeaponProperties.LevelWeaponUpshot.Ex.damageRate;
		weaponUpshotExProjectile.PlayerId = player.id;
		weaponUpshotExProjectile.rotateDir = Mathf.Sign(player.gameObject.transform.localScale.x);
		MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Ex);
		meterScoreTracker.Add(weaponUpshotExProjectile);
		return weaponUpshotExProjectile;
	}

	public override void BeginBasic()
	{
		base.BeginBasic();
		AudioManager.Play("player_weapon_upshot_start");
		emitAudioFromObject.Add("player_weapon_upshot_start");
		BasicSoundLoop("player_weapon_upshot_loop_p1", "player_weapon_upshot_loop_p2");
	}

	public override void EndBasic()
	{
		base.EndBasic();
		StopLoopSound("player_weapon_upshot_loop_p1", "player_weapon_upshot_loop_p2");
	}
}
