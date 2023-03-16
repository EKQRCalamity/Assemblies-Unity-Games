using System.Collections;
using UnityEngine;

public class WeaponExploderProjectile : BasicProjectile
{
	[SerializeField]
	private WeaponExploderProjectileExplosion explosionPrefab;

	[SerializeField]
	private BasicProjectile shrapnelPrefab;

	[SerializeField]
	private bool isEx;

	public float explodeRadius;

	public float easeTime;

	public MinMax minMaxSpeed;

	public WeaponExploder weapon;

	private new MeterScoreTracker tracker;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!isEx)
		{
			UpdateDamageState();
		}
	}

	private void UpdateDamageState()
	{
		if (base.lifetime < WeaponProperties.LevelWeaponExploder.Basic.timeStateTwo)
		{
			Damage = WeaponProperties.LevelWeaponExploder.Basic.baseDamage;
			base.transform.SetScale(1f, 1f);
			explodeRadius = WeaponProperties.LevelWeaponExploder.Basic.baseExplosionRadius;
		}
		else if (base.lifetime < WeaponProperties.LevelWeaponExploder.Basic.timeStateThree)
		{
			Damage = WeaponProperties.LevelWeaponExploder.Basic.damageStateTwo;
			base.transform.SetScale(1.5f, 1.5f);
			explodeRadius = WeaponProperties.LevelWeaponExploder.Basic.explosionRadiusStateTwo;
		}
		else
		{
			Damage = WeaponProperties.LevelWeaponExploder.Basic.damageStateThree;
			base.transform.SetScale(2.5f, 2.5f);
			explodeRadius = WeaponProperties.LevelWeaponExploder.Basic.explosionRadiusStateThree;
		}
	}

	protected override void Die()
	{
		base.Die();
		explosionPrefab.Create(base.transform.position, explodeRadius, Damage, base.DamageMultiplier, weapon, tracker);
		if (shrapnelPrefab != null)
		{
			BasicProjectile basicProjectile = shrapnelPrefab.Create(base.transform.position, base.transform.eulerAngles.z + 180f, WeaponProperties.LevelWeaponExploder.Ex.shrapnelSpeed);
			if (!WeaponProperties.LevelWeaponExploder.Ex.damageOn)
			{
				basicProjectile.DamagesType.Player = false;
			}
		}
		Object.Destroy(base.gameObject);
	}

	public override void AddToMeterScoreTracker(MeterScoreTracker tracker)
	{
		base.AddToMeterScoreTracker(tracker);
		this.tracker = tracker;
	}

	public void EaseSpeed()
	{
		StartCoroutine(ease_speed_cr());
	}

	private IEnumerator ease_speed_cr()
	{
		float t = 0f;
		float time = easeTime;
		while (t < time)
		{
			t += (float)CupheadTime.Delta;
			Speed = minMaxSpeed.GetFloatAt(t / time);
			yield return null;
		}
	}
}
