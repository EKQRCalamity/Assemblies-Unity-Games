using System;
using DG.Tweening;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class GuardSlide : Ability
{
	private Penitent _penitent;

	private readonly int _guardToIdleAnim = UnityEngine.Animator.StringToHash("GuardToIdle");

	private readonly int _guardSlideAnim = UnityEngine.Animator.StringToHash("GuardSlide");

	[FoldoutGroup("Slide Settings", 0)]
	public AnimationCurve SlideAnimationCurve;

	[FoldoutGroup("Slide Settings", 0)]
	public float LapseBeforeDash;

	[FoldoutGroup("Slide Settings", 0)]
	public float DistanceFactorByImpactForce;

	[FoldoutGroup("Slide Settings", 0)]
	public float HitLapse;

	[FoldoutGroup("Slide Settings", 0)]
	public float TimeTakenDuringLerp = 0.5f;

	[FoldoutGroup("Slide Settings", 0)]
	public GameObject GuardSlideImpactEffect;

	[FoldoutGroup("Audio Settings", 0)]
	[EventRef]
	public string GuardSlideSoundFx;

	private Vector3 guardingHitPosition;

	public const float GuardToIdleDuration = 0.35f;

	private float _currentLapseBeforeDash;

	private float _timeLerpingRemaining;

	private static readonly int Dashing = UnityEngine.Animator.StringToHash("DASHING");

	private static readonly int Dash = UnityEngine.Animator.StringToHash("DASH");

	private bool orientationBeingForced;

	private EntityOrientation forcedOrientation;

	public bool IsGuard { get; set; }

	private bool CanSlide => !_penitent.FloorChecker.IsSideBlocked && _penitent.FloorChecker.IsGrounded;

	protected override void OnAwake()
	{
		base.OnAwake();
		SpawnManager.OnPlayerSpawn += OnPenitentReady;
	}

	private void OnDestroy()
	{
		SpawnManager.OnPlayerSpawn -= OnPenitentReady;
		if ((bool)_penitent)
		{
			MotionLerper motionLerper = _penitent.MotionLerper;
			motionLerper.OnLerpStart = (Core.SimpleEvent)Delegate.Remove(motionLerper.OnLerpStart, new Core.SimpleEvent(OnLerpStart));
			MotionLerper motionLerper2 = _penitent.MotionLerper;
			motionLerper2.OnLerpStop = (Core.SimpleEvent)Delegate.Remove(motionLerper2.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		PoolManager.Instance.CreatePool(GuardSlideImpactEffect, 1);
	}

	private void OnPenitentReady(Penitent penitent)
	{
		_penitent = penitent;
		MotionLerper motionLerper = _penitent.MotionLerper;
		motionLerper.OnLerpStart = (Core.SimpleEvent)Delegate.Combine(motionLerper.OnLerpStart, new Core.SimpleEvent(OnLerpStart));
		MotionLerper motionLerper2 = _penitent.MotionLerper;
		motionLerper2.OnLerpStop = (Core.SimpleEvent)Delegate.Combine(motionLerper2.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ((bool)_penitent && _penitent.Status.IsGrounded && base.Casting)
		{
			_currentLapseBeforeDash += Time.deltaTime;
			_timeLerpingRemaining -= Time.deltaTime;
			if (_penitent.MotionLerper.IsLerping)
			{
				SetOwnerOrientation();
			}
			if (!CanSlide)
			{
				_penitent.MotionLerper.StopLerping();
			}
			if (_timeLerpingRemaining <= 0.35f && IsGuard && _penitent.MotionLerper.IsLerping)
			{
				IsGuard = false;
				base.EntityOwner.Animator.Play(_guardToIdleAnim);
			}
			if (_penitent.PlatformCharacterInput.Rewired.GetButton(7) && base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("GuardToIdle"))
			{
				CastDash();
				_penitent.MotionLerper.StopLerping();
			}
		}
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		IsGuard = true;
		ImpactGamePause();
		Vector3 position = base.EntityOwner.transform.position;
		PoolManager.Instance.ReuseObject(GuardSlideImpactEffect, position, Quaternion.identity);
		_penitent.Parry.StopCast();
		EnableParticleSystem();
		Core.Audio.EventOneShotPanned(GuardSlideSoundFx, position);
	}

	protected override void OnCastEnd(float castingTime)
	{
		base.OnCastEnd(castingTime);
		_currentLapseBeforeDash = 0f;
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		base.EntityOwner.Animator.Play(_guardToIdleAnim);
		EnableParticleSystem(enableParticleSystem: false);
	}

	private void OnLerpStart()
	{
		if (base.Casting)
		{
			base.EntityOwner.Animator.Play(_guardSlideAnim);
		}
	}

	private void OnLerpStop()
	{
		orientationBeingForced = false;
		StopCast();
	}

	public void CastSlide(Hit hit)
	{
		if (!base.Casting)
		{
			if (hit.OnGuardCallback != null)
			{
				hit.OnGuardCallback(hit);
			}
			guardingHitPosition = hit.AttackingEntity.transform.position;
			if (hit.ForceGuardSlideDirection && hit.AttackingEntity.GetComponent<Entity>() != null)
			{
				SetOwnerOrientationByAttackingEntity(hit);
			}
			else
			{
				SetOwnerOrientation();
			}
			Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
			SetLerping(hit);
			Cast();
		}
	}

	private void CastDash()
	{
		base.EntityOwner.Animator.SetBool(Dashing, value: true);
		base.EntityOwner.Animator.SetTrigger(Dash);
		_penitent.Dash.Cast();
	}

	private void SetOwnerOrientationByAttackingEntity(Hit hit)
	{
		orientationBeingForced = true;
		EntityOrientation orientation = hit.AttackingEntity.GetComponent<Entity>().Status.Orientation;
		forcedOrientation = ((orientation != EntityOrientation.Left) ? EntityOrientation.Left : EntityOrientation.Right);
		base.EntityOwner.SetOrientation(forcedOrientation);
	}

	private void SetOwnerOrientation()
	{
		if (orientationBeingForced)
		{
			base.EntityOwner.SetOrientation(forcedOrientation);
		}
		else
		{
			base.EntityOwner.SetOrientation((!(guardingHitPosition.x > base.EntityOwner.transform.position.x)) ? EntityOrientation.Left : EntityOrientation.Right);
		}
	}

	private void SetLerping(Hit hit)
	{
		if ((bool)_penitent.MotionLerper)
		{
			_penitent.MotionLerper.distanceToMove = hit.Force * DistanceFactorByImpactForce;
			_penitent.MotionLerper.speedCurve = SlideAnimationCurve;
			_penitent.MotionLerper.TimeTakenDuringLerp = TimeTakenDuringLerp;
		}
	}

	private void Slide()
	{
		if ((bool)_penitent.MotionLerper)
		{
			Vector2 vector = Vector2.right;
			if ((orientationBeingForced && forcedOrientation == EntityOrientation.Right) || (!orientationBeingForced && guardingHitPosition.x > base.EntityOwner.transform.position.x))
			{
				vector = Vector2.left;
			}
			if (_penitent.SlopeAngle >= 5f)
			{
				vector.y += 0.25f;
			}
			else if (_penitent.SlopeAngle <= -5f)
			{
				vector.y -= 0.25f;
			}
			_penitent.MotionLerper.StartLerping(vector);
		}
	}

	private void EnableParticleSystem(bool enableParticleSystem = true)
	{
		ParticleSystem.EmissionModule emission = Core.Logic.Penitent.ParticleSystem.emission;
		emission.enabled = enableParticleSystem;
		if (enableParticleSystem)
		{
			Core.Logic.Penitent.ParticleSystem.Play();
		}
		else
		{
			Core.Logic.Penitent.ParticleSystem.Stop();
		}
	}

	private void ImpactGamePause()
	{
		DOTween.Sequence().SetDelay(HitLapse).OnStart(delegate
		{
			Core.Logic.CurrentLevelConfig.sleepTime = HitLapse;
			Core.Logic.CurrentLevelConfig.SleepTime();
		})
			.OnComplete(delegate
			{
				_currentLapseBeforeDash = 0f;
				_timeLerpingRemaining = TimeTakenDuringLerp;
				Slide();
			});
	}
}
