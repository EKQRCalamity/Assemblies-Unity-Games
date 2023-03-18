using System;
using System.Text.RegularExpressions;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.UI.Others.UIGameLogic;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Damage;

[RequireComponent(typeof(BoxCollider2D))]
public class PenitentDamageArea : DamageArea
{
	public delegate void PlayerDamagedEvent(Penitent damaged, Gameplay.GameControllers.Entities.Hit hit);

	public delegate void PlayerHitEvent(Penitent penitent, Gameplay.GameControllers.Entities.Hit hit);

	public static PlayerDamagedEvent OnDamagedGlobal;

	public static PlayerHitEvent OnHitGlobal;

	public PlayerDamagedEvent OnDamaged;

	private Penitent _penitent;

	private Vector2 _damageAreaOriginalOffset;

	private Vector2 _damageAreaOriginalSize;

	[Range(0f, 0.5f)]
	public float damageAreaIncrementalXSizeFactor = 0.2f;

	[Range(0f, 0.5f)]
	public float damageAreaIncrementalXOffsetFactor = 0.2f;

	private bool _enableEnemyAttack;

	private int _enemyAttackAreaLayerValue;

	private bool _damageAreaResized;

	private LogicManager _logicManager;

	[Tooltip("Invulnerability lapse after ground hurt animation ends.")]
	public float InvulnerabilityLapse = 0.5f;

	private BoxCollider2D _damageAreaCollider;

	private readonly int _airHurtAnimHash = UnityEngine.Animator.StringToHash("Hurt In The Air");

	private static readonly int Throw = UnityEngine.Animator.StringToHash("THROW");

	public bool IsFallingForwardResized { get; set; }

	public Vector2 DefaultSkinColliderCenter { get; private set; }

	public Vector2 DefaultSkinColliderSize { get; private set; }

	public bool PrayerProtectionEnabled { get; set; }

	public bool IsIncludeEnemyLayer { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		_damageAreaCollider = GetComponent<BoxCollider2D>();
		_damageAreaOriginalOffset = _damageAreaCollider.offset;
		_damageAreaOriginalSize = _damageAreaCollider.size;
		_penitent = GetComponentInParent<Penitent>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		SmartPlatformCollider smartPlatformCollider = _penitent.PlatformCharacterController.SmartPlatformCollider;
		DefaultSkinColliderCenter = new Vector2(smartPlatformCollider.Center.x, smartPlatformCollider.Center.y);
		DefaultSkinColliderSize = new Vector2(smartPlatformCollider.Size.x, smartPlatformCollider.Size.y);
		_enemyAttackAreaLayerValue = LayerMask.NameToLayer("Gate");
		_enableEnemyAttack = true;
		_logicManager = Core.Logic;
		MotionLerper motionLerper = _penitent.MotionLerper;
		motionLerper.OnLerpStop = (Core.SimpleEvent)Delegate.Combine(motionLerper.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
		MotionLerper motionLerper2 = _penitent.MotionLerper;
		motionLerper2.OnLerpStart = (Core.SimpleEvent)Delegate.Combine(motionLerper2.OnLerpStart, new Core.SimpleEvent(OnLerpStart));
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		DeltaRecoverTime += Time.deltaTime;
		ResizeDamageArea();
		if (_penitent.Status.Dead && _damageAreaCollider.enabled && !_penitent.Animator.GetBool(Throw))
		{
			_damageAreaCollider.enabled = false;
		}
		if (_penitent.MotionLerper.IsLerping && (!_penitent.FloorChecker.IsGrounded || _penitent.FloorChecker.IsSideBlocked))
		{
			_penitent.MotionLerper.StopLerping();
		}
	}

	private void ResizeDamageArea()
	{
		if (_penitent.AnimatorInyector.IsJumpingForward && IsFallingForwardResized && _penitent.PlatformCharacterController.GroundDist >= 1.5f)
		{
			SetTopSmallDamageArea();
		}
		else if ((_penitent.IsCrouched || _penitent.Dash.CrouchAfterDash || _penitent.IsDashing || _penitent.LungeAttack.Casting) && !_damageAreaResized)
		{
			SetBottomSmallDamageArea();
		}
		else if (!_penitent.IsCrouched && !_penitent.Dash.CrouchAfterDash && !_penitent.IsDashing && !_penitent.LungeAttack.Casting && _damageAreaResized)
		{
			SetDefaultDamageArea();
		}
	}

	private void SetBottomSmallDamageArea()
	{
		_damageAreaResized = true;
		_damageAreaCollider.size = new Vector2(_damageAreaCollider.size.x + _damageAreaCollider.size.x * damageAreaIncrementalXSizeFactor, _damageAreaCollider.size.y / 2f);
		_damageAreaCollider.offset = new Vector2(_damageAreaCollider.offset.x + _damageAreaCollider.offset.x * damageAreaIncrementalXOffsetFactor, _damageAreaCollider.size.y / 2f);
	}

	private void SetTopSmallDamageArea()
	{
		_damageAreaResized = true;
		Vector2 size = new Vector2(_damageAreaCollider.size.x, 0.6f);
		Vector2 offset = new Vector2(_damageAreaCollider.offset.x, 1.1f);
		_damageAreaCollider.size = size;
		_damageAreaCollider.offset = offset;
	}

	private void SetDefaultDamageArea()
	{
		_damageAreaResized = false;
		_damageAreaCollider.size = _damageAreaOriginalSize;
		_damageAreaCollider.offset = _damageAreaOriginalOffset;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && collision.contacts.Length > 0)
		{
			ContactPoint2D contactPoint2D = collision.contacts[0];
			if ((double)Vector3.Dot(contactPoint2D.normal, Vector3.up) > 0.5)
			{
				Debug.Log("collision was from below");
			}
		}
	}

