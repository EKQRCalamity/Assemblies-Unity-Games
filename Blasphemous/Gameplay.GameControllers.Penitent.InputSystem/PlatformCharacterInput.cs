using System;
using System.Collections;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.GameControllers.Penitent.InputSystem;

[RequireComponent(typeof(PlatformCharacterController))]
public class PlatformCharacterInput : MonoBehaviour
{
	[Serializable]
	public class JumpEvent : UnityEvent<Vector3>
	{
	}

	public enum EInputMode
	{
		Keyboard,
		Gamepad
	}

	public JumpEvent onJumpOff;

	public const string PlayerInputBlocker = "PLAYER_LOGIC";

	public const float VerticalJoystickOffset = 0.75f;

	private Penitent _penitent;

	public float AbsHAxis;

	public float AbsHorAxis;

	private bool aKey;

	public bool Attack;

	public float AxisMovingThreshold = 0.2f;

	private bool bKey;

	public bool canAirAttack;

	public bool Dash;

	private float deltaTimeButtonHeld;

	private float deltaTimeInputThreshold;

	private float deltaTimeJumpButtonHeld;

	private float deltaTimeOrientationThreshold;

	private float deltaTimeToJumpOff;

	public bool faceRight;

	public EInputMode InputMode = EInputMode.Gamepad;

	public bool isAirAttacking;

	public bool IsAttacking;

	public bool isJoystickDown;

	public bool isJoystickUp;

	public float forceHorizontalMovement;

	private bool jumpBlocked;

	private PlatformCharacterController m_platformCtrl;

	private bool pressedJumpButton;

	public bool ReachAxisThreshold;

	public bool simulatingMove;

	[Tooltip("Time pressing button to fire hold attack")]
	public float timeInputAttackHold = 0.5f;

	public float timeInputJumpHold = 0.5f;

	public float timeInputThreshold = 0.15f;

	private readonly float timeOrientationThreshold = 0.1f;

	[Range(0f, 1f)]
	public float timeToJumpOff = 1f;

	public bool unHang;

	public bool UseAxisAsSpeedFactor = true;

	private float verticalAxis;

	private float horizontalAxis;

	private bool xKey;

	public bool BlockJump { get; private set; }

	public bool IsJumpOff { get; private set; }

	public bool Blocked => Core.Input.InputBlocked;

	public Player Rewired { get; private set; }

	public float FVerAxis { get; private set; }

	public float ClampledVerticalAxis => verticalAxis;

	public float FHorAxis { get; private set; }

	public bool Jump { get; private set; }

	public float RVerticalAxis { get; private set; }

	public bool IsAttackButtonHold { get; private set; }

	public bool IsDashButtonHold { get; private set; }

	public bool IsJumpButtonHold { get; private set; }

	private bool IsFrontBlocked => _penitent.HasFlag("FRONT_BLOCKED");

	private void Awake()
	{
		_penitent = GetComponent<Entity>() as Penitent;
	}

	private void Start()
	{
		m_platformCtrl = GetComponent<PlatformCharacterController>();
		IsJumpButtonHold = false;
	}

