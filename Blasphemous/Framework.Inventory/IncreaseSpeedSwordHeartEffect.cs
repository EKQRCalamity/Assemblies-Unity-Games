using System;
using FMOD.Studio;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Effects.Player.Sprint;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using Gameplay.GameControllers.Penitent.Animator;
using Sirenix.Utilities;
using UnityEngine;

namespace Framework.Inventory;

public class IncreaseSpeedSwordHeartEffect : ObjectEffect
{
	[EventRef]
	public string SpecialFoostepSound;

	private Penitent _penitent;

	private GhostTrailGenerator GhostTrail;

	public Dash.MoveSetting MotionSettings;

	private Dash.MoveSetting DefaultMotionSettings;

	private bool isEffectApplied;

	private bool isItemEquipped;

	private bool RemoveEffectFlag;

	private bool SuscribedToTriggerEvents;

	private SprintEffects sprintFX;

	private GameObject SprintEffect;

	private IncreaseSpeedBeadEffect beadEffect;

	[EventRef]
	public string prayerActivationSoundFx;

	[EventRef]
	public string prayerUseLoopSoundFx;

	private EventInstance _activationSoundLoopInstance;

	protected override void OnAwake()
	{
		base.OnAwake();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
		LevelManager.OnBeforeLevelLoad += OnLevelPreload;
	}

