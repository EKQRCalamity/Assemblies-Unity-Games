using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.BellGhost;
using Gameplay.GameControllers.Entities;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Projectiles;

public class BouncingProjectile : StraightProjectile
{
	public float yImpulse = 10f;

	public float gravity = 9.8f;

	public EntityMotionChecker motionChecker;

	public int hits = 5;

	[EventRef]
	public string BounceFxSound;

	public GameObject splashOnBounce;

	public ProjectileWeapon projectileWeapon;

	public bool loseImpulseAfterBounce;

	public float instantiationCooldown = 0.3f;

	private float instantiationCounter;

	private float currentYImpulse;

	private int currentHits;

	protected override void OnStart()
	{
		base.OnStart();
		projectileWeapon = GetComponent<ProjectileWeapon>();
		PoolManager.Instance.CreatePool(splashOnBounce, 5);
		instantiationCounter = -1f;
		currentYImpulse = yImpulse;
		currentHits = hits;
	}

	protected override void OnUpdate()
	{
		base.transform.Translate(velocity * Time.deltaTime, Space.World);
		UpdateGravity();
		CheckRebound();
		UpdateOrientation();
		UpdateRotation();
		UpdateCounter();
	}

	private void UpdateCounter()
	{
		if (instantiationCounter > 0f)
		{
			instantiationCounter -= Time.deltaTime;
		}
	}

	private void UpdateRotation()
	{
		if (faceVelocityDirection)
		{
			Vector2 normalized = velocity.normalized;
			float num = 57.29578f * Mathf.Atan2(normalized.y, normalized.x);
			if (normalized.x < 0f)
			{
				num += 180f;
			}
			spriteRenderer.transform.eulerAngles = new Vector3(0f, 0f, num);
			motionChecker.transform.eulerAngles = new Vector3(num, 0f, 0f);
		}
	}

	public override void Init(Vector3 direction, float speed)
	{
		base.Init(direction, speed);
	}

	private void UpdateGravity()
	{
		velocity.y -= gravity * Time.deltaTime;
	}

	public bool CanInstanceOnBounce()
	{
		return instantiationCounter <= 0f;
	}

	private void CheckRebound()
	{
		if (motionChecker.HitsFloor)
		{
			velocity.y = currentYImpulse;
			if (CanInstanceOnBounce())
			{
				PoolManager.Instance.ReuseObject(splashOnBounce, base.transform.position, Quaternion.identity);
				instantiationCounter = instantiationCooldown;
			}
			if (loseImpulseAfterBounce)
			{
				currentYImpulse *= 0.95f;
				if (currentYImpulse <= 2f)
				{
					Deactivate();
				}
			}
			if (!BounceFxSound.IsNullOrWhitespace())
			{
				Core.Audio.PlaySfx(BounceFxSound);
			}
		}
		else if (motionChecker.HitsBlock)
		{
			velocity.x *= -1f;
			currentHits--;
			if (!BounceFxSound.IsNullOrWhitespace())
			{
				Core.Audio.PlaySfx(BounceFxSound);
			}
			if (currentHits == 0)
			{
				Deactivate();
			}
		}
	}

	private void Deactivate()
	{
		if (projectileWeapon != null)
		{
			currentYImpulse = yImpulse;
			currentHits = hits;
			projectileWeapon.ForceDestroy();
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
