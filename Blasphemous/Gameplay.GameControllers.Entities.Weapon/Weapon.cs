using System.Collections.Generic;
using Framework.Pooling;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.Weapon;

public abstract class Weapon : PoolObject
{
	public Entity WeaponOwner;

	protected AttackArea[] _attackAreas;

	protected List<IDamageable> DamageableEntities;

	public AttackArea[] AttackAreas => _attackAreas;

	private void Awake()
	{
		_attackAreas = GetComponentsInChildren<AttackArea>();
		Entity componentInParent = GetComponentInParent<Entity>();
		if (componentInParent != null)
		{
			WeaponOwner = componentInParent;
		}
		DamageableEntities = new List<IDamageable>();
		OnAwake();
	}

	protected virtual void OnAwake()
	{
	}

	private void Start()
	{
		OnStart();
	}

	protected virtual void OnStart()
	{
	}

	private void Update()
	{
		OnUpdate();
	}

	protected virtual void OnUpdate()
	{
	}

	private void FixedUpdate()
	{
		OnFixedUpdate();
	}

	protected virtual void OnFixedUpdate()
	{
	}

	public abstract void Attack(Hit weapondHit);

	public abstract void OnHit(Hit weaponHit);

	public virtual void SetHit(Hit hit)
	{
	}

	protected List<IDamageable> GetDamageableEntities()
	{
		if (AttackAreas == null)
		{
			return null;
		}
		for (int i = 0; i < AttackAreas.Length; i++)
		{
			GameObject[] array = AttackAreas[i].OverlappedEntities();
			if (array.Length <= 0)
			{
				continue;
			}
			for (int j = 0; j < array.Length; j++)
			{
				IDamageable componentInParent = array[j].GetComponentInParent<IDamageable>();
				if (componentInParent != null)
				{
					DamageableEntities.Add(componentInParent);
				}
			}
		}
		return DamageableEntities;
	}

	protected List<IDamageable> GetDamageableEntitiesWithCircleArea(CircleAttackArea circleArea)
	{
		GameObject[] array = circleArea.OverlappedEntities();
		if (array.Length <= 0)
		{
			return null;
		}
		for (int i = 0; i < array.Length; i++)
		{
			IDamageable componentInParent = array[i].GetComponentInParent<IDamageable>();
			if (componentInParent != null && !DamageableEntities.Contains(componentInParent))
			{
				DamageableEntities.Add(componentInParent);
			}
		}
		return DamageableEntities;
	}

	protected void AttackDamageableEntities(Hit weaponHit)
	{
		if (DamageableEntities != null && DamageableEntities.Count > 0)
		{
			OnHit(weaponHit);
			for (int i = 0; i < DamageableEntities.Count; i++)
			{
				DamageableEntities[i].Damage(weaponHit);
			}
			DamageableEntities.Clear();
		}
	}

	public void ClearDamageableEntities()
	{
		if (DamageableEntities.Count > 0)
		{
			DamageableEntities.Clear();
		}
	}
}
