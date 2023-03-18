using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent.Attack;
using Gameplay.GameControllers.Penitent.Gizmos;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class ChargedAttack : Ability
{
	public float chargedAttackAreaWidth;

	public float chargedAttackScopeDetection;

	public GameObject ChargedAttackProjectile;

	private float _defaultEnemyScopeDetection;

	private bool _isAttackAreaResized;

	private Penitent _penitent;

	private AttackArea _attackArea;

	public float BaseChargingTimeTier1 = 1.5f;

	public float BaseChargingTimeTier2 = 0.75f;

	private float _currentChargingTime;

	private bool _isChargingAttack;

	[EventRef]
	public string ChargedAttackProjectileFx;

	public bool IsChargingAttack
	{
		get
		{
			return _isChargingAttack;
		}
		set
		{
			_isChargingAttack = value;
		}
	}

	public RootMotionDriver RootMotionDriver { get; private set; }

	public bool IsAvailableSkilledAbility => CanExecuteSkilledAbility() && base.HasEnoughFervour;

	protected override void OnStart()
	{
		base.OnStart();
		_penitent = (Penitent)base.EntityOwner;
		_attackArea = _penitent.AttackArea;
		RootMotionDriver = _penitent.GetComponentInChildren<RootMotionDriver>();
		_defaultEnemyScopeDetection = _attackArea.entityScopeDetection;
		PoolManager.Instance.CreatePool(ChargedAttackProjectile, 1);
	}

	protected override void OnUpdate()
	{
		if (base.Casting)
		{
			_currentChargingTime -= Time.deltaTime;
			if (_currentChargingTime <= 0f)
			{
				base.EntityOwner.Animator.SetBool("CHARGE_ATTACK_TIER", value: true);
			}
			if (!_penitent.PlatformCharacterInput.IsAttackButtonHold)
			{
				StopCast();
			}
		}
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		_isChargingAttack = true;
		SetChargingTimeByTier();
	}

	protected override void OnCastEnd(float castingTime)
	{
		base.OnCastEnd(castingTime);
		_isChargingAttack = false;
		base.EntityOwner.Animator.SetBool("CHARGE_ATTACK_TIER", value: false);
	}

	public void ResizeAttackArea(bool resize = true)
	{
		if (resize && !_isAttackAreaResized)
		{
			_isAttackAreaResized = true;
			_attackArea.entityScopeDetection = chargedAttackScopeDetection;
			_attackArea.SetSize(PenitentSword.MaxSizeAttackCollider);
		}
		else if (!resize && _isAttackAreaResized)
		{
			_isAttackAreaResized = false;
			_attackArea.entityScopeDetection = _defaultEnemyScopeDetection;
			_attackArea.SetSize(PenitentSword.MinSizeAttackCollider);
		}
	}

	private void SetChargingTimeByTier()
	{
		UnlockableSkill lastUnlockedSkill = GetLastUnlockedSkill();
		if (lastUnlockedSkill != null)
		{
			string id = lastUnlockedSkill.id;
			_currentChargingTime = ((!id.Equals("CHARGED_1")) ? BaseChargingTimeTier2 : BaseChargingTimeTier1);
		}
	}

	public void InstantiateProjectile()
	{
		if (CanExecuteSkilledAbility() && GetLastUnlockedSkill().id.Equals("CHARGED_3") && !(ChargedAttackProjectile == null))
		{
			Vector3 position = ((_penitent.Status.Orientation != 0) ? RootMotionDriver.ReversePosition : RootMotionDriver.transform.position);
			PoolManager.Instance.ReuseObject(ChargedAttackProjectile, position, Quaternion.identity);
			PlayChargedAttackProjectileFx();
		}
	}

	private void PlayChargedAttackProjectileFx()
	{
		if (!string.IsNullOrEmpty(ChargedAttackProjectileFx))
		{
			Core.Audio.PlaySfx(ChargedAttackProjectileFx);
		}
	}
}
