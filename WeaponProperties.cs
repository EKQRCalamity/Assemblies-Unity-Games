public static class WeaponProperties
{
	public static class ArcadeWeaponPeashot
	{
		public static class Basic
		{
			public static readonly float damage = 4f;

			public static readonly float speed = 850f;

			public static readonly bool rapidFire;

			public static readonly float rapidFireRate;
		}

		public static class Ex
		{
		}

		public static readonly int value = 2;

		public static readonly string iconPath = "Icons/";

		public static readonly Weapon id = Weapon.arcade_weapon_peashot;

		public static string displayName => GetDisplayName(Weapon.arcade_weapon_peashot);

		public static string subtext => GetSubtext(Weapon.arcade_weapon_peashot);

		public static string description => GetDescription(Weapon.arcade_weapon_peashot);
	}

	public static class ArcadeWeaponRocketPeashot
	{
		public static class Basic
		{
			public static readonly float damage = 4f;

			public static readonly float speed = 700f;

			public static readonly bool rapidFire;

			public static readonly float rapidFireRate;
		}

		public static class Ex
		{
		}

		public static readonly int value = 10;

		public static readonly string iconPath = "Icons/";

		public static readonly Weapon id = Weapon.arcade_weapon_rocket_peashot;

		public static string displayName => GetDisplayName(Weapon.arcade_weapon_rocket_peashot);

		public static string subtext => GetSubtext(Weapon.arcade_weapon_rocket_peashot);

		public static string description => GetDescription(Weapon.arcade_weapon_rocket_peashot);
	}

	public static class CharmChalice
	{
		public static readonly int value = 10;

		public static readonly string iconPath = "Icons/equip_icon_charm_chalice";

		public static readonly Charm id = Charm.charm_chalice;

		public static string displayName => GetDisplayName(Charm.charm_chalice);

		public static string subtext => GetSubtext(Charm.charm_chalice);

		public static string description => GetDescription(Charm.charm_chalice);
	}

	public static class CharmCharmParryPlus
	{
		public static readonly int value = 3;

		public static readonly string iconPath = "Icons/equip_icon_charm_parry_slapper";

		public static readonly Charm id = Charm.charm_parry_plus;

		public static string displayName => GetDisplayName(Charm.charm_parry_plus);

		public static string subtext => GetSubtext(Charm.charm_parry_plus);

		public static string description => GetDescription(Charm.charm_parry_plus);
	}

	public static class CharmCurse
	{
		public static readonly int value = 1;

		public static readonly string iconPath = "Icons/equip_icon_charm_curse";

		public static readonly Charm id = Charm.charm_curse;

		public static readonly int[] availableWeaponIDs = new int[9] { 1456773641, 1456773649, 1460621839, 1466518900, 1466416941, 1467024095, 1487081743, 1568276855, 1614768724 };

		public static readonly int[] availableShmupWeaponIDs = new int[2] { 1457006169, 1492758857 };

		public static readonly int[] healthModifierValues = new int[5] { -2, -2, -2, -2, 0 };

		public static readonly float superMeterDelay = 1f;

		public static readonly float[] superMeterAmount = new float[5] { 0f, 0.13f, 0.26f, 0.39f, 0.52f };

		public static readonly int[] smokeDashInterval = new int[5] { 7, 4, 2, 1, 0 };

		public static readonly int[] whetstoneInterval = new int[5] { 7, 4, 2, 1, 0 };

		public static readonly string[] healerInterval = new string[5] { "3,3,4", "2,3,4", "1,3,4", "1,2,4", "1,2,3" };

		public static readonly int[] levelThreshold = new int[5] { 0, 4, 8, 12, 16 };

		public static string displayName => GetDisplayName(Charm.charm_curse);

		public static string subtext => GetSubtext(Charm.charm_curse);

		public static string description => GetDescription(Charm.charm_curse);
	}

	public static class CharmDirectionalDash
	{
		public static readonly int value = 10;

		public static readonly string iconPath = "Icons/";

		public static readonly Charm id = Charm.charm_directional_dash;

		public static string displayName => GetDisplayName(Charm.charm_directional_dash);

		public static string subtext => GetSubtext(Charm.charm_directional_dash);

		public static string description => GetDescription(Charm.charm_directional_dash);
	}

	public static class CharmEXCharm
	{
		public static readonly int value = 4;

		public static readonly string iconPath = "Icons/";

		public static readonly Charm id = Charm.charm_EX;

		public static readonly float planePeashotEXDebuff = 0.15f;

		public static string displayName => GetDisplayName(Charm.charm_EX);

		public static string subtext => GetSubtext(Charm.charm_EX);

		public static string description => GetDescription(Charm.charm_EX);
	}

	public static class CharmFloat
	{
		public static readonly int value = 10;

		public static readonly string iconPath = "Icons/";

		public static readonly Charm id = Charm.charm_float;

		public static readonly float maxTime = 2f;

		public static readonly float falloffStartTime = 1f;

		public static readonly float minFallSpeed = 0.1f;

		public static readonly float maxFallSpeed = 0.5f;

		public static string displayName => GetDisplayName(Charm.charm_float);

		public static string subtext => GetSubtext(Charm.charm_float);

		public static string description => GetDescription(Charm.charm_float);
	}

	public static class CharmHealer
	{
		public static readonly int value = 3;

		public static readonly string iconPath = "Icons/equip_icon_charm_healer";

		public static readonly Charm id = Charm.charm_healer;

		public static string displayName => GetDisplayName(Charm.charm_healer);

		public static string subtext => GetSubtext(Charm.charm_healer);

		public static string description => GetDescription(Charm.charm_healer);
	}

	public static class CharmHealthUpOne
	{
		public static readonly int value = 3;

		public static readonly string iconPath = "Icons/equip_icon_charm_hp1";

		public static readonly Charm id = Charm.charm_health_up_1;

		public static readonly int healthIncrease = 1;

		public static readonly float weaponDebuff = 0.05f;

		public static string displayName => GetDisplayName(Charm.charm_health_up_1);

		public static string subtext => GetSubtext(Charm.charm_health_up_1);

		public static string description => GetDescription(Charm.charm_health_up_1);
	}

	public static class CharmHealthUpTwo
	{
		public static readonly int value = 5;

		public static readonly string iconPath = "Icons/equip_icon_charm_hp2";

		public static readonly Charm id = Charm.charm_health_up_2;

		public static readonly int healthIncrease = 2;

		public static readonly float weaponDebuff = 0.1f;

		public static string displayName => GetDisplayName(Charm.charm_health_up_2);

		public static string subtext => GetSubtext(Charm.charm_health_up_2);

		public static string description => GetDescription(Charm.charm_health_up_2);
	}

	public static class CharmParryAttack
	{
		public static readonly int value = 3;

		public static readonly string iconPath = "Icons/equip_icon_charm_parry_attack";

		public static readonly Charm id = Charm.charm_parry_attack;

		public static readonly float damage = 16f;

		public static readonly float bounce;

		public static string displayName => GetDisplayName(Charm.charm_parry_attack);

		public static string subtext => GetSubtext(Charm.charm_parry_attack);

		public static string description => GetDescription(Charm.charm_parry_attack);
	}

	public static class CharmPitSaver
	{
		public static readonly int value = 3;

		public static readonly string iconPath = "Icons/equip_icon_charm_pitsaver";

		public static readonly Charm id = Charm.charm_pit_saver;

		public static readonly float meterAmount = 10f;

		public static readonly float invulnerabilityMultiplier = 1.6f;

		public static string displayName => GetDisplayName(Charm.charm_pit_saver);

		public static string subtext => GetSubtext(Charm.charm_pit_saver);

		public static string description => GetDescription(Charm.charm_pit_saver);
	}

	public static class CharmSmokeDash
	{
		public static readonly int value = 3;

		public static readonly string iconPath = "Icons/equip_icon_charm_smoke-dash";

		public static readonly Charm id = Charm.charm_smoke_dash;

		public static string displayName => GetDisplayName(Charm.charm_smoke_dash);

		public static string subtext => GetSubtext(Charm.charm_smoke_dash);

		public static string description => GetDescription(Charm.charm_smoke_dash);
	}

	public static class CharmSuperBuilder
	{
		public static readonly int value = 3;

		public static readonly string iconPath = "Icons/equip_icon_charm_coffee";

		public static readonly Charm id = Charm.charm_super_builder;

		public static readonly float delay = 1f;

		public static readonly float amount = 0.4f;

		public static string displayName => GetDisplayName(Charm.charm_super_builder);

		public static string subtext => GetSubtext(Charm.charm_super_builder);

		public static string description => GetDescription(Charm.charm_super_builder);
	}

	public static class LevelSuperBeam
	{
		public static readonly int value;

		public static readonly string iconPath = "Icons/equip_icon_super_beam";

		public static readonly Super id = Super.level_super_beam;

		public static readonly float time = 1.25f;

		public static readonly float damage = 14.5f;

		public static readonly float damageRate = 0.25f;

		public static string displayName => GetDisplayName(Super.level_super_beam);

		public static string subtext => GetSubtext(Super.level_super_beam);

		public static string description => GetDescription(Super.level_super_beam);
	}

	public static class LevelSuperChaliceBounce
	{
		public static readonly int value = 10;

		public static readonly string iconPath = "Icons/";

		public static readonly Super id = Super.level_super_chalice_bounce;

		public static readonly bool launchedVersion = true;

		public static readonly float damage = 30f;

		public static readonly float damageRate = 1f;

		public static readonly float maxDamage = 300f;

		public static readonly float duration = 7.5f;

		public static readonly float horizontalAcceleration = 1200f;

		public static readonly float maxHorizontalSpeed = 1000f;

		public static readonly float bounceVelocity = 2250f;

		public static readonly float bounceModifierNoJump = 1f;

		public static readonly float gravity = 7000f;

		public static readonly float enemyReboundMultiplier = 2f;

		public static readonly float enemyMultihitDelay = 0.5f;

		public static string displayName => GetDisplayName(Super.level_super_chalice_bounce);

		public static string subtext => GetSubtext(Super.level_super_chalice_bounce);

		public static string description => GetDescription(Super.level_super_chalice_bounce);
	}

	public static class LevelSuperChaliceIII
	{
		public static readonly int value = 10;

		public static readonly string iconPath = "Icons/equip_icon_super_ghost";

		public static readonly Super id = Super.level_super_chalice_iii;

		public static readonly float superDuration = 6.5f;

		public static string displayName => GetDisplayName(Super.level_super_chalice_iii);

		public static string subtext => GetSubtext(Super.level_super_chalice_iii);

		public static string description => GetDescription(Super.level_super_chalice_iii);
	}

	public static class LevelSuperChaliceShield
	{
		public static readonly int value = 10;

		public static readonly string iconPath = "Icons/equip_icon_super_invincible";

		public static readonly Super id = Super.level_super_chalice_shield;

		public static string displayName => GetDisplayName(Super.level_super_chalice_shield);

		public static string subtext => GetSubtext(Super.level_super_chalice_shield);

		public static string description => GetDescription(Super.level_super_chalice_shield);
	}

	public static class LevelSuperChaliceVertBeam
	{
		public static readonly int value = 10;

		public static readonly string iconPath = "Icons/equip_icon_super_beam";

		public static readonly Super id = Super.level_super_chalice_vert_beam;

		public static readonly float time = 1.25f;

		public static readonly float damage = 21.5f;

		public static readonly float damageRate = 0.25f;

		public static string displayName => GetDisplayName(Super.level_super_chalice_vert_beam);

		public static string subtext => GetSubtext(Super.level_super_chalice_vert_beam);

		public static string description => GetDescription(Super.level_super_chalice_vert_beam);
	}

	public static class LevelSuperGhost
	{
		public static readonly int value;

		public static readonly string iconPath = "Icons/equip_icon_super_ghost";

		public static readonly Super id = Super.level_super_ghost;

		public static readonly float initialSpeed = 700f;

		public static readonly float maxSpeed = 1250f;

		public static readonly float initialSpeedTime = 1.8f;

		public static readonly float maxSpeedTime = 3.8f;

		public static readonly float noHeartMaxSpeedTime = 3.7f;

		public static readonly float accelerationTime = 1f;

		public static readonly float heartSpeed = 100f;

		public static readonly float damage = 5.1f;

		public static readonly float damageRate = 0.22f;

		public static readonly float turnaroundEaseMultiplier = 4f;

		public static string displayName => GetDisplayName(Super.level_super_ghost);

		public static string subtext => GetSubtext(Super.level_super_ghost);

		public static string description => GetDescription(Super.level_super_ghost);
	}

	public static class LevelSuperInvincibility
	{
		public static readonly int value;

		public static readonly string iconPath = "Icons/equip_icon_super_invincible";

		public static readonly Super id = Super.level_super_invincible;

		public static readonly float durationInvincible = 4.85f;

		public static readonly float durationFX = 4.55f;

		public static string displayName => GetDisplayName(Super.level_super_invincible);

		public static string subtext => GetSubtext(Super.level_super_invincible);

		public static string description => GetDescription(Super.level_super_invincible);
	}

	public static class LevelWeaponAccuracy
	{
		public static class Basic
		{
			public static readonly float LvlOneFireRate = 0.28f;

			public static readonly float LvlOneSpeed = 1200f;

			public static readonly float LvlOneSize = 1.2f;

			public static readonly float LvlOneDamage = 4f;

			public static readonly int LvlTwoCounter = 20;

			public static readonly float LvlTwoFireRate = 0.23f;

			public static readonly float LvlTwoSpeed = 1500f;

			public static readonly float LvlTwoSize = 1.8f;

			public static readonly float LvlTwoDamage = 5.5f;

			public static readonly int LvlThreeCounter = 40;

			public static readonly float LvlThreeFireRate = 0.2f;

			public static readonly float LvlThreeSpeed = 1900f;

			public static readonly float LvlThreeSize = 3.2f;

			public static readonly float LvlThreeDamage = 7.5f;

			public static readonly int LvlFourCounter = 60;

			public static readonly float LvlFourFireRate = 0.18f;

			public static readonly float LvlFourSpeed = 2350f;

			public static readonly float LvlFourSize = 5f;

			public static readonly float LvlFourDamage = 8.5f;
		}

		public static class Ex
		{
			public static readonly float exSpeed = 1800f;

			public static readonly float exDamage = 25f;

			public static readonly int exShotEquivalent = 15;

			public static readonly float exShotSize = 8f;
		}

		public static readonly int value = 4;

		public static readonly string iconPath = "Icons/";

		public static readonly Weapon id = Weapon.level_weapon_accuracy;

		public static string displayName => GetDisplayName(Weapon.level_weapon_accuracy);

		public static string subtext => GetSubtext(Weapon.level_weapon_accuracy);

		public static string description => GetDescription(Weapon.level_weapon_accuracy);
	}

	public static class LevelWeaponArc
	{
		public static class Basic
		{
			public static readonly int Movement;

			public static readonly float launchSpeed = 1600f;

			public static readonly float gravity = 2750f;

			public static readonly float straightShotAngle = 65f;

			public static readonly float fireRate = 0.4f;

			public static readonly bool rapidFire = true;

			public static readonly int maxNumMines = 1;

			public static readonly float baseDamage = 14f;

			public static readonly float timeStateTwo = 1.25f;

			public static readonly float damageStateTwo = 7.5f;

			public static readonly float timeStateThree = 2.5f;

			public static readonly float damageStateThree = 11.25f;

			public static readonly float diagLaunchSpeed = 600f;

			public static readonly float diagGravity = 1000f;

			public static readonly float diagShotAngle = 45f;
		}

		public static class Ex
		{
			public static readonly float launchSpeed = 1600f;

			public static readonly float gravity = 2750f;

			public static readonly float damage = 28f;

			public static readonly float explodeDelay = 2f;
		}

		public static readonly int value = 2;

		public static readonly string iconPath = "Icons/equip_icon_weapon_peashot";

		public static readonly Weapon id = Weapon.level_weapon_arc;

		public static string displayName => GetDisplayName(Weapon.level_weapon_arc);

		public static string subtext => GetSubtext(Weapon.level_weapon_arc);

		public static string description => GetDescription(Weapon.level_weapon_arc);
	}

	public static class LevelWeaponBoomerang
	{
		public static class Basic
		{
			public static readonly float fireRate = 0.25f;

			public static readonly float speed = 1400f;

			public static readonly float damage = 8.5f;

			public static readonly string xDistanceString = "550,450,520,480";

			public static readonly string yDistanceString = "100,  50,  80, 70";
		}

		public static class Ex
		{
			public static readonly float speed = 1000f;

			public static readonly float damage = 5f;

			public static readonly float damageRate = 0.2f;

			public static readonly float maxDamage = 35f;

			public static readonly float xDistance = 400f;

			public static readonly float yDistance = 110f;

			public static readonly string pinkString = "2,3,2,4";

			public static readonly float hitFreezeTime = 0.1f;
		}

		public static readonly int value = 4;

		public static readonly string iconPath = "Icons/equip_icon_weapon_boomerang";

		public static readonly Weapon id = Weapon.level_weapon_boomerang;

		public static string displayName => GetDisplayName(Weapon.level_weapon_boomerang);

		public static string subtext => GetSubtext(Weapon.level_weapon_boomerang);

		public static string description => GetDescription(Weapon.level_weapon_boomerang);
	}

	public static class LevelWeaponBouncer
	{
		public static class Basic
		{
			public static readonly float launchSpeed = 1200f;

			public static readonly float gravity = 3600f;

			public static readonly float bounceRatio = 1.3f;

			public static readonly float bounceSpeedDampening = 800f;

			public static readonly float straightExtraAngle = 22.5f;

			public static readonly float diagonalUpExtraAngle;

			public static readonly float diagonalDownExtraAngle = 10f;

			public static readonly float damage = 11.6f;

			public static readonly float fireRate = 0.33f;

			public static readonly int numBounces = 2;
		}

		public static class Ex
		{
			public static readonly float launchSpeed = 1600f;

			public static readonly float gravity = 2750f;

			public static readonly float damage = 28f;

			public static readonly float explodeDelay = 2f;
		}

		public static readonly int value = 4;

		public static readonly string iconPath = "Icons/equip_icon_weapon_bouncer";

		public static readonly Weapon id = Weapon.level_weapon_bouncer;

		public static string displayName => GetDisplayName(Weapon.level_weapon_bouncer);

		public static string subtext => GetSubtext(Weapon.level_weapon_bouncer);

		public static string description => GetDescription(Weapon.level_weapon_bouncer);
	}

	public static class LevelWeaponCharge
	{
		public static class Basic
		{
			public static readonly float fireRate = 0.25f;

			public static readonly float baseDamage = 6f;

			public static readonly float speed = 1050f;

			public static readonly float timeStateTwo = 9999f;

			public static readonly float damageStateTwo = 20f;

			public static readonly float speedStateTwo = 1300f;

			public static readonly float timeStateThree = 1f;

			public static readonly float damageStateThree = 46f;
		}

		public static class Ex
		{
			public static readonly float damage = 26f;

			public static readonly float radius = 300f;
		}

		public static readonly int value = 4;

		public static readonly string iconPath = "Icons/equip_icon_weapon_charge";

		public static readonly Weapon id = Weapon.level_weapon_charge;

		public static string displayName => GetDisplayName(Weapon.level_weapon_charge);

		public static string subtext => GetSubtext(Weapon.level_weapon_charge);

		public static string description => GetDescription(Weapon.level_weapon_charge);
	}

	public static class LevelWeaponCrackshot
	{
		public static class Basic
		{
			public static readonly float fireRate = 0.32f;

			public static readonly float initialSpeed = 1050f;

			public static readonly float crackDistance = 290f;

			public static readonly float crackedSpeed = 2500f;

			public static readonly float initialDamage = 10.56f;

			public static readonly float crackedDamage = 6.7f;

			public static readonly bool enableMaxAngle = true;

			public static readonly float maxAngle = 170f;
		}

		public static class Ex
		{
			public static readonly float launchDistance = 100f;

			public static readonly float timeToHoverPoint = 0.5f;

			public static readonly float hoverWidth = 37f;

			public static readonly float hoverHeight = 35f;

			public static readonly float hoverSpeed = 0.9f;

			public static readonly float bulletSpeed = 2000f;

			public static readonly float bulletDamage = 3.5f;

			public static readonly float collideDamage = 12f;

			public static readonly int shotNumber = 5;

			public static readonly float shootDelay = 1f;

			public static readonly float riseSpeed;

			public static readonly bool isPink = true;

			public static readonly float parryBulletDamage = 14f;

			public static readonly float parryBulletSpeed = 2000f;

			public static readonly float parryTimeOut = 0.15f;
		}

		public static readonly int value = 4;

		public static readonly string iconPath = "Icons/equip_icon_weapon_crackshot";

		public static readonly Weapon id = Weapon.level_weapon_crackshot;

		public static string displayName => GetDisplayName(Weapon.level_weapon_crackshot);

		public static string subtext => GetSubtext(Weapon.level_weapon_crackshot);

		public static string description => GetDescription(Weapon.level_weapon_crackshot);
	}

	public static class LevelWeaponExploder
	{
		public static class Basic
		{
			public static readonly float fireRate = 0.35f;

			public static readonly bool rapideFire = true;

			public static readonly float speed = 1200f;

			public static readonly float sinSpeed = 10f;

			public static readonly float sinSize = 0.1f;

			public static readonly float baseDamage = 6f;

			public static readonly float baseExplosionRadius = 15f;

			public static readonly float baseScale = 0.1f;

			public static readonly float timeStateTwo = 0.25f;

			public static readonly float damageStateTwo = 10f;

			public static readonly float explosionRadiusStateTwo = 70f;

			public static readonly float scaleStateTwo = 0.5f;

			public static readonly float timeStateThree = 0.5f;

			public static readonly float damageStateThree = 12.75f;

			public static readonly float explosionRadiusStateThree = 130f;

			public static readonly float scaleStateThree = 1f;

			public static readonly bool easing = true;

			public static readonly MinMax easeSpeed = new MinMax(900f, 2500f);

			public static readonly float easeTime = 1f;
		}

		public static class Ex
		{
			public static readonly float speed = 1300f;

			public static readonly float damage = 35f;

			public static readonly float hitRate;

			public static readonly float explodeRadius = 300f;

			public static readonly float shrapnelSpeed = 1200f;

			public static readonly bool damageOn = true;
		}

		public static readonly int value = 2;

		public static readonly string iconPath = "Icons/";

		public static readonly Weapon id = Weapon.level_weapon_exploder;

		public static string displayName => GetDisplayName(Weapon.level_weapon_exploder);

		public static string subtext => GetSubtext(Weapon.level_weapon_exploder);

		public static string description => GetDescription(Weapon.level_weapon_exploder);
	}

	public static class LevelWeaponFirecracker
	{
		public static class Basic
		{
			public static readonly float fireRate = 0.06f;

			public static readonly float bulletSpeed = 2250f;

			public static readonly float bulletLife = 0.17f;

			public static readonly float explosionDamage = 2.6f;

			public static readonly float explosionSize = 10f;

			public static readonly float explosionDuration = 0.1f;
		}

		public static class Ex
		{
			public static readonly float exSpeed = 1700f;

			public static readonly float explosionRadius = 20f;

			public static readonly float damageRate = 0.5f;

			public static readonly float explosionDamage = 3f;

			public static readonly float explosionTime = 2f;

			public static readonly float exLife = 1f;
		}

		public static readonly int value = 4;

		public static readonly string iconPath = "Icons/";

		public static readonly Weapon id = Weapon.level_weapon_firecracker;

		public static string displayName => GetDisplayName(Weapon.level_weapon_firecracker);

		public static string subtext => GetSubtext(Weapon.level_weapon_firecracker);

		public static string description => GetDescription(Weapon.level_weapon_firecracker);
	}

	public static class LevelWeaponFirecrackerB
	{
		public static class Basic
		{
			public static readonly float fireRate = 0.09f;

			public static readonly float bulletSpeed = 2000f;

			public static readonly float bulletLife = 0.2f;

			public static readonly float explosionDamage = 0.5f;

			public static readonly float explosionSize = 5f;

			public static readonly float explosionDuration = 0.16f;

			public static readonly string explosionAngleString = "45,180,270,135,315,225,90,0";

			public static readonly float explosionsRadiusSize = 68f;
		}

		public static class Ex
		{
			public static readonly float exSpeed = 1700f;

			public static readonly float explosionRadius = 20f;

			public static readonly float damageRate = 0.5f;

			public static readonly float explosionDamage = 3f;

			public static readonly float explosionTime = 2f;

			public static readonly float exLife = 1f;
		}

		public static readonly int value = 4;

		public static readonly string iconPath = "Icons/";

		public static readonly Weapon id = Weapon.level_weapon_firecrackerB;

		public static string displayName => GetDisplayName(Weapon.level_weapon_firecrackerB);

		public static string subtext => GetSubtext(Weapon.level_weapon_firecrackerB);

		public static string description => GetDescription(Weapon.level_weapon_firecrackerB);
	}

	public static class LevelWeaponHoming
	{
		public static class Basic
		{
			public static readonly MinMax fireRate = new MinMax(0.15f, 0.15f);

			public static readonly float speed = 1000f;

			public static readonly float damage = 2.85f;

			public static readonly MinMax rotationSpeed = new MinMax(0f, 500f);

			public static readonly float timeBeforeEaseRotationSpeed = 0f;

			public static readonly float rotationSpeedEaseTime = 0.4f;

			public static readonly float lockedShotAccelerationTime = 0.5f;

			public static readonly float speedVariation = 100f;

			public static readonly float angleVariation = 5f;

			public static readonly int trailFrameDelay = 2;

			public static readonly float maxHomingTime = 2.5f;
		}

		public static class Ex
		{
			public static readonly float speed = 1500f;

			public static readonly float damage = 7f;

			public static readonly float spread = 90f;

			public static readonly int bulletCount = 4;

			public static readonly float swirlDistance = 100f;

			public static readonly float swirlEaseTime = 0.75f;

			public static readonly int trailFrameDelay = 2;
		}

		public static readonly int value = 4;

		public static readonly string iconPath = "Icons/equip_icon_weapon_homing";

		public static readonly Weapon id = Weapon.level_weapon_homing;

		public static string displayName => GetDisplayName(Weapon.level_weapon_homing);

		public static string subtext => GetSubtext(Weapon.level_weapon_homing);

		public static string description => GetDescription(Weapon.level_weapon_homing);
	}

	public static class LevelWeaponPeashot
	{
		public static class Basic
		{
			public static readonly float damage = 4f;

			public static readonly float speed = 2250f;

			public static readonly bool rapidFire = true;

			public static readonly float rapidFireRate = 0.11f;
		}

		public static class Ex
		{
			public static readonly float damage = 8.334f;

			public static readonly float maxDamage = 25f;

			public static readonly float damageDistance = 80f;

			public static readonly float speed = 1500f;

			public static readonly float freezeTime = 0.05f;
		}

		public static readonly int value = 2;

		public static readonly string iconPath = "Icons/equip_icon_weapon_peashot";

		public static readonly Weapon id = Weapon.level_weapon_peashot;

		public static string displayName => GetDisplayName(Weapon.level_weapon_peashot);

		public static string subtext => GetSubtext(Weapon.level_weapon_peashot);

		public static string description => GetDescription(Weapon.level_weapon_peashot);
	}

	public static class LevelWeaponPushback
	{
		public static class Basic
		{
			public static readonly float damage = 4f;

			public static readonly MinMax fireRate = new MinMax(0.1f, 0.7f);

			public static readonly MinMax speed = new MinMax(700f, 1300f);

			public static readonly float speedTime = 3f;

			public static readonly float pushbackSpeed = 30f;
		}

		public static class Ex
		{
		}

		public static readonly int value = 4;

		public static readonly string iconPath = "Icons/";

		public static readonly Weapon id = Weapon.level_weapon_pushback;

		public static string displayName => GetDisplayName(Weapon.level_weapon_pushback);

		public static string subtext => GetSubtext(Weapon.level_weapon_pushback);

		public static string description => GetDescription(Weapon.level_weapon_pushback);
	}

	public static class LevelWeaponSplitter
	{
		public static class Basic
		{
			public static readonly float fireRate = 0.22f;

			public static readonly float speed = 1700f;

			public static readonly float splitDistanceA = 200f;

			public static readonly float splitDistanceB = 550f;

			public static readonly float bulletDamage = 4f;

			public static readonly float bulletDamageA = 2.15f;

			public static readonly float bulletDamageB = 1.65f;

			public static readonly float splitAngle = 20f;

			public static readonly float angleDistance = 100f;
		}

		public static class Ex
		{
		}

		public static readonly int value = 10;

		public static readonly string iconPath = "Icons/";

		public static readonly Weapon id = Weapon.level_weapon_splitter;

		public static string displayName => GetDisplayName(Weapon.level_weapon_splitter);

		public static string subtext => GetSubtext(Weapon.level_weapon_splitter);

		public static string description => GetDescription(Weapon.level_weapon_splitter);
	}

	public static class LevelWeaponSpreadshot
	{
		public static class Basic
		{
			public static readonly float damage = 1.24f;

			public static readonly float speed = 2250f;

			public static readonly float distance = 375f;

			public static readonly float rapidFireRate = 0.13f;
		}

		public static class Ex
		{
			public static readonly float damage = 4.3f;

			public static readonly float speed = 500f;

			public static readonly int childCount = 8;

			public static readonly float radius = 100f;
		}

		public static readonly int value = 4;

		public static readonly string iconPath = "Icons/equip_icon_weapon_spread";

		public static readonly Weapon id = Weapon.level_weapon_spreadshot;

		public static string displayName => GetDisplayName(Weapon.level_weapon_spreadshot);

		public static string subtext => GetSubtext(Weapon.level_weapon_spreadshot);

		public static string description => GetDescription(Weapon.level_weapon_spreadshot);
	}

	public static class LevelWeaponUpshot
	{
		public static class Basic
		{
			public static readonly float damage = 2.33f;

			public static readonly float fireRate = 0.2f;

			public static readonly float[] xSpeed = new float[3] { 630f, 819f, 945f };

			public static readonly MinMax[] ySpeed = new MinMax[3]
			{
				new MinMax(0f, 3240f),
				new MinMax(0f, 3240f),
				new MinMax(0f, 3240f)
			};

			public static readonly float[] timeToMaxSpeed = new float[3] { 1.08f, 0.81f, 0.945f };
		}

		public static class Ex
		{
			public static readonly float minRotationSpeed = 375f;

			public static readonly float maxRotationSpeed = 185f;

			public static readonly float rotationRampTime = 1.8f;

			public static readonly float minRadiusSpeed = 195f;

			public static readonly float maxRadiusSpeed = 365f;

			public static readonly float radiusRampTime = 1.8f;

			public static readonly float damage = 8f;

			public static readonly float damageRate = 0.3f;

			public static readonly float maxDamage = 37f;

			public static readonly float freezeTime = 0.1f;
		}

		public static readonly int value = 4;

		public static readonly string iconPath = "Icons/equip_icon_weapon_upshot";

		public static readonly Weapon id = Weapon.level_weapon_upshot;

		public static string displayName => GetDisplayName(Weapon.level_weapon_upshot);

		public static string subtext => GetSubtext(Weapon.level_weapon_upshot);

		public static string description => GetDescription(Weapon.level_weapon_upshot);
	}

	public static class LevelWeaponWideShot
	{
		public static class Basic
		{
			public static readonly float damage = 2.67f;

			public static readonly float speed = 1800f;

			public static readonly float distance = 2000f;

			public static readonly float rapidFireRate = 0.22f;

			public static readonly MinMax angleRange = new MinMax(50f, 8f);

			public static readonly float closingAngleSpeed = 1.1f;

			public static readonly float openingAngleSpeed = 1.8f;

			public static readonly float projectileSpeed = 2f;
		}

		public static class Ex
		{
			public static readonly float exDamage = 21f;

			public static readonly float exDuration = 0.3f;

			public static readonly float exHeight = 86.5f;
		}

		public static readonly int value = 4;

		public static readonly string iconPath = "Icons/equip_icon_weapon_wide_shot";

		public static readonly Weapon id = Weapon.level_weapon_wide_shot;

		public static string displayName => GetDisplayName(Weapon.level_weapon_wide_shot);

		public static string subtext => GetSubtext(Weapon.level_weapon_wide_shot);

		public static string description => GetDescription(Weapon.level_weapon_wide_shot);
	}

	public static class PlaneSuperBomb
	{
		public static readonly int value = 10;

		public static readonly string iconPath = "Icons/";

		public static readonly Super id = Super.plane_super_bomb;

		public static readonly float damage = 38f;

		public static readonly float damageRate = 0.25f;

		public static readonly float countdownTime = 3f;

		public static string displayName => GetDisplayName(Super.plane_super_bomb);

		public static string subtext => GetSubtext(Super.plane_super_bomb);

		public static string description => GetDescription(Super.plane_super_bomb);
	}

	public static class PlaneSuperChaliceSuperBomb
	{
		public static readonly int value = 10;

		public static readonly string iconPath = "Icons/";

		public static readonly Super id = Super.plane_super_chalice_bomb;

		public static readonly float damage = 25.5f;

		public static readonly float damageRate = 0.25f;

		public static readonly float turnRate = 1f;

		public static readonly float maxAngle = 60f;

		public static readonly float angleDamp = 0.98f;

		public static readonly float accel = 600f;

		public static string displayName => GetDisplayName(Super.plane_super_chalice_bomb);

		public static string subtext => GetSubtext(Super.plane_super_chalice_bomb);

		public static string description => GetDescription(Super.plane_super_chalice_bomb);
	}

	public static class PlaneWeaponBomb
	{
		public static class Basic
		{
			public static readonly float damage = 11.5f;

			public static readonly float speed = 1200f;

			public static readonly bool Up;

			public static readonly float sizeExplosion = 1f;

			public static readonly float size = 1f;

			public static readonly float angle = 45f;

			public static readonly float gravity = 4500f;

			public static readonly bool rapidFire = true;

			public static readonly float rapidFireRate = 0.6f;
		}

		public static class Ex
		{
			public static readonly float damage = 6f;

			public static readonly float speed = 700f;

			public static readonly float[] angles = new float[2] { 180f, 170f };

			public static readonly int[] counts = new int[2] { 6, 3 };

			public static readonly MinMax rotationSpeed = new MinMax(0f, 250f);

			public static readonly float timeBeforeEaseRotationSpeed = 0f;

			public static readonly float rotationSpeedEaseTime = 1f;

			public static readonly float maxHomingTime = 2.5f;
		}

		public static readonly int value = 2;

		public static readonly string iconPath = "Icons/";

		public static readonly Weapon id = Weapon.plane_weapon_bomb;

		public static string displayName => GetDisplayName(Weapon.plane_weapon_bomb);

		public static string subtext => GetSubtext(Weapon.plane_weapon_bomb);

		public static string description => GetDescription(Weapon.plane_weapon_bomb);
	}

	public static class PlaneWeaponChaliceBomb
	{
		public static class Basic
		{
			public static readonly float damage = 6.6f;

			public static readonly float size = 1f;

			public static readonly float sizeExplosion = 1f;

			public static readonly float angleRange = 35f;

			public static readonly float gravity = 1700f;

			public static readonly float speed = 700f;

			public static readonly bool rapidFire = true;

			public static readonly float rapidFireRate = 0.2f;

			public static readonly float damageExplosion = 2.5f;
		}

		public static class Ex
		{
			public static readonly float damage = 15.5f;

			public static readonly float damageRate = 0.17f;

			public static readonly float damageRateIncrease = 0.07f;

			public static readonly float startSpeed = 600f;

			public static readonly float gravity = 1900f;

			public static readonly float freezeTime = 0.125f;
		}

		public static readonly int value = 10;

		public static readonly string iconPath = "Icons/equip_icon_chalice_shmup_bomb";

		public static readonly Weapon id = Weapon.plane_chalice_weapon_bomb;

		public static string displayName => GetDisplayName(Weapon.plane_chalice_weapon_bomb);

		public static string subtext => GetSubtext(Weapon.plane_chalice_weapon_bomb);

		public static string description => GetDescription(Weapon.plane_chalice_weapon_bomb);
	}

	public static class PlaneWeaponChaliceWay
	{
		public static class Basic
		{
			public static readonly float damage = 3.65f;

			public static readonly float speed = 1650f;

			public static readonly float distance;

			public static readonly float rapidFireRate = 0.23f;

			public static readonly float angle = 9f;
		}

		public static class Ex
		{
			public static readonly float damageBeforeLaunch = 2.4f;

			public static readonly float damageRateBeforeLaunch = 0.25f;

			public static readonly float arcSpeed = 5f;

			public static readonly float arcX = 250f;

			public static readonly float arcY = 40f;

			public static readonly float pauseTime;

			public static readonly float damageAfterLaunch = 17f;

			public static readonly float speedAfterLaunch = -1250f;

			public static readonly float accelAfterLaunch = 8000f;

			public static readonly float freezeTime = 0.125f;

			public static readonly float minXDistance = 75f;

			public static readonly int xDistanceNoTarget = 500;
		}

		public static readonly int value = 10;

		public static readonly string iconPath = "Icons/equip_icon_chalice_shmup_3way";

		public static readonly Weapon id = Weapon.plane_chalice_weapon_3way;

		public static string displayName => GetDisplayName(Weapon.plane_chalice_weapon_3way);

		public static string subtext => GetSubtext(Weapon.plane_chalice_weapon_3way);

		public static string description => GetDescription(Weapon.plane_chalice_weapon_3way);
	}

	public static class PlaneWeaponLaser
	{
		public static class Basic
		{
			public static readonly float damage = 8f;

			public static readonly float speed = 4000f;

			public static readonly bool rapidFire = true;

			public static readonly float rapidFireRate = 0.1f;
		}

		public static class Ex
		{
			public static readonly float damage = 3f;

			public static readonly float speed = 2000f;

			public static readonly float[] angles = new float[2] { 180f, 170f };

			public static readonly int[] counts = new int[2] { 12, 6 };
		}

		public static readonly int value = 2;

		public static readonly string iconPath = "Icons/";

		public static readonly Weapon id = Weapon.plane_weapon_laser;

		public static string displayName => GetDisplayName(Weapon.plane_weapon_laser);

		public static string subtext => GetSubtext(Weapon.plane_weapon_laser);

		public static string description => GetDescription(Weapon.plane_weapon_laser);
	}

	public static class PlaneWeaponPeashot
	{
		public static class Basic
		{
			public static readonly float damage = 4f;

			public static readonly float speed = 1800f;

			public static readonly bool rapidFire = true;

			public static readonly float rapidFireRate = 0.07f;
		}

		public static class Ex
		{
			public static readonly float damage = 15f;

			public static readonly float damageDistance = 100f;

			public static readonly float acceleration = 2500f;

			public static readonly float maxSpeed = 1500f;

			public static readonly float freezeTime = 0.125f;
		}

		public static readonly int value = 2;

		public static readonly string iconPath = "Icons/equip_icon_weapon_peashot";

		public static readonly Weapon id = Weapon.plane_weapon_peashot;

		public static string displayName => GetDisplayName(Weapon.plane_weapon_peashot);

		public static string subtext => GetSubtext(Weapon.plane_weapon_peashot);

		public static string description => GetDescription(Weapon.plane_weapon_peashot);
	}

	public static string GetDisplayName(Weapon weapon)
	{
		TranslationElement translationElement = Localization.Find(weapon.ToString() + "_name");
		if (translationElement == null)
		{
			return "ERROR";
		}
		return translationElement.translation.text;
	}

	public static string GetDisplayName(Super super)
	{
		TranslationElement translationElement = Localization.Find(super.ToString() + "_name");
		if (translationElement == null)
		{
			return "ERROR";
		}
		return translationElement.translation.text;
	}

	public static string GetDisplayName(Charm charm)
	{
		TranslationElement translationElement = Localization.Find(charm.ToString() + "_name");
		if (translationElement == null)
		{
			return "ERROR";
		}
		return translationElement.translation.text;
	}

	public static string GetSubtext(Weapon weapon)
	{
		TranslationElement translationElement = Localization.Find(weapon.ToString() + "_subtext");
		if (translationElement == null)
		{
			return "ERROR";
		}
		return translationElement.translation.text;
	}

	public static string GetSubtext(Super super)
	{
		TranslationElement translationElement = Localization.Find(super.ToString() + "_subtext");
		if (translationElement == null)
		{
			return "ERROR";
		}
		return translationElement.translation.text;
	}

	public static string GetSubtext(Charm charm)
	{
		TranslationElement translationElement = Localization.Find(charm.ToString() + "_subtext");
		if (translationElement == null)
		{
			return "ERROR";
		}
		return translationElement.translation.text;
	}

	public static string GetIconPath(Weapon weapon)
	{
		return weapon switch
		{
			Weapon.level_weapon_peashot => "Icons/equip_icon_weapon_peashot", 
			Weapon.level_weapon_spreadshot => "Icons/equip_icon_weapon_spread", 
			Weapon.level_weapon_arc => "Icons/equip_icon_weapon_peashot", 
			Weapon.level_weapon_homing => "Icons/equip_icon_weapon_homing", 
			Weapon.level_weapon_exploder => "Icons/", 
			Weapon.level_weapon_boomerang => "Icons/equip_icon_weapon_boomerang", 
			Weapon.level_weapon_charge => "Icons/equip_icon_weapon_charge", 
			Weapon.level_weapon_bouncer => "Icons/equip_icon_weapon_bouncer", 
			Weapon.level_weapon_wide_shot => "Icons/equip_icon_weapon_wide_shot", 
			Weapon.level_weapon_accuracy => "Icons/", 
			Weapon.level_weapon_firecracker => "Icons/", 
			Weapon.level_weapon_upshot => "Icons/equip_icon_weapon_upshot", 
			Weapon.level_weapon_firecrackerB => "Icons/", 
			Weapon.level_weapon_pushback => "Icons/", 
			Weapon.plane_weapon_peashot => "Icons/equip_icon_weapon_peashot", 
			Weapon.plane_weapon_laser => "Icons/", 
			Weapon.plane_weapon_bomb => "Icons/", 
			Weapon.plane_chalice_weapon_3way => "Icons/equip_icon_chalice_shmup_3way", 
			Weapon.arcade_weapon_peashot => "Icons/", 
			Weapon.arcade_weapon_rocket_peashot => "Icons/", 
			Weapon.plane_chalice_weapon_bomb => "Icons/equip_icon_chalice_shmup_bomb", 
			Weapon.level_weapon_crackshot => "Icons/equip_icon_weapon_crackshot", 
			Weapon.level_weapon_splitter => "Icons/", 
			Weapon.None => "Icons/equip_icon_empty", 
			_ => "ERROR", 
		};
	}

	public static string GetIconPath(Super super)
	{
		return super switch
		{
			Super.level_super_beam => "Icons/equip_icon_super_beam", 
			Super.level_super_ghost => "Icons/equip_icon_super_ghost", 
			Super.level_super_invincible => "Icons/equip_icon_super_invincible", 
			Super.level_super_chalice_iii => "Icons/equip_icon_super_ghost", 
			Super.level_super_chalice_vert_beam => "Icons/equip_icon_super_beam", 
			Super.level_super_chalice_shield => "Icons/equip_icon_super_invincible", 
			Super.level_super_chalice_bounce => "Icons/", 
			Super.plane_super_bomb => "Icons/", 
			Super.plane_super_chalice_bomb => "Icons/", 
			Super.None => "Icons/equip_icon_empty", 
			_ => "ERROR", 
		};
	}

	public static string GetIconPath(Charm charm)
	{
		return charm switch
		{
			Charm.charm_health_up_1 => "Icons/equip_icon_charm_hp1", 
			Charm.charm_health_up_2 => "Icons/equip_icon_charm_hp2", 
			Charm.charm_super_builder => "Icons/equip_icon_charm_coffee", 
			Charm.charm_smoke_dash => "Icons/equip_icon_charm_smoke-dash", 
			Charm.charm_parry_plus => "Icons/equip_icon_charm_parry_slapper", 
			Charm.charm_pit_saver => "Icons/equip_icon_charm_pitsaver", 
			Charm.charm_parry_attack => "Icons/equip_icon_charm_parry_attack", 
			Charm.charm_chalice => "Icons/equip_icon_charm_chalice", 
			Charm.charm_directional_dash => "Icons/", 
			Charm.charm_healer => "Icons/equip_icon_charm_healer", 
			Charm.charm_EX => "Icons/", 
			Charm.charm_curse => "Icons/equip_icon_charm_curse", 
			Charm.charm_float => "Icons/", 
			Charm.None => "Icons/equip_icon_empty", 
			_ => "ERROR", 
		};
	}

	public static string GetDescription(Weapon weapon)
	{
		TranslationElement translationElement = Localization.Find(weapon.ToString() + "_description");
		if (translationElement == null)
		{
			return "ERROR";
		}
		return translationElement.translation.text;
	}

	public static string GetDescription(Super super)
	{
		TranslationElement translationElement = Localization.Find(super.ToString() + "_description");
		if (translationElement == null)
		{
			return "ERROR";
		}
		return translationElement.translation.text;
	}

	public static string GetDescription(Charm charm)
	{
		TranslationElement translationElement = Localization.Find(charm.ToString() + "_description");
		if (translationElement == null)
		{
			return "ERROR";
		}
		return translationElement.translation.text;
	}

	public static int GetValue(Weapon weapon)
	{
		return weapon switch
		{
			Weapon.level_weapon_peashot => 2, 
			Weapon.level_weapon_spreadshot => 4, 
			Weapon.level_weapon_arc => 2, 
			Weapon.level_weapon_homing => 4, 
			Weapon.level_weapon_exploder => 2, 
			Weapon.level_weapon_boomerang => 4, 
			Weapon.level_weapon_charge => 4, 
			Weapon.level_weapon_bouncer => 4, 
			Weapon.level_weapon_wide_shot => 4, 
			Weapon.level_weapon_accuracy => 4, 
			Weapon.level_weapon_firecracker => 4, 
			Weapon.level_weapon_upshot => 4, 
			Weapon.level_weapon_firecrackerB => 4, 
			Weapon.level_weapon_pushback => 4, 
			Weapon.plane_weapon_peashot => 2, 
			Weapon.plane_weapon_laser => 2, 
			Weapon.plane_weapon_bomb => 2, 
			Weapon.plane_chalice_weapon_3way => 10, 
			Weapon.arcade_weapon_peashot => 2, 
			Weapon.arcade_weapon_rocket_peashot => 10, 
			Weapon.plane_chalice_weapon_bomb => 10, 
			Weapon.level_weapon_crackshot => 4, 
			Weapon.level_weapon_splitter => 10, 
			_ => 0, 
		};
	}

	public static int GetValue(Super super)
	{
		return super switch
		{
			Super.level_super_beam => 0, 
			Super.level_super_ghost => 0, 
			Super.level_super_invincible => 0, 
			Super.level_super_chalice_iii => 10, 
			Super.level_super_chalice_vert_beam => 10, 
			Super.level_super_chalice_shield => 10, 
			Super.level_super_chalice_bounce => 10, 
			Super.plane_super_bomb => 10, 
			Super.plane_super_chalice_bomb => 10, 
			_ => 0, 
		};
	}

	public static int GetValue(Charm charm)
	{
		return charm switch
		{
			Charm.charm_health_up_1 => 3, 
			Charm.charm_health_up_2 => 5, 
			Charm.charm_super_builder => 3, 
			Charm.charm_smoke_dash => 3, 
			Charm.charm_parry_plus => 3, 
			Charm.charm_pit_saver => 3, 
			Charm.charm_parry_attack => 3, 
			Charm.charm_chalice => 10, 
			Charm.charm_directional_dash => 10, 
			Charm.charm_healer => 3, 
			Charm.charm_EX => 4, 
			Charm.charm_curse => 1, 
			Charm.charm_float => 10, 
			_ => 0, 
		};
	}
}
