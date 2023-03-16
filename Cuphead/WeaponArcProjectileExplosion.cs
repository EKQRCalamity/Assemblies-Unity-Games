using UnityEngine;

public class WeaponArcProjectileExplosion : Effect
{
	private DamageDealer damageDealer;

	public DamageDealer DamageDealer => damageDealer;

	public WeaponArcProjectileExplosion Create(Vector2 position, float damage, float damageMultiplier, PlayerId playerId)
	{
		WeaponArcProjectileExplosion weaponArcProjectileExplosion = base.Create(position) as WeaponArcProjectileExplosion;
		weaponArcProjectileExplosion.damageDealer.SetDamage(damage);
		weaponArcProjectileExplosion.damageDealer.DamageMultiplier *= damageMultiplier;
		weaponArcProjectileExplosion.damageDealer.SetDamageFlags(damagesPlayer: false, damagesEnemy: true, damagesOther: false);
		weaponArcProjectileExplosion.damageDealer.PlayerId = playerId;
		return weaponArcProjectileExplosion;
	}

	protected override void Awake()
	{
		base.Awake();
		damageDealer = new DamageDealer(1f, 0f);
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionEnemy(hit, phase);
		if (phase == CollisionPhase.Enter && damageDealer != null)
		{
			damageDealer.DealDamage(hit);
		}
	}
}
