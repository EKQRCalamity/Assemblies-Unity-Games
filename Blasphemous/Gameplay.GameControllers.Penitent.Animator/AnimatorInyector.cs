using CreativeSpore.SmartColliders;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent.Abilities;
using Gameplay.GameControllers.Penitent.InputSystem;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Animator;

[RequireComponent(typeof(PlatformCharacterController))]
public class AnimatorInyector : MonoBehaviour
{
	public Core.SimpleEvent OnAttack;

	private readonly int _climbCliffAnim = UnityEngine.Animator.StringToHash("Player_Climb_Edge");

	private PlatformCharacterController _platformCharacterController;

	private PlatformCharacterInput _playerInput;

	public UnityEngine.Animator SpriteAnimator;

	private Penitent _penitent;

	private Dash _playerDash;

	private bool _isGrounded;

	private bool _isJumpReady;

	private bool _startRun;

	private bool _readyToRun;

	private const float AttackTime = 0.05f;

	private float _deltaAttackTime;

	private bool _setDeadAnimation;

	private bool _isChargeAttackTriggered;

	private bool _setImpaledAnimation;

	[Tooltip("Max VSPeed without stunting fall")]
	[Range(0f, 20f)]
	public float MaxVSpeedFallStunt;

	private static readonly int IsAttackCharged = UnityEngine.Animator.StringToHash("IS_ATTACK_CHARGED");

	private static readonly int IsAttackHold = UnityEngine.Animator.StringToHash("IS_ATTACK_HOLD");

	private static readonly int Attack = UnityEngine.Animator.StringToHash("ATTACK");

	private static readonly int AirAttackParam = UnityEngine.Animator.StringToHash("AIR_ATTACK");

	private static readonly int ChargeAttack = UnityEngine.Animator.StringToHash("CHARGE_ATTACK");

	private static readonly int RunStep = UnityEngine.Animator.StringToHash("RUN_STEP");

	private static readonly int RunningParam = UnityEngine.Animator.StringToHash("RUNNING");

	private static readonly int Grounded = UnityEngine.Animator.StringToHash("GROUNDED");

	private static readonly int CanJumpOff = UnityEngine.Animator.StringToHash("CAN_JUMP_OFF");

	private static readonly int IsJumpingOff = UnityEngine.Animator.StringToHash("IS_JUMPING_OFF");

	private static readonly int CanAirAttack = UnityEngine.Animator.StringToHash("CAN_AIR_ATTACK");

	private static readonly int Ladder = UnityEngine.Animator.StringToHash("IS_CLIMBING_LADDER");

	private static readonly int IsGrabbingLadder = UnityEngine.Animator.StringToHash("IS_GRABBING_LADDER");

	private static readonly int IsHurt = UnityEngine.Animator.StringToHash("IS_HURT");

	private static readonly int Dead = UnityEngine.Animator.StringToHash("IS_DEAD");

	private static readonly int ClimbCliffLedge = UnityEngine.Animator.StringToHash("CLIMB_CLIFF_LEDGE");

	private static readonly int IsGrabbingCliffLede = UnityEngine.Animator.StringToHash("IS_GRABBING_CLIFF_LEDE");

	private static readonly int AirAttacking = UnityEngine.Animator.StringToHash("AIR_ATTACKING");

	private static readonly int Unhang = UnityEngine.Animator.StringToHash("UNHANG");

	private static readonly int AxisThreshold = UnityEngine.Animator.StringToHash("AXIS_THRESHOLD");

	private static readonly int JoystickUp = UnityEngine.Animator.StringToHash("JOYSTICK_UP");

	private static readonly int JoystickDown = UnityEngine.Animator.StringToHash("JOYSTICK_DOWN");

	private static readonly int Jump = UnityEngine.Animator.StringToHash("JUMP");

	private static readonly int JumpOff = UnityEngine.Animator.StringToHash("JUMP_OFF");

	private static readonly int IsCrouch = UnityEngine.Animator.StringToHash("IS_CROUCH");

