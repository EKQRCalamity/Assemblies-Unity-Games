using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Guardian.Animation;
using Gameplay.GameControllers.Entities.StateMachine;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using Gameplay.GameControllers.Penitent.Animator;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.Guardian.AI;

public class GuardianPrayerBehaviour : MonoBehaviour
{
	public enum GuardianState
	{
		Idle,
		Follow,
		Attack,
		Guard,
		Vanish
	}

	[FoldoutGroup("Guardian Skill Settings", 0)]
	[Range(0f, 2f)]
	public float ActionCooldown = 1f;

	[FoldoutGroup("Guardian Motion Settings", 0)]
	[MinMaxSlider(0f, 500f, false)]
	public Vector2 FollowSpeed;

	[FoldoutGroup("Guardian Motion Settings", 0)]
	[MinMaxSlider(0f, 10f, false)]
	public Vector2 FollowDistance;

	[FoldoutGroup("Guardian Motion Settings", 0)]
	[Range(0f, 10f)]
	public float ThresholdSpeed = 3f;

	[FoldoutGroup("Guardian Motion Settings", 0)]
	[Range(0f, 10f)]
	public float FollowingCooldown = 2f;

	[FoldoutGroup("Guardian Motion Settings", 0)]
	public Vector2 MasterOffsetPosition;

	[FoldoutGroup("Guardian Motion Settings", 0)]
	public float SmoothDampElongation = 0.3f;

	[FoldoutGroup("Guardian Motion Settings", 0)]
	[Range(0f, 0.1f)]
	public float FloatingVerticalElongation = 0.25f;

	[FoldoutGroup("Guardian Motion Settings", 0)]
	[Range(1f, 5f)]
	public float FloatingSpeed = 1.5f;

	[FoldoutGroup("Guardian Attack Settings", 0)]
	public float AttackDistance = 3f;

	[FoldoutGroup("Guardian Attack Settings", 0)]
	public float ShieldDistance = 1.5f;

	public GuardianPrayer Guardian;

	private Gameplay.GameControllers.Entities.StateMachine.StateMachine StateMachine;

	private GuardianState currentState;

	private float _currentActionCooldown;

	private float _reverseCoolDown = 0.25f;

	private float _currentReverseCoolDown;

	public Entity Master { get; set; }

	public bool VanishFlag { get; set; }

	public bool IdleFlag { get; set; }

	private bool LeftOrientation { get; set; }

	public bool IsTurning { get; set; }

	public EntityOrientation GuessedOrientation { get; private set; }

	public Vector3 GetMasterOffSetPosition
	{
		get
		{
			float hSpeed = Core.Logic.Penitent.PlatformCharacterController.PlatformCharacterPhysics.HSpeed;
			if (Mathf.Abs(hSpeed) > ThresholdSpeed)
			{
				return Master.transform.position;
			}
			if (Master.Status.Orientation == EntityOrientation.Right)
			{
				return Master.transform.position - (Vector3)MasterOffsetPosition;
			}
			return Master.transform.position + (Vector3)MasterOffsetPosition;
		}
	}

	private float GetMaxDisplacement => Mathf.Abs(Master.transform.position.x - base.transform.position.x);

	private bool ConsumeActionCooldown => _currentActionCooldown >= ActionCooldown;

	private bool CanPerformAction => !IsTurning && IsAlignedWithMaster;

	public float GetMasterDistance => Vector2.Distance(Master.transform.position, base.transform.position);

	public bool IsAlignedWithMaster => Guardian.Status.Orientation == Master.Status.Orientation;

	private void Awake()
	{
		StateMachine = GetComponentInChildren<Gameplay.GameControllers.Entities.StateMachine.StateMachine>();
	}

	private void OnEnable()
	{
		currentState = GuardianState.Follow;
		Guardian.SpriteRenderer.enabled = true;
		SetInitialOrientation();
	}

	private void Start()
	{
		Master = Core.Logic.Penitent;
		AnimatorInyector animatorInyector = Core.Logic.Penitent.AnimatorInyector;
		animatorInyector.OnAttack = (Core.SimpleEvent)Delegate.Combine(animatorInyector.OnAttack, new Core.SimpleEvent(OnAttack));
		Parry parry = Core.Logic.Penitent.Parry;
		parry.OnParryCast = (Core.SimpleEvent)Delegate.Combine(parry.OnParryCast, new Core.SimpleEvent(OnShieldCast));
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(OnMasterDead));
		SetInitialOrientation();
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		_currentActionCooldown += deltaTime;
		_currentReverseCoolDown += deltaTime;
		switch (currentState)
		{
		case GuardianState.Idle:
			if (VanishFlag)
			{
				SetState(GuardianState.Vanish);
			}
			else
			{
				StateMachine.SwitchState<GuardianPrayerIdleState>();
			}
			break;
		case GuardianState.Follow:
			if (VanishFlag)
			{
				SetState(GuardianState.Vanish);
			}
			else if (IdleFlag)
			{
				SetState(GuardianState.Idle);
			}
			else
			{
				StateMachine.SwitchState<GuardianPrayerFollowState>();
			}
			break;
		case GuardianState.Attack:
			_currentActionCooldown = 0f;
			StateMachine.SwitchState<GuardianPrayerAttackState>();
			break;
		case GuardianState.Guard:
			_currentActionCooldown = 0f;
			StateMachine.SwitchState<GuardianPrayerGuardState>();
			break;
		case GuardianState.Vanish:
			StateMachine.SwitchState<GuardianPrayerVanishState>();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void SetState(GuardianState state)
	{
		currentState = state;
	}

	public float GetActionDirection(float distance)
	{
		Vector3 position = base.transform.position;
		int num = ((Master.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
		float num2 = Mathf.Clamp(distance, 0f, GetMaxDisplacement);
		return position.x + num2 * (float)num;
	}

	public void SetInitialOrientation()
	{
		if ((bool)Master)
		{
			GuessedOrientation = Master.Status.Orientation;
			Guardian.SetOrientation(GuessedOrientation);
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
		if (!Core.Logic.Penitent.Animator.GetCurrentAnimatorStateInfo(0).IsName("HardLanding") && currentState == GuardianState.Follow && CanPerformAction && ConsumeActionCooldown)
		{
			SetState(GuardianState.Attack);
		}
	}

	private void OnShieldCast()
	{
		if (currentState == GuardianState.Follow && CanPerformAction && ConsumeActionCooldown)
		{
			SetState(GuardianState.Guard);
		}
	}

	private void OnMasterDead()
	{
		VanishFlag = true;
		SetState(GuardianState.Vanish);
	}

	private void SetOrientation(bool flip)
	{
		if (flip && GuessedOrientation != EntityOrientation.Left)
		{
			GuessedOrientation = EntityOrientation.Left;
			FlipGuardian();
		}
		else if (!flip && GuessedOrientation != 0)
		{
			GuessedOrientation = EntityOrientation.Right;
			FlipGuardian();
		}
	}

	private void FlipGuardian()
	{
		Guardian.AnimationHandler.SetAnimatorTrigger(GuardianPrayerAnimationHandler.TurnTrigger);
		Guardian.Audio.PlayTurn();
	}
}
