using System;
using System.Collections.Generic;
using CreativeSpore.SmartColliders;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Gameplay.GameControllers.Penitent.Abilities;
using Gameplay.GameControllers.Penitent.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Attack;

public class PenitentAttack : Gameplay.GameControllers.Entities.Attack
{
	private readonly int _combo1HashAnim = UnityEngine.Animator.StringToHash("Combo_1");

	private readonly int _combo2HashAnim = UnityEngine.Animator.StringToHash("Combo_2");

	private readonly int _combo3HashAnim = UnityEngine.Animator.StringToHash("Combo_3");

	public Combo Combo;

	[SerializeField]
	[BoxGroup("Prayer Effect", true, false, 0)]
	[Range(0f, 100f)]
	[Tooltip("Life amount percentage drained to an enemy by usage of Distressing Saeta")]
	public float LifeDrainedByPrayerUse;

	[SerializeField]
	[BoxGroup("Audio Attack", true, false, 0)]
	[EventRef]
	public string SimpleEnemyHit;

	[SerializeField]
	[BoxGroup("Audio Attack", true, false, 0)]
	[EventRef]
	public string HeavyEnemyHit;

	[SerializeField]
	[BoxGroup("Audio Attack", true, false, 0)]
	[EventRef]
	public string CriticalEnemyHit;

	private List<GameObject> _hitEntities = new List<GameObject>();

	private bool _newEnemyHit;

	[BoxGroup("Combo Chance by Tier", true, false, 0)]
	public float FirstComboTierExecutionBonus = 3f;

	[BoxGroup("Combo Chance by Tier", true, false, 0)]
	public float SecondComboTierExecutionBonus = 6f;

	[BoxGroup("Air attack", true, false, 0)]
	public float airAttackImpulse = 6f;

	private int _currentImpulses;

	private Penitent _penitent;

	public int comboCharge;

	public const int MAX_COMBO_CHARGE = 2;

	private Vector2 _defaultWeaponColliderSize;

	private Vector2 _defaultWeaponColliderOffset;

	private float _animationCompletionThreshold = 0.8f;

	private float _attackTimeThreshold = 0.15f;

	private float _deltaAttackTimeThreshold;

	private AttackArea _playerAttackArea;

	private float _playerAttackAreaOriginalHeight;

	private float _playerAttackAreaCrouchHeight;

	public float HeavyAttackMultiplier;

	[Range(1f, 10f)]
	public float ParryMultiplier = 2f;

	public PenitentSword.AttackColor AttackColor;

	private int _currentLevel;

	private float _attackSpeed;

	private PlatformCharacterInput _playerInput;

	private bool _isComboChained;

	public bool WindowAttackOpen { get; set; }

	private bool HitImpulseTriggered { get; set; }

	public bool IsHeavyAttackPrayerEquipped { get; set; }

	public bool IsRunningCombo { get; private set; }

	public bool IsRunningUpgradedCombo { get; private set; }

