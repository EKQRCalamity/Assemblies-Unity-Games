using System;
using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.Physics;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyFloorChecker : MonoBehaviour, ICollisionEmitter
{
	private Enemy _enemy;

	private SmartPlatformCollider _enemySmartPlatformCollider;

	private BoxCollider2D _feetBoxCollider;

	private float _heightChecker;

	private RaycastHit2D[] _leftHits;

	private RaycastHit2D[] _rightHits;

	private RaycastHit2D[] _sideHits;

	private int _maxReturnedIntersections;

	private Vector2 _topCenterBoxCollider;

	private float _widthChecker;

	public float CurrentGroundDetection = 0.15f;

	public float CurrentSideRangeDetection = 0.5f;

	public LayerMask FloorLayerMasks;

	public Core.SimpleEvent OnTrapFall;

	public bool IsGrounded { get; private set; }

	public bool IsSideBlocked { get; private set; }

	public Vector3 EnemyFloorCollisionNormal { get; private set; }

	public event EventHandler<Collider2DParam> OnEnter;

	public event EventHandler<Collider2DParam> OnStay;

	public event EventHandler<Collider2DParam> OnExit;

	private void Awake()
	{
		_feetBoxCollider = GetComponent<BoxCollider2D>();
	}

	private void Start()
	{
		_enemy = GetComponentInParent<Enemy>();
		_enemySmartPlatformCollider = _enemy.GetComponent<SmartPlatformCollider>();
		SmartPlatformCollider enemySmartPlatformCollider = _enemySmartPlatformCollider;
		enemySmartPlatformCollider.OnSideCollision = (SmartRectCollider2D.OnSideCollisionDelegate)Delegate.Combine(enemySmartPlatformCollider.OnSideCollision, new SmartRectCollider2D.OnSideCollisionDelegate(enemy_OnSideCollision));
		_widthChecker = _feetBoxCollider.bounds.extents.x;
		_heightChecker = _feetBoxCollider.bounds.extents.y;
		_maxReturnedIntersections = 2;
		_leftHits = new RaycastHit2D[_maxReturnedIntersections];
		_rightHits = new RaycastHit2D[_maxReturnedIntersections];
		_sideHits = new RaycastHit2D[_maxReturnedIntersections];
	}

	protected void enemy_OnSideCollision(SmartCollision2D col, GameObject go)
	{
		int layer = go.layer;
		if (layer == LayerMask.NameToLayer("Floor") || layer == LayerMask.NameToLayer("OneWayDown"))
		{
			EnemyFloorCollisionNormal = col.contacts[0].normal;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Trap") && collision.gameObject.CompareTag("SpikeTrap") && OnTrapFall != null)
		{
			OnTrapFall();
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		OnTriggerStay2DNotify(other);
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		OnTriggerExit2DNotify(other);
	}

	private void Update()
	{
		if ((bool)_enemy)
		{
			IsGrounded = CheckGrounded();
			_enemy.Status.IsGrounded = _enemy.Controller.IsGrounded;
			if (_enemy.Status.IsGrounded)
			{
				_enemy.SlopeAngle = _enemy.Controller.SmartPlatformCollider.CalculateSlopeAngle();
			}
			_topCenterBoxCollider = new Vector2(_feetBoxCollider.bounds.center.x, _feetBoxCollider.bounds.max.y);
			if (_enemy.Status.Orientation == EntityOrientation.Left)
			{
				Vector2 vector = _topCenterBoxCollider + ((Vector2)base.transform.right * _widthChecker + Vector2.up * _heightChecker * 3f);
				Debug.DrawLine(vector, vector + (Vector2)base.transform.right * CurrentSideRangeDetection, Color.yellow);
				IsSideBlocked = Physics2D.LinecastNonAlloc(vector, vector + (Vector2)base.transform.right * CurrentSideRangeDetection, _sideHits, FloorLayerMasks) > 0;
			}
			else
			{
				Vector2 vector2 = _topCenterBoxCollider - ((Vector2)base.transform.right * _widthChecker - Vector2.up * _heightChecker * 3f);
				Debug.DrawLine(vector2, vector2 - (Vector2)base.transform.right * CurrentSideRangeDetection, Color.yellow);
				IsSideBlocked = Physics2D.LinecastNonAlloc(vector2, vector2 - (Vector2)base.transform.right * CurrentSideRangeDetection, _sideHits, FloorLayerMasks) > 0;
			}
			if (_sideHits.Length > 0)
			{
				Array.Clear(_sideHits, 0, 1);
			}
		}
	}

	private bool CheckGrounded()
	{
		Vector2 vector = (Vector2)_enemy.EntityDamageArea.Center() - (Vector2)_enemy.EntityDamageArea.transform.right * _widthChecker;
		Vector2 vector2 = (Vector2)_enemy.EntityDamageArea.Center() + (Vector2)_enemy.EntityDamageArea.transform.right * _widthChecker;
		Debug.DrawLine(vector, vector - Vector2.up * CurrentGroundDetection, Color.red);
		bool flag = Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * CurrentGroundDetection, _leftHits, FloorLayerMasks) > 0;
		Debug.DrawLine(vector2, vector2 - Vector2.up * CurrentGroundDetection, Color.red);
		bool flag2 = Physics2D.LinecastNonAlloc(vector2, vector2 - Vector2.up * CurrentGroundDetection, _rightHits, FloorLayerMasks) > 0;
		return flag && flag2;
	}

	public void OnTriggerEnter2DNotify(Collider2D c)
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