	public override void TakeDamage(Gameplay.GameControllers.Entities.Hit hit, bool force = false)
	{
		if (Core.PenitenceManager.UseStocksOfHealth)
		{
			hit.DamageAmount = PlayerHealthPE02.StocksDamage;
		}
		base.TakeDamage(hit, force);
		RaiseHitEvent(hit);
		if ((!force && !CanTakeHit(hit)) || DeltaRecoverTime < RecoverTime)
		{
			return;
		}
		DeltaRecoverTime = 0f;
		base.LastHit = hit;
		CameraShake();
		TriggerLevelSleepTime(hit);
		switch (hit.DamageType)
		{
		case DamageType.Normal:
			_penitent.Rumble.UsePreset("NormalDamage");
			if (_penitent.AnimatorInyector.IsAirAttacking || _penitent.AnimatorInyector.IsJumpingForward)
			{
				_penitent.PenitentMoveAnimations.GetPushBackSparks();
				if (_penitent.Status.Dead)
				{
					_penitent.Animator.Play(_airHurtAnimHash, 0, 0f);
				}
			}
			else if (!_penitent.Status.Dead)
			{
				SetDamageAnimation(hit.DamageType, hit.AttackingEntity.transform.position);
			}
			break;
		case DamageType.Heavy:
			_penitent.Rumble.UsePreset("HeavyDamage");
			SetDamageAnimation(hit.DamageType, hit.AttackingEntity.transform.position);
			break;
		case DamageType.Simple:
			_penitent.Rumble.UsePreset("NormalDamage");
			SetDamageAnimation(hit.DamageType, hit.AttackingEntity.transform.position);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case DamageType.Critical:
			break;
		}
		RaiseDamageEvent(hit);
	}

	private bool CanTakeHit(Gameplay.GameControllers.Entities.Hit hit)
	{
		if (PrayerProtectionEnabled || _penitent.Status.Invulnerable)
		{
			return false;
		}
		if (_penitent.Status.Unattacable)
		{
			return hit.Unnavoidable;
		}
		return !_penitent.Status.IsHurt;
	}

	public void EnableEnemyAttack(bool enable = true)
	{
		if (enable && !_enableEnemyAttack)
		{
			_enableEnemyAttack = true;
			enemyAttackAreaLayer = (int)enemyAttackAreaLayer | (1 << _enemyAttackAreaLayerValue);
		}
		else if (!enable && _enableEnemyAttack)
		{
			_enableEnemyAttack = false;
			enemyAttackAreaLayer = (int)enemyAttackAreaLayer ^ (1 << _enemyAttackAreaLayerValue);
		}
	}

