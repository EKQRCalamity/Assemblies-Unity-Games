using System;
using Com.LuisPedroFonseca.ProCamera2D;
using CreativeSpore.SmartColliders;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Environment.MovingPlatforms;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Sensor;

public class FloorDistanceChecker : Trait, ICollisionEmitter
{
	public static Core.GenericEvent OnStepLadder;

	private Penitent _penitent;

	public LayerMask layerGroundedCollision;

	public LayerMask layerSearchSafeCollision;

	public LayerMask layerEnemyCollision;

	public LayerMask oneWayDownLayers;

	[Range(0f, 3f)]
	public float rayLength;

	private BoxCollider2D playerBoxCollider;

	private SmartPlatformCollider playerSmartCollider;

	private Vector2 playerSmartColliderDefaultSize;

	private Vector2 playerSmartColliderReducedSize;

	private float halfWidth;

	private float myHeight;

	private Vector2 topCenterBoxCollider;

	private bool isEnemyBlocked;

	private bool isFrontBlocked;

	private SmartPlatformCollider _smartPlatformCollider;

	private RaycastHit2D[] bottomHits;

	private RaycastHit2D[] forwardHits;

	private int maxHitsAllocated;

	[SerializeField]
	private LayerMask LayerFloor;

	private Vector3 bottomNormalCollision;

	private bool oneWayDownCollision;

	public bool IsOnFloorPlatform { get; private set; }

	public Vector3 BottonNormalCollision => bottomNormalCollision;

	public bool OneWayDownCollision => oneWayDownCollision;

	public bool OnMovingPlatform { get; set; }

	public bool IsGrounded { get; private set; }

	public bool IsSideBlocked { get; private set; }

	public event EventHandler<Collider2DParam> OnEnter;

	public event EventHandler<Collider2DParam> OnStay;

	public event EventHandler<Collider2DParam> OnExit;

	private void Start()
	{
		_penitent = (Penitent)base.EntityOwner;
		playerBoxCollider = _penitent.DamageArea.GetComponent<BoxCollider2D>();
		playerSmartCollider = _penitent.GetComponent<SmartPlatformCollider>();
		playerSmartColliderDefaultSize = playerSmartCollider.Size;
		playerSmartColliderReducedSize = new Vector2(0.1f, playerSmartColliderDefaultSize.y);
		SmartPlatformCollider smartPlatformCollider = playerSmartCollider;
		smartPlatformCollider.OnSideCollision = (SmartRectCollider2D.OnSideCollisionDelegate)Delegate.Combine(smartPlatformCollider.OnSideCollision, new SmartRectCollider2D.OnSideCollisionDelegate(player_OnSideCollision));
		_smartPlatformCollider = _penitent.GetComponentInChildren<SmartPlatformCollider>();
		maxHitsAllocated = 1;
		bottomHits = new RaycastHit2D[maxHitsAllocated];
		forwardHits = new RaycastHit2D[maxHitsAllocated];
	}

	private void Update()
	{
		CheckBlock();
		_penitent.PlatformCharacterInput.canAirAttack = !IsGrounded;
		if (_penitent.Status.Dead)
		{
			_penitent.DamageArea.IncludeEnemyLayer(include: false);
		}
		else
		{
			EvaluateEnemyCollision();
		}
	}

	private void OnDestroy()
	{
		if (playerSmartCollider != null)
		{
			SmartPlatformCollider smartPlatformCollider = playerSmartCollider;
			smartPlatformCollider.OnSideCollision = (SmartRectCollider2D.OnSideCollisionDelegate)Delegate.Remove(smartPlatformCollider.OnSideCollision, new SmartRectCollider2D.OnSideCollisionDelegate(player_OnSideCollision));
		}
	}

