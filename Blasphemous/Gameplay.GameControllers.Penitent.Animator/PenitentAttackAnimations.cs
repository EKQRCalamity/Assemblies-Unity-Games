using System;
using System.Collections.Generic;
using DamageEffect;
using Framework.Managers;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Effects.Player.Dust;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Animations;
using Gameplay.GameControllers.Penitent.Abilities;
using Gameplay.GameControllers.Penitent.Attack;
using Gameplay.GameControllers.Penitent.Damage;
using Tools.Level.Layout;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Animator;

public class PenitentAttackAnimations : AttackAnimationsEvents
{
	public List<MaterialsPerDamageElement> damageMaterials;

	protected PenitentAttack PenitentAttack;

	private Penitent _penitent;

	private UnityEngine.Animator _playerAnimator;

	private LevelInitializer _currentLevel;

	private CameraManager _cameraManager;

	private DamageEffectScript _flash;

	private MasterShaderEffects _spriteEffects;

	private ParryDust _parryDust;

	private void Awake()
	{
		PenitentAttack = base.transform.parent.GetComponentInChildren<PenitentAttack>();
		_penitent = GetComponentInParent<Penitent>();
		_playerAnimator = GetComponent<UnityEngine.Animator>();
		_parryDust = GetComponentInChildren<ParryDust>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_currentLevel = Core.Logic.CurrentLevelConfig;
		_cameraManager = Core.Logic.CameraManager;
		_spriteEffects = GetComponent<MasterShaderEffects>();
		PenitentDamageArea damageArea = _penitent.DamageArea;
		damageArea.OnDamaged = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Combine(damageArea.OnDamaged, new PenitentDamageArea.PlayerDamagedEvent(OnDamaged));
	}

	private Material GetMaterialByHit(Hit h)
	{
		return damageMaterials.Find((MaterialsPerDamageElement x) => x.element == h.DamageElement).mat;
	}

	private void OnDamaged(Penitent damaged, Hit hit)
	{
		if (hit.DamageElement == DamageArea.DamageElement.Contact || hit.DamageElement == DamageArea.DamageElement.Normal)
		{
			if (_spriteEffects != null)
			{
				_spriteEffects.TriggerColorFlash();
			}
			return;
		}
		Material materialByHit = GetMaterialByHit(hit);
		if (_spriteEffects != null)
		{
			_spriteEffects.DamageEffectBlink(0f, 0.2f, materialByHit);
		}
	}

	public override void WeaponBlowUp(float bladeBlowUp)
	{
		bool isWeaponBlowingUp = (int)Mathf.Clamp01(bladeBlowUp) > 0;
		if (PenitentAttack != null)
		{
			PenitentAttack.IsWeaponBlowingUp = isWeaponBlowingUp;
		}
	}

	public void DamageByFall(int fallingDamage, float killByFallDamageThreshold)
	{
		if (!((float)fallingDamage > 0f))
		{
			return;
		}
		float current = _penitent.Stats.Life.Current;
		if ((float)fallingDamage >= current * killByFallDamageThreshold)
		{
			if (!_penitent.IsSmashed)
			{
				_penitent.IsSmashed = true;
				_playerAnimator.Play("Death Fall");
			}
		}
		else
		{
			_penitent.Damage(fallingDamage, string.Empty);
		}
	}

	public void LevelSleepTime(float sleepTime)
	{
		if (!(_currentLevel == null))
		{
			_currentLevel.sleepTime = sleepTime;
			if (!_currentLevel.IsSleeping)
			{
				_currentLevel.SleepTime();
			}
		}
	}

	public void ComboCameraShake()
	{
		if (_cameraManager != null && _penitent.AttackArea.IsEnemyHit() && _cameraManager.ProCamera2DShake != null)
		{
			_cameraManager.ProCamera2DShake.ShakeUsingPreset("FinalHitCombo");
		}
	}

	public void ChargedAttackCameraShake()
	{
		if (!(_cameraManager == null) && _penitent.AttackArea.IsEnemyHit() && _cameraManager.ProCamera2DShake != null)
		{
			_cameraManager.ProCamera2DShake.ShakeUsingPreset("ChargedAttack");
		}
	}

	public void SetSwordSlash(PenitentSword.AttackType type)
	{
		PenitentSword penitentSword = (PenitentSword)_penitent.PenitentAttack.CurrentPenitentWeapon;
		PenitentSword.SwordSlash swordSlash = default(PenitentSword.SwordSlash);
		swordSlash.Type = type;
		swordSlash.Level = _penitent.PenitentAttack.CurrentLevel;
		PenitentSword.SwordSlash slashAnimation = swordSlash;
		PenitentSword.AttackColor attackColor = ((slashAnimation.Level > 1) ? _penitent.PenitentAttack.AttackColor : PenitentSword.AttackColor.Default);
		slashAnimation.Color = penitentSword.SlashAnimator.GetColorValue(attackColor);
		penitentSword.SlashAnimator.SetSlashAnimation(slashAnimation);
	}

	public void FinishingUpwardCombo()
	{
		_penitent.PenitentAttack.CurrentWeaponAttack(DamageArea.DamageType.Heavy, applyDamageTypeMultipliers: true);
	}

	public void FinishingDownwardCombo()
	{
		_penitent.PenitentAttack.CurrentWeaponAttack(DamageArea.DamageType.Heavy, applyDamageTypeMultipliers: true);
	}

