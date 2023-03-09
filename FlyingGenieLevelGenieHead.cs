using UnityEngine;

public class FlyingGenieLevelGenieHead : AbstractProjectile
{
	[SerializeField]
	private Effect headExplode;

	[SerializeField]
	private SpriteRenderer darkSprite;

	private DamageReceiver damageReceiver;

	private FlyingGenieLevelGenie parent;

	private float health;

	public void Init(Vector3 pos, float health, FlyingGenieLevelGenie parent)
	{
		base.transform.position = pos;
		this.parent = parent;
		this.health = health;
	}

	protected override void Start()
	{
		base.Start();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		darkSprite.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		health -= info.damage;
		if (health < 0f)
		{
			Die();
		}
		parent.DoDamage(info.damage);
	}

	protected override void Die()
	{
		AudioManager.Play("genie_pillar_destruction");
		emitAudioFromObject.Add("genie_pillar_destruction");
		headExplode.Create(new Vector3(base.transform.position.x - 75f, base.transform.position.y));
		GetComponent<SpriteRenderer>().enabled = false;
		darkSprite.GetComponent<SpriteRenderer>().enabled = false;
		base.Die();
	}
}
