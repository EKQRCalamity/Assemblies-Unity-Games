using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Enemies.BellGhost;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Tools.Items;

public class PenitentCrawlerOrbsEffect : ObjectEffect
{
	private Penitent _owner;

	private BossStraightProjectileAttack _crawlerOrbs;

	public int DamageAmount = 1;

	protected override bool OnApplyEffect()
	{
		_owner = Core.Logic.Penitent;
		_crawlerOrbs = _owner.GetComponentInChildren<PrayerUse>().crawlerBallsPrayer;
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("SimpleHit");
		float final = _owner.Stats.PrayerStrengthMultiplier.Final;
		StraightProjectile straightProjectile = _crawlerOrbs.Shoot(Vector2.right, Vector2.right * 0.01f, final);
		straightProjectile.GetComponent<ProjectileWeapon>().SetDamage(DamageAmount);
		straightProjectile = _crawlerOrbs.Shoot(Vector2.left, Vector2.left * 0.01f, final);
		straightProjectile.GetComponent<ProjectileWeapon>().SetDamage(DamageAmount);
		return base.OnApplyEffect();
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