	public override void CurrentWeaponAttack(DamageArea.DamageType damageType)
	{
		if (!(PenitentAttack == null))
		{
			PenitentAttack.CurrentWeaponAttack(damageType);
		}
	}

	public override void CurrentWeaponRawAttack(DamageArea.DamageType damageType)
	{
		base.CurrentWeaponRawAttack(damageType);
		PenitentAttack.CurrentWeaponAttack(damageType, applyDamageTypeMultipliers: false);
	}

	public void OpenParryWindow()
	{
		if (!(_penitent == null))
		{
			_penitent.Parry.IsOnParryChance = true;
		}
	}

	public void CloseParryWindow()
	{
		if (!(_penitent == null))
		{
			_penitent.Parry.IsOnParryChance = false;
		}
	}

	public void TriggerParryEffect()
	{
	}

	public void RaiseParryDust()
	{
		if (!(_parryDust == null))
		{
			_parryDust.TriggerParryDust();
		}
	}

	public void PrayerUseAttack()
	{
		if (!(_penitent == null) && _penitent.Status.IsGrounded)
		{
			VerticalAttack componentInChildren = _penitent.GetComponentInChildren<VerticalAttack>();
			if (!(componentInChildren == null))
			{
				componentInChildren.EnableVerticalAttackCollider();
				componentInChildren.TriggerVerticalAttack(instantiateExplosion: false);
				PrayerUse componentInChildren2 = _penitent.GetComponentInChildren<PrayerUse>();
				componentInChildren2.Cast();
			}
		}
	}

	public void InstanceRangeAttackProjectile()
	{
		if (!(_penitent == null))
		{
			RangeAttack componentInChildren = _penitent.GetComponentInChildren<RangeAttack>();
			if (componentInChildren.Casting)
			{
				componentInChildren.InstanceProjectile();
			}
		}
	}

	public void CanLungeAttack(Activation activation)
	{
		LungeAttack componentInChildren = Core.Logic.Penitent.GetComponentInChildren<LungeAttack>();
		componentInChildren.CanHit = activation == Activation.True;
	}

	public void FireChargedAttackProjectile()
	{
		_penitent.ChargedAttack.InstantiateProjectile();
	}

	public void OpenAttackWindow()
	{
		if (!(_penitent == null))
		{
			_penitent.PenitentAttack.WindowAttackOpen = true;
		}
	}

	public void CloseAttackWindow()
	{
		if (!(_penitent == null))
		{
			_penitent.PenitentAttack.WindowAttackOpen = false;
		}
	}

	public void CastActiveRiposte()
	{
		if (!(_penitent == null))
		{
			_penitent.ActiveRiposte.Cast();
		}
	}

	public void PlayRangeAttack()
	{
		_penitent.Audio.PlayRangeAttack();
	}

	public void PlayBasicAttack1()
	{
		_penitent.Audio.PlayBasicAttack1();
	}

	public void PlayBasicAttack2()
	{
		_penitent.Audio.PlayBasicAttack2();
	}

	public void PlayAirAttack1()
	{
		_penitent.Audio.PlayAirAttack1();
	}

	public void PlayAirAttack2()
	{
		_penitent.Audio.PlayBasicAttack2();
	}

	public void PlayHeavyAttack()
	{
		_penitent.Audio.PlayHeavyAttack();
	}

	public void PlayLoadingChargeAttack()
	{
	}

	public void PlayLoadedChargedAttack()
	{
	}

	public void PlayParryHit()
	{
		_penitent.Audio.PlayParryHit();
	}

	public void PlayReleaseChargeAttack()
	{
		_penitent.Audio.PlayReleaseChargedAttack();
	}

	public void PlayParry()
	{
		_penitent.Audio.PlayParryAttack();
	}

	public void StartParry()
	{
		_penitent.Audio.PlayStartParry();
	}

	public void PlayOverThrow()
	{
		_penitent.Audio.PlayOverthrow();
	}

	public void PlayDeath()
	{
		_penitent.Audio.PlayDeath();
	}

	public void PlayDeathSpikes()
	{
		_penitent.Audio.PlayDeathSpikes();
	}

	public void PlayDeathFall()
	{
		_penitent.Audio.PlayDeathFall();
	}

	public void PlaySimpleDamage()
	{
		_penitent.Audio.PlaySimpleDamage();
	}

	public void PlayPushBack()
	{
		_penitent.Audio.PlayPushBack();
	}

	public void PlayVerticalAttackStart()
	{
		_penitent.Audio.PlayVerticalAttackStart();
	}

	public void PlayVerticalAttackFalling()
	{
		_penitent.Audio.PlayVerticalAttackFalling();
	}

	public void PlayVerticalAttackLanding()
	{
		_penitent.Audio.PlayVerticalAttackLanding();
	}

	public void PlayFinishingComboDown()
	{
		_penitent.Audio.PlayFinishingComboDown();
	}

	public void PlayHealingExplosion()
	{
		_penitent.Audio.HealingExplosion();
	}

	public void PlayPrayerUse()
	{
		_penitent.Audio.ActivatePrayer();
	}

	public void PlayComboHit()
	{
		_penitent.Audio.PlayComboHit();
	}

	public void PlayComboHitUp()
	{
		_penitent.Audio.PlayComboHitUp();
	}

	public void PlayComboHitDown()
	{
		_penitent.Audio.PlayComboHitDown();
	}
}
