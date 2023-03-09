using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCrackshotExProjectile : BasicProjectile
{
	[SerializeField]
	private WeaponCrackshotExProjectileChild childPrefab;

	[SerializeField]
	private Effect shootFXPrefab;

	[SerializeField]
	private Effect launchFXPrefab;

	[SerializeField]
	private Collider2D coll;

	private Vector3 basePos;

	private Vector3 startPos;

	private int shotNumber = 5;

	private float shootTimer;

	private bool timerSet;

	private bool parried;

	private float parryTimeOut;

	private float angle;

	public override float ParryMeterMultiplier => (parryTimeOut != 0f) ? 0.1f : 1f;

	protected override void OnDieDistance()
	{
	}

	protected override void OnDieLifetime()
	{
	}

	protected override void Start()
	{
		base.Start();
		_countParryTowardsScore = false;
		move = false;
		shotNumber = WeaponProperties.LevelWeaponCrackshot.Ex.shotNumber;
		base.transform.position += base.transform.right * 120f;
		angle = base.transform.eulerAngles.z;
		base.transform.eulerAngles = Vector3.zero;
		base.transform.localScale = new Vector3(Mathf.Sign(MathUtils.AngleToDirection(angle).x), 1f);
		basePos = base.transform.position;
		startPos = base.transform.position;
		damageDealer.SetDamage(WeaponProperties.LevelWeaponCrackshot.Ex.collideDamage);
		damageDealer.isDLCWeapon = true;
		SetParryable(WeaponProperties.LevelWeaponCrackshot.Ex.isPink);
		AudioManager.FadeSFXVolume("player_weapon_crackshot_turret_loop", 0.0001f, 0.0001f);
	}

	public void GetReplaced()
	{
		LaunchAtTarget();
	}

	private void AniEvent_StartSpinSFX()
	{
		AudioManager.Play("player_weapon_crackshot_turret_loop_start");
		emitAudioFromObject.Add("player_weapon_crackshot_turret_loop_start");
		AudioManager.PlayLoop("player_weapon_crackshot_turret_loop");
		emitAudioFromObject.Add("player_weapon_crackshot_turret_loop");
		AudioManager.FadeSFXVolumeLinear("player_weapon_crackshot_turret_loop", 0.3f, 1f);
	}

	public override void OnParry(AbstractPlayerController player)
	{
		if (!parried)
		{
			LaunchAtTarget();
		}
		else
		{
			Die();
		}
	}

	private void LaunchAtTarget()
	{
		base.animator.Play("Launch");
		AudioManager.Stop("player_weapon_crackshot_turret_loop");
		AudioManager.Play("player_weapon_crackshot_turret_parry");
		emitAudioFromObject.Add("player_weapon_crackshot_turret_parry");
		Collider2D collider2D = FindTarget();
		if ((bool)collider2D)
		{
			angle = MathUtils.DirectionToAngle(collider2D.bounds.center - base.transform.position);
		}
		Effect effect = launchFXPrefab.Create(base.transform.position);
		effect.transform.eulerAngles = new Vector3(0f, 0f, angle);
		effect.transform.localScale = new Vector3(-1f, MathUtils.PlusOrMinus());
		parried = true;
		base.transform.SetEulerAngles(0f, 0f, angle);
		base.transform.localScale = new Vector3(-1f, 1f);
		Speed = WeaponProperties.LevelWeaponCrackshot.Ex.parryBulletSpeed;
		damageDealer.SetDamage(WeaponProperties.LevelWeaponCrackshot.Ex.parryBulletDamage);
		SetParryable(parryable: false);
		parryTimeOut = WeaponProperties.LevelWeaponCrackshot.Ex.parryTimeOut;
		move = true;
	}

	protected override void Die()
	{
		base.animator.Play("Explode");
		AudioManager.Stop("player_weapon_crackshot_turret_parry");
		AudioManager.Play("player_weapon_crackshot_turret_parryexplode");
		emitAudioFromObject.Add("player_weapon_crackshot_turret_parryexplode");
		base.transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(0, 360));
		base.transform.localScale = new Vector3(MathUtils.PlusOrMinus(), MathUtils.PlusOrMinus());
		move = false;
		coll.enabled = false;
	}

	private void _OnDieAnimComplete()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		AudioManager.FadeSFXVolume("player_weapon_crackshot_turret_loop", 0.0001f, 0.25f);
	}

	private void HandleShot()
	{
		shootTimer -= CupheadTime.FixedDelta;
		if (shootTimer <= 0f)
		{
			Collider2D collider2D = FindTarget();
			if ((bool)collider2D)
			{
				MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Ex);
				BasicProjectile projectile = childPrefab.Create(base.transform.position, MathUtils.DirectionToAngle(collider2D.bounds.center - base.transform.position), WeaponProperties.LevelWeaponCrackshot.Ex.bulletSpeed);
				childPrefab.Damage = WeaponProperties.LevelWeaponCrackshot.Ex.bulletDamage;
				childPrefab.Speed = WeaponProperties.LevelWeaponCrackshot.Ex.bulletSpeed;
				childPrefab.PlayerId = PlayerId;
				meterScoreTracker.Add(projectile);
				StartCoroutine(shoot_stretch_squash_cr());
				shootFXPrefab.Create(base.transform.position + (collider2D.bounds.center - base.transform.position).normalized * 25f);
				AudioManager.Play("player_weapon_crackshot_turret_shoot");
				emitAudioFromObject.Add("player_weapon_crackshot_turret_shoot");
			}
			shotNumber--;
			if (shotNumber == 0)
			{
				base.animator.SetTrigger("Disappear");
				coll.enabled = false;
			}
			else
			{
				shootTimer += WeaponProperties.LevelWeaponCrackshot.Ex.shootDelay;
			}
		}
	}

	private IEnumerator shoot_stretch_squash_cr()
	{
		base.transform.localScale = new Vector3(Mathf.Sign(base.transform.localScale.x) * 1.2f, Mathf.Sign(base.transform.localScale.y) * 1.2f);
		yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
		base.transform.localScale = new Vector3(Mathf.Sign(base.transform.localScale.x) * 1.25f, Mathf.Sign(base.transform.localScale.y) * 1.25f);
		yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
		base.transform.localScale = new Vector3(Mathf.Sign(base.transform.localScale.x) * 1.22f, Mathf.Sign(base.transform.localScale.y) * 1.22f);
		yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
		base.transform.localScale = new Vector3(Mathf.Sign(base.transform.localScale.x) * 1.16f, Mathf.Sign(base.transform.localScale.y) * 1.16f);
		yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
		base.transform.localScale = new Vector3(Mathf.Sign(base.transform.localScale.x), Mathf.Sign(base.transform.localScale.y));
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (parried)
		{
			if (parryTimeOut > 0f)
			{
				parryTimeOut -= CupheadTime.FixedDelta;
				if (parryTimeOut <= 0f)
				{
					parryTimeOut = 0f;
					SetParryable(parryable: true);
				}
			}
			return;
		}
		if (base.lifetime < WeaponProperties.LevelWeaponCrackshot.Ex.timeToHoverPoint)
		{
			basePos = Vector3.Lerp(startPos, startPos + (Vector3)MathUtils.AngleToDirection(angle) * WeaponProperties.LevelWeaponCrackshot.Ex.launchDistance, EaseUtils.EaseOutSine(0f, 1f, base.lifetime / WeaponProperties.LevelWeaponCrackshot.Ex.timeToHoverPoint));
		}
		else
		{
			if (!timerSet)
			{
				timerSet = true;
				basePos = startPos + (Vector3)MathUtils.AngleToDirection(angle) * WeaponProperties.LevelWeaponCrackshot.Ex.launchDistance;
				shootTimer = WeaponProperties.LevelWeaponCrackshot.Ex.shootDelay;
			}
			basePos += Vector3.up * WeaponProperties.LevelWeaponCrackshot.Ex.riseSpeed * CupheadTime.FixedDelta;
			HandleShot();
		}
		if (shotNumber > 0)
		{
			float num = base.lifetime * WeaponProperties.LevelWeaponCrackshot.Ex.hoverSpeed;
			base.transform.position = basePos + new Vector3(Mathf.Cos(num + (float)Math.PI / 2f) * WeaponProperties.LevelWeaponCrackshot.Ex.hoverWidth * (0f - Mathf.Sign(MathUtils.AngleToDirection(angle).x)), Mathf.Sin(num * 2f) * WeaponProperties.LevelWeaponCrackshot.Ex.hoverHeight);
		}
	}

	public Collider2D FindTarget()
	{
		return findBestTarget(AbstractProjectile.FindOverlapScreenDamageReceivers());
	}

	private Collider2D findBestTarget(IEnumerable<DamageReceiver> damageReceivers)
	{
		float num = float.MaxValue;
		Collider2D result = null;
		Vector2 vector = base.transform.position;
		foreach (DamageReceiver damageReceiver in damageReceivers)
		{
			if (!damageReceiver.gameObject.activeInHierarchy || !damageReceiver.enabled || damageReceiver.type != 0)
			{
				continue;
			}
			Collider2D[] components = damageReceiver.GetComponents<Collider2D>();
			foreach (Collider2D collider2D in components)
			{
				if (collider2D.isActiveAndEnabled && CupheadLevelCamera.Current.ContainsPoint(collider2D.bounds.center, collider2D.bounds.size / 2f))
				{
					float sqrMagnitude = (vector - (Vector2)collider2D.bounds.center).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						result = collider2D;
					}
				}
			}
			DamageReceiverChild[] componentsInChildren = damageReceiver.GetComponentsInChildren<DamageReceiverChild>();
			foreach (DamageReceiverChild damageReceiverChild in componentsInChildren)
			{
				Collider2D[] components2 = damageReceiverChild.GetComponents<Collider2D>();
				foreach (Collider2D collider2D2 in components2)
				{
					if (collider2D2.isActiveAndEnabled && CupheadLevelCamera.Current.ContainsPoint(collider2D2.bounds.center, collider2D2.bounds.size / 2f))
					{
						float sqrMagnitude2 = (vector - (Vector2)collider2D2.bounds.center).sqrMagnitude;
						if (sqrMagnitude2 < num)
						{
							num = sqrMagnitude2;
							result = collider2D2;
						}
					}
				}
			}
		}
		return result;
	}
}
