using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

[RequireComponent(typeof(Collider2D))]
public class EnemyBarrier : Trait
{
	[FoldoutGroup("Raycast Settings", 0)]
	public LayerMask TargetLayer;

	[FoldoutGroup("Raycast Settings", 0)]
	public float HeightOffset;

	[FoldoutGroup("Raycast Settings", 0)]
	public Color gizmoColor;

	private Collider2D _barrierCollider;

	private Vector2 _bottomLeft;

	private Vector2 _bottomRight;

	[FoldoutGroup("Activation settings", 0)]
	private bool activateOnlyIfPlayerSeen;

	[FoldoutGroup("Activation settings", 0)]
	public bool UseSpriteRenderer = true;

	[FoldoutGroup("Hit Settings", 0)]
	public DamageArea.DamageType DamageType;

	[FoldoutGroup("Hit Settings", 0)]
	[EventRef]
	public string HitSoundFx;

	[FoldoutGroup("Hit Settings", 0)]
	public float RaycastDistance = 0.1f;

	[FoldoutGroup("Hit Settings", 0)]
	public bool OnlyForward;

	[FoldoutGroup("Hit Settings", 0)]
	public GameObject HitVfx;

	[FoldoutGroup("Hit Settings", 0)]
	public Vector2 HitVfxOffset;

	private Hit _contactHit;

	private bool _doDamage;

	private float cd = 0.5f;

	private float _cdCounter;

	private EnemyBehaviour behaviour;

	protected override void OnAwake()
	{
		base.OnAwake();
		_barrierCollider = GetComponent<BoxCollider2D>();
		if (_barrierCollider == null)
		{
			_barrierCollider = GetComponent<PolygonCollider2D>();
		}
		_contactHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = 0f,
			DamageElement = DamageArea.DamageElement.Contact,
			DamageType = DamageArea.DamageType.Normal,
			HitSoundId = HitSoundFx,
			Unnavoidable = true
		};
		if (base.EntityOwner != null && base.EntityOwner is Enemy)
		{
			behaviour = base.EntityOwner.GetComponent<EnemyBehaviour>();
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if ((bool)HitVfx)
		{
			PoolManager.Instance.CreatePool(HitVfx, 1);
		}
		base.EntityOwner.OnDeath += OnOwnerDeath;
	}

	private void OnOwnerDeath()
	{
		base.EntityOwner.OnDeath -= OnOwnerDeath;
		base.enabled = false;
	}

	private void CheckDeactivation()
	{
		if (behaviour != null)
		{
			_barrierCollider.enabled = behaviour.IsPlayerSeen();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (activateOnlyIfPlayerSeen)
		{
			CheckDeactivation();
		}
		if (_cdCounter > 0f)
		{
			_cdCounter -= Time.deltaTime;
		}
		else
		{
			if (UseSpriteRenderer && !base.EntityOwner.SpriteRenderer.isVisible)
			{
				return;
			}
			_bottomRight = new Vector2(_barrierCollider.bounds.max.x, _barrierCollider.bounds.min.y + HeightOffset);
			_bottomLeft = new Vector2(_barrierCollider.bounds.min.x, _barrierCollider.bounds.min.y + HeightOffset);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(_bottomRight, Vector2.right, RaycastDistance, TargetLayer);
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(_bottomLeft, -Vector2.right, RaycastDistance, TargetLayer);
			bool flag = raycastHit2D;
			bool flag2 = raycastHit2D2;
			if (OnlyForward)
			{
				if (base.EntityOwner.Status.Orientation == EntityOrientation.Left)
				{
					flag = false;
				}
				else if (base.EntityOwner.Status.Orientation == EntityOrientation.Right)
				{
					flag2 = false;
				}
			}
			if ((flag || flag2) && Core.Logic.Penitent.IsDashing)
			{
				StopPlayerDash();
				if ((bool)HitVfx)
				{
					int num = ((!flag2) ? 1 : (-1));
					Vector3 vector = new Vector3(HitVfxOffset.x * (float)num, HitVfxOffset.y, 0f);
					GameObject gameObject = PoolManager.Instance.ReuseObject(HitVfx, base.transform.position + vector, Quaternion.identity).GameObject;
					gameObject.transform.localScale = new Vector3(num, 1f, 1f);
				}
				_cdCounter = cd;
			}
		}
	}

	private void StopPlayerDash()
	{
		Core.Logic.Penitent.Damage(_contactHit);
	}

	private void OnDrawGizmos()
	{
		if (_barrierCollider == null)
		{
			_barrierCollider = GetComponent<BoxCollider2D>();
			if (_barrierCollider == null)
			{
				_barrierCollider = GetComponent<PolygonCollider2D>();
			}
		}
		Vector2 vector = new Vector2(_barrierCollider.bounds.max.x, _barrierCollider.bounds.min.y + HeightOffset);
		Vector2 vector2 = new Vector2(_barrierCollider.bounds.min.x, _barrierCollider.bounds.min.y + HeightOffset);
		Gizmos.color = gizmoColor;
		Gizmos.DrawRay(vector, Vector2.right * RaycastDistance);
		Gizmos.DrawRay(vector2, -Vector2.right * RaycastDistance);
	}
}
