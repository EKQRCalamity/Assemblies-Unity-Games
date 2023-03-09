using UnityEngine;

public class PlaneWeaponBombExplosion : Effect
{
	private DamageDealer damageDealer;

	public void Create(Vector2 position, float damage, float damageMultiplier, float size)
	{
		PlaneWeaponBombExplosion planeWeaponBombExplosion = base.Create(position) as PlaneWeaponBombExplosion;
		planeWeaponBombExplosion.damageDealer.SetDamage(damage);
		planeWeaponBombExplosion.damageDealer.DamageMultiplier *= damageMultiplier;
		planeWeaponBombExplosion.damageDealer.SetDamageFlags(damagesPlayer: false, damagesEnemy: true, damagesOther: false);
		planeWeaponBombExplosion.transform.SetScale(size, size);
	}

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
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
