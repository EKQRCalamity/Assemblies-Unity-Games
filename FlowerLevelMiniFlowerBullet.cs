using System.Collections;
using UnityEngine;

public class FlowerLevelMiniFlowerBullet : AbstractProjectile
{
	[SerializeField]
	private Effect puff;

	private bool friendlyFire;

	private bool initDamage;

	private float damage;

	private int bulletSpeed;

	private Vector3 targetDirection;

	public void OnBulletSpawned(Vector3 target, int speed, float damage, bool friendlyFireDamage = false)
	{
		friendlyFire = friendlyFireDamage;
		targetDirection = (target - base.transform.position).normalized;
		bulletSpeed = speed;
		this.damage = damage;
		StartCoroutine(spawn_fx_cr());
	}

	protected override void Awake()
	{
		initDamage = false;
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		if (!initDamage)
		{
			damageDealer.SetDamage(damage);
			damageDealer.SetDamageFlags(damagesPlayer: true, friendlyFire, damagesOther: false);
			initDamage = true;
		}
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
		base.Update();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		base.transform.position += targetDirection * bulletSpeed * CupheadTime.FixedDelta;
		base.transform.up = -targetDirection;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
			Die();
		}
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		if (friendlyFire && hit.GetComponent<FlowerLevelFlower>() != null)
		{
			base.OnCollisionEnemy(hit, phase);
			damageDealer.DealDamage(hit);
			Die();
		}
		base.OnCollisionEnemy(hit, phase);
	}

	protected override void OnCollisionGround(GameObject hit, CollisionPhase phase)
	{
		Die();
		base.OnCollisionGround(hit, phase);
	}

	protected override void Die()
	{
		bulletSpeed = 0;
		base.transform.Rotate(Vector3.forward, 360f);
		StopAllCoroutines();
		AudioManager.Play("flower_minion_simple_deathpop_high");
		emitAudioFromObject.Add("flower_minion_simple_deathpop_high");
		base.Die();
	}

	private IEnumerator spawn_fx_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.17f);
		while (true)
		{
			puff.Create(base.transform.position).transform.SetEulerAngles(null, null, MathUtils.DirectionToAngle(targetDirection));
			yield return CupheadTime.WaitForSeconds(this, 0.2f);
		}
	}
}
