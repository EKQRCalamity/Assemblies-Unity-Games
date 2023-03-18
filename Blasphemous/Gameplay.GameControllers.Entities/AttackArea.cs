using System;
using Framework.FrameworkCore;
using Framework.Util;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

[RequireComponent(typeof(Collider2D))]
public class AttackArea : MonoBehaviour, ICollisionEmitter
{
	public LayerMask enemyLayerMask;

	[Range(0f, 1f)]
	public float entityScopeDetection = 2.5f;

	public bool ChangesColliderOrientation = true;

	public Entity Entity;

	private EntityOrientation _entityOrientation;

	private Vector2 _bottomRayCastOrigin;

	private Vector2 _topRayCastOrigin;

	public bool EnableTargetRayCast;

	private ContactFilter2D _contactFilter;

	private Collider2D[] hits;

	public bool IsTargetHit { get; private set; }

	public bool EnemyIsInAttackArea { get; private set; }

	public Vector3 LocalPos { get; set; }

	public Vector2 RayCastOrigin => _bottomRayCastOrigin;

	public Collider2D WeaponCollider { get; private set; }

	public event EventHandler<Collider2DParam> OnEnter;

	public event EventHandler<Collider2DParam> OnStay;

	public event EventHandler<Collider2DParam> OnExit;

	public void SetLayerMask(LayerMask lm)
	{
		enemyLayerMask = lm;
		SetContactFilter();
	}

	private void SetContactFilter()
	{
		_contactFilter = new ContactFilter2D
		{
			layerMask = enemyLayerMask,
			useLayerMask = true,
			useTriggers = true
		};
	}

	private void Awake()
	{
		WeaponCollider = GetComponent<Collider2D>();
		Entity componentInParent = GetComponentInParent<Entity>();
		if (componentInParent != null)
		{
			Entity = componentInParent;
		}
		LocalPos = base.transform.localPosition;
		hits = new Collider2D[10];
		SetContactFilter();
	}

	private void Update()
	{
		if (EnableTargetRayCast)
		{
			IsTargetHit = IsEnemyHit();
		}
		if (!(Entity == null))
		{
			if (Entity.Status.IsVisibleOnCamera && !Entity.Status.Dead)
			{
				Entity.EntityAttack.IsEnemyHit = IsTargetHit;
			}
			if (ChangesColliderOrientation)
			{
				SetColliderOrientation();
			}
		}
	}

	private void SetColliderOrientation()
	{
		Vector3 localScale = base.transform.localScale;
		switch (Entity.Status.Orientation)
		{
		case EntityOrientation.Left:
			if (localScale.x > -1f)
			{
				localScale.x = -1f;
			}
			break;
		case EntityOrientation.Right:
			if (localScale.x < 1f)
			{
				localScale.x = 1f;
			}
			break;
		}
		base.transform.localScale = localScale;
	}

	public bool IsEnemyHit()
	{
		Vector2 vector = ((Entity.Status.Orientation != EntityOrientation.Left) ? Vector2.right : (-Vector2.right));
		Vector3 vector2 = base.transform.TransformDirection(vector);
		float x = ((Entity.Status.Orientation != EntityOrientation.Left) ? WeaponCollider.bounds.min.x : WeaponCollider.bounds.max.x);
		_topRayCastOrigin = new Vector2(x, WeaponCollider.bounds.max.y);
		_bottomRayCastOrigin = new Vector2(x, WeaponCollider.bounds.min.y);
		bool flag = Physics2D.Raycast(_topRayCastOrigin, vector2, WeaponCollider.bounds.size.x + entityScopeDetection, enemyLayerMask);
		bool flag2 = Physics2D.Raycast(_bottomRayCastOrigin, vector2, WeaponCollider.bounds.size.x + entityScopeDetection, enemyLayerMask);
		Debug.DrawRay(_topRayCastOrigin, vector2 * (WeaponCollider.bounds.size.x + entityScopeDetection), Color.blue);
		Debug.DrawRay(_bottomRayCastOrigin, vector2 * (WeaponCollider.bounds.size.x + entityScopeDetection), Color.blue);
		return flag2 || flag;
	}

	private void DrawDebugCross(Vector2 point, Color c, float seconds)
	{
		float num = 0.6f;
		Debug.DrawLine(point - Vector2.up * num, point + Vector2.up * num, c, seconds);
		Debug.DrawLine(point - Vector2.right * num, point + Vector2.right * num, c, seconds);
	}

	public GameObject[] OverlappedEntities()
	{
		int num = WeaponCollider.OverlapCollider(_contactFilter, hits);
		GameObject[] array = new GameObject[num];
		for (int i = 0; i < num; i++)
		{
			GameObject value = hits[i].gameObject;
			array.SetValue(value, i);
		}
		return array;
	}

	public void SetLocalHeight(float yLocalPos)
	{
		if (Math.Abs(base.transform.localPosition.y - yLocalPos) > Mathf.Epsilon)
		{
			Vector3 localPosition = new Vector3(base.transform.localPosition.x, yLocalPos, base.transform.localPosition.z);
			base.transform.localPosition = localPosition;
		}
	}

	public void SetSize(Vector2 size)
	{
		if (!(WeaponCollider == null))
		{
			((BoxCollider2D)WeaponCollider).size = size;
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