	private bool IsDemakeMode => Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE);

	public int CurrentLevel
	{
		get
		{
			return _currentLevel;
		}
		set
		{
			_currentLevel = Mathf.Clamp(value, 1, 2);
		}
	}

	public float AttackSpeed
	{
		get
		{
			return _attackSpeed;
		}
		set
		{
			_attackSpeed = Mathf.Clamp(value, 1f, 1.5f);
			PenitentSword penitentSword = (PenitentSword)CurrentPenitentWeapon;
			penitentSword.SlashAnimator.SetAnimatorSpeed(_attackSpeed);
		}
	}

	public PlatformCharacterController PenitentController { get; set; }

	public Weapon CurrentPenitentWeapon { get; set; }

	private bool IsFinalComboAvailable
	{
		get
		{
			if (!Combo.IsAvailable)
			{
				return false;
			}
			UnlockableSkill getMaxSkill = Combo.GetMaxSkill;
			return getMaxSkill.id.Equals("COMBO_3");
		}
	}

	public bool PlayerHasSwordHeart => Core.InventoryManager.IsSwordEquipped("HE10");

	public event Action<PenitentAttack> OnAttackTriggered;

	protected override void OnAwake()
	{
		if (_penitent != null)
		{
			motionLerper = _penitent.MotionLerper;
		}
		PenitentController = base.EntityOwner.GetComponent<PlatformCharacterController>();
		_playerInput = base.EntityOwner.GetComponent<PlatformCharacterInput>();
		isAttacking = false;
		CurrentPenitentWeapon = GetComponent<Weapon>();
		CurrentLevel = 1;
	}

	protected override void OnStart()
	{
		if (motionLerper != null)
		{
			motionLerper.speedCurve = speedCurve;
		}
		_penitent = (Penitent)base.EntityOwner;
		_penitent.OnDeath += PenitentOnEntityDie;
		_playerAttackArea = base.EntityOwner.GetComponentInChildren<AttackArea>();
		_playerAttackAreaOriginalHeight = _playerAttackArea.transform.localPosition.y;
		_playerAttackAreaCrouchHeight = _playerAttackAreaOriginalHeight - Mathf.Abs(_playerAttackAreaOriginalHeight);
		SetAttackAreaSize();
		SetAttackAreaOffset();
		_playerAttackArea.OnStay += OnStayAttackArea;
	}

	private void SetAttackAreaSize()
	{
		Bounds bounds = _penitent.AttackArea.WeaponCollider.bounds;
		float x = ((!IsDemakeMode) ? bounds.size.x : 2f);
		float y = ((!IsDemakeMode) ? bounds.size.y : 2f);
		_defaultWeaponColliderSize = new Vector2(x, y);
	}

	private void SetAttackAreaOffset()
	{
		Vector2 offset = _penitent.AttackArea.WeaponCollider.offset;
		_defaultWeaponColliderOffset = new Vector2(offset.x, offset.y);
	}

	protected override void OnUpdate()
	{
		if (IsAttackTriggered() && this.OnAttackTriggered != null)
		{
			this.OnAttackTriggered(this);
		}
		if (_penitent.PlatformCharacterController.IsGrounded)
		{
			_currentImpulses = 0;
		}
		AttackTrigger();
		CheckHitImpulseFired();
		bool flag = base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("GroundUpwardAttack");
		bool flag2 = base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("AirUpwardAttack");
		if (!IsDemakeMode)
		{
			_playerInput.IsAttacking = IsRunningBasicCombo() || flag;
		}
		_playerAttackArea.SetLocalHeight((!_penitent.IsCrouched) ? _playerAttackAreaOriginalHeight : _playerAttackAreaCrouchHeight);
		if (_penitent.Status.IsGrounded && !_penitent.ReleaseChargedAttack && !flag && !flag2 && !_penitent.LungeAttack.Casting && !IsRunningUpgradedCombo && !isHeavyAttacking)
		{
			_penitent.AttackArea.SetSize(_defaultWeaponColliderSize);
			_penitent.AttackArea.SetOffset(_defaultWeaponColliderOffset);
		}
		AttackSpeed = ((!(_penitent.Stats.AttackSpeed.Final > 1f)) ? 1f : _penitent.Stats.AttackSpeed.Final);
	}

	public void ChargeCombo()
	{
		comboCharge++;
	}

	public void ResetCombo()
	{
		if (comboCharge > 0)
		{
			comboCharge = 0;
		}
	}

	private bool IsComboCharged()
	{
		return comboCharge >= 2;
	}

	protected bool IsRunningBasicCombo()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
		{
			isAttacking = true;
			_isComboChained = animator.GetCurrentAnimatorStateInfo(0).normalizedTime > _animationCompletionThreshold && IsAttackTriggered() && PenitentController.IsGrounded && _playerAttackArea.IsTargetHit;
			if (_isComboChained)
			{
				animator.Play(_combo1HashAnim);
			}
		}
		else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack_Running"))
		{
			isAttacking = true;
			_isComboChained = animator.GetCurrentAnimatorStateInfo(0).normalizedTime > _animationCompletionThreshold && IsAttackTriggered() && PenitentController.IsGrounded && _playerAttackArea.IsTargetHit;
			if (_isComboChained)
			{
				animator.Play(_combo1HashAnim);
			}
		}
		else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Combo_1"))
		{
			isAttacking = true;
			_isComboChained = animator.GetCurrentAnimatorStateInfo(0).normalizedTime > _animationCompletionThreshold && IsAttackTriggered() && PenitentController.IsGrounded;
			if (_isComboChained && IsComboCharged() && _playerAttackArea.IsTargetHit)
			{
				if (!IsDemakeMode)
				{
					animator.Play(_combo3HashAnim);
				}
				else
				{
					FinishCombo();
				}
			}
			else if ((_isComboChained && !IsComboCharged()) || (_isComboChained && IsComboCharged()))
			{
				animator.Play(_combo2HashAnim);
			}
		}
		else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Combo_2"))
		{
			isAttacking = true;
			_isComboChained = animator.GetCurrentAnimatorStateInfo(0).normalizedTime > _animationCompletionThreshold && IsAttackTriggered() && PenitentController.IsGrounded;
			if (_isComboChained)
			{
				animator.Play(_combo1HashAnim);
			}
		}
		else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Combo_3"))
		{
			isHeavyAttacking = true;
			isAttacking = true;
			IsRunningCombo = true;
			ResetCombo();
			if (Combo.IsAvailable)
			{
				_isComboChained = animator.GetCurrentAnimatorStateInfo(0).normalizedTime > _animationCompletionThreshold && IsAttackTriggered() && PenitentController.IsGrounded;
			}
		}
		else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Combo_4") || animator.GetCurrentAnimatorStateInfo(0).IsName("ComboFinisherDown"))
		{
			IsRunningUpgradedCombo = true;
			isHeavyAttacking = true;
			isAttacking = true;
		}
		else if (animator.GetCurrentAnimatorStateInfo(0).IsName("ComboFinisherUp"))
		{
			IsRunningUpgradedCombo = true;
			isHeavyAttacking = true;
		}
		else
		{
			FinishCombo();
		}
		animator.SetBool("ATTACKING", isAttacking || isRangeAttacking);
		return isAttacking;
	}

	private void FinishCombo()
	{
		comboCharge = 0;
		isAttacking = false;
		isHeavyAttacking = false;
		IsRunningCombo = false;
		IsRunningUpgradedCombo = false;
		_penitent.EntityAttack.IsWeaponBlowingUp = false;
	}

	public void ClearHitEntityList()
	{
		_hitEntities.Clear();
	}

	public override void CurrentWeaponAttack(DamageArea.DamageType damageType, bool applyDamageTypeMultipliers)
	{
		base.CurrentWeaponAttack(damageType, applyDamageTypeMultipliers);
		if (CurrentPenitentWeapon == null || _penitent == null)
		{
			return;
		}
		float final = _penitent.Stats.Strength.Final;
		Hit hit = default(Hit);
		hit.AttackingEntity = _penitent.gameObject;
		hit.DamageType = ((!IsRunningCombo) ? damageType : ((!IsFinalComboAvailable || IsRunningUpgradedCombo) ? damageType : DamageArea.DamageType.Normal));
		hit.DamageAmount = final;
		Hit weapondHit = hit;
		weapondHit.DamageType = (IsHeavyAttackPrayerEquipped ? DamageArea.DamageType.Heavy : weapondHit.DamageType);
		switch (damageType)
		{
		case DamageArea.DamageType.Normal:
			weapondHit.HitSoundId = SimpleEnemyHit;
			weapondHit.DestroysProjectiles = PlayerHasSwordHeart;
			weapondHit.Force = 1f;
			break;
		case DamageArea.DamageType.Heavy:
			weapondHit.DestroysProjectiles = true;
			weapondHit.HitSoundId = HeavyEnemyHit;
			weapondHit.Force = 2f;
			if (applyDamageTypeMultipliers)
			{
				bool @bool = base.EntityOwner.Animator.GetBool("PARRY");
				weapondHit.DamageAmount = ((!@bool) ? (final * HeavyAttackMultiplier) : (final * ParryMultiplier));
			}
			break;
		case DamageArea.DamageType.Critical:
			weapondHit.Force = 2f;
			weapondHit.HitSoundId = CriticalEnemyHit;
			break;
		}
		weapondHit.DamageAmount *= base.EntityOwner.Stats.DamageMultiplier.Final;
		if (CheckCriticalHit())
		{
			weapondHit.DamageAmount *= base.EntityOwner.Stats.CriticalMultiplier.Final;
			weapondHit.DamageType = DamageArea.DamageType.Critical;
		}
		CurrentPenitentWeapon.Attack(weapondHit);
	}

	public override void CurrentWeaponAttack(DamageArea.DamageType damageType)
	{
		base.CurrentWeaponAttack(damageType);
		if ((bool)CurrentPenitentWeapon && (bool)_penitent)
		{
			float final = _penitent.Stats.Strength.Final;
			Hit hit = default(Hit);
			hit.AttackingEntity = _penitent.gameObject;
			hit.DamageType = ((!IsRunningCombo) ? damageType : ((!IsFinalComboAvailable || IsRunningUpgradedCombo) ? damageType : DamageArea.DamageType.Normal));
			Hit weapondHit = hit;
			weapondHit.DamageType = (IsHeavyAttackPrayerEquipped ? DamageArea.DamageType.Heavy : weapondHit.DamageType);
			switch (damageType)
			{
			case DamageArea.DamageType.Normal:
				weapondHit.HitSoundId = SimpleEnemyHit;
				weapondHit.DestroysProjectiles = PlayerHasSwordHeart;
				weapondHit.Force = 1f;
				weapondHit.DamageAmount = final;
				break;
			case DamageArea.DamageType.Heavy:
			{
				bool @bool = base.EntityOwner.Animator.GetBool("PARRY");
				weapondHit.DamageAmount = ((!@bool) ? (final * HeavyAttackMultiplier) : (final * ParryMultiplier));
				weapondHit.HitSoundId = HeavyEnemyHit;
				weapondHit.DestroysProjectiles = true;
				weapondHit.Force = 2f;
				break;
			}
			case DamageArea.DamageType.Critical:
				weapondHit.HitSoundId = CriticalEnemyHit;
				weapondHit.Force = 2f;
				break;
			default:
				weapondHit.DamageAmount = final;
				break;
			}
			weapondHit.DamageAmount *= base.EntityOwner.Stats.DamageMultiplier.Final;
			if (CheckCriticalHit())
			{
				weapondHit.DamageAmount *= base.EntityOwner.Stats.CriticalMultiplier.Final;
				weapondHit.DamageType = DamageArea.DamageType.Critical;
			}
			CurrentPenitentWeapon.Attack(weapondHit);
		}
	}

	private void AttackTrigger()
	{
		_deltaAttackTimeThreshold += Time.deltaTime;
		if (_playerInput.Attack)
		{
			_deltaAttackTimeThreshold = 0f;
		}
	}

	private void CheckHitImpulseFired()
	{
		HitImpulseTriggered = !_penitent.Status.IsGrounded && (_penitent.PlatformCharacterInput.isJoystickDown || _penitent.PlatformCharacterInput.IsDashButtonHold);
	}

	private bool IsAttackTriggered()
	{
		return _deltaAttackTimeThreshold <= _attackTimeThreshold;
	}

	private bool CheckCriticalHit()
	{
		float num = UnityEngine.Random.Range(0f, 1f);
		return num <= base.EntityOwner.Stats.CriticalChance.Final;
	}

	public float GetExecutionBonus()
	{
		float result = 0f;
		if (Combo.IsAvailable)
		{
			UnlockableSkill getMaxSkill = Combo.GetMaxSkill;
			result = ((!getMaxSkill.id.Equals("COMBO_1")) ? SecondComboTierExecutionBonus : FirstComboTierExecutionBonus);
		}
		return result;
	}

	private void OnStayAttackArea(object sender, Collider2DParam e)
	{
		if (!WindowAttackOpen)
		{
			return;
		}
		GameObject gameObject = e.Collider2DArg.gameObject;
		if (!_hitEntities.Contains(gameObject))
		{
			_newEnemyHit = false;
			if (_hitEntities.Count < 1)
			{
				HitImpulse();
			}
			_hitEntities.Add(gameObject);
			IDamageable componentInChildren = gameObject.transform.root.GetComponentInChildren<IDamageable>();
			if (componentInChildren != null)
			{
				CurrentWeaponAttack(DamageArea.DamageType.Normal, applyDamageTypeMultipliers: true);
			}
		}
	}

	private void HitImpulse()
	{
		if (!_penitent.PlatformCharacterController.IsGrounded && (float)_currentImpulses < base.EntityOwner.Stats.AirImpulses.Final && HitImpulseTriggered)
		{
			Vector2 vector = new Vector2(_penitent.PlatformCharacterController.InstantVelocity.x, 1f);
			_penitent.PlatformCharacterController.PlatformCharacterPhysics.Velocity = vector * airAttackImpulse;
			_currentImpulses++;
		}
	}

	private void PenitentOnEntityDie()
	{
		_penitent.Animator.speed = 1f;
		_hitEntities.Clear();
		_playerAttackArea.OnStay -= OnStayAttackArea;
	}
}
