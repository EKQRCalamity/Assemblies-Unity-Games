using UnityEngine;

public class AirplaneLevelTurretDog : AbstractPausableComponent
{
	private const float BALL_OFFSET = 30f;

	[SerializeField]
	private AirplaneLevelTurretBullet bulletPrefab;

	[SerializeField]
	private Transform rootPos;

	[SerializeField]
	private Effect FX;

	private float velocityX;

	private float velocityY;

	private float gravity;

	private void ShootProjectile()
	{
		FX.Create(base.transform.position, base.transform.localScale);
		Vector3 vector = new Vector3(rootPos.position.x + 30f, rootPos.position.y);
		Vector3 vector2 = new Vector3(rootPos.position.x - 30f, rootPos.position.y);
		AirplaneLevelTurretBullet airplaneLevelTurretBullet = bulletPrefab.Create(vector, new Vector3(velocityX, velocityY), gravity);
		airplaneLevelTurretBullet.GetComponent<SpriteRenderer>().sortingOrder = 1;
		airplaneLevelTurretBullet.GetComponent<Animator>().Play("TennisBallA");
		AirplaneLevelTurretBullet airplaneLevelTurretBullet2 = bulletPrefab.Create(rootPos.position, new Vector3(0f, velocityY), gravity);
		airplaneLevelTurretBullet2.GetComponent<SpriteRenderer>().sortingOrder = 2;
		airplaneLevelTurretBullet2.GetComponent<Animator>().Play("TennisBallB");
		AirplaneLevelTurretBullet airplaneLevelTurretBullet3 = bulletPrefab.Create(vector2, new Vector3(0f - velocityX, velocityY), gravity);
		airplaneLevelTurretBullet3.GetComponent<SpriteRenderer>().sortingOrder = 1;
		airplaneLevelTurretBullet3.GetComponent<Animator>().Play("TennisBallC");
	}

	public void StartAttack(float velocityX, float velocityY, float gravity)
	{
		base.animator.Play("Flap");
		this.velocityX = velocityX;
		this.velocityY = velocityY;
		this.gravity = gravity;
	}

	private void AnimationEvent_SFX_DOGFIGHT_BulldogPlane_TurretDogHatchOpen()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_terrierplane_hatchopen");
	}

	private void AnimationEvent_SFX_DOGFIGHT_BulldogPlane_TurretDogBark()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_terrierplane_bark");
	}

	private void AnimationEvent_SFX_DOGFIGHT_BulldogPlane_TurretDogWhistle()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_terrierplane_baseball_whistle");
	}

	private void AnimationEvent_SFX_DOGFIGHT_BulldogPlane_TurretDogToss()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_terrierplane_baseball_toss");
	}
}
