using System;
using System.Collections;
using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Penitent.Movement;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class Dash : Ability
{
	[Serializable]
	public struct MoveSetting
	{
		public float Drag;

		public float Speed;

		public MoveSetting(float drag, float speed)
		{
			Drag = drag;
			Speed = speed;
		}
	}

	public Core.SimpleEvent OnStartDash;

	public Core.SimpleEvent OnFinishDash;

	public GameObject cooldownFinishedFX;

	public Vector2 fxOffset;

	public const float MAX_WALK_SPEED = 5f;

	private Penitent _penitent;

	public float DashDrag;

	public float DashMaxWalkingSpeed;

	[SerializeField]
	protected GhostTrailGenerator GhostTrailGenerator;

	[Tooltip("Mandatoy completion of the dash animation before can dash.")]
	[Range(0f, 1f)]
	public float CompletionBeforeDash;

	private float _deltaTimeDashing;

	private bool _isDashDirectionSet;

	private float _dashDirection;

	public Vector3 DashCollisionCenter;

	public Vector2 DashCollisionSize;

	[FoldoutGroup("Damage Area Dash Boundaries", true, 0)]
	public float DamageAreaDashHeight;

	[FoldoutGroup("Damage Area Dash Boundaries", true, 0)]
	public float DamageAreaDashYOffset;

	private Vector2 _defaultDamageColliderSize;

	private Vector2 _defaultDamageColliderOffset;

	private Vector2 _dashDamageAreaSize;

	private Vector2 _dashDamageAreaOffset;

	private Vector2 _damageCollider;

	private Vector2 _defaultDamageColliderHeight;

	private Penitent.CollisionSkin _defaultCollisionSkin;

	private Penitent.CollisionSkin _dashCollisionSkin;

	private SmartPlatformCollider _penitentCollider;

	public MoveSetting DefaultMoveSetting;

	public MoveSetting DashMoveSetting;

	private PlatformCharacterController _playerController;

	public LayerMask UpperBlockLayer;

	public bool StandUpAfterDash { get; set; }

	public bool CrouchAfterDash { get; set; }

	public bool StopByDamage { get; private set; }

	public bool IsUpperBlocked
	{
		get
		{
			float num = Mathf.Abs(_penitent.PlatformCharacterController.SlopeAngle);
			if (num > 1f)
			{
				return false;
			}
			Vector3 position = base.EntityOwner.transform.position;
			position.y += 0.1f;
			Vector2 origin = position;
			Vector2 origin2 = position;
			origin.x = position.x - 0.25f;
			origin2.x = position.x + 0.25f;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, base.EntityOwner.transform.up, 2.5f, UpperBlockLayer);
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(origin2, base.EntityOwner.transform.up, 2.5f, UpperBlockLayer);
			return (bool)raycastHit2D || (bool)raycastHit2D2;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_playerController = base.EntityOwner.GetComponent<PlatformCharacterController>();
		DefaultMoveSetting = new MoveSetting(_playerController.WalkingDrag, _playerController.MaxWalkingSpeed);
		DashMoveSetting = new MoveSetting(DashDrag, DashMaxWalkingSpeed);
		_deltaTimeDashing = 0f;
		if (cooldownFinishedFX != null)
		{
			PoolManager.Instance.CreatePool(cooldownFinishedFX, 1);
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (!_penitent)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitentCollider = _playerController.SmartPlatformCollider;
		Vector3 center = _penitentCollider.Center;
		Vector2 size = _penitentCollider.Size;
		_defaultCollisionSkin = new Penitent.CollisionSkin(center, size);
		_dashCollisionSkin = new Penitent.CollisionSkin(DashCollisionCenter, DashCollisionSize);
		BoxCollider2D boxCollider2D = (BoxCollider2D)_penitent.DamageArea.DamageAreaCollider;
		_defaultDamageColliderSize = boxCollider2D.size;
		_defaultDamageColliderOffset = boxCollider2D.offset;
		_dashDamageAreaSize = new Vector2(_defaultDamageColliderSize.x, DamageAreaDashHeight);
		_dashDamageAreaOffset = new Vector2(_defaultDamageColliderOffset.x, DamageAreaDashYOffset);
		base.EntityOwner.OnDamaged += OnDamaged;
		CharacterMotionProfile.OnMotionProfileLoaded += OnMotionProfileLoaded;
	}

	private void OnMotionProfileLoaded()
	{
		DefaultMoveSetting = new MoveSetting(_playerController.WalkingDrag, _playerController.MaxWalkingSpeed);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!_penitent.Status.IsGrounded)
		{
			CrouchAfterDash = false;
			StopCast();
		}
		if (base.Casting)
		{
			_deltaTimeDashing += Time.deltaTime;
			AddDashForce();
			if (_deltaTimeDashing >= base.EntityOwner.Stats.DashRide.Final || _penitent.HasFlag("FRONT_BLOCKED"))
			{
				StopCast();
			}
		}
		else
		{
			_deltaTimeDashing = 0f;
		}
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		if (OnStartDash != null)
		{
			OnStartDash();
		}
		SetCooldown(base.EntityOwner.Stats.DashCooldown.Final);
		if (!_penitent.IsDashing)
		{
			_penitent.IsDashing = true;
		}
		_penitent.DamageArea.IncludeEnemyLayer(include: false);
		_penitent.DamageArea.EnableEnemyAttack(enable: false);
		SetDashSkinCollision();
		if (!GhostTrailGenerator.EnableGhostTrail)
		{
			GhostTrailGenerator.EnableGhostTrail = true;
		}
		_penitent.DashDustGenerator.GetStartDashDust();
	}

	protected override void OnCastEnd(float castingTime)
	{
		base.OnCastEnd(castingTime);
		if (_penitent.IsDashing)
		{
			_penitent.IsDashing = false;
		}
		_playerController.WalkingDrag = DefaultMoveSetting.Drag;
		_playerController.MaxWalkingSpeed = DefaultMoveSetting.Speed;
		_isDashDirectionSet = false;
		if (!StopByDamage)
		{
			SetDefaultSkinCollision();
		}
		else
		{
			StartCoroutine(DelaySetDefaultCollision());
		}
		_penitent.DamageArea.IncludeEnemyLayer();
		_penitent.DamageArea.EnableEnemyAttack();
		if (_penitent.Status.Unattacable)
		{
			_penitent.Status.Unattacable = false;
		}
		if (GhostTrailGenerator.EnableGhostTrail)
		{
			GhostTrailGenerator.EnableGhostTrail = false;
		}
		if (OnFinishDash != null)
		{
			OnFinishDash();
		}
	}

	protected override void OnCooldownFinished()
	{
		base.OnCooldownFinished();
		if (cooldownFinishedFX != null)
		{
			PoolManager.Instance.ReuseObject(cooldownFinishedFX, (Vector2)base.transform.position + fxOffset, Quaternion.identity);
		}
	}

	private IEnumerator DelaySetDefaultCollision()
	{
		yield return new WaitForSeconds(0.85f);
		StopByDamage = false;
		SetDefaultSkinCollision();
	}

	public void AddDashForce()
	{
		if (_penitent.Status.IsGrounded && !_penitent.HasFlag("FRONT_BLOCKED"))
		{
			_playerController.WalkingDrag = DashMoveSetting.Drag;
			_playerController.MaxWalkingSpeed = DashMoveSetting.Speed;
			if (!_isDashDirectionSet)
			{
				_isDashDirectionSet = true;
				_dashDirection = _penitent.PlatformCharacterInput.Rewired.GetAxisRaw(0);
			}
			if (_dashDirection < 0f)
			{
				_playerController.SetActionState(eControllerActions.Left, value: true);
				_penitent.SetOrientation(EntityOrientation.Left);
			}
			else if (_dashDirection > 0f)
			{
				_playerController.SetActionState(eControllerActions.Right, value: true);
				_penitent.SetOrientation(EntityOrientation.Right);
			}
			else
			{
				_playerController.SetActionState(eControllerActions.Right, _penitent.Status.Orientation == EntityOrientation.Right);
				_playerController.SetActionState(eControllerActions.Left, _penitent.Status.Orientation == EntityOrientation.Left);
			}
		}
	}

	private void OnDamaged()
	{
		if (_penitent.IsDashing)
		{
			StopByDamage = true;
			StopCast();
		}
	}

	public void SetDashSkinCollision()
	{
		_penitentCollider.Center = _dashCollisionSkin.CenterCollision;
		_penitentCollider.Size = _dashCollisionSkin.CollisionSize;
	}

	public void SetDefaultSkinCollision()
	{
		_penitentCollider.Center = _defaultCollisionSkin.CenterCollision;
		_penitentCollider.Size = _defaultCollisionSkin.CollisionSize;
	}

	private void OnDestroy()
	{
		base.EntityOwner.OnDamaged -= OnDamaged;
		CharacterMotionProfile.OnMotionProfileLoaded -= OnMotionProfileLoaded;
	}
}
