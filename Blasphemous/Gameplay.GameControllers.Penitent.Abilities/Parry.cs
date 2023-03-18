using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent.Attack;
using Gameplay.GameControllers.Penitent.Audio;
using Gameplay.UI;
using Tools.Level;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class Parry : Ability
{
	public Core.SimpleEvent OnParryCast;

	public PenitentSword Sword;

	private float _timeGrounded;

	private const int RightButton = 1;

	public float slowTimeDuration = 0.3f;

	public AnimationCurve slowTimeCurve;

	[EventRef]
	public string ParryGuardFx;

	private readonly int _startParryAnim = UnityEngine.Animator.StringToHash("ParryStart");

	private static readonly int Parry1 = UnityEngine.Animator.StringToHash("PARRY");

	public bool SuccessParry { get; set; }

	public bool IsOnParryChance { get; set; }

	public float ParryWindow { get; set; }

	private bool ParryInput => base.Rewired.GetButtonDown(38);

	public bool IsRunningParryAnim
	{
		get
		{
			bool flag = base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("ParryStart");
			bool flag2 = base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("ParryChance");
			bool flag3 = base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("ParrySuccess");
			return flag || flag2 || flag3;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (Sword == null)
		{
			Debug.LogError("This component needs and Penitent Attack Component");
		}
		Interactable.SInteractionStarted += OnSInteractionStarted;
	}

	private void OnDestroy()
	{
		Interactable.SInteractionStarted -= OnSInteractionStarted;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		bool flag = IsGrounded();
		if (UIController.instance.IsTutorialActive())
		{
			StopParry();
		}
		if (ParryInput)
		{
			if (!flag || IsRunningParryAnim || !ReadyToCast() || SuccessParry || Core.Input.InputBlocked)
			{
				return;
			}
			RaiseParryEvent();
			Cast();
		}
		else
		{
			if (!base.Casting || Core.Logic.Penitent.GuardSlide.Casting)
			{
				return;
			}
			CheckParryWindow();
			Core.Logic.Penitent.Parry.IsOnParryChance = base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("ParryStart") || base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("ParryChance");
			if (base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
			{
				StopCast();
			}
		}
		if (!base.EntityOwner.Status.IsGrounded || base.EntityOwner.Status.Dead || base.EntityOwner.Status.IsHurt)
		{
			StopCast();
		}
	}

	private void RaiseParryEvent()
	{
		if (OnParryCast != null)
		{
			OnParryCast();
		}
	}

	private void OnSInteractionStarted(Interactable entity)
	{
		if (IsOnParryChance)
		{
			IsOnParryChance = false;
		}
	}

	private void CheckParryWindow()
	{
		ParryWindow -= Time.deltaTime;
		if (!(ParryWindow > 0f) && !base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("ParryFailed") && !base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !base.EntityOwner.Status.IsHurt && !SuccessParry)
		{
			base.EntityOwner.Animator.Play("ParryFailed");
		}
	}

	public bool CheckParry(Hit hit)
	{
		if (!IsHitParryable(hit))
		{
			return false;
		}
		SuccessParry = Sword.SuccessParryChance(hit);
		ParrySuccess(SuccessParry);
		if (SuccessParry)
		{
			Core.Events.LaunchEvent("PARRY", string.Empty);
			Core.Logic.ScreenFreeze.Freeze(0.1f, slowTimeDuration, 0f, slowTimeCurve);
		}
		return SuccessParry;
	}

	private bool IsHitParryable(Hit hit)
	{
		bool result = true;
		switch (hit.DamageElement)
		{
		case DamageArea.DamageElement.Fire:
		case DamageArea.DamageElement.Toxic:
		case DamageArea.DamageElement.Magic:
		case DamageArea.DamageElement.Lightning:
		case DamageArea.DamageElement.Contact:
			result = false;
			break;
		}
		DamageArea.DamageType damageType = hit.DamageType;
		if (damageType == DamageArea.DamageType.Heavy || damageType == DamageArea.DamageType.Simple)
		{
			result = false;
		}
		return result;
	}

	public void ParrySuccess(bool success)
	{
		Core.Metrics.CustomEvent("PARRY_SUCCESS", string.Empty);
		if (success)
		{
			base.EntityOwner.Status.Invulnerable = true;
		}
		base.EntityOwner.Animator.SetBool(Parry1, SuccessParry);
		if (SuccessParry)
		{
			base.EntityOwner.GetComponentInChildren<PenitentAudio>().PlayParryAttack();
		}
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		StartParry();
	}

	protected override void OnCastEnd(float castingTime)
	{
		base.OnCastEnd(castingTime);
		StopParry();
	}

	private void StartParry()
	{
		Core.Metrics.CustomEvent("PARRY_USED", string.Empty);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		if (Core.Logic.Penitent.PenitentAttack.IsRunningCombo)
		{
			Core.Logic.Penitent.CancelEffect.PlayCancelEffect();
		}
		base.EntityOwner.Animator.Play(_startParryAnim);
		ParryWindow = base.EntityOwner.Stats.ParryWindow.Final;
		Core.Audio.EventOneShotPanned(ParryGuardFx, base.EntityOwner.transform.position);
	}

	private void StopParry()
	{
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		base.EntityOwner.Status.Invulnerable = false;
		SuccessParry = false;
		base.EntityOwner.Animator.SetBool(Parry1, SuccessParry);
		Core.Logic.Penitent.Parry.IsOnParryChance = false;
		Core.Logic.Penitent.Audio.StopParryFx();
	}

	private bool IsGrounded()
	{
		if (base.EntityOwner.Status.IsGrounded)
		{
			_timeGrounded += Time.deltaTime;
		}
		else
		{
			_timeGrounded = 0f;
		}
		return _timeGrounded > 0.1f;
	}

	private bool ReadyToCast()
	{
		bool flag = base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("HardLanding");
		bool flag2 = base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("Charged Attack");
		bool isHurt = base.EntityOwner.Status.IsHurt;
		return !flag && !isHurt && !flag2;
	}
}
