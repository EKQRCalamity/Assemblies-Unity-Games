using System.Collections;
using UnityEngine;

public class WeaponWideShot : AbstractLevelWeapon
{
	private float maxAngle;

	private bool isInitialized;

	private int animationCycleCount;

	protected override bool rapidFire => true;

	protected override float rapidFireRate => WeaponProperties.LevelWeaponWideShot.Basic.rapidFireRate;

	private void Start()
	{
		maxAngle = WeaponProperties.LevelWeaponWideShot.Basic.angleRange.max;
		StartCoroutine(angle_cr());
		isInitialized = true;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (isInitialized)
		{
			StartCoroutine(angle_cr());
		}
	}

	public override void BeginBasic()
	{
		base.BeginBasic();
		BasicSoundOneShot("player_wide_shot_start", "player_wide_shot_start_p2");
	}

	protected override AbstractProjectile fireBasic()
	{
		BasicSoundOneShot("player_wide_shot_shoot", "player_wide_shot_shoot_p2");
		float damage = WeaponProperties.LevelWeaponWideShot.Basic.damage;
		BasicProjectile basicProjectile = null;
		MinMax minMax = new MinMax(0f, maxAngle);
		animationCycleCount++;
		for (int i = 0; (float)i < 3f; i++)
		{
			float floatAt = minMax.GetFloatAt((float)i / 2f);
			float num = minMax.max / 2f;
			basicProjectile = ((i != 0) ? (fireBasicNoEffect() as BasicProjectile) : (base.fireBasic() as BasicProjectile));
			basicProjectile.Speed = WeaponProperties.LevelWeaponWideShot.Basic.speed;
			basicProjectile.DestroyDistance = WeaponProperties.LevelWeaponWideShot.Basic.distance - 20f * (float)(i + 1);
			basicProjectile.Damage = damage;
			basicProjectile.PlayerId = player.id;
			basicProjectile.transform.AddEulerAngles(0f, 0f, floatAt - num);
			basicProjectile.transform.position += basicProjectile.transform.right * 50f;
			basicProjectile.animator.SetInteger("Variant", (animationCycleCount + i) % 3);
		}
		return basicProjectile;
	}

	protected override AbstractProjectile fireEx()
	{
		WeaponWideShotExProjectile weaponWideShotExProjectile = base.fireEx() as WeaponWideShotExProjectile;
		weaponWideShotExProjectile.Damage = WeaponProperties.LevelWeaponWideShot.Ex.exDamage;
		weaponWideShotExProjectile.DamageRate = 0f;
		weaponWideShotExProjectile.origin = weaponWideShotExProjectile.transform.position;
		weaponWideShotExProjectile.mainDuration = WeaponProperties.LevelWeaponWideShot.Ex.exDuration;
		weaponWideShotExProjectile.GetComponent<BoxCollider2D>().size = new Vector2(2000f, WeaponProperties.LevelWeaponWideShot.Ex.exHeight);
		weaponWideShotExProjectile.PlayerId = player.id;
		MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Ex);
		meterScoreTracker.Add(weaponWideShotExProjectile);
		return weaponWideShotExProjectile;
	}

	private IEnumerator angle_cr()
	{
		float openTimeMax = WeaponProperties.LevelWeaponWideShot.Basic.openingAngleSpeed;
		float closeTimeMax = WeaponProperties.LevelWeaponWideShot.Basic.closingAngleSpeed;
		float t = 0f;
		float val = 0f;
		bool playerLocked = false;
		while (true)
		{
			if (playerLocked)
			{
				if (val < 1f)
				{
					val = t / closeTimeMax;
					t += (float)CupheadTime.Delta;
				}
				else
				{
					val = 1f;
					t = 1f;
				}
			}
			else if (val > 0f)
			{
				val = t / openTimeMax;
				t -= (float)CupheadTime.Delta;
			}
			else
			{
				val = 0f;
				t = 0f;
			}
			playerLocked = player.input.actions.GetButton(6);
			maxAngle = WeaponProperties.LevelWeaponWideShot.Basic.angleRange.GetFloatAt(val);
			yield return null;
		}
	}
}