	private void OnLevelPreload(Level oldlevel, Level newlevel)
	{
		UnsubscribePlayerEvents();
		StopUseLoopFx();
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		_penitent = penitent;
		SubscribePlayerEvents();
		GhostTrail = _penitent.GetComponentInChildren<GhostTrailGenerator>();
		DefaultMotionSettings = _penitent.Dash.DefaultMoveSetting;
		if (!SprintEffect)
		{
			SprintEffect = GameObject.FindWithTag("SprintEffect");
		}
		if ((bool)SprintEffect)
		{
			sprintFX = SprintEffect.GetComponentInChildren<SprintEffects>();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ((bool)_penitent && isItemEquipped)
		{
			if (!_penitent.Status.IsGrounded && isEffectApplied)
			{
				RemoveEffectFlag = true;
			}
			if (_penitent.Status.IsGrounded && RemoveEffectFlag)
			{
				RemoveEffectFlag = false;
				ApplyEffect(apply: false);
			}
			if (_penitent.PlatformCharacterController.IsClimbing)
			{
				ApplyEffect(apply: false);
			}
			if (_penitent.IsGrabbingCliffLede)
			{
				ApplyEffect(apply: false);
			}
			if (_penitent.IsStickedOnWall)
			{
				ApplyEffect(apply: false);
			}
			if (_penitent.Status.IsGrounded && isEffectApplied && Mathf.Abs(_penitent.PlatformCharacterController.PlatformCharacterPhysics.HSpeed) <= 0.1f)
			{
				ApplyEffect(apply: false);
			}
		}
	}

	private void OnFinishDash()
	{
		if (isItemEquipped && _penitent.Status.IsGrounded)
		{
			ApplyEffect();
			ActivationSound();
		}
	}

	protected override bool OnApplyEffect()
	{
		isItemEquipped = true;
		return true;
	}

	protected override void OnRemoveEffect()
	{
		isItemEquipped = false;
		ApplyEffect(apply: false);
		base.OnRemoveEffect();
	}

	private void OnLungeAttackStart()
	{
		if (isItemEquipped)
		{
			ApplyEffect(apply: false);
		}
	}

	private void ApplyEffect(bool apply = true)
	{
		isEffectApplied = apply;
		ApplySpeedSettings(apply);
		ApplyGhostingEffect(apply);
		ApplySprintEffect(apply);
		ApplySoundLoopFx(apply);
	}

	private void ApplySpeedSettings(bool apply = true)
	{
		if (!_penitent)
		{
			return;
		}
		float speed = MotionSettings.Speed;
		float drag = MotionSettings.Drag;
		if (!apply)
		{
			if (Core.InventoryManager.IsRosaryBeadEquipped("RB203"))
			{
				if (beadEffect == null)
				{
					beadEffect = UnityEngine.Object.FindObjectOfType<IncreaseSpeedBeadEffect>();
				}
				speed = beadEffect.BeadMoveSettings.Speed;
				drag = beadEffect.BeadMoveSettings.Drag;
			}
			else
			{
				speed = DefaultMotionSettings.Speed;
				drag = DefaultMotionSettings.Drag;
			}
		}
		_penitent.PlatformCharacterController.MaxWalkingSpeed = speed;
		_penitent.PlatformCharacterController.WalkingDrag = drag;
	}

	private void ApplyGhostingEffect(bool apply = true)
	{
		if ((bool)GhostTrail)
		{
			GhostTrail.EnableGhostTrail = apply;
		}
	}

	private void ApplySprintEffect(bool apply = true)
	{
		if (!SprintEffect)
		{
			SprintEffect = GameObject.FindWithTag("SprintEffect");
		}
		if ((bool)SprintEffect)
		{
			sprintFX = SprintEffect.GetComponentInChildren<SprintEffects>();
			if ((bool)sprintFX && apply)
			{
				sprintFX.EmitOnStart();
			}
		}
	}

	private void ApplySoundLoopFx(bool apply = true)
	{
		if (apply)
		{
			StartUseLoopFx();
		}
		else
		{
			StopUseLoopFx();
		}
	}

	private void OnStep(object param)
	{
		if (isEffectApplied)
		{
			Vector3 vector = (Vector3)param;
			float num = 0.75f;
			vector += Vector3.right * (num * (float)((Core.Logic.Penitent.Status.Orientation == EntityOrientation.Right) ? 1 : (-1)));
			vector += Vector3.up * 0.1f;
			PlaySpecialFootStepFx(vector);
			sprintFX.EmitFeet(vector);
		}
	}

	private void SubscribePlayerEvents()
	{
		if ((bool)_penitent && !SuscribedToTriggerEvents)
		{
			SuscribedToTriggerEvents = true;
			Dash dash = _penitent.Dash;
			dash.OnFinishDash = (Core.SimpleEvent)Delegate.Combine(dash.OnFinishDash, new Core.SimpleEvent(OnFinishDash));
			LungeAttack lungeAttack = _penitent.LungeAttack;
			lungeAttack.OnLungeAttackStart = (Core.SimpleEvent)Delegate.Combine(lungeAttack.OnLungeAttackStart, new Core.SimpleEvent(OnLungeAttackStart));
			PenitentMoveAnimations penitentMoveAnimations = _penitent.PenitentMoveAnimations;
			penitentMoveAnimations.OnStep = (Core.SimpleEventParam)Delegate.Combine(penitentMoveAnimations.OnStep, new Core.SimpleEventParam(OnStep));
		}
	}

	private void UnsubscribePlayerEvents()
	{
		if ((bool)_penitent && SuscribedToTriggerEvents)
		{
			SuscribedToTriggerEvents = false;
			Dash dash = _penitent.Dash;
			dash.OnFinishDash = (Core.SimpleEvent)Delegate.Remove(dash.OnFinishDash, new Core.SimpleEvent(OnFinishDash));
			LungeAttack lungeAttack = _penitent.LungeAttack;
			lungeAttack.OnLungeAttackStart = (Core.SimpleEvent)Delegate.Remove(lungeAttack.OnLungeAttackStart, new Core.SimpleEvent(OnLungeAttackStart));
			PenitentMoveAnimations penitentMoveAnimations = _penitent.PenitentMoveAnimations;
			penitentMoveAnimations.OnStep = (Core.SimpleEventParam)Delegate.Remove(penitentMoveAnimations.OnStep, new Core.SimpleEventParam(OnStep));
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
		LevelManager.OnBeforeLevelLoad -= OnLevelPreload;
		UnsubscribePlayerEvents();
	}

	private void PlaySpecialFootStepFx(Vector2 position)
	{
		if (isEffectApplied && !string.IsNullOrEmpty(SpecialFoostepSound))
		{
			Core.Audio.EventOneShotPanned(SpecialFoostepSound, position);
		}
	}

	private void ActivationSound()
	{
		if (!prayerActivationSoundFx.IsNullOrWhitespace())
		{
			Core.Audio.PlayOneShot(prayerActivationSoundFx);
		}
	}

	private void StartUseLoopFx()
	{
		if (!_activationSoundLoopInstance.isValid() && !prayerUseLoopSoundFx.IsNullOrWhitespace())
		{
			_activationSoundLoopInstance = Core.Audio.CreateEvent(prayerUseLoopSoundFx);
			_activationSoundLoopInstance.start();
		}
	}

	private void StopUseLoopFx()
	{
		if (_activationSoundLoopInstance.isValid())
		{
			_activationSoundLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_activationSoundLoopInstance.release();
			_activationSoundLoopInstance = default(EventInstance);
		}
	}
}
