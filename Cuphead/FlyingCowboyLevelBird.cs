using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelBird : AbstractProjectile
{
	[SerializeField]
	private FlyingCowboyLevelBirdProjectile projectilePrefab;

	[SerializeField]
	private SpriteRenderer holdingFeetRenderer;

	[SerializeField]
	private SpriteRenderer emptyFeetRenderer;

	[SerializeField]
	private Transform projectileSpawnPoint;

	private LevelProperties.FlyingCowboy.Bird properties;

	private FlyingCowboyLevelCowboy cowgirl;

	private float bulletLandingPosition;

	private bool projectileSpawned;

	public void Initialize(Vector3 startPosition, Vector3 endPosition, float bulletLandingPosition, LevelProperties.FlyingCowboy.Bird properties, FlyingCowboyLevelCowboy cowgirl)
	{
		this.bulletLandingPosition = bulletLandingPosition;
		this.properties = properties;
		this.cowgirl = cowgirl;
		StartCoroutine(move_cr(startPosition, endPosition, properties));
		StartCoroutine(attack_cr());
	}

	public void InitializeIntro(Vector3 startPosition)
	{
		base.transform.position = startPosition;
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		component.sortingLayerName = "Default";
		component.sortingOrder = -120;
		base.animator.Play("Return");
	}

	public void MoveIntro(Vector3 endPosition, LevelProperties.FlyingCowboy.Bird properties)
	{
		StartCoroutine(moveIntro_cr(endPosition, properties));
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void move()
	{
		Vector3 position = base.transform.position;
		position.x += properties.speed * CupheadTime.FixedDelta;
		base.transform.position = position;
	}

	private IEnumerator move_cr(Vector3 startPosition, Vector3 endPosition, LevelProperties.FlyingCowboy.Bird properties)
	{
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		while (base.transform.position.x < endPosition.x - 60f)
		{
			yield return wait;
			move();
		}
		while (base.animator.GetCurrentAnimatorStateInfo(1).IsName("Throw"))
		{
			yield return wait;
			move();
		}
		float normalizedTime = base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		while (normalizedTime >= 0.18181819f && normalizedTime <= 0.8181818f)
		{
			yield return wait;
			move();
			normalizedTime = base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}
		base.animator.Play("Turn");
		float slowdownTime = KinematicUtilities.CalculateTimeToChangeVelocity(properties.speed, 0f, 60f);
		float elapsedTime2 = 0f;
		while (elapsedTime2 < slowdownTime)
		{
			yield return wait;
			elapsedTime2 += CupheadTime.FixedDelta;
			Vector3 position = base.transform.position;
			position.x += Mathf.Lerp(properties.speed, 0f, elapsedTime2 / slowdownTime) * CupheadTime.FixedDelta;
			base.transform.position = position;
		}
		yield return base.animator.WaitForNormalizedTime(this, 0.75f, "Turn");
		elapsedTime2 = 0f;
		while (base.transform.position.x > startPosition.x)
		{
			yield return wait;
			elapsedTime2 += CupheadTime.FixedDelta;
			float speed = Mathf.Lerp(0f, properties.speed, elapsedTime2 / 0.25f);
			Vector3 position2 = base.transform.position;
			position2.x -= properties.speed * CupheadTime.FixedDelta;
			base.transform.position = position2;
		}
		Object.Destroy(base.gameObject);
	}

	private IEnumerator moveIntro_cr(Vector3 endPosition, LevelProperties.FlyingCowboy.Bird properties)
	{
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		while (base.transform.position.x > endPosition.x)
		{
			yield return wait;
			Vector3 position = base.transform.position;
			position.x -= properties.speed * CupheadTime.FixedDelta;
			base.transform.position = position;
		}
		Object.Destroy(base.gameObject);
	}

	private IEnumerator attack_cr()
	{
		if (!(bulletLandingPosition > -400f))
		{
			while (base.transform.position.x < -385f)
			{
				yield return null;
			}
			if (!projectileSpawned && !base.animator.GetCurrentAnimatorStateInfo(0).IsName("Turn"))
			{
				base.animator.RoundFrame();
				base.animator.Play("Throw", 1);
				yield return base.animator.WaitForNormalizedTime(this, 1f, "Throw", 1, allowEqualTime: true);
				base.animator.Play("Off", 1);
				holdingFeetRenderer.enabled = false;
				emptyFeetRenderer.enabled = true;
			}
		}
	}

	private void spawnProjectile()
	{
		if (!projectileSpawned)
		{
			projectileSpawned = true;
			SFX_COWGIRL_COWGIRL_P1_BirdCall();
			Vector3 position = projectileSpawnPoint.position;
			float num = KinematicUtilities.CalculateInitialSpeedToReachApex(properties.bulletArcHeight, properties.bulletGravity);
			float num2 = bulletLandingPosition - position.x;
			float x = KinematicUtilities.CalculateHorizontalSpeedToTravelDistance(num2, num, position.y - FlyingCowboyLevelBirdProjectile.HighLandingPosition, properties.bulletGravity);
			Vector2 initialVelocity = new Vector2(x, num);
			float num3 = Mathf.Atan2(initialVelocity.y, initialVelocity.x) * 57.29578f;
			FlyingCowboyLevelBirdProjectile flyingCowboyLevelBirdProjectile = projectilePrefab.Create(projectileSpawnPoint.position) as FlyingCowboyLevelBirdProjectile;
			flyingCowboyLevelBirdProjectile.Initialize(initialVelocity, properties.bulletGravity, properties.shrapnelSecondStageDelay, properties.shrapnelSpeed, properties.shrapnelSpreadAngle, cowgirl);
			flyingCowboyLevelBirdProjectile.shrapnelCount = properties.shrapnelCount;
		}
	}

	private void animationEvent_SpawnProjectile()
	{
		spawnProjectile();
	}

	private void animationEvent_ShiftLayers()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			spriteRenderer.sortingLayerName = "Background";
		}
	}

	private void SFX_COWGIRL_COWGIRL_P1_BirdCall()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p1_birdcall");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p1_birdcall");
	}
}
