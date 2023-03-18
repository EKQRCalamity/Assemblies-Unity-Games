using System;
using System.Collections;
using CreativeSpore.SmartColliders;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent.InputSystem;
using Gameplay.UI;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class WallJump : Trait
{
	private readonly int _jumpForwardAnim = UnityEngine.Animator.StringToHash("Jump Forward");

	private readonly int _wallClimbContactAnim = UnityEngine.Animator.StringToHash("WallClimbContact");

	public PlatformCharacterController CharacterController;

	public PlatformCharacterInput CharacterInput;

	public Ability[] ToogledAbilities;

	private bool _jumpOffWall;

	private GrabCliffLede DisabledAbilityWhenUse;

	private bool _stickToWall;

	private RaycastHit2D _wallHit;

	[BoxGroup("Stick Params", true, false, 0)]
	[SerializeField]
	public LayerMask WallLayerMask;

	[BoxGroup("Stick Params", true, false, 0)]
	[SerializeField]
	public float HookHeightFromPivotPoint = 1f;

	[BoxGroup("Stick Params", true, false, 0)]
	[SerializeField]
	public float Distance = 1f;

	[BoxGroup("Stick Params", true, false, 0)]
	[SerializeField]
	public float StickCoolDown = 1f;

	[BoxGroup("Stick Params", true, false, 0)]
	[SerializeField]
	public float JumpOffCoolDown = 1f;

	[BoxGroup("Stick Params", true, false, 0)]
	[SerializeField]
	public float GravityDragDistance = 0.25f;

	[BoxGroup("Stick Params", true, false, 0)]
	[SerializeField]
	public float GravityDragLapse = 1f;

	[BoxGroup("Jump Params", true, false, 0)]
	[SerializeField]
	public float StickDistanceToWall = 2f;

	[BoxGroup("Jump Params", true, false, 0)]
	[SerializeField]
	public float WallJumpAcc = 2f;

	[BoxGroup("Jump Params", true, false, 0)]
	[SerializeField]
	public float WallJumpSpeed = 2f;

	[BoxGroup("Jump Params", true, false, 0)]
	[SerializeField]
	public float RestoreWallJumpAfterDamageLapse = 0.4f;

	[BoxGroup("Debug", true, false, 0)]
	[SerializeField]
	public bool debugOn;

	private float _wallJumpTimer = -1f;

	private float _stickCoolDownTimer = -1f;

	private float _jumpOffCoolDownTimer = -1f;

	private float _defaultRayCastDistance;

	private EntityOrientation playerStickedOrientation;

	private bool _unHang;

	private bool _isJumpOffStacked;

	public Player Rewired { get; private set; }

	private bool EndStickCoolDown => _stickCoolDownTimer < 0f;

	private bool EndJumpOffCoolDown => _jumpOffCoolDownTimer < 0f;

	private bool CanCancelHook
	{
		get
		{
			bool flag = base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("WallClimbIdle");
			bool flag2 = base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("WallClimbContact");
			float num = 0f;
			if (flag2)
			{
				num = base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
			}
			return flag || (flag2 && num > 0.1f);
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		Rewired = ReInput.players.GetPlayer(0);
		DisabledAbilityWhenUse = base.EntityOwner.GetComponentInChildren<GrabCliffLede>();
		base.EntityOwner.OnDamaged += EntityOwnerOnDamaged;
		CameraShakeManager.OnCameraShakeOverthrow = (Core.SimpleEvent)Delegate.Combine(CameraShakeManager.OnCameraShakeOverthrow, new Core.SimpleEvent(OnCameraShakeOverthrow));
		_defaultRayCastDistance = Distance;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.EntityOwner.Status.IsGrounded)
		{
			ResetWallJumpStatus();
			if (Distance <= 0f)
			{
				Distance = _defaultRayCastDistance;
			}
		}
		Vector3 vector = new Vector3(base.transform.position.x, base.transform.position.y + HookHeightFromPivotPoint, base.transform.position.z);
		float num = ((base.EntityOwner.Status.Orientation != 0) ? (-1f) : 1f);
		_wallHit = Physics2D.Raycast(vector, Vector2.right * num, Distance, WallLayerMask);
		if (Rewired.GetButton(5) && !CharacterController.IsGrounded && _wallHit.collider != null && !_stickToWall && EndStickCoolDown)
		{
			Core.Logic.Penitent.Audio.SetParametersValuesByWall(_wallHit.collider);
			_stickToWall = true;
			playerStickedOrientation = Core.Logic.Penitent.Status.Orientation;
			base.EntityOwner.Animator.ResetTrigger("AIR_ATTACK");
			base.EntityOwner.Animator.Play(_wallClimbContactAnim);
			base.EntityOwner.transform.position = GetClimbPosition(_wallHit.collider);
			Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
			base.EntityOwner.transform.DOMoveY(base.EntityOwner.transform.position.y - GravityDragDistance, GravityDragLapse).SetEase(Ease.OutSine).OnUpdate(CheckWallCollider);
		}
		if (_stickToWall)
		{
			Stick();
		}
		if (Rewired.GetButtonDown(6) && _stickToWall && EndJumpOffCoolDown && base.EntityOwner.Animator.GetBool("STICK_ON_WALL") && !UIController.instance.IsShowingMenu)
		{
			DOTween.Kill(base.EntityOwner.transform);
			ToogleAbilities(t: true);
			Detach();
		}
		if (_jumpOffWall)
		{
			JumpOff();
		}
		else
		{
			_wallJumpTimer = CharacterController.JumpingAccTime;
		}
	}

	private void OnDestroy()
	{
		base.EntityOwner.OnDamaged -= EntityOwnerOnDamaged;
		CameraShakeManager.OnCameraShakeOverthrow = (Core.SimpleEvent)Delegate.Remove(CameraShakeManager.OnCameraShakeOverthrow, new Core.SimpleEvent(OnCameraShakeOverthrow));
	}

	private void OnDrawGizmos()
	{
		if (debugOn)
		{
			UnityEngine.Gizmos.color = Color.red;
			Vector3 vector = new Vector3(base.transform.position.x, base.transform.position.y + HookHeightFromPivotPoint, base.transform.position.z);
			float num = ((base.EntityOwner.Status.Orientation != 0) ? (-1f) : 1f);
			UnityEngine.Gizmos.DrawLine(vector, vector + Vector3.right * num * Distance);
		}
	}

	private void EntityOwnerOnDamaged()
	{
		UnhangByEvent();
		StartCoroutine(RestoreRayCastAfterDamageDelay());
	}

	private IEnumerator RestoreRayCastAfterDamageDelay()
	{
		if (!(Distance > 0f))
		{
			yield return new WaitForSeconds(RestoreWallJumpAfterDamageLapse);
			Distance = _defaultRayCastDistance;
		}
	}

	private void OnCameraShakeOverthrow()
	{
		if (_stickToWall)
		{
			UnhangByEvent();
			StartCoroutine(ShakeOverthrowPenalty());
		}
	}

	private IEnumerator ShakeOverthrowPenalty()
	{
		float penaltyTime = 0.35f;
		while (penaltyTime > 0f)
		{
			penaltyTime -= Time.deltaTime;
			_stickToWall = false;
			_stickCoolDownTimer = StickCoolDown;
			base.EntityOwner.Animator.SetBool("STICK_ON_WALL", _stickToWall);
			CharacterController.PlatformCharacterPhysics.Gravity = new Vector3(0f, -9.8f, 0f);
			yield return null;
		}
		_stickCoolDownTimer = -1f;
		if (Distance <= 0f)
		{
			Distance = _defaultRayCastDistance;
		}
	}

	private void UnhangByEvent()
	{
		ResetWallJumpStatus();
		_stickToWall = false;
		Distance = 0f;
		_stickCoolDownTimer = -1f;
		base.EntityOwner.Animator.SetBool("STICK_ON_WALL", value: false);
		base.EntityOwner.Animator.ResetTrigger("WALLCLIMB_UNHANG");
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
	}

	private void CheckCancelHook()
	{
		if (!UIController.instance.IsShowingMenu && Rewired.GetButton(65) && !_unHang)
		{
			_unHang = true;
			base.EntityOwner.Animator.SetTrigger("WALLCLIMB_UNHANG");
			StartCoroutine(UnHang());
		}
	}

	private void ResetWallJumpStatus()
	{
		_jumpOffWall = false;
		_unHang = false;
		_isJumpOffStacked = false;
		_stickToWall = false;
		_stickCoolDownTimer = -1f;
		CharacterController.PlatformCharacterPhysics.Gravity = new Vector3(0f, -9.8f, 0f);
	}

	private void Stick()
	{
		_jumpOffWall = false;
		ToogleAbilities(t: false);
		_jumpOffCoolDownTimer -= Time.deltaTime;
		_stickCoolDownTimer = StickCoolDown;
		_isJumpOffStacked = false;
		CharacterController.PlatformCharacterPhysics.Velocity = Vector3.zero;
		CharacterController.PlatformCharacterPhysics.VSpeed = 0f;
		CharacterController.PlatformCharacterPhysics.Gravity = Vector3.zero;
		CharacterController.PlatformCharacterPhysics.Acceleration = Vector3.zero;
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		Core.Logic.Penitent.SetOrientation(playerStickedOrientation);
		if (CanCancelHook)
		{
			CheckCancelHook();
		}
	}

	private void Detach()
	{
		_stickToWall = false;
		_jumpOffWall = true;
		CharacterController.PlatformCharacterPhysics.Gravity = new Vector3(0f, -9.8f, 0f);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		CharacterController.PlatformCharacterPhysics.Velocity = new Vector2(WallJumpSpeed * CharacterInput.FHorAxis, WallJumpSpeed);
		base.EntityOwner.Animator.SetBool("STICK_ON_WALL", value: false);
		if (DisabledAbilityWhenUse != null)
		{
			StartCoroutine(DisableAbility());
		}
	}

	private void JumpOff()
	{
		_jumpOffCoolDownTimer = JumpOffCoolDown;
		_stickCoolDownTimer -= Time.deltaTime;
		if (!_isJumpOffStacked)
		{
			_isJumpOffStacked = true;
			float x = ((playerStickedOrientation != 0) ? 1f : (-1f));
			CharacterInput.Move(x, 0.1f);
			base.EntityOwner.Animator.Play(_jumpForwardAnim);
			base.EntityOwner.SetOrientation(GetReverseOrientation(base.EntityOwner.Status.Orientation));
		}
		if (_wallJumpTimer > 0f)
		{
			_wallJumpTimer -= Time.deltaTime;
			CharacterController.PlatformCharacterPhysics.Acceleration += base.transform.up * WallJumpAcc;
		}
	}

	private EntityOrientation GetReverseOrientation(EntityOrientation currentOrientation)
	{
		return (currentOrientation != EntityOrientation.Left) ? EntityOrientation.Left : EntityOrientation.Right;
	}

	private IEnumerator UnHang()
	{
		yield return new WaitForSeconds(0.35f);
		_stickToWall = false;
		ToogleAbilities(t: true);
		if (!_jumpOffWall)
		{
			_jumpOffWall = true;
		}
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		CharacterController.PlatformCharacterPhysics.Gravity = new Vector3(0f, -9.8f, 0f);
		base.EntityOwner.Animator.SetBool("STICK_ON_WALL", value: false);
		if (_unHang)
		{
			_unHang = !_unHang;
		}
	}

	private Vector3 GetClimbPosition(Collider2D climbCollider)
	{
		float x = ((base.EntityOwner.Status.Orientation != 0) ? (climbCollider.bounds.max.x + StickDistanceToWall) : (climbCollider.bounds.min.x - StickDistanceToWall));
		Vector2 vector = new Vector2(x, base.EntityOwner.transform.position.y);
		return vector;
	}

	private IEnumerator DisableAbility()
	{
		DisabledAbilityWhenUse.enabled = false;
		yield return new WaitForSeconds(0.1f);
		DisabledAbilityWhenUse.enabled = true;
	}

	private void CheckWallCollider()
	{
		if (_wallHit.collider == null)
		{
			DOTween.Kill(base.EntityOwner.transform);
		}
	}

	public void ToogleAbilities(bool t)
	{
		for (int i = 0; i < ToogledAbilities.Length; i++)
		{
			ToogledAbilities[i].enabled = t;
		}
	}
}
