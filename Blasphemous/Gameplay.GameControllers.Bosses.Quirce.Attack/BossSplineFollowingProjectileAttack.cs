using System;
using BezierSplines;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce.Attack;

public class BossSplineFollowingProjectileAttack : EnemyAttack, IProjectileAttack
{
	private Hit _weaponHit;

	public GameObject projectilePrefab;

	public Transform projectileSource;

	public int poolSize = 3;

	public Vector2 lastPosition;

	private SplineFollowingProjectile _currentProjectile;

	public float PathFollowingProjectileDamage;

	public event Action<BossSplineFollowingProjectileAttack, float, float> OnPathAdvanced;

	public event Action<BossSplineFollowingProjectileAttack> OnPathFinished;

	protected override void OnStart()
	{
		base.OnStart();
		PoolManager.Instance.CreatePool(projectilePrefab, poolSize);
		_weaponHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = base.EntityOwner.Stats.Strength.Final,
			DamageType = DamageType,
			Force = Force,
			HitSoundId = HitSound,
			Unnavoidable = false
		};
	}

	public void SetProjectileWeaponDamage(int damage)
	{
		if (damage > 0)
		{
			PathFollowingProjectileDamage = damage;
		}
	}

	public void SetProjectileWeaponDamage(Projectile projectile, int damage)
	{
		SetProjectileWeaponDamage(damage);
		if (!(projectile == null))
		{
			PathFollowingProjectile component = projectile.GetComponent<PathFollowingProjectile>();
			if ((bool)component)
			{
				component.SetDamage(damage);
			}
		}
	}

	public void Shoot(BezierSpline spline, AnimationCurve curve, float totalSeconds = 4f)
	{
		base.CurrentWeaponAttack();
		GameObject gameObject = PoolManager.Instance.ReuseObject(projectilePrefab, projectileSource.position, Quaternion.identity).GameObject;
		SplineFollowingProjectile component = gameObject.GetComponent<SplineFollowingProjectile>();
		component.Init(component.transform.position, spline, totalSeconds, curve);
		_weaponHit.AttackingEntity = component.gameObject;
		PathFollowingProjectile component2 = component.GetComponent<PathFollowingProjectile>();
		if (component2 != null)
		{
			PathFollowingProjectile component3 = component.GetComponent<PathFollowingProjectile>();
			if ((bool)component3)
			{
				component3.SetHit(_weaponHit);
			}
		}
		SetProjectileWeaponDamage(component, (int)PathFollowingProjectileDamage);
		component.OnSplineCompletedEvent += OnSplineCompleted;
		component.OnSplineAdvancedEvent += OnSplineAdvanced;
		_currentProjectile = component;
	}

	public void Shoot(BezierSpline spline, AnimationCurve curve, float totalSeconds, Vector3 origin)
	{
		spline.SetControlPoint(0, spline.transform.InverseTransformPoint(origin));
		spline.SetControlPoint(spline.ControlPointCount - 1, spline.transform.InverseTransformPoint(origin));
		Shoot(spline, curve, totalSeconds);
	}

	public void Shoot(BezierSpline spline, AnimationCurve curve, float totalSeconds, Vector3 origin, Vector3 end)
	{
		spline.SetControlPoint(0, spline.transform.InverseTransformPoint(origin));
		spline.SetControlPoint(spline.ControlPointCount - 1, spline.transform.InverseTransformPoint(end));
		Shoot(spline, curve, totalSeconds);
	}

	public SplineFollowingProjectile GetCurrentProjectile()
	{
		return _currentProjectile;
	}

	private void OnSplineAdvanced(SplineFollowingProjectile p, float maxS, float elapS)
	{
		if (this.OnPathAdvanced != null)
		{
			this.OnPathAdvanced(this, maxS, elapS);
		}
	}

	private void OnSplineCompleted(SplineFollowingProjectile obj)
	{
		obj.OnSplineCompletedEvent -= OnSplineCompleted;
		lastPosition = obj.transform.position;
		Debug.Log("ATTACK: SPLINE COMPLETED!!");
		if (this.OnPathFinished != null)
		{
			this.OnPathFinished(this);
		}
	}
}