	public void IncludeEnemyLayer(bool include = true)
	{
	}

	private void RaiseDamageEvent(Gameplay.GameControllers.Entities.Hit hit)
	{
		float num = hit.DamageAmount;
		if (!Core.PenitenceManager.UseStocksOfHealth)
		{
			num = _penitent.GetReducedDamage(hit);
		}
		if (Core.LevelManager.currentLevel.LevelName.Equals("D24Z01S01") && Core.Logic.Penitent.CurrentLife - num <= 0f)
		{
			num = Core.Logic.Penitent.CurrentLife - 1f;
			Core.Logic.ScreenFreeze.Freeze(0.05f, 0.5f);
			Core.Logic.Penitent.Physics.Enable2DCollision(enable: false);
		}
		CheckPenitentKilledEvent(num, hit.AttackingEntity);
		int num2 = Mathf.CeilToInt(num);
		_penitent.Damage(num2, hit.HitSoundId);
		_logicManager.PlayerCurrentLife = _penitent.Stats.Life.Current;
		if (OnDamaged != null)
		{
			OnDamaged(_penitent, hit);
		}
		if (OnDamagedGlobal != null)
		{
			OnDamagedGlobal(_penitent, hit);
		}
	}

	private void RaiseHitEvent(Gameplay.GameControllers.Entities.Hit hit)
	{
		if (OnHitGlobal != null)
		{
			OnHitGlobal(_penitent, hit);
		}
	}

	private void CheckPenitentKilledEvent(float damage, GameObject attacker)
	{
		if ((bool)attacker && !_penitent.Status.Dead)
		{
			string text = Regex.Replace(attacker.name, " \\([1-9]\\)", string.Empty);
			text = text.Replace("(Clone)", string.Empty);
			if (Core.Logic.Penitent.CurrentLife - damage <= 0f)
			{
				Core.Metrics.CustomEvent("PLAYER_DEATH", text);
			}
			Core.Metrics.HeatmapEvent("PLAYER_DEATH", _penitent.transform.position);
		}
	}

	private void TriggerLevelSleepTime(Gameplay.GameControllers.Entities.Hit hit)
	{
		_penitent.PenitentAttackAnimations.LevelSleepTime(0.1f);
	}

	private void CameraShake()
	{
		if ((bool)_penitent.CameraManager.ProCamera2DShake)
		{
			_penitent.CameraManager.ProCamera2DShake.ShakeUsingPreset("SimpleHit");
		}
	}

	public void SetDamageAnimation(DamageType damageType, Vector3 enemyPosition)
	{
		if (!_penitent.AnimatorInyector.IsHoldingChargeAttack || damageType == DamageType.Heavy)
		{
			if (damageType != DamageType.Simple)
			{
				_penitent.HurtOrientation = _penitent.SetOrientationbyHit(enemyPosition);
			}
			_penitent.AnimatorInyector.PlayerGetDamage(damageType);
		}
	}

	public void HitDisplacement(Vector3 enemyPos)
	{
		if (_penitent.Status.IsGrounded && !base.LastHit.forceGuardslide && !_penitent.Status.Dead && !_penitent.HasFlag("SIDE_BLOCKED"))
		{
			HitDisplacementForce(enemyPos);
		}
	}

	private void OnLerpStop()
	{
		IncludeEnemyLayer();
	}

	private void OnLerpStart()
	{
		IncludeEnemyLayer(include: false);
	}

	private void HitDisplacementForce(Vector3 enemyPos)
	{
		bool flag = Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE);
		Vector3 dir = ((!(enemyPos.x > base.transform.position.x)) ? base.transform.right : (-base.transform.right));
		if (!_penitent.MotionLerper.IsLerping && !(_penitent.SlopeAngle > 1f))
		{
			_penitent.MotionLerper.TimeTakenDuringLerp = ((!flag) ? 0.85f : 0.15f);
			_penitent.MotionLerper.distanceToMove = ((!flag) ? 2.75f : 1f);
			_penitent.MotionLerper.StartLerping(dir);
		}
	}
}
