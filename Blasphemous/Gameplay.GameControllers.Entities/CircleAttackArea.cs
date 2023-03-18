using System;
using Framework.FrameworkCore;
using Framework.Util;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

[RequireComponent(typeof(CircleCollider2D))]
public class CircleAttackArea : MonoBehaviour, ICollisionEmitter
{
	public LayerMask enemyLayerMask;

	[Range(0f, 1f)]
	public float entityScopeDetection = 2.5f;

	public Entity Entity;

	private EntityOrientation _entityOrientation;

	public bool IsTargetHit { get; private set; }

	public bool EnemyIsInAttackArea { get; private set; }

	public Vector3 LocalPos { get; set; }

	public CircleCollider2D WeaponCollider { get; private set; }

	public event EventHandler<Collider2DParam> OnEnter;

	public event EventHandler<Collider2DParam> OnStay;

	public event EventHandler<Collider2DParam> OnExit;

	private void Awake()
	{
		WeaponCollider = GetComponent<CircleCollider2D>();
		Entity componentInParent = GetComponentInParent<Entity>();
		if (componentInParent != null)
		{
			Entity = componentInParent;
		}
		LocalPos = base.transform.localPosition;
	}

	private void Update()
	{
	}

	public GameObject[] OverlappedEntities()
	{
		Collider2D[] array = Physics2D.OverlapAreaAll(WeaponCollider.bounds.min, WeaponCollider.bounds.max, enemyLayerMask);
		GameObject[] array2 = new GameObject[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			GameObject value = array[i].gameObject;
			array2.SetValue(value, i);
		}
		return array2;
	}

	public void SetRadius(float radius)
	{
		if (!(WeaponCollider == null))
		{
			WeaponCollider.radius = radius;
		}
	}

	public void SetOffset(Vector2 offset)
	{
		if (!(WeaponCollider == null))
		{
			WeaponCollider.offset = offset;
		}
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if ((enemyLayerMask.value & (1 << col.gameObject.layer)) > 0)
		{
			OnTriggerEnter2DNotify(col);
		}
	}

	private void OnTriggerStay2D(Collider2D col)
	{
		if ((enemyLayerMask.value & (1 << col.gameObject.layer)) > 0)
		{
			OnTriggerStay2DNotify(col);
			if (!EnemyIsInAttackArea)
			{
				EnemyIsInAttackArea = true;
			}
		}
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if ((enemyLayerMask.value & (1 << col.gameObject.layer)) > 0)
		{
			OnTriggerExit2DNotify(col);
			if (EnemyIsInAttackArea)
			{
				EnemyIsInAttackArea = !EnemyIsInAttackArea;
			}
		}
	}

	public void OnTriggerEnter2DNotify(Collider2D c)
	{
		OnEnter2DNotify(c);
	}

	public void OnEnter2DNotify(Collider2D c)
	{
		if (this.OnEnter != null)
		{
			this.OnEnter(this, new Collider2DParam
			{
				Collider2DArg = c
			});
		}
	}

	public void OnTriggerStay2DNotify(Collider2D c)
	{
		OnStay2DNotify(c);
	}

	private void OnStay2DNotify(Collider2D c)
	{
		if (this.OnStay != null)
		{
			this.OnStay(this, new Collider2DParam
			{
				Collider2DArg = c
			});
		}
	}

	public void OnTriggerExit2DNotify(Collider2D c)
	{
		OnExit2DNotify(c);
	}

	public void OnExit2DNotify(Collider2D c)
	{
		if (this.OnExit != null)
		{
			this.OnExit(this, new Collider2DParam
			{
				Collider2DArg = c
			});
		}
	}
}
