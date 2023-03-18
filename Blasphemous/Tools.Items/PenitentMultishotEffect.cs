using System.Collections;
using Framework.FrameworkCore;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Tools.Items;

public class PenitentMultishotEffect : ObjectEffect
{
	private Penitent _owner;

	private BossInstantProjectileAttack _instantProjectileAttack;

	public int DamageAmount = 1;

	protected override bool OnApplyEffect()
	{
		Debug.Log("PENITENT VERTICAL BEAM EFFECT");
		StartCoroutine(MultiShotCoroutine());
		return base.OnApplyEffect();
	}

	private float CalculateDamageStrength(float prayerStrMult)
	{
		return 1f + 0.35f * (prayerStrMult - 1f);
	}

	private IEnumerator MultiShotCoroutine()
	{
		_owner = Core.Logic.Penitent;
		_instantProjectileAttack = _owner.GetComponentInChildren<PrayerUse>().multishotPrayer;
		_instantProjectileAttack.SetDamageStrength(CalculateDamageStrength(_owner.Stats.PrayerStrengthMultiplier.Final));
		_instantProjectileAttack.SetDamage(DamageAmount);
		Vector2 dir = Vector2.right * ((_owner.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
		_instantProjectileAttack.transform.localPosition = dir;
		Vector3 projectilePosition = _instantProjectileAttack.transform.position;
		_instantProjectileAttack.Shoot(projectilePosition, dir);
		yield return new WaitForSeconds(0.15f);
		float randomOff2 = Random.Range(-1f, 1f) * 1f;
		_instantProjectileAttack.Shoot(projectilePosition + Vector3.up * randomOff2, dir);
		yield return new WaitForSeconds(0.15f);
		randomOff2 = Random.Range(-1f, 1f) * 1f;
		_instantProjectileAttack.Shoot(projectilePosition + Vector3.up * randomOff2, dir);
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	private void Update()
	{
	}

	protected override void OnRemoveEffect()
	{
		base.OnRemoveEffect();
	}
}
