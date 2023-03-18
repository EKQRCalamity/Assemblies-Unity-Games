using System;
using System.Collections;
using CreativeSpore.SmartColliders;
using DG.Tweening;
using DG.Tweening.Core.Surrogates;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent.Gizmos;
using Gameplay.GameControllers.Penitent.Sensor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class GrabLadder : Trait
{
	private Penitent _penitent;

	private RootMotionDriver _rootMotionDriver;

	private LayerMask _layerMaskLadder;

	private float _maxClimbingSpeed;

	private PlatformCharacterController _playerController;

	[Tooltip("Remain time to start climbing when penitent puts the joystick up")]
	[Range(0f, 0.5f)]
	[FoldoutGroup("Climb Settings", 0)]
	public float timeToStartClimbing = 0.2f;

	public const float MAX_CLIMB_SPEED = 2.25f;

	private LayerMask _climbingLadderLayermask;

	private int _ladderLayer;

	private bool _enableClimbLadderAbility;

	public float LadderRepositionLapse = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	[FoldoutGroup("Climb Settings", 0)]
	private float TakeOffLadderCoolDown = 0.5f;

	private Vector2 _ladderBottomPoint;

	[SerializeField]
	[Tooltip("Consider the player repositioned when closer than ladderWidth * ladderWidthFactor")]
	[Range(0f, 1f)]
	private float ladderWidthFactor = 0.2f;

	private static readonly int IsCollidingLadderHash = UnityEngine.Animator.StringToHash("IS_COLLIDING_LADDER");

	private static readonly int StepOnLadderHash = UnityEngine.Animator.StringToHash("STEP_ON_LADDER");

	public bool IsBottomLadderRepositioning { get; set; }

	public bool IsTopLadderReposition { get; set; }

	public bool StartGoingDown { get; set; }

	public Collider2D CurrentLadderCollider { get; private set; }

	public Vector3 LadderTopPoint { get; private set; }

	private Vector2 PlayerBottomPointCollider
	{
		get
		{
			float x = _penitent.DamageArea.DamageAreaCollider.bounds.center.x;
			float y = _penitent.DamageArea.DamageAreaCollider.bounds.min.y;
			return new Vector2(x, y);
		}
	}

	private bool IsTakingOffLadder => false || base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("grab_ladder_to_go_down") || base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("release_ladder_to_floor_up");

	protected override void OnStart()
	{
		base.OnStart();
		_penitent = (Penitent)base.EntityOwner;
		_penitent.OnDamaged += PenitentOnDamaged;
		_playerController = _penitent.GetComponent<PlatformCharacterController>();
		_rootMotionDriver = _penitent.GetComponentInChildren<RootMotionDriver>();
		_layerMaskLadder = _penitent.PlatformCharacterController.ClimbingLayers;
		_penitent.PlatformCharacterController.ClimbingSpeed = 0f;
		_climbingLadderLayermask = _playerController.ClimbingLayers;
		_ladderLayer = LayerMask.NameToLayer("Ladder");
		IsBottomLadderRepositioning = false;
		_enableClimbLadderAbility = true;
		_penitent.CanClimbLadder = true;
		FloorDistanceChecker.OnStepLadder = (Core.GenericEvent)Delegate.Combine(FloorDistanceChecker.OnStepLadder, new Core.GenericEvent(OnStepLadder));
		_penitent.OnDeath += PenitentOnDeath;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (IsBottomLadderRepositioning)
		{
			IsBottomLadderRepositioning = false;
		}
		StartGoingDown = _penitent.StepOnLadder && _penitent.PlatformCharacterInput.isJoystickDown && !_penitent.PlatformCharacterController.IsClimbing && _penitent.Status.IsGrounded;
		bool flag = false;
		if (CurrentLadderCollider != null)
		{
			float num = DistanceToTopLadder(_penitent.transform.position);
			flag = num < CurrentLadderCollider.bounds.size.x * ladderWidthFactor;
		}
		if (StartGoingDown && !IsTopLadderReposition)
		{
			IsTopLadderReposition = true;
			TopLadderReposition();
		}
		bool value = _penitent.StepOnLadder && flag && _penitent.CanClimbLadder;
		base.EntityOwner.Animator.SetBool(StepOnLadderHash, value);
		base.EntityOwner.Animator.SetBool(IsCollidingLadderHash, _penitent.IsOnLadder);
		if (!_penitent.StepOnLadder)
		{
			IsTopLadderReposition = false;
		}
		if (_penitent.PlatformCharacterInput.Rewired.GetButtonDown(65) && !IsTakingOffLadder && !Core.Input.InputBlocked)
		{
			TakeOffLadder();
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if ((_layerMaskLadder.value & (1 << collision.gameObject.layer)) > 0)
		{
			_penitent.IsJumpingOff = false;
			LadderTopPoint = new Vector3(collision.bounds.center.x, collision.bounds.max.y, 0f);
			CurrentLadderCollider = collision;
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if ((_layerMaskLadder.value & (1 << collision.gameObject.layer)) <= 0 || !base.enabled)
		{
			return;
		}
		_penitent.IsOnLadder = true;
		_penitent.RootMotionDrive = _rootMotionDriver.transform.position;
		if (_penitent.RootMotionDrive.y >= LadderTopPoint.y)
		{
			if (collision.CompareTag("HasTop"))
			{
				SetClimbingSpeed(0f);
			}
			else
			{
				_penitent.ReachTopLadder = true;
			}
		}
		else
		{
			_penitent.ReachTopLadder = false;
		}
		if (_penitent.Status.IsGrounded && !_penitent.IsClimbingLadder && _penitent.PlatformCharacterInput.isJoystickUp && !IsBottomLadderRepositioning)
		{
			IsBottomLadderRepositioning = true;
		}
		_penitent.ReachBottonLadder = PlayerBottomPointCollider.y < LadderBottomPointCollider(collision).y;
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if ((_layerMaskLadder.value & (1 << collision.gameObject.layer)) > 0)
		{
			_penitent.IsOnLadder = false;
			_penitent.IsGrabbingLadder = false;
			_penitent.CanJumpFromLadder = true;
			if (_penitent.ReachBottonLadder)
			{
				_penitent.ReachBottonLadder = !_penitent.ReachBottonLadder;
			}
		}
	}

	public float DistanceToTopLadder(Vector3 playerPos)
	{
		Vector2 b = new Vector2(CurrentLadderCollider.bounds.center.x, playerPos.y);
		return Vector2.Distance(playerPos, b);
	}

	public void TopLadderReposition()
	{
		float x = CurrentLadderCollider.bounds.center.x;
		float y = _penitent.transform.position.y;
		Vector3 position = new Vector3(x, y, _penitent.transform.position.z);
		_penitent.transform.position = position;
	}

	public void GrabLadderPlayerReposition()
	{
		Vector3 ladderCenteredPosition = GetLadderCenteredPosition(CurrentLadderCollider);
		DOTween.To(() => _penitent.transform.position, delegate(Vector3Wrapper x)
		{
			_penitent.transform.position = x;
		}, ladderCenteredPosition, 0f);
	}

	public float GetClimbingSpeed()
	{
		float result = -1f;
		if (_penitent != null)
		{
			result = _penitent.PlatformCharacterController.ClimbingSpeed;
		}
		return result;
	}

	public void SetClimbingSpeed(float climbingSpeed)
	{
		if (_penitent != null)
		{
			_penitent.PlatformCharacterController.ClimbingSpeed = climbingSpeed;
		}
	}

	public void EnableClimbLadderAbility(bool enable = true)
	{
		if (enable && !_enableClimbLadderAbility)
		{
			_enableClimbLadderAbility = true;
			if (!_penitent.CanClimbLadder)
			{
				_penitent.CanClimbLadder = true;
			}
			_climbingLadderLayermask = (int)_climbingLadderLayermask | (1 << _ladderLayer);
			_playerController.ClimbingLayers = _climbingLadderLayermask;
		}
		else if (!enable && _enableClimbLadderAbility)
		{
			_enableClimbLadderAbility = false;
			if (_penitent.CanClimbLadder)
			{
				_penitent.CanClimbLadder = !_penitent.CanClimbLadder;
			}
			_climbingLadderLayermask = (int)_climbingLadderLayermask ^ (1 << _ladderLayer);
			_playerController.ClimbingLayers = _climbingLadderLayermask;
		}
	}

	private Vector3 GetLadderCenteredPosition(Collider2D ladderCollider)
	{
		Vector3 position = _penitent.transform.position;
		Vector3 result = new Vector3(ladderCollider.bounds.center.x, position.y);
		return result;
	}

	private static Vector2 LadderBottomPointCollider(Collider2D col)
	{
		float x = col.bounds.center.x;
		float y = col.bounds.min.y;
		return new Vector2(x, y);
	}

	private void OnStepLadder(UnityEngine.Object param)
	{
		CurrentLadderCollider = param as Collider2D;
	}

	private void PenitentOnDeath()
	{
		EnableClimbLadderAbility(enable: false);
	}

	private void PenitentOnDamaged()
	{
		IEnumerator routine = DisabledClimbAbilityLapse(_penitent.Animator.GetCurrentAnimatorStateInfo(0).length);
		StartCoroutine(routine);
	}

	private IEnumerator DisabledClimbAbilityLapse(float lapse)
	{
		EnableClimbLadderAbility(enable: false);
		yield return new WaitForSeconds(lapse);
		if (!_penitent.Status.Dead)
		{
			EnableClimbLadderAbility();
		}
	}

	private void TakeOffLadder()
	{
		if (_penitent.PlatformCharacterController.IsClimbing && !(TakeOffLadderCoolDown <= 0f))
		{
			_penitent.PlatformCharacterController.StopClimbing();
			StartCoroutine(DisabledClimbAbilityLapse(TakeOffLadderCoolDown));
		}
	}

	private void OnEnable()
	{
		if (_penitent != null)
		{
			EnableClimbLadderAbility();
		}
	}

	private void OnDisable()
	{
		if (_penitent != null)
		{
			EnableClimbLadderAbility(enable: false);
		}
	}

	private void OnDestroy()
	{
		FloorDistanceChecker.OnStepLadder = (Core.GenericEvent)Delegate.Remove(FloorDistanceChecker.OnStepLadder, new Core.GenericEvent(OnStepLadder));
		if (_penitent != null)
		{
			_penitent.OnDeath -= PenitentOnDeath;
		}
	}
}
