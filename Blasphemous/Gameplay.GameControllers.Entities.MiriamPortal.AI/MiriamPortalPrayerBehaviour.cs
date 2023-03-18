using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Entities.MiriamPortal.Animation;
using Gameplay.GameControllers.Entities.StateMachine;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Animator;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.MiriamPortal.AI;

public class MiriamPortalPrayerBehaviour : MonoBehaviour
{
	public enum MiriamPortalState
	{
		Idle,
		Follow,
		Attack,
		Vanish
	}

	[FoldoutGroup("Motion Settings", 0)]
	[MinMaxSlider(0f, 500f, false)]
	public Vector2 FollowSpeed;

	[FoldoutGroup("Motion Settings", 0)]
	[MinMaxSlider(0f, 10f, false)]
	public Vector2 FollowDistance;

	[FoldoutGroup("Motion Settings", 0)]
	[Range(0f, 10f)]
	public float ThresholdSpeed = 3f;

	[FoldoutGroup("Motion Settings", 0)]
	[Range(0f, 10f)]
	public float FollowingCooldown = 2f;

	[FoldoutGroup("Motion Settings", 0)]
	public Vector2 MasterOffsetPosition;

	[FoldoutGroup("Motion Settings", 0)]
	public float SmoothDampElongation = 0.3f;

	[FoldoutGroup("Motion Settings", 0)]
	[Range(0f, 0.1f)]
	public float FloatingVerticalElongation = 0.25f;

	[FoldoutGroup("Motion Settings", 0)]
	[Range(1f, 5f)]
	public float FloatingSpeed = 1.5f;

	[FoldoutGroup("Attack Settings", 0)]
	public float HorizontalAttackDistance = 4f;

	[FoldoutGroup("Attack Settings", 0)]
	public float VerticalAttackDistance = 1f;

	[FoldoutGroup("Attack Settings", 0)]
	public LayerMask FloorMask;

	[FoldoutGroup("Attack Settings", 0)]
	public BossAreaSummonAttack Pillar;

	[FoldoutGroup("Attack Settings", 0)]
	public float MaxDistanceToHitGround = 9f;

	public MiriamPortalPrayer MiriamPortal;

	public GameObject PortalShatteringVfx;

	public GameObject LandingVfx;

	private MiriamPortalState currentState;

	private float _reverseCoolDown = 0.25f;

	private float _attackCoolDown = 0.2f;

	private float _currentReverseCoolDown;

	private float _currentAttackCoolDown;

	private Gameplay.GameControllers.Entities.StateMachine.StateMachine StateMachine;

	private RaycastHit2D[] results;

	private bool reachedGround;

	public Entity Master { get; set; }

	public bool VanishFlag { get; set; }

	public bool IdleFlag { get; set; }

	public bool ReappearFlag { get; set; }

	public bool IsTurning { get; set; }

	public EntityOrientation GuessedOrientation { get; private set; }

	private bool LeftOrientation { get; set; }

	public Vector3 GetMasterOffSetPosition
	{
		get
		{
			Vector3 position = Master.transform.position;
			position.y += MasterOffsetPosition.y;
			float hSpeed = Core.Logic.Penitent.PlatformCharacterController.PlatformCharacterPhysics.HSpeed;
			if (Mathf.Abs(hSpeed) < ThresholdSpeed)
			{
				if (Master.Status.Orientation == EntityOrientation.Right)
				{
					position.x -= MasterOffsetPosition.x;
				}
				else
				{
					position.x += MasterOffsetPosition.x;
				}
			}
			return position;
		}
	}

	private bool CanPerformAction => !IsTurning && IsAlignedWithMaster;

	public float GetMasterDistance => Vector2.Distance(Master.transform.position, base.transform.position);

	public bool IsAlignedWithMaster => MiriamPortal.Status.Orientation == Master.Status.Orientation;

	private void Awake()
	{
		StateMachine = GetComponentInChildren<Gameplay.GameControllers.Entities.StateMachine.StateMachine>();
	}

	private void OnEnable()
	{
		VanishFlag = false;
		MiriamPortal.SpriteRenderer.enabled = true;
		SetInitialOrientation();
	}

