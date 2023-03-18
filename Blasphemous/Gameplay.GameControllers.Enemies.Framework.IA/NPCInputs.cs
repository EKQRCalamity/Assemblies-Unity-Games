using CreativeSpore.SmartColliders;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

[RequireComponent(typeof(PlatformCharacterController))]
public class NPCInputs : MonoBehaviour
{
	public enum eInputMode
	{
		Keyboard,
		Gamepad
	}

	private readonly float timeOrientationThreshold = 0.1f;

	private float _horizontalInput;

	private float _verticalInput;

	public float AbsHAxis;

	public float AbsHorAxis;

	public bool Attack;

	public float AxisMovingThreshold = 0.2f;

	public bool canAirAttack;

	public bool Dash;

	private float deltaTimeInputThreshold;

	private float deltaTimeOrientationThreshold;

	private float deltaTimeToJumpOff;

	public bool faceRight = true;

	public eInputMode InputMode = eInputMode.Gamepad;

	public bool isAirAttacking;

	public bool isAttacking;

	public bool isJoystickDown;

	public bool isJoystickUp;

	private PlatformCharacterController m_platformCtrl;

	public bool ReachAxisThreshold;

	public float timeInputThreshold = 0.15f;

	[Range(0f, 1f)]
	public float timeToJumpOff = 1f;

	public bool unHang;

	public bool UseAxisAsSpeedFactor = true;

	public float HorizontalInput
	{
		get
		{
			return _horizontalInput;
		}
		set
		{
			_horizontalInput = value;
		}
	}

	public float VerticalInput
	{
		set
		{
			_verticalInput = value;
		}
	}

	public bool IsJumpOff { get; private set; }

	public float FVerAxis { get; private set; }

	public float FHorAxis { get; private set; }

	public bool Jump { get; set; }

	private void Start()
	{
		m_platformCtrl = GetComponent<PlatformCharacterController>();
	}

	private void Update()
	{
		if (InputMode == eInputMode.Gamepad)
		{
			Dash = Input.GetButtonDown("Dash");
			Attack = Input.GetButtonDown("Attack");
			float num = (FHorAxis = _horizontalInput);
			isJoystickDown = IsJoystickDown();
			isJoystickUp = IsJoystickUp();
			AbsHorAxis = Mathf.Abs(num);
			num *= Mathf.Abs(num);
			float num2 = (FVerAxis = _verticalInput);
			num2 *= Mathf.Abs(num2);
			float num3 = Mathf.Abs(num);
			num3 = (AbsHAxis = ((!(num3 > AxisMovingThreshold)) ? num3 : 1f));
			float num4 = Mathf.Abs(num2);
			ReachAxisThreshold = num3 > AxisMovingThreshold;
			if (num3 >= AxisMovingThreshold)
			{
				m_platformCtrl.HorizontalSpeedScale = ((!UseAxisAsSpeedFactor) ? 1f : num3);
			}
			if (num4 >= AxisMovingThreshold)
			{
				m_platformCtrl.VerticalSpeedScale = ((!UseAxisAsSpeedFactor) ? 1f : num4);
			}
			m_platformCtrl.SetActionState(eControllerActions.Left, num <= 0f - AxisMovingThreshold);
			m_platformCtrl.SetActionState(eControllerActions.Right, num >= AxisMovingThreshold);
			m_platformCtrl.SetActionState(eControllerActions.Down, num2 <= 0f - AxisMovingThreshold);
			m_platformCtrl.SetActionState(eControllerActions.Up, num2 >= AxisMovingThreshold);
			m_platformCtrl.SetActionState(eControllerActions.Jump, Jump);
		}
	}

	protected float setHorInputFactor(float horInput)
	{
		if (Mathf.Abs(horInput) > AxisMovingThreshold)
		{
			deltaTimeInputThreshold += Time.deltaTime;
			if (deltaTimeInputThreshold >= timeInputThreshold)
			{
				return horInput;
			}
			return 0f;
		}
		deltaTimeInputThreshold = 0f;
		return 0f;
	}

	protected void setOrientation(float horRawInput)
	{
		deltaTimeOrientationThreshold += Time.deltaTime;
		if (horRawInput > 0f && deltaTimeOrientationThreshold >= timeOrientationThreshold)
		{
			deltaTimeOrientationThreshold = 0f;
			faceRight = true;
		}
		else if (horRawInput < 0f && deltaTimeOrientationThreshold >= timeOrientationThreshold)
		{
			deltaTimeOrientationThreshold = 0f;
			faceRight = false;
		}
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

	protected bool IsJoystickDown()
	{
		return Mathf.Approximately(Input.GetAxisRaw("Vertical"), -1f) && Mathf.Abs(Input.GetAxis("Horizontal")) < AxisMovingThreshold;
	}

	protected bool IsJoystickUp()
	{
		return Mathf.Approximately(Input.GetAxisRaw("Vertical"), 1f) && Mathf.Abs(Input.GetAxis("Horizontal")) < AxisMovingThreshold;
	}
}