	private void Update()
	{
		if (ReInput.players.playerCount <= 0)
		{
			return;
		}
		if (Rewired == null)
		{
			Player player = ReInput.players.GetPlayer(0);
			if (player == null)
			{
				return;
			}
			Rewired = player;
		}
		bool flag = !Blocked;
		if (!flag)
		{
			jumpBlocked = true;
		}
		if (flag && Rewired.GetButton(6))
		{
			jumpBlocked = false;
		}
		if (!simulatingMove)
		{
			if (flag)
			{
				horizontalAxis = Rewired.GetAxisRaw(0);
				verticalAxis = Rewired.GetAxisRaw(4);
				RVerticalAxis = Rewired.GetAxisRaw(20);
				if (Math.Abs(forceHorizontalMovement) > Mathf.Epsilon)
				{
					horizontalAxis = forceHorizontalMovement;
				}
				bKey = Rewired.GetButtonDown(5);
				xKey = Rewired.GetButtonDown(7);
				IsDashButtonHold = Rewired.GetButton(7);
				if (!jumpBlocked)
				{
					aKey = Rewired.GetButton(6);
				}
			}
			else
			{
				ResetInputs();
			}
		}
		verticalAxis = ResolveVerticalInputs(verticalAxis);
		if (InputMode != EInputMode.Gamepad)
		{
			return;
		}
		Dash = xKey;
		Attack = bKey;
		float num2 = (FHorAxis = horizontalAxis);
		isJoystickDown = IsJoystickDown();
		isJoystickUp = IsJoystickUp();
		Jump = aKey;
		AbsHorAxis = Mathf.Abs(num2);
		num2 = SetHorInputFactor(num2);
		num2 *= Mathf.Abs(num2);
		if (_penitent.Status.Paralyzed)
		{
			num2 *= 0f;
		}
		float num4 = (FVerAxis = verticalAxis);
		num4 *= Mathf.Abs(num4);
		float num5 = Mathf.Abs(num2);
		num5 = ((!(num5 > AxisMovingThreshold)) ? num5 : 1f);
		num5 = (AbsHAxis = ((!_penitent.AnimatorInyector.IsDashAnimRunning) ? num5 : 0f));
		float num6 = Mathf.Abs(num4);
		AttackButtonHold();
		ResetAttackButtonHold();
		if (Jump)
		{
			_penitent.IsStickedOnWall = false;
		}
		if (isAirAttacking)
		{
			num5 = 0f;
			return;
		}
		if (IsHorizontalClamped())
		{
			num5 = 0f;
			num6 = 0f;
			ReachAxisThreshold = false;
			if (m_platformCtrl.PlatformCharacterPhysics.VSpeed >= 1f)
			{
				m_platformCtrl.PlatformCharacterPhysics.VSpeed = 0f;
			}
			m_platformCtrl.PlatformCharacterPhysics.HSpeed = 0f;
			m_platformCtrl.SetActionState(eControllerActions.Jump, value: false);
			m_platformCtrl.SetActionState(eControllerActions.Left, value: false);
			m_platformCtrl.SetActionState(eControllerActions.Right, value: false);
			return;
		}
		ReachAxisThreshold = num5 > AxisMovingThreshold;
		if (_penitent.IsDashing)
		{
			num2 = 0f;
		}
		if (_penitent.Status.IsHurt || _penitent.Status.IsIdle)
		{
			IsJumpOff = false;
		}
		if (!_penitent.IsClimbingCliffLede)
		{
			SetOrientation(horizontalAxis);
			if (num5 >= AxisMovingThreshold)
			{
				m_platformCtrl.HorizontalSpeedScale = ((!UseAxisAsSpeedFactor) ? 1f : num5);
			}
			if (num6 >= AxisMovingThreshold)
			{
				m_platformCtrl.VerticalSpeedScale = ((!UseAxisAsSpeedFactor) ? 1f : num6);
			}
			if (_penitent.IsGrabbingLadder || _penitent.IsCrouched || _penitent.BeginCrouch || _penitent.IsCrouchAttacking || IsFrontBlocked)
			{
				m_platformCtrl.SetActionState(eControllerActions.Left, value: false);
				m_platformCtrl.SetActionState(eControllerActions.Right, value: false);
			}
			else
			{
				m_platformCtrl.SetActionState(eControllerActions.Left, num2 <= 0f - AxisMovingThreshold && flag);
				m_platformCtrl.SetActionState(eControllerActions.Right, num2 >= AxisMovingThreshold && flag);
			}
			m_platformCtrl.SetActionState(eControllerActions.Down, num4 <= 0f - AxisMovingThreshold && flag);
			m_platformCtrl.SetActionState(eControllerActions.Up, num4 >= AxisMovingThreshold && flag);
			if (Jump && !IsJumpOff && m_platformCtrl.IsGrounded && _penitent.isJumpOffReady && !pressedJumpButton && !_penitent.FloorChecker.IsOnFloorPlatform && !_penitent.StepOnLadder && isJoystickDown && !Core.LevelManager.currentLevel.LevelName.Equals("D24Z01S01"))
			{
				pressedJumpButton = true;
				IsJumpOff = true;
				if (onJumpOff != null)
				{
					onJumpOff.Invoke(base.transform.position);
				}
				StartCoroutine(JumpOff());
			}
			else if (!Jump)
			{
				pressedJumpButton = false;
			}
			else
			{
				IsJumpOff = false;
			}
			bool isGrounded = m_platformCtrl.IsGrounded;
			if ((!IsAttacking || !isGrounded) && !_penitent.IsDashing && !_penitent.BeginCrouch && !_penitent.GrabLadder.IsBottomLadderRepositioning && !_penitent.GrabLadder.StartGoingDown)
			{
				if (!_penitent.IsClimbingLadder)
				{
					bool value = aKey && !BlockJump && !Blocked && (!Rewired.GetButton(5) || !isGrounded) && !IsJoystickDown() && !Rewired.GetButtonDown(23) && !_penitent.IsFallingStunt;
					m_platformCtrl.SetActionState(eControllerActions.Jump, value);
				}
				else if (_penitent.CanJumpFromLadder && Rewired.GetAxis(4) > -1f)
				{
					if (aKey)
					{
						m_platformCtrl.ResetLadderJumpTimeThreshold();
					}
					m_platformCtrl.SetActionState(eControllerActions.Jump, aKey && !BlockJump && !Rewired.GetButton(5));
				}
			}
			JumpButtonHold();
			ResetJumpButtonHold();
		}
		else
		{
			unHang = _penitent.CanLowerCliff;
		}
	}

	private float SetHorInputFactor(float horInput)
	{
		float result = 0f;
		if (horInput > AxisMovingThreshold)
		{
			result = 1f;
		}
		else if (horInput < 0f - AxisMovingThreshold)
		{
			result = -1f;
		}
		return result;
	}

