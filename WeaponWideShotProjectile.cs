using UnityEngine;

public class WeaponWideShotProjectile : BasicProjectile
{
	private const float HITSPARK_OFFSET = 100f;

	[SerializeField]
	private Effect hitSpark;

	protected override void Start()
	{
		base.Start();
		damageDealer.isDLCWeapon = true;
		GetComponent<SpriteRenderer>().flipY = Rand.Bool();
	}

	protected override void OnDealDamage(float damage, DamageReceiver receiver, DamageDealer damageDealer)
	{
		base.OnDealDamage(damage, receiver, damageDealer);
		hitSpark.Create(base.transform.position + base.transform.right * 100f);
	}

	protected override void OnCollisionDie(GameObject hit, CollisionPhase phase)
	{
		hitSpark.Create(base.transform.position + base.transform.right * 100f);
		base.OnCollisionDie(hit, phase);
		Object.Destroy(base.gameObject);
	}
}
