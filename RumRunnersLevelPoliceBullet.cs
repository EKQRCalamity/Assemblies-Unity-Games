using UnityEngine;

public class RumRunnersLevelPoliceBullet : BasicProjectile
{
	private DamageDealer spiderDamageDealer;

	public float spiderDamage { get; set; }

	public RumRunnersLevelPoliceman.Direction direction { get; set; }

	protected override void Start()
	{
		base.Start();
		spiderDamageDealer = new DamageDealer(this);
		spiderDamageDealer.SetDamage(spiderDamage);
		spiderDamageDealer.OnDealDamage += OnDealDamage;
		spiderDamageDealer.SetStoneTime(base.StoneTime);
		spiderDamageDealer.PlayerId = PlayerId;
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		if (phase == CollisionPhase.Exit)
		{
			return;
		}
		DamageReceiver damageReceiver = hit.GetComponent<DamageReceiver>();
		if (damageReceiver == null)
		{
			DamageReceiverChild component = hit.GetComponent<DamageReceiverChild>();
			if (component != null)
			{
				damageReceiver = component.Receiver;
			}
		}
		if (damageReceiver != null && damageReceiver.GetComponent<RumRunnersLevelSpider>() != null)
		{
			spiderDamageDealer.DealDamage(hit);
			base.OnCollisionEnemy(hit, phase);
			Die();
		}
	}
}
