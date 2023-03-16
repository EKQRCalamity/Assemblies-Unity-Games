public class WeaponAccuracy : AbstractLevelWeapon
{
	private enum Levels
	{
		One,
		Two,
		Three,
		Four
	}

	private int shotCounter;

	private Levels level;

	private float speed;

	private float fireRate;

	private float size;

	private float damage;

	protected override bool rapidFire => true;

	protected override float rapidFireRate => fireRate;

	private void Start()
	{
		level = Levels.One;
		speed = WeaponProperties.LevelWeaponAccuracy.Basic.LvlOneSpeed;
		fireRate = WeaponProperties.LevelWeaponAccuracy.Basic.LvlOneFireRate;
		size = WeaponProperties.LevelWeaponAccuracy.Basic.LvlOneSize;
		damage = WeaponProperties.LevelWeaponAccuracy.Basic.LvlOneDamage;
	}

	protected override AbstractProjectile fireBasic()
	{
		WeaponAccuracyProjectile weaponAccuracyProjectile = base.fireBasic() as WeaponAccuracyProjectile;
		weaponAccuracyProjectile.Speed = speed;
		weaponAccuracyProjectile.PlayerId = player.id;
		weaponAccuracyProjectile.CollisionDeath.PlayerProjectileDefault();
		weaponAccuracyProjectile.EnemyDeath = EnemyHit;
		weaponAccuracyProjectile.Damage = damage;
		weaponAccuracyProjectile.SetSize(size);
		return weaponAccuracyProjectile;
	}

	protected override AbstractProjectile fireEx()
	{
		WeaponAccuracyProjectile weaponAccuracyProjectile = base.fireEx() as WeaponAccuracyProjectile;
		weaponAccuracyProjectile.Speed = WeaponProperties.LevelWeaponAccuracy.Ex.exSpeed;
		weaponAccuracyProjectile.Damage = WeaponProperties.LevelWeaponAccuracy.Ex.exDamage;
		weaponAccuracyProjectile.SetSize(WeaponProperties.LevelWeaponAccuracy.Ex.exShotSize);
		weaponAccuracyProjectile.CollisionDeath.PlayerProjectileDefault();
		weaponAccuracyProjectile.PlayerId = player.id;
		weaponAccuracyProjectile.EnemyDeath = EXEnemyHit;
		MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Ex);
		meterScoreTracker.Add(weaponAccuracyProjectile);
		return weaponAccuracyProjectile;
	}

	private void EXEnemyHit(bool exEnemyHit)
	{
		if (exEnemyHit)
		{
			shotCounter += WeaponProperties.LevelWeaponAccuracy.Ex.exShotEquivalent;
			CheckLevels();
		}
		else
		{
			shotCounter = 0;
			LevelOne();
		}
	}

	private void EnemyHit(bool enemyHit)
	{
		if (enemyHit)
		{
			shotCounter++;
			CheckLevels();
		}
		else
		{
			shotCounter = 0;
			LevelOne();
		}
	}

	private void CheckLevels()
	{
		switch (level)
		{
		case Levels.One:
			if (shotCounter >= WeaponProperties.LevelWeaponAccuracy.Basic.LvlTwoCounter)
			{
				LevelTwo();
			}
			break;
		case Levels.Two:
			if (shotCounter >= WeaponProperties.LevelWeaponAccuracy.Basic.LvlThreeCounter)
			{
				LevelThree();
			}
			break;
		case Levels.Three:
			if (shotCounter >= WeaponProperties.LevelWeaponAccuracy.Basic.LvlFourCounter)
			{
				LevelFour();
			}
			break;
		case Levels.Four:
			break;
		default:
			LevelOne();
			break;
		}
	}

	private void LevelOne()
	{
		level = Levels.One;
		speed = WeaponProperties.LevelWeaponAccuracy.Basic.LvlOneSpeed;
		fireRate = WeaponProperties.LevelWeaponAccuracy.Basic.LvlOneFireRate;
		size = WeaponProperties.LevelWeaponAccuracy.Basic.LvlOneSize;
		damage = WeaponProperties.LevelWeaponAccuracy.Basic.LvlOneDamage;
	}

	private void LevelTwo()
	{
		level = Levels.Two;
		speed = WeaponProperties.LevelWeaponAccuracy.Basic.LvlTwoSpeed;
		fireRate = WeaponProperties.LevelWeaponAccuracy.Basic.LvlTwoFireRate;
		size = WeaponProperties.LevelWeaponAccuracy.Basic.LvlTwoSize;
		damage = WeaponProperties.LevelWeaponAccuracy.Basic.LvlTwoDamage;
	}

	private void LevelThree()
	{
		level = Levels.Three;
		speed = WeaponProperties.LevelWeaponAccuracy.Basic.LvlThreeSpeed;
		fireRate = WeaponProperties.LevelWeaponAccuracy.Basic.LvlThreeFireRate;
		size = WeaponProperties.LevelWeaponAccuracy.Basic.LvlThreeSize;
		damage = WeaponProperties.LevelWeaponAccuracy.Basic.LvlThreeDamage;
	}

	private void LevelFour()
	{
		level = Levels.Four;
		speed = WeaponProperties.LevelWeaponAccuracy.Basic.LvlFourSpeed;
		fireRate = WeaponProperties.LevelWeaponAccuracy.Basic.LvlFourFireRate;
		size = WeaponProperties.LevelWeaponAccuracy.Basic.LvlFourSize;
		damage = WeaponProperties.LevelWeaponAccuracy.Basic.LvlFourDamage;
	}
}
