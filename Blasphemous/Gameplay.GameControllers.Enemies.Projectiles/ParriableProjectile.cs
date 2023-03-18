using System;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.BellGhost;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Projectiles;

public class ParriableProjectile : StraightProjectile
{
	[SerializeField]
	private string parryAnimationTrigger;

	private ProjectileWeapon projectileWeapon;

	public Animator animator;

	private GhostTrailGenerator ghostTrail;

	public GameObject flipParent;

	public GameObject spawnOnCollision;

	private bool parried;

	public event Action<ParriableProjectile> OnProjectileParried;

	protected override void OnStart()
	{
		projectileWeapon = GetComponent<ProjectileWeapon>();
		if (spawnOnCollision != null)
		{
			PoolManager.Instance.CreatePool(spawnOnCollision, 1);
		}
		projectileWeapon.OnProjectileDeath += ProjectileWeapon_OnProjectileDeath;
		base.OnStart();
	}

	private void ProjectileWeapon_OnProjectileDeath(ProjectileWeapon obj)
	{
		ghostTrail.EnableGhostTrail = false;
		if (spawnOnCollision != null && !parried)
		{
			PoolManager.Instance.ReuseObject(spawnOnCollision, base.transform.position, Quaternion.identity);
		}
	}

	public override void Init(Vector3 origin, Vector3 target, float speed)
	{
		if (ghostTrail == null)
		{
			ghostTrail = GetComponentInChildren<GhostTrailGenerator>();
		}
		ghostTrail.EnableGhostTrail = true;
		PlayFlying();
		parried = false;
		base.Init(origin, target, speed);
	}

	public override void Init(Vector3 direction, float speed)
	{
		if (ghostTrail == null)
		{
			ghostTrail = GetComponentInChildren<GhostTrailGenerator>();
		}
		ghostTrail.EnableGhostTrail = true;
		base.Init(direction, speed);
	}

	private void PlayParry()
	{
		animator.SetTrigger(parryAnimationTrigger);
	}

	private void PlayFlying()
	{
		animator.Play("FLY", 0);
		animator.Play("FLY", 1);
	}

	private void OnParryAnimation()
	{
		PlayParry();
		velocity = Vector2.zero;
		ghostTrail.EnableGhostTrail = false;
	}

	public void OnParry()
	{
		parried = true;
		OnParryAnimation();
		ShakeWave();
		if (this.OnProjectileParried != null)
		{
			this.OnProjectileParried(this);
		}
	}

	public void OnDeathAnimation()
	{
		projectileWeapon.ForceDestroy();
	}

	public void ShakeWave()
	{
		Core.Logic.ScreenFreeze.Freeze(0.05f, 0.2f);
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 0.5f, 0.3f, 2f);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.75f, Vector3.down * 1f, 12, 0.2f, 0.01f, default(Vector3), 0.01f, ignoreTimeScale: true);
	}
}