	private void SetOrientation(float horRawInput)
	{
		if (horRawInput > AxisMovingThreshold)
		{
			faceRight = true;
		}
		else if (horRawInput < 0f - AxisMovingThreshold)
		{
			faceRight = false;
		}
	}

	private static float ResolveVerticalInputs(float currentVerticalInput)
	{
		float result = 0f;
		if (currentVerticalInput > 0.65f)
		{
			result = 1f;
		}
		else if (currentVerticalInput < -0.65f)
		{
			result = -1f;
		}
		return result;
	}

	private IEnumerator JumpOff()
	{
		m_platformCtrl.SetActionState(eControllerActions.PlatformDropDown, value: false);
		deltaTimeToJumpOff = 0f;
		while (deltaTimeToJumpOff <= timeToJumpOff)
		{
			deltaTimeToJumpOff += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		m_platformCtrl.SetActionState(eControllerActions.PlatformDropDown, value: true);
		IsJumpOff = false;
	}

	public void CancelPlatformDropDown()
	{
		m_platformCtrl.SetActionState(eControllerActions.PlatformDropDown, value: false);
	}

	public void CancelJump()
	{
		m_platformCtrl.SetActionState(eControllerActions.Jump, value: false);
		m_platformCtrl.PlatformCharacterPhysics.VSpeed = 0f;
	}

	private bool IsJoystickDown()
	{
		return !Blocked && Rewired.GetAxis(4) <= -0.75f && Rewired.GetAxis(4) >= -1f;
	}

	private bool IsJoystickUp()
	{
		return !Blocked && Rewired.GetAxis(4) >= 0.75f && Rewired.GetAxis(4) <= 1f;
	}

	private void AttackButtonHold()
	{
		if (Core.Input.HasBlocker("DIALOG") || Core.Input.HasBlocker("PLAYER_LOGIC"))
		{
			return;
		}
		if (_penitent.Status.Dead || !_penitent.Status.IsGrounded)
		{
			deltaTimeButtonHeld = 0f;
			IsAttackButtonHold = false;
		}
		if (Rewired.GetButton(5))
		{
			deltaTimeButtonHeld += Time.deltaTime;
			if (deltaTimeButtonHeld >= timeInputAttackHold && !IsAttackButtonHold)
			{
				deltaTimeButtonHeld = 0f;
				IsAttackButtonHold = true;
			}
		}
	}

	public void ResetInputs()
	{
		horizontalAxis = 0f;
		verticalAxis = 0f;
		bKey = false;
		xKey = false;
		aKey = false;
	}

	public void ResetActions()
	{
		if ((bool)m_platformCtrl)
		{
			m_platformCtrl.SetActionState(eControllerActions.Jump, value: false);
			m_platformCtrl.SetActionState(eControllerActions.Up, value: false);
			m_platformCtrl.SetActionState(eControllerActions.Down, value: false);
			m_platformCtrl.SetActionState(eControllerActions.Left, value: false);
			m_platformCtrl.SetActionState(eControllerActions.Right, value: false);
		}
	}

	public void ResetHorizontalBlockers()
	{
		if ((bool)_penitent)
		{
			_penitent.IsGrabbingLadder = false;
			_penitent.IsCrouched = false;
			_penitent.BeginCrouch = false;
			_penitent.IsCrouchAttacking = false;
		}
	}

	private void ResetAttackButtonHold()
	{
		if (Rewired.GetButtonUp(5))
		{
			deltaTimeButtonHeld = 0f;
			IsAttackButtonHold = false;
		}
	}

	private void JumpButtonHold()
	{
		if (!Rewired.GetButton(6))
		{
			return;
		}
		deltaTimeJumpButtonHeld += Time.deltaTime;
		if (deltaTimeJumpButtonHeld >= timeInputJumpHold && !IsJumpButtonHold)
		{
			deltaTimeJumpButtonHeld = 0f;
			if (!IsJumpButtonHold)
			{
				IsJumpButtonHold = true;
			}
			if (!BlockJump)
			{
				BlockJump = true;
			}
		}
	}

	private void ResetJumpButtonHold()
	{
		if (!Rewired.GetButton(6))
		{
			deltaTimeJumpButtonHeld = 0f;
			if (IsJumpButtonHold)
			{
				IsJumpButtonHold = !IsJumpButtonHold;
			}
			if (BlockJump)
			{
				BlockJump = !BlockJump;
			}
		}
	}

	private bool IsHorizontalClamped()
	{
		return _penitent.Status.IsHurt || _penitent.IsJumpingOff || _penitent.Status.Dead || _penitent.IsChargingAttack || IsAttacking;
	}

	public void Move(float x, float time)
	{
		StartCoroutine(MoveAction(x, time));
	}

	public IEnumerator MoveAction(float x, float time)
	{
		simulatingMove = true;
		horizontalAxis = x;
		yield return new WaitForSeconds(time);
		simulatingMove = false;
	}
}
