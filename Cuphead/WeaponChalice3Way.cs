using UnityEngine;

public class WeaponChalice3Way : AbstractPlaneWeapon
{
	private int animatorOffset;

	private int bulletNumber;

	protected override bool rapidFire => true;

	protected override float rapidFireRate => WeaponProperties.PlaneWeaponChaliceWay.Basic.rapidFireRate;

	protected override Effect GetEffect(Mode mode)
	{
		if (mode == Mode.Basic || mode != Mode.Ex)
		{
			return (bulletNumber != 0) ? null : basicEffectPrefab;
		}
		return exEffectPrefab;
	}

	protected override AbstractProjectile fireBasic()
	{
		float damage = WeaponProperties.PlaneWeaponChaliceWay.Basic.damage;
		BasicProjectile basicProjectile = null;
		float z = 0f;
		float angle = WeaponProperties.PlaneWeaponChaliceWay.Basic.angle;
		bulletNumber = 0;
		while ((float)bulletNumber < 3f)
		{
			if (bulletNumber > 0)
			{
				z = ((!((float)bulletNumber >= 2f)) ? (0f - angle) : angle);
			}
			basicProjectile = base.fireBasic() as BasicProjectile;
			basicProjectile.Speed = WeaponProperties.PlaneWeaponChaliceWay.Basic.speed;
			basicProjectile.DestroyDistance = WeaponProperties.PlaneWeaponChaliceWay.Basic.distance - 20f * (float)(bulletNumber + 1);
			basicProjectile.Damage = damage;
			basicProjectile.PlayerId = player.id;
			basicProjectile.transform.AddEulerAngles(0f, 0f, z);
			basicProjectile.transform.position += Vector3.right * ((bulletNumber != 0) ? (-40f) : 40f);
			Animator component = basicProjectile.GetComponent<Animator>();
			component.Play(((bulletNumber + animatorOffset) % 3).ToString(), 0, Random.Range(0f, 1f));
			AudioManager.Play("player_plane_weapon_chalice");
			emitAudioFromObject.Add("player_plane_weapon_chalice");
			bulletNumber++;
		}
		animatorOffset++;
		return basicProjectile;
	}

	protected override AbstractProjectile fireEx()
	{
		PlaneWeaponChalice3WayExProjectile[] array = new PlaneWeaponChalice3WayExProjectile[2];
		for (int i = 0; i < 2; i++)
		{
			array[i] = base.fireEx() as PlaneWeaponChalice3WayExProjectile;
			array[i].Damage = WeaponProperties.PlaneWeaponChaliceWay.Ex.damageBeforeLaunch;
			array[i].PlayerId = player.id;
			array[i].arcSpeed = WeaponProperties.PlaneWeaponChaliceWay.Ex.arcSpeed;
			array[i].arcX = WeaponProperties.PlaneWeaponChaliceWay.Ex.arcX;
			array[i].arcY = WeaponProperties.PlaneWeaponChaliceWay.Ex.arcY;
			array[i].damageAfterLaunch = WeaponProperties.PlaneWeaponChaliceWay.Ex.damageAfterLaunch;
			array[i].pauseTime = WeaponProperties.PlaneWeaponChaliceWay.Ex.pauseTime;
			array[i].FreezeTime = WeaponProperties.PlaneWeaponChaliceWay.Ex.freezeTime;
			array[i].speedAfterLaunch = WeaponProperties.PlaneWeaponChaliceWay.Ex.speedAfterLaunch;
			array[i].accelAfterLaunch = WeaponProperties.PlaneWeaponChaliceWay.Ex.accelAfterLaunch;
			array[i].minXDistance = WeaponProperties.PlaneWeaponChaliceWay.Ex.minXDistance;
			array[i].xDistanceNoTarget = WeaponProperties.PlaneWeaponChaliceWay.Ex.xDistanceNoTarget;
			array[i].transform.parent = base.transform;
			array[i].SetArcPosition();
			array[i].vDirection = ((i != 0) ? 1 : (-1));
			array[i].DamageRate = WeaponProperties.PlaneWeaponChaliceWay.Ex.damageRateBeforeLaunch;
			array[i].CollisionDeath.OnlyBounds();
			array[i].ID = i;
			MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Ex);
			meterScoreTracker.Add(array[i]);
		}
		array[0].partner = array[1];
		array[1].partner = array[0];
		return null;
	}
}