	private void Start()
	{
		Master = Core.Logic.Penitent;
		SetInitialOrientation();
		results = new RaycastHit2D[1];
		PoolManager.Instance.CreatePool(PortalShatteringVfx, 1);
		PoolManager.Instance.CreatePool(LandingVfx, 1);
		AnimatorInyector animatorInyector = Core.Logic.Penitent.AnimatorInyector;
		animatorInyector.OnAttack = (Core.SimpleEvent)Delegate.Combine(animatorInyector.OnAttack, new Core.SimpleEvent(OnAttack));
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(OnMasterDead));
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		_currentReverseCoolDown += deltaTime;
		_currentAttackCoolDown += deltaTime;
		switch (currentState)
		{
		case MiriamPortalState.Idle:
			if (VanishFlag)
			{
				SetState(MiriamPortalState.Vanish);
			}
			else
			{
				StateMachine.SwitchState<MiriamPortalPrayerIdleState>();
			}
			break;
		case MiriamPortalState.Follow:
			if (VanishFlag)
			{
				SetState(MiriamPortalState.Vanish);
			}
			else if (IdleFlag)
			{
				SetState(MiriamPortalState.Idle);
			}
			else if (ReappearFlag)
			{
				SetState(MiriamPortalState.Idle);
			}
			else
			{
				StateMachine.SwitchState<MiriamPortalPrayerFollowState>();
			}
			break;
		case MiriamPortalState.Attack:
			if (!(_currentAttackCoolDown < _attackCoolDown))
			{
				StateMachine.SwitchState<MiriamPortalPrayerAttackState>();
			}
			break;
		case MiriamPortalState.Vanish:
			if (ReappearFlag)
			{
				SetState(MiriamPortalState.Idle);
			}
			StateMachine.SwitchState<MiriamPortalPrayerVanishState>();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void SetState(MiriamPortalState state)
	{
		currentState = state;
	}

	public Vector2 GetActionDirection(bool checkToHitGorund)
	{
		int num = ((MiriamPortal.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
		float x = base.transform.position.x + HorizontalAttackDistance * (float)num;
		float y = base.transform.position.y - VerticalAttackDistance;
		if (checkToHitGorund && CanHitGround())
		{
			reachedGround = true;
			y = results[0].point.y + 0.3f;
		}
		return new Vector2(x, y);
	}

	private bool CanHitGround()
	{
		Vector2 actionDirection = GetActionDirection(checkToHitGorund: false);
		if (Physics2D.RaycastNonAlloc(actionDirection, Vector2.down, results, MaxDistanceToHitGround, FloorMask) > 0)
		{
			Debug.DrawLine(actionDirection, results[0].point, Color.cyan, 3f);
			return true;
		}
		return false;
	}

	public void CheckAndSpawnLandingVfx()
	{
		if (reachedGround)
		{
			Vector3 position = base.transform.position;
			position.x += ((MiriamPortal.Status.Orientation != 0) ? (-0.7f) : 0.7f);
			position.y -= 0.6f;
			PoolManager.Instance.ReuseObject(LandingVfx, position, Quaternion.identity);
			Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("HardFall");
			position.y += 5f;
			Pillar.SetDamageStrength(MiriamPortal.Stats.Strength.Final * Core.Logic.Penitent.Stats.DamageMultiplier.Final * 0.2f);
			Vector2 vector = ((MiriamPortal.Status.Orientation != 0) ? Vector2.left : Vector2.right);
			Pillar.SummonAreas(position, vector, MiriamPortal.Status.Orientation);
		}
	}

	public void SetInitialOrientation()
	{
		if ((bool)Master)
		{
			GuessedOrientation = Master.Status.Orientation;
			MiriamPortal.SetOrientation(GuessedOrientation);
		}
	}

	public void LookAtMaster()
	{
		if (!(Master == null) && !(_currentReverseCoolDown < _reverseCoolDown))
		{
			_currentReverseCoolDown = 0f;
			float num = ((Master.Status.Orientation != EntityOrientation.Left) ? 0.5f : (-0.5f));
			LeftOrientation = base.transform.position.x > Master.transform.position.x + num;
			SetOrientation(LeftOrientation);
		}
	}

	private void OnAttack()
	{
		if (!Core.Logic.Penitent.Animator.GetCurrentAnimatorStateInfo(0).IsName("HardLanding") && currentState == MiriamPortalState.Follow && CanPerformAction)
		{
			_currentAttackCoolDown = 0f;
			reachedGround = false;
			SetState(MiriamPortalState.Attack);
		}
	}

	private void OnMasterDead()
	{
		VanishFlag = true;
		SetState(MiriamPortalState.Vanish);
	}

	private void SetOrientation(bool flip)
	{
		if (flip && GuessedOrientation != EntityOrientation.Left)
		{
			GuessedOrientation = EntityOrientation.Left;
			FlipMiriamPortal();
		}
		else if (!flip && GuessedOrientation != 0)
		{
			GuessedOrientation = EntityOrientation.Right;
			FlipMiriamPortal();
		}
	}

	private void FlipMiriamPortal()
	{
		MiriamPortal.AnimationHandler.SetAnimatorTrigger(MiriamPortalPrayerAnimationHandler.TurnTrigger);
		MiriamPortal.Audio.PlayTurn();
	}
}
