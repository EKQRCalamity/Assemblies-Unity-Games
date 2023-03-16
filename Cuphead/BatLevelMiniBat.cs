using UnityEngine;

public class BatLevelMiniBat : BasicSineProjectile
{
	private DamageReceiver damageReceiver;

	private float health;

	public BatLevelMiniBat Create(Vector2 pos, float rotation, float velocity, float sinVelocity, float sinSize, float health)
	{
		BatLevelMiniBat batLevelMiniBat = Create(pos, rotation, velocity, sinVelocity, sinSize) as BatLevelMiniBat;
		batLevelMiniBat.health = health;
		return batLevelMiniBat;
	}

	protected override void Awake()
	{
		base.Awake();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		health -= info.damage;
		if (health < 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
