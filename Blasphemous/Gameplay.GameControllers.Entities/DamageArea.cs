using System;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.GameControllers.Entities;

public class DamageArea : MonoBehaviour
{
	[Serializable]
	public class TakeDamageEvent : UnityEvent<Gameplay.GameControllers.Entities.Hit>
	{
	}

	public enum DamageType
	{
		Normal,
		Heavy,
		Critical,
		Simple,
		Stunt,
		OptionalStunt
	}

	public enum DamageElement
	{
		Normal,
		Fire,
		Toxic,
		Magic,
		Lightning,
		Contact
	}

	public struct Hit
	{
		public Entity AttackingEntity;

		public Entity DamagedEntity;

		public DamageType DamageType;

		public Vector2 Position;

		public Hit(Entity attackingEntity, Entity damagedEntity, DamageType damageType)
		{
			AttackingEntity = attackingEntity;
			DamagedEntity = damagedEntity;
			DamageType = damageType;
			Position = DamagedEntity.transform.position;
		}

		public Hit(Entity attackingEntity, Entity damagedEntity, DamageType damageType, Vector2 position)
		{
			AttackingEntity = attackingEntity;
			DamagedEntity = damagedEntity;
			DamageType = damageType;
			Position = position;
		}
	}

	public TakeDamageEvent onTakeDamage;

	public LayerMask enemyAttackAreaLayer;

	protected Entity Entity;

	public float RecoverTime = 0.5f;

	protected float DeltaRecoverTime;

	[SerializeField]
	protected Collider2D damageAreaCollider;

	public Entity OwnerEntity { get; set; }

	public Gameplay.GameControllers.Entities.Hit LastHit { get; set; }

	public Vector3 TopCenter
	{
		get
		{
			float x = damageAreaCollider.bounds.center.x;
			float y = damageAreaCollider.bounds.max.y;
			return new Vector2(x, y);
		}
	}

	public Collider2D DamageAreaCollider => damageAreaCollider;

	public float DamageAreaLenght()
	{
		return damageAreaCollider.bounds.max.y - damageAreaCollider.bounds.min.y;
	}

	public Vector3 Center()
	{
		return damageAreaCollider.bounds.center;
	}

	private void Awake()
	{
		OwnerEntity = GetComponentInParent<Entity>();
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

	public virtual void TakeDamage(Gameplay.GameControllers.Entities.Hit hit, bool force = false)
	{
		if (onTakeDamage != null)
		{
			onTakeDamage.Invoke(hit);
		}
	}
}