	private static readonly int Falling = UnityEngine.Animator.StringToHash("FALLING");

	private static readonly int Dash = UnityEngine.Animator.StringToHash("DASH");

	private static readonly int DashingParam = UnityEngine.Animator.StringToHash("DASHING");

	private static readonly int Sliding = UnityEngine.Animator.StringToHash("LADDER_SLIDING");

	private static readonly int Hurt = UnityEngine.Animator.StringToHash("HURT");

	private static readonly int Throw = UnityEngine.Animator.StringToHash("THROW");

	private static readonly int Death = UnityEngine.Animator.StringToHash("DEATH");

	private static readonly int DeathSpike = UnityEngine.Animator.StringToHash("DEATH_SPIKE");

	private static readonly int Demake = UnityEngine.Animator.StringToHash("DEMAKE");

	public bool FireJumpOffTrigger { get; set; }

	public bool ForwardJump { get; set; }

	public bool IsJumpingForward { get; set; }

	public bool IsAirAttacking { get; set; }

	public bool IsDashAnimRunning { get; private set; }

	public bool IsJumpWhileDashing { get; set; }

	public bool IsFalling { get; private set; }

	public float TimeGrounded { get; private set; }

	private bool IsDemakeMode => Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE);

	private EntityOrientation GetPlayerInputOrientation => (!_playerInput.faceRight) ? EntityOrientation.Left : EntityOrientation.Right;

	public bool IsHoldingChargeAttack => _penitent.ChargedAttack.IsChargingAttack || _penitent.ReleaseChargedAttack;

	private void Awake()
	{
		_platformCharacterController = GetComponent<PlatformCharacterController>();
		_playerInput = GetComponent<PlatformCharacterInput>();
		_penitent = GetComponent<Penitent>();
		_playerDash = _penitent.GetComponentInChildren<Dash>();
	}

	private void Start()
	{
		_setDeadAnimation = false;
		_setImpaledAnimation = false;
	}

	private void Update()
	{
		_deltaAttackTime += Time.deltaTime;
		_penitent.Status.IsGrounded = _platformCharacterController.IsGrounded;
		_isGrounded = _penitent.Status.IsGrounded;
		_penitent.SlopeAngle = _platformCharacterController.SmartPlatformCollider.CalculateSlopeAngle();
		CheckStuntFall();
		_penitent.WatchBelow = _playerInput.RVerticalAxis <= -0.2f && _penitent.Status.IsGrounded;
		JoystickAxisInput();
		UpdateActions();
	}

	private void JoystickAxisInput()
	{
		SpriteAnimator.SetBool(AxisThreshold, _playerInput.ReachAxisThreshold);
		SpriteAnimator.SetBool(JoystickUp, _playerInput.FVerAxis > 0f && _playerInput.FVerAxis > _playerInput.AxisMovingThreshold);
		SpriteAnimator.SetBool(JoystickDown, _playerInput.FVerAxis < 0f && Mathf.Abs(_playerInput.FVerAxis) > _playerInput.AxisMovingThreshold);
	}

	private void EnableAnimator(bool enable = true)
	{
		SpriteAnimator.enabled = enable;
	}

	private void UpdateActions()
	{
		if (!SpriteAnimator)
		{
			return;
		}
		SpriteAnimator.SetBool(Grounded, _isGrounded);
		SpriteAnimator.SetBool(CanJumpOff, _penitent.FloorChecker.OneWayDownCollision && _penitent.isJumpOffReady);
		SpriteAnimator.SetBool(IsJumpingOff, _penitent.IsJumpingOff || _playerInput.IsJumpOff);
		SpriteAnimator.SetBool(CanAirAttack, !_isGrounded);
		SpriteAnimator.SetBool(Ladder, _penitent.IsClimbingLadder);
		SpriteAnimator.SetBool(IsGrabbingLadder, _penitent.IsGrabbingLadder);
		SpriteAnimator.SetBool(IsHurt, _penitent.Status.IsHurt);
		SpriteAnimator.SetBool(Dead, _penitent.Status.Dead);
		SpriteAnimator.SetBool(Demake, IsDemakeMode);
		if (_isGrounded)
		{
			TimeGrounded += Time.deltaTime;
			SpriteAnimator.ResetTrigger(AirAttackParam);
			SpriteAnimator.ResetTrigger(ClimbCliffLedge);
			SpriteAnimator.SetBool(IsGrabbingCliffLede, value: false);
			SpriteAnimator.SetBool(AirAttacking, value: false);
			SpriteAnimator.ResetTrigger(Unhang);
			IsDashAnimRunning = SpriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dash");
			_penitent.IsGrabbingCliffLede = false;
			_penitent.IsStickedOnWall = false;
			_penitent.CanJumpFromLadder = false;
			_penitent.JumpFromLadder = false;
			IsJumpingForward = false;
			IsAirAttacking = false;
			_isJumpReady = true;
			ForwardJump = false;
			IsJumpWhileDashing = false;
			Dashing();
			GroundAttack();
			ChargedAttack();
			Crouch();
		}
		else
		{
			TimeGrounded = 0f;
			FireJumpOffTrigger = false;
			if (!_penitent.IsClimbingLadder)
			{
				SpriteAnimator.ResetTrigger(Jump);
			}
			if (_penitent.IsDashing)
			{
				_penitent.Dash.StopCast();
			}
			SpriteAnimator.ResetTrigger(JumpOff);
			SpriteAnimator.SetBool(CanJumpOff, _isGrounded);
			SpriteAnimator.SetBool(IsCrouch, _isGrounded);
			SpriteAnimator.SetBool(IsGrabbingCliffLede, _penitent.IsGrabbingCliffLede);
			IsJumpingForward = SpriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Jump Forward") || SpriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Falling Forward");
			IsAirAttacking = SpriteAnimator.GetBool(AirAttacking);
			IsDashAnimRunning = false;
			AirAttack();
			ClimbingLadder();
			LadderSliding();
			ClimbingCliffLede();
		}
		Running();
		Jumping();
		IsClimbingLadder();
		IsDead();
	}

	public void UpdateAirAttackingAction()
	{
		IsAirAttacking = SpriteAnimator.GetBool(AirAttacking);
	}

	private void SetOrientation(EntityOrientation playerOrientation)
	{
		if ((bool)_penitent)
		{
			_penitent.SetOrientation(playerOrientation);
		}
	}

	private void Running()
	{
		if (!SpriteAnimator)
		{
			return;
		}
		if (!IsDashAnimRunning && !_penitent.Status.IsHurt && !_penitent.IsClimbingLadder)
		{
			EntityOrientation getPlayerInputOrientation = GetPlayerInputOrientation;
			float absHorAxis = _playerInput.AbsHorAxis;
			if (_penitent.Status.Orientation != getPlayerInputOrientation && absHorAxis > 0f)
			{
				SetOrientation(getPlayerInputOrientation);
			}
		}
		_startRun = _playerInput.ReachAxisThreshold && _isGrounded;
		SpriteAnimator.SetBool(RunStep, _startRun);
		SpriteAnimator.SetBool(RunningParam, _startRun);
	}

	public void RaiseAttackEvent()
	{
		if (OnAttack != null)
		{
			OnAttack();
		}
	}

	private void GroundAttack()
	{
		if (_playerInput.Attack && !_playerInput.Jump)
		{
			_deltaAttackTime = 0f;
			SpriteAnimator.SetTrigger(Attack);
		}
		else if (_deltaAttackTime >= 0.05f)
		{
			SpriteAnimator.ResetTrigger(Attack);
		}
	}

	private void AirAttack()
	{
		if ((!IsDemakeMode || !_playerInput.isJoystickUp) && _playerInput.Attack)
		{
			SpriteAnimator.SetTrigger(AirAttackParam);
		}
	}

	private void ChargedAttack()
	{
		if (!_playerInput.Blocked)
		{
			SpriteAnimator.SetBool(IsAttackCharged, _penitent.IsAttackCharged);
			SpriteAnimator.SetBool(IsAttackHold, _playerInput.IsAttackButtonHold);
			ChargeAttackTriggered();
		}
	}

	private void ChargeAttackTriggered()
	{
		if (_playerInput.IsAttackButtonHold && !_penitent.ReleaseChargedAttack && _penitent.ChargedAttack.IsAvailableSkilledAbility)
		{
			if (!_isChargeAttackTriggered)
			{
				_isChargeAttackTriggered = true;
				SpriteAnimator.SetTrigger(ChargeAttack);
			}
		}
		else
		{
			_isChargeAttackTriggered = false;
			SpriteAnimator.ResetTrigger(ChargeAttack);
		}
	}

	private void Jumping()
	{
		if (!SpriteAnimator)
		{
			return;
		}
		if (!_isGrounded && _isJumpReady && !IsFalling && !_penitent.IsClimbingCliffLede && !_playerInput.isJoystickDown && !_penitent.StepOnLadder && !_penitent.JumpFromLadder)
		{
			if (SpriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("ComboFinisherUp"))
			{
				return;
			}
			_isJumpReady = !_isJumpReady;
			string trigger = ((!_playerInput.ReachAxisThreshold) ? "JUMP" : "FORWARD_JUMP");
			if (IsJumpWhileDashing)
			{
				trigger = "FORWARD_JUMP";
			}
			SpriteAnimator.SetTrigger(trigger);
			_penitent.OnJumpTrigger(this);
		}
		else if (!_isGrounded && IsFalling)
		{
			SpriteAnimator.SetBool(RunningParam, _playerInput.ReachAxisThreshold);
		}
		IsJumping();
	}

	public void CancelJumpOff()
	{
		_penitent.IsJumpingOff = false;
	}

	private void IsJumping()
	{
		int shortNameHash = SpriteAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash;
		bool isJumping = shortNameHash == UnityEngine.Animator.StringToHash("Jump") || shortNameHash == UnityEngine.Animator.StringToHash("Falling") || shortNameHash == UnityEngine.Animator.StringToHash("Jump Forward") || ((shortNameHash == UnityEngine.Animator.StringToHash("Falling Forward")) ? true : false);
		_penitent.IsJumping = isJumping;
	}

	private void CheckStuntFall()
	{
		if (!_isGrounded)
		{
			float vSpeed = _platformCharacterController.PlatformCharacterPhysics.VSpeed;
			IsFalling = vSpeed <= -0.1f;
			if (vSpeed <= 0f - MaxVSpeedFallStunt && !IsDemakeMode)
			{
				_penitent.IsFallingStunt = true;
			}
		}
		else
		{
			IsFalling = false;
		}
		SpriteAnimator.SetBool(Falling, IsFalling);
	}

	public void ResetStuntByFall()
	{
		if (_penitent.IsFallingStunt)
		{
			_penitent.IsFallingStunt = false;
		}
	}

	private void Dashing()
	{
		if (_playerInput.Dash && !_playerInput.Jump && _penitent.Dash.enabled && _playerDash.ReadyToUse && !_penitent.IsGrabbingCliffLede && !_penitent.Status.IsHurt && !_penitent.Status.Dead && !_penitent.Dash.StandUpAfterDash && !_penitent.IsChargingAttack && !_penitent.PlatformCharacterInput.Blocked && !_penitent.IsFallingStunt)
		{
			SpriteAnimator.SetTrigger(Dash);
			SpriteAnimator.ResetTrigger(Jump);
			SpriteAnimator.SetBool(DashingParam, value: true);
			_playerDash.Cast();
		}
		SpriteAnimator.SetBool(DashingParam, SpriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dash"));
	}

	private void ClimbingLadder()
	{
		if (_penitent.IsClimbingLadder)
		{
			_penitent.JumpFromLadder = false;
			SpriteAnimator.ResetTrigger(Jump);
			_penitent.CanJumpFromLadder = true;
			if (_playerInput.Jump && !ForwardJump)
			{
				SpriteAnimator.ResetTrigger(AirAttackParam);
				ForwardJump = true;
				_penitent.IsGrabbingLadder = false;
			}
		}
		else
		{
			_penitent.CanJumpFromLadder = false;
		}
	}

	private void LadderSliding()
	{
		bool flag = false;
		if (_penitent.IsOnLadder)
		{
			flag = _playerInput.isJoystickDown && _playerInput.IsJumpButtonHold;
		}
		_penitent.IsLadderSliding = flag;
		SpriteAnimator.SetBool(Sliding, flag);
	}

	private void ClimbingCliffLede()
	{
		if (_playerInput.isJoystickUp && _penitent.canClimbCliffLede && _penitent.cliffLedeClimbingStarted)
		{
			ClimbCliffLede();
		}
		if (_penitent.IsClimbingCliffLede && _playerInput.unHang)
		{
			ReleaseCliffLede();
		}
	}

	public void ClimbCliffLede()
	{
		_penitent.canClimbCliffLede = false;
		_penitent.transform.position = _penitent.RootMotionDrive;
		SpriteAnimator.Play(_climbCliffAnim);
	}

	public void ReleaseCliffLede()
	{
		_penitent.GrabCliffLede.ReleaseCliffLede();
		SpriteAnimator.SetTrigger(Unhang);
		if (_penitent.CanLowerCliff)
		{
			_penitent.CanLowerCliff = false;
		}
	}

	public void ManualHangOffCliff()
	{
		_penitent.Physics.EnablePhysics();
		_penitent.CanLowerCliff = true;
		float num = ((_penitent.Status.Orientation != EntityOrientation.Left) ? (-0.5f) : 0.5f);
		float endValue = _penitent.transform.position.x + num;
		_penitent.transform.DOMoveX(endValue, 0.1f).SetEase(Ease.OutSine);
	}

	private void IsClimbingLadder()
	{
		_penitent.IsClimbingLadder = _penitent.PlatformCharacterController.IsClimbing;
	}

	private void Crouch()
	{
		_penitent.IsCrouched = _playerInput.isJoystickDown && !_playerInput.IsAttackButtonHold && !IsDemakeMode && !_penitent.IsChargingAttack && !_penitent.IsDashing && !_penitent.PenitentAttack.IsRunningCombo;
		SpriteAnimator.SetBool(IsCrouch, _penitent.IsCrouched && !_playerInput.IsJumpOff);
		if (_playerInput.IsJumpOff && !FireJumpOffTrigger)
		{
			SpriteAnimator.SetTrigger(JumpOff);
			FireJumpOffTrigger = true;
		}
	}

	public void PlayerGetDamage(DamageArea.DamageType damageType)
	{
		switch (damageType)
		{
		case DamageArea.DamageType.Normal:
			if (_penitent.Status.IsGrounded || _penitent.IsClimbingLadder)
			{
				SpriteAnimator.SetTrigger(Hurt);
			}
			break;
		case DamageArea.DamageType.Heavy:
			SpriteAnimator.SetBool(Throw, value: true);
			break;
		case DamageArea.DamageType.Critical:
			break;
		case DamageArea.DamageType.Simple:
			break;
		}
	}

	private void IsDead()
	{
		if (_penitent.Status.Dead)
		{
			if (!_setDeadAnimation && !_penitent.IsDeadInAir)
			{
				_setDeadAnimation = true;
				SpriteAnimator.SetTrigger(Death);
			}
		}
		else if (_setDeadAnimation)
		{
			_setDeadAnimation = false;
			EnableAnimator();
			SpriteAnimator.ResetTrigger(Death);
		}
		if (_penitent.IsImpaled && !_setImpaledAnimation)
		{
			_setImpaledAnimation = true;
			SpriteAnimator.SetTrigger(DeathSpike);
		}
		else if (!_penitent.IsImpaled && _setImpaledAnimation)
		{
			_setImpaledAnimation = !_setImpaledAnimation;
			SpriteAnimator.ResetTrigger(DeathSpike);
		}
	}
}
