using System.Collections;
using Gameplay.GameControllers.Enemies.BellGhost;
using Gameplay.GameControllers.Enemies.Projectiles;
using UnityEngine;

public class BulletTimeProjectile : StraightProjectile
{
	public AnimationCurve accelerationCurve;

	public float secondsToAccelerate = 1f;

	public float minVelocity = 1f;

	public float maxVelocity = 10f;

	private float maxVelocityModifier = 1f;

	public bool slowOnHit;

	public float reductionOnHit = 0.5f;

	private ProjectileWeapon projectileWeapon;

	private float counter;

	protected override void OnAwake()
	{
		base.OnAwake();
		projectileWeapon = GetComponent<ProjectileWeapon>();
		if (projectileWeapon != null)
		{
			projectileWeapon.OnProjectileHitsSomething += OnProjectileHitsSomething;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		projectileWeapon.OnProjectileHitsSomething -= OnProjectileHitsSomething;
	}

	private void OnProjectileHitsSomething(ProjectileWeapon obj)
	{
		if (slowOnHit)
		{
			counter = secondsToAccelerate * reductionOnHit;
		}
	}

	public void Accelerate(float maxVelocityModifier = 1f)
	{
		this.maxVelocityModifier = maxVelocityModifier;
		StopAllCoroutines();
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(AccelerateInSeconds(secondsToAccelerate));
		}
	}

	private IEnumerator AccelerateInSeconds(float seconds)
	{
		counter = 0f;
		while (counter < seconds)
		{
			counter += Time.deltaTime;
			float lerpValue = accelerationCurve.Evaluate(counter / seconds);
			velocity = velocity.normalized * Mathf.Lerp(minVelocity, maxVelocity * maxVelocityModifier, lerpValue);
			yield return null;
		}
		velocity = velocity.normalized * maxVelocity * maxVelocityModifier;
	}
}
