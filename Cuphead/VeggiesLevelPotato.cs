using System.Collections;
using UnityEngine;

public class VeggiesLevelPotato : LevelProperties.Veggies.Entity
{
	public enum State
	{
		Incomplete,
		Complete
	}

	public delegate void OnDamageTakenHandler(float damage);

	private const float START_SHOOTING_TIME = 0.6f;

	[SerializeField]
	private Transform gunRoot;

	[SerializeField]
	private VeggiesLevelSpit projectilePrefab;

	[SerializeField]
	private Effect spitEffect;

	private new LevelProperties.Veggies.Potato properties;

	private float hp;

	private DamageDealer damageDealer;

	private bool didShoot = true;

	private bool projectileParryFlag;

	public State state { get; private set; }

	public event OnDamageTakenHandler OnDamageTakenEvent;

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
		damageDealer.SetDirection(DamageDealer.Direction.Neutral, base.transform);
	}

	private void Start()
	{
		SfxGround();
	}

	public override void LevelInitWithGroup(AbstractLevelPropertyGroup propertyGroup)
	{
		base.LevelInitWithGroup(propertyGroup);
		properties = propertyGroup as LevelProperties.Veggies.Potato;
		hp = properties.hp;
		GetComponent<DamageReceiver>().OnDamageTaken += OnDamageTaken;
		StartCoroutine(potato_cr());
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (this.OnDamageTakenEvent != null)
		{
			this.OnDamageTakenEvent(info.damage);
		}
		hp -= info.damage;
		if (hp <= 0f)
		{
			Die();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void Die()
	{
		StopAllCoroutines();
		GetComponent<Collider2D>().enabled = false;
		base.animator.SetTrigger("Dead");
	}

	private void StartExplosions()
	{
		GetComponent<LevelBossDeathExploder>().StartExplosion();
	}

	private void EndExplosions()
	{
		GetComponent<LevelBossDeathExploder>().StopExplosions();
	}

	private void SfxGround()
	{
		AudioManager.Play("level_veggies_potato_ground");
	}

	private void OnInAnimComplete()
	{
	}

	private void OnDeathAnimComplete()
	{
		state = State.Complete;
		Object.Destroy(base.gameObject);
	}

	private void Shoot()
	{
		if (!projectileParryFlag)
		{
			AudioManager.Play("levels_veggies_potato_spit");
		}
		else
		{
			AudioManager.Play("level_veggies_potato_spit_worm");
		}
		didShoot = true;
		BasicProjectile basicProjectile = projectilePrefab.Create(gunRoot.position, gunRoot.eulerAngles.z, properties.bulletSpeed);
		basicProjectile.SetParryable(projectileParryFlag);
		spitEffect.Create(gunRoot.position);
	}

	private IEnumerator potato_cr()
	{
		while (true)
		{
			int groups = 0;
			int shots = 0;
			while (groups < properties.seriesCount)
			{
				float delay = properties.bulletDelay.GetFloatAt(1f - (float)groups / ((float)properties.seriesCount - 1f));
				while (shots < properties.bulletCount)
				{
					shots++;
					base.animator.SetTrigger("Shoot");
					didShoot = false;
					projectileParryFlag = shots == properties.bulletCount;
					while (!didShoot)
					{
						yield return null;
					}
					yield return CupheadTime.WaitForSeconds(this, delay);
				}
				groups++;
				shots = 0;
				if (groups != properties.seriesCount)
				{
					yield return CupheadTime.WaitForSeconds(this, properties.seriesDelay);
					yield return CupheadTime.WaitForSeconds(this, 0.6f);
				}
			}
			yield return CupheadTime.WaitForSeconds(this, properties.idleTime);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		projectilePrefab = null;
		spitEffect = null;
	}
}
