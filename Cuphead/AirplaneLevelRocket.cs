using System.Collections;
using UnityEngine;

public class AirplaneLevelRocket : HomingProjectile
{
	private float homingTimer;

	private float health;

	[SerializeField]
	private Transform effectRoot;

	[SerializeField]
	private Effect effectFX;

	[SerializeField]
	private Effect deathFX;

	[SerializeField]
	private Effect deathOnPlaneFX;

	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private MinMax fxSpawnRate;

	public AirplaneLevelRocket Create(AbstractPlayerController player, Vector2 pos, float speed, float rotationSpeed, float health, float homingTime)
	{
		AirplaneLevelRocket airplaneLevelRocket = Create(pos, -90f, speed, speed, rotationSpeed, DestroyLifetime, 0f, player) as AirplaneLevelRocket;
		airplaneLevelRocket.DamagesType.OnlyPlayer();
		airplaneLevelRocket.Init(health);
		airplaneLevelRocket.homingTimer = homingTime;
		return airplaneLevelRocket;
	}

	protected override void Start()
	{
		base.Start();
		StartCoroutine(sfx_rocket_spawn_cr());
		StartCoroutine(spawn_effect_cr());
	}

	private void Init(float health)
	{
		this.health = health;
		GetComponent<DamageReceiver>().OnDamageTaken += OnDamageTaken;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		AudioManager.Play("sfx_DLC_Dogfight_P1_HydrantMissile_Impact");
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (!(health <= 0f))
		{
			health -= info.damage;
			if (health <= 0f)
			{
				Level.Current.RegisterMinionKilled();
				Die();
			}
		}
	}

	private IEnumerator continue_without_homing_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			base.transform.AddPosition(velocity.x * CupheadTime.FixedDelta, velocity.y * CupheadTime.FixedDelta);
			yield return wait;
		}
	}

	protected override void Die()
	{
		base.Die();
		StopAllCoroutines();
		GetComponent<Collider2D>().enabled = false;
		sprite.GetComponent<SpriteRenderer>().enabled = false;
		GameObject gameObject = GameObject.Find("BullDogPlane");
		if ((bool)gameObject && Mathf.Abs(gameObject.transform.position.x - base.transform.position.x) < 800f && Mathf.Abs(gameObject.transform.position.y - base.transform.position.y) < 175f)
		{
			deathOnPlaneFX.Create(base.transform.position);
		}
		else
		{
			deathFX.Create(base.transform.position);
		}
		AudioManager.Play("sfx_DLC_Dogfight_P1_HydrantMissile_DeathExplode");
	}

	private IEnumerator spawn_effect_cr()
	{
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, fxSpawnRate.RandomFloat());
			effectFX.Create(effectRoot.position);
			AudioManager.Play("sfx_DLC_Dogfight_P1_HydrantMissile_Chuff");
			yield return null;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (homingTimer > 0f)
		{
			homingTimer -= CupheadTime.Delta;
			if (homingTimer <= 0f)
			{
				StopAllCoroutines();
				StartCoroutine(continue_without_homing_cr());
			}
		}
	}

	private IEnumerator sfx_rocket_spawn_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		AudioManager.Play("sfx_DLC_Dogfight_P1_HydrantMissile_Entrance");
	}
}
