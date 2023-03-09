using UnityEngine;

public class WeaponSplitterProjectile : BasicProjectile
{
	public bool isMain;

	public float nextDistance;

	public float baseAngle;

	private float distancePastSplit;

	private float splitAngle;

	private float dist;

	private float splitDamage = -1f;

	protected override void Start()
	{
		base.Start();
		if (splitDamage > -1f)
		{
			damageDealer.SetDamage(splitDamage);
		}
	}

	protected override void OnDieDistance()
	{
		if (!base.dead)
		{
			Die();
			base.animator.SetTrigger("OnDistanceDie");
		}
	}

	protected override void Die()
	{
		Object.Destroy(base.gameObject);
	}

	private void _OnDieAnimComplete()
	{
		Object.Destroy(base.gameObject);
	}

	private void Split()
	{
		baseAngle = base.transform.eulerAngles.z;
		if (isMain)
		{
			damageDealer.SetDamage((nextDistance != WeaponProperties.LevelWeaponSplitter.Basic.splitDistanceB) ? WeaponProperties.LevelWeaponSplitter.Basic.bulletDamageA : WeaponProperties.LevelWeaponSplitter.Basic.bulletDamageB);
			WeaponSplitterProjectile weaponSplitterProjectile = Object.Instantiate(this, base.transform.position, Quaternion.identity);
			weaponSplitterProjectile.isMain = false;
			weaponSplitterProjectile.splitAngle = 0f - WeaponProperties.LevelWeaponSplitter.Basic.splitAngle;
			weaponSplitterProjectile.transform.eulerAngles = new Vector3(0f, 0f, baseAngle + splitAngle);
			weaponSplitterProjectile.distancePastSplit = WeaponProperties.LevelWeaponSplitter.Basic.angleDistance;
			weaponSplitterProjectile.dist = dist;
			weaponSplitterProjectile.splitDamage = Damage;
			weaponSplitterProjectile = Object.Instantiate(this, base.transform.position, Quaternion.identity);
			weaponSplitterProjectile.isMain = false;
			weaponSplitterProjectile.splitAngle = WeaponProperties.LevelWeaponSplitter.Basic.splitAngle;
			weaponSplitterProjectile.transform.eulerAngles = new Vector3(0f, 0f, baseAngle + splitAngle);
			weaponSplitterProjectile.distancePastSplit = WeaponProperties.LevelWeaponSplitter.Basic.angleDistance;
			weaponSplitterProjectile.dist = dist;
			weaponSplitterProjectile.splitDamage = Damage;
		}
		else
		{
			base.transform.eulerAngles = new Vector3(0f, 0f, baseAngle + splitAngle);
			distancePastSplit = WeaponProperties.LevelWeaponSplitter.Basic.angleDistance;
		}
		if (nextDistance == WeaponProperties.LevelWeaponSplitter.Basic.splitDistanceB)
		{
			nextDistance = float.MaxValue;
		}
		else
		{
			nextDistance = WeaponProperties.LevelWeaponSplitter.Basic.splitDistanceB;
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		dist += Speed * CupheadTime.FixedDelta;
		if (dist > nextDistance)
		{
			Split();
		}
		if (distancePastSplit > 0f)
		{
			distancePastSplit -= Speed * CupheadTime.FixedDelta;
			if (distancePastSplit <= 0f)
			{
				base.transform.eulerAngles = new Vector3(0f, 0f, baseAngle);
			}
		}
	}
}