	private void CheckBlock()
	{
		halfWidth = playerBoxCollider.bounds.extents.x / 2f;
		myHeight = playerBoxCollider.bounds.extents.y;
		topCenterBoxCollider = new Vector2(playerBoxCollider.bounds.center.x, playerBoxCollider.bounds.max.y);
		float num = 1.2f;
		if (_penitent.Status.Orientation == EntityOrientation.Left)
		{
			Vector2 vector = (Vector2)base.transform.position + (Vector2)base.transform.right * halfWidth;
			Debug.DrawLine(vector, vector - Vector2.up * num, Color.magenta);
			IsGrounded = Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * num, bottomHits, layerGroundedCollision) > 0;
			oneWayDownCollision = Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * num, bottomHits, oneWayDownLayers) > 0;
			Vector2 vector2 = topCenterBoxCollider - ((Vector2)base.transform.right * (halfWidth * 0.5f) + Vector2.up * myHeight);
			Debug.DrawLine(vector2, vector2 - (Vector2)base.transform.right * rayLength, Color.cyan);
			isEnemyBlocked = Physics2D.LinecastNonAlloc(vector2, vector2 - (Vector2)base.transform.right * rayLength, forwardHits, layerEnemyCollision) > 0;
			if (forwardHits.Length > 0)
			{
				isFrontBlocked = IsFrontBlocked(forwardHits[0]);
			}
			Vector2 vector3 = topCenterBoxCollider + ((Vector2)base.transform.right * halfWidth - Vector2.up * myHeight);
			Debug.DrawLine(vector3, vector3 + (Vector2)base.transform.right * rayLength, Color.yellow);
			IsSideBlocked = Physics2D.LinecastNonAlloc(vector3, vector3 + (Vector2)base.transform.right * rayLength, forwardHits, layerGroundedCollision) > 0;
		}
		else
		{
			Vector2 vector = (Vector2)base.transform.position - (Vector2)base.transform.right * halfWidth;
			Debug.DrawLine(vector, vector - Vector2.up * num, Color.magenta);
			IsGrounded = Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * num, bottomHits, layerGroundedCollision) > 0;
			oneWayDownCollision = Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * num, bottomHits, oneWayDownLayers) > 0;
			Vector2 vector2 = topCenterBoxCollider + ((Vector2)base.transform.right * (halfWidth * 0.5f) - Vector2.up * myHeight);
			Debug.DrawLine(vector2, vector2 + (Vector2)base.transform.right * rayLength, Color.cyan);
			isEnemyBlocked = Physics2D.LinecastNonAlloc(vector2, vector2 + (Vector2)base.transform.right * rayLength, forwardHits, layerEnemyCollision) > 0;
			if (forwardHits.Length > 0)
			{
				isFrontBlocked = IsFrontBlocked(forwardHits[0]);
			}
			Vector2 vector3 = topCenterBoxCollider - ((Vector2)base.transform.right * halfWidth + Vector2.up * myHeight);
			Debug.DrawLine(vector3, vector3 - (Vector2)base.transform.right * rayLength, Color.yellow);
			IsSideBlocked = Physics2D.LinecastNonAlloc(vector3, vector3 - (Vector2)base.transform.right * rayLength, forwardHits, layerGroundedCollision) > 0;
		}
		if (!_penitent.Status.Dead && IsGrounded && !_penitent.IsClimbingLadder && !_penitent.IsStickedOnWall && !_penitent.Dash.IsUpperBlocked && !_penitent.TrapChecker.DeathBySpike)
		{
			Vector2 origin = (Vector2)base.transform.position - 3.7f * halfWidth * Vector2.right;
			Vector2 origin2 = (Vector2)base.transform.position + 3.7f * halfWidth * Vector2.right;
			if (CheckPositionToBeSafe(origin2) && CheckPositionToBeSafe(origin))
			{
				Vector3 position = _penitent.GetPosition();
				if ((bool)bottomHits[0])
				{
					position.y = bottomHits[0].collider.bounds.max.y;
				}
				Core.LevelManager.SetPlayerSafePosition(position);
			}
		}
		if (_penitent.HasFlag("SIDE_BLOCKED") != IsSideBlocked)
		{
			_penitent.SetFlag("SIDE_BLOCKED", IsSideBlocked);
		}
		if (_penitent.HasFlag("FRONT_BLOCKED") != isFrontBlocked)
		{
			_penitent.SetFlag("FRONT_BLOCKED", isFrontBlocked);
		}
		if (forwardHits.Length > 0)
		{
			Array.Clear(forwardHits, 0, 1);
		}
	}

	private void OnDrawGizmos()
	{
		UnityEngine.Gizmos.color = Color.yellow;
		UnityEngine.Gizmos.DrawSphere(Core.LevelManager.LastSafePosition, 0.5f);
	}

	private bool CheckPositionToBeSafe(Vector2 origin)
	{
		Debug.DrawRay(origin, -Vector2.up * 1.1f, Color.white);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, -Vector2.up, 1.1f, layerSearchSafeCollision);
		if (!raycastHit2D)
		{
			return false;
		}
		GameObject gameObject = raycastHit2D.collider.gameObject;
		if (((1 << gameObject.layer) & (int)layerSearchSafeCollision) == 0)
		{
			return false;
		}
		bool flag = !gameObject.GetComponent(typeof(INoSafePosition)) && (!gameObject.GetComponent<StraightMovingPlatform>() || !DOTween.IsTweening(gameObject.transform)) && (!gameObject.GetComponent<WaypointsMovingPlatform>() || !DOTween.IsTweening(gameObject.transform)) && !gameObject.CompareTag("SpikeTrap") && !gameObject.CompareTag("AbyssTrap");
		ProCamera2DNumericBoundaries proCamera2DNumericBoundaries = Core.Logic.CameraManager.ProCamera2DNumericBoundaries;
		if (flag && proCamera2DNumericBoundaries.UseNumericBoundaries)
		{
			if (flag && proCamera2DNumericBoundaries.UseRightBoundary)
			{
				flag = origin.x <= proCamera2DNumericBoundaries.RightBoundary;
			}
			if (flag && proCamera2DNumericBoundaries.UseLeftBoundary)
			{
				flag = origin.x >= proCamera2DNumericBoundaries.LeftBoundary;
			}
			if (flag && proCamera2DNumericBoundaries.UseTopBoundary)
			{
				flag = origin.y <= proCamera2DNumericBoundaries.TopBoundary;
			}
			if (flag && proCamera2DNumericBoundaries.UseBottomBoundary)
			{
				flag = origin.y >= proCamera2DNumericBoundaries.BottomBoundary;
			}
		}
		return flag;
	}

	public void IncreaseSkinWidth(bool increase = true)
	{
		playerSmartCollider.Size = ((!increase) ? playerSmartColliderReducedSize : playerSmartColliderDefaultSize);
	}

	private void EvaluateEnemyCollision()
	{
		if (!_penitent.Status.IsGrounded || _penitent.IsJumpingOff)
		{
			_penitent.DamageArea.IncludeEnemyLayer(include: false);
		}
		else if (_penitent.Status.IsGrounded && !_penitent.IsDashing && !_penitent.MotionLerper.IsLerping)
		{
			_penitent.DamageArea.IncludeEnemyLayer();
		}
	}

	private void player_OnSideCollision(SmartCollision2D col, GameObject go)
	{
		int layer = go.layer;
		if ((int)layerGroundedCollision == ((int)layerGroundedCollision | (1 << layer)))
		{
			bottomNormalCollision = col.contacts[0].normal;
		}
		_penitent.SetFlag("SIDE_BLOCKED", active: false);
	}

	private bool IsFrontBlocked(RaycastHit2D rayCastHit)
	{
		bool result = false;
		if ((bool)rayCastHit.collider)
		{
			GameObject gameObject = rayCastHit.collider.gameObject;
			if (gameObject.layer != LayerMask.NameToLayer("Enemy"))
			{
				result = (int)layerEnemyCollision == ((int)layerEnemyCollision | (1 << gameObject.layer));
			}
		}
		return result;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Ladder") && OnStepLadder != null)
		{
			OnStepLadder(other);
			_penitent.StepOnLadder = true;
		}
		if (!IsOnFloorPlatform && ((1 << other.gameObject.layer) & (int)LayerFloor) != 0)
		{
			IsOnFloorPlatform = true;
		}
		OnTriggerEnter2DNotify(other);
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Ladder") && _penitent.Status.IsGrounded && _smartPlatformCollider.SkinBottomOff01 > 0f)
		{
			_smartPlatformCollider.SkinBottomOff01 = 0f;
		}
		OnTriggerStay2DNotify(other);
		if (!IsOnFloorPlatform && ((1 << other.gameObject.layer) & (int)LayerFloor) != 0)
		{
			IsOnFloorPlatform = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Ladder"))
		{
			_smartPlatformCollider.SkinBottomOff01 = 0.1f;
			_penitent.StepOnLadder = false;
		}
		if (IsOnFloorPlatform && ((1 << other.gameObject.layer) & (int)LayerFloor) != 0)
		{
			IsOnFloorPlatform = false;
		}
		OnTriggerExit2DNotify(other);
	}

	public void OnTriggerEnter2DNotify(Collider2D c)
	{
		OnEnter2DNotify(c);
	}

	private void OnEnter2DNotify(Collider2D c)
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
