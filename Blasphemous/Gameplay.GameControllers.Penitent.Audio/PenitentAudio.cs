using System;
using System.Collections.Generic;
using FMOD.Studio;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Audio;

public class PenitentAudio : EntityAudio
{
	public const string ClimbCliffEventKey = "PenitentClimbCliff";

	public const string ClimbLadderEventKey = "PenitentClimbLadder";

	public const string DashEventKey = "PenitentDash";

	public const string FootStepEventKey = "PenitentFootStep";

	public const string HangCliffEventKey = "PenitentHangCliff";

	public const string JumpEventKey = "PenitentJump";

	public const string JumpOffEventKey = "PenitentJumpOff";

	public const string LandingForwardEventKey = "PenitentLandingForward";

	public const string LandingEventKey = "PenitentLanding";

	public const string RunStopEventKey = "PenitentRunStop";

	public const string SlidingLadderEventKey = "PenitentSlidingLadder";

	public const string SlidingLadderLandingEventKey = "PenitentSlidingLadderLanding";

	public const string StickToWallEventKey = "PenitentStickToWall";

	public const string UnHangFromWallEventKey = "PenitentUnHangFromWall";

	public const string HardLanding = "PenitentHardLanding";

	public const string IdleModeBlood = "PenitentIdleBlood";

	public const string IdleModeSword = "PenitentIdleSword";

	public const string StartDialogue = "PenitentStartTalk";

	public const string EndDialogue = "PenitentEndTalk";

	public const string BasicAttack1EventKey = "PenitentAttack1";

	public const string BasicAttack2EventKey = "PenitentAttack2";

	public const string BasicAirAttack1EventKey = "PenitentAirAttack1";

	public const string BasicAirAttack2EventKey = "PenitentAirAttack2";

	public const string PenitentHeavyAttackEventKey = "PenitentHeavyAttack";

	public const string LoadingChargedAttackEventKey = "PenitentLoadingChargedAttack";

	public const string LoadedChargedAttackEventKey = "PenitentLoadedChargedAttack";

	public const string ReleaseChargedAttackEventKey = "PenitentReleaseChargedAttack";

	public const string ParryEventKey = "PenitentParry";

	public const string ParryHitEventKey = "PenitentParryHit";

	public const string StartParryEventKey = "PenitentStartParry";

	public const string VerticalAttackStart = "PenitentVerticalAttackStart";

	public const string VerticalAttackFalling = "PenitentVerticalAttackFalling";

	public const string VerticalAttackLanding = "PenitentVerticalAttackLanding";

	public const string ComboHit = "PenitentComboHit";

	public const string ComboHitUp = "PenitentComboHitUp";

	public const string ComboHitDown = "PenitentComboHitDown";

	public const string RangeAttack = "PenitentRangeAttack";

	public const string RangeAttackDisappear = "PenitentRangeAttackDisappear";

	public const string RangeAttackHit = "PenitentRangeAttackHit";

	public const string RangeAttackFlight = "PenitentRangeAttackFlight";

	public const string SimpleEnemyHitEventKey = "PenitentSimpleEnemyHit";

	public const string PushBackEventKey = "PenitentPushBack";

	public const string HeavyEnemyHitEventKey = "PenitentHeavyEnemyHit";

	public const string CriticalEnemyHitEventKey = "PenitentCriticalEnemyHit";

	public const string SimpleEnemyDamageEventKey = "PenitentSimpleEnemyDamage";

	public const string HeavyEnemyDamageEventKey = "PenitentHeavyEnemyDamage";

	public const string DeathEventKey = "PenitentDeath";

	public const string DeathBySpikesEventKey = "PenitentDeathBySpike";

	public const string DeathByFallEventKey = "PenitentDeathByFall";

	public const string OverthrowEventKey = "PenitentOverthrow";

	public const string UseFlaskEventKey = "UseFlask";

	public const string EmptyFlaskEventKey = "EmptyFlask";

	public const string HealingExplosionEventKey = "HealingExplosion";

	public const string PenitentActivatePrayerEventKey = "PenitentActivatePrayer";

	public const string PrayerInvincibilityEventKey = "PenitentInvincibility";

	private EventInstance _dashEventInstance;

	private EventInstance _idleModeBlood;

	private EventInstance _chargedAttackEventInstance;

	private EventInstance _useFlaskEventInstance;

	private EventInstance _parryHitEventInstance;

	private EventInstance _prayerEventInstance;

	private void OnDestroy()
	{
		if (FloorSensorEmitter != null)
		{
			FloorSensorEmitter.OnEnter -= FloorSensorEmitterOnEnter;
			FloorSensorEmitter.OnExit -= FloorSensorListenerOnExit;
			FloorSensorEmitter.OnStay -= FloorSensorEmitterOnStay;
		}
		if (WeaponSensorEmitter != null)
		{
			WeaponSensorEmitter.OnStay -= WeaponSensorEmitterOnStay;
			WeaponSensorEmitter.OnExit -= WeaponSensorEmitterOnExit;
		}
		ReleaseAudioEvents();
	}

	protected override void OnWake()
	{
		base.OnWake();
		EventInstances = new List<EventInstance>();
		if (FloorCollider != null)
		{
			FloorSensorEmitter = FloorCollider.GetComponent<ICollisionEmitter>();
		}
		if (WeaponCollider != null)
		{
			WeaponSensorEmitter = WeaponCollider.GetComponent<ICollisionEmitter>();
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		try
		{
			FloorSensorEmitter.OnEnter += FloorSensorEmitterOnEnter;
			FloorSensorEmitter.OnExit += FloorSensorListenerOnExit;
			FloorSensorEmitter.OnStay += FloorSensorEmitterOnStay;
			WeaponSensorEmitter.OnStay += WeaponSensorEmitterOnStay;
			WeaponSensorEmitter.OnExit += WeaponSensorEmitterOnExit;
		}
		catch (NullReferenceException ex)
		{
			if (!InitilizationError)
			{
				InitilizationError = true;
			}
			Debug.LogWarning(ex.Message + ex.StackTrace);
		}
	}

	private void FloorSensorEmitterOnEnter(object sender, Collider2DParam collider2DParam)
	{
		if ((FloorLayerMask.value & (1 << collider2DParam.Collider2DArg.gameObject.layer)) > 0)
		{
			SetParameterValuesByFloor(collider2DParam.Collider2DArg);
		}
	}

	private void FloorSensorListenerOnExit(object sender, Collider2DParam collider2DParam)
	{
		if ((FloorLayerMask.value & (1 << collider2DParam.Collider2DArg.gameObject.layer)) > 0)
		{
		}
	}

	private void FloorSensorEmitterOnStay(object sender, Collider2DParam collider2DParam)
	{
		if ((FloorLayerMask.value & (1 << collider2DParam.Collider2DArg.gameObject.layer)) > 0)
		{
			SetParameterValuesByFloor(collider2DParam.Collider2DArg);
		}
	}

	private void WeaponSensorEmitterOnStay(object sender, Collider2DParam collider2DParam)
	{
		if ((WeaponLayerMask.value & (1 << collider2DParam.Collider2DArg.gameObject.layer)) > 0)
		{
			SetParametersValuesByEnemy(collider2DParam.Collider2DArg);
		}
	}

	private void WeaponSensorEmitterOnExit(object sender, Collider2DParam collider2DParam)
	{
		if ((WeaponLayerMask.value & (1 << collider2DParam.Collider2DArg.gameObject.layer)) > 0)
		{
		}
	}

	public void SetParametersValuesByWall(Collider2D material)
	{
		if (material.CompareTag("Material:Wood"))
		{
			WallWoodValue = 1f;
			WallStoneValue = 0f;
		}
		else if (material.CompareTag("Material:Stone"))
		{
			WallWoodValue = 0f;
			WallStoneValue = 1f;
		}
		else
		{
			WallWoodValue = 0f;
			WallStoneValue = 1f;
		}
	}

	private void SetParametersValuesByEnemy(Collider2D material)
	{
		if (material.CompareTag("Material:Flesh"))
		{
			FleshValue = 1f;
		}
	}

	private void SetParameterValuesByFloor(Collider2D material)
	{
		switch (material.tag)
		{
		case "Material:Dirt":
			DirtValue = 1f;
			StoneValue = 0f;
			SnowValue = 0f;
			WoodValue = 0f;
			MarbleValue = 0f;
			WaterValue = 0f;
			FleshValue = 0f;
			MetalValue = 0f;
			MudValue = 0f;
			SecretValue = 0f;
			GrassValue = 0f;
			DemakeValue = 0f;
			PalioValue = 0f;
			SnakeValue = 0f;
			break;
		case "Material:Stone":
			DirtValue = 0f;
			StoneValue = 1f;
			SnowValue = 0f;
			WoodValue = 0f;
			MarbleValue = 0f;
			WaterValue = 0f;
			FleshValue = 0f;
			MetalValue = 0f;
			MudValue = 0f;
			SecretValue = 0f;
			GrassValue = 0f;
			DemakeValue = 0f;
			PalioValue = 0f;
			SnakeValue = 0f;
			break;
		case "Material:Snow":
			DirtValue = 0f;
			StoneValue = 0f;
			SnowValue = 1f;
			WoodValue = 0f;
			MarbleValue = 0f;
			WaterValue = 0f;
			FleshValue = 0f;
			MetalValue = 0f;
			MudValue = 0f;
			SecretValue = 0f;
			GrassValue = 0f;
			DemakeValue = 0f;
			PalioValue = 0f;
			SnakeValue = 0f;
			break;
		case "Material:Wood":
			DirtValue = 0f;
			StoneValue = 0f;
			SnowValue = 0f;
			WoodValue = 1f;
			MarbleValue = 0f;
			WaterValue = 0f;
			FleshValue = 0f;
			MetalValue = 0f;
			MudValue = 0f;
			SecretValue = 0f;
			GrassValue = 0f;
			DemakeValue = 0f;
			PalioValue = 0f;
			SnakeValue = 0f;
			break;
		case "Material:Marble":
			DirtValue = 0f;
			StoneValue = 0f;
			SnowValue = 0f;
			WoodValue = 0f;
			MarbleValue = 1f;
			WaterValue = 0f;
			FleshValue = 0f;
			MetalValue = 0f;
			MudValue = 0f;
			SecretValue = 0f;
			GrassValue = 0f;
			DemakeValue = 0f;
			PalioValue = 0f;
			SnakeValue = 0f;
			break;
		case "Material:Water":
			DirtValue = 0f;
			StoneValue = 0f;
			SnowValue = 0f;
			WoodValue = 0f;
			MarbleValue = 0f;
			WaterValue = 1f;
			FleshValue = 0f;
			MetalValue = 0f;
			MudValue = 0f;
			SecretValue = 0f;
			GrassValue = 0f;
			DemakeValue = 0f;
			PalioValue = 0f;
			SnakeValue = 0f;
			break;
		case "Material:Flesh":
			DirtValue = 0f;
			StoneValue = 0f;
			SnowValue = 0f;
			WoodValue = 0f;
			MarbleValue = 0f;
			WaterValue = 0f;
			FleshValue = 1f;
			MetalValue = 0f;
			MudValue = 0f;
			SecretValue = 0f;
			GrassValue = 0f;
			DemakeValue = 0f;
			PalioValue = 0f;
			SnakeValue = 0f;
			break;
		case "Material:Metal":
			DirtValue = 0f;
			StoneValue = 0f;
			SnowValue = 0f;
			WoodValue = 0f;
			MarbleValue = 0f;
			WaterValue = 0f;
			FleshValue = 0f;
			MetalValue = 1f;
			MudValue = 0f;
			SecretValue = 0f;
			GrassValue = 0f;
			DemakeValue = 0f;
			PalioValue = 0f;
			SnakeValue = 0f;
			break;
		case "Material:Mud":
			DirtValue = 0f;
			StoneValue = 0f;
			SnowValue = 0f;
			WoodValue = 0f;
			MarbleValue = 0f;
			WaterValue = 0f;
			FleshValue = 0f;
			MetalValue = 0f;
			MudValue = 1f;
			SecretValue = 0f;
			GrassValue = 0f;
			DemakeValue = 0f;
			PalioValue = 0f;
			SnakeValue = 0f;
			break;
		case "Material:Secret":
			DirtValue = 0f;
			StoneValue = 0f;
			SnowValue = 0f;
			WoodValue = 0f;
			MarbleValue = 0f;
			WaterValue = 0f;
			FleshValue = 0f;
			MetalValue = 0f;
			MudValue = 0f;
			SecretValue = 1f;
			GrassValue = 0f;
			DemakeValue = 0f;
			PalioValue = 0f;
			SnakeValue = 0f;
			break;
		case "Material:Grass":
			DirtValue = 0f;
			StoneValue = 0f;
			SnowValue = 0f;
			WoodValue = 0f;
			MarbleValue = 0f;
			WaterValue = 0f;
			FleshValue = 0f;
			MetalValue = 0f;
			MudValue = 0f;
			SecretValue = 0f;
			GrassValue = 1f;
			DemakeValue = 0f;
			PalioValue = 0f;
			SnakeValue = 0f;
			break;
		case "Material:Demake":
			DirtValue = 0f;
			StoneValue = 0f;
			SnowValue = 0f;
			WoodValue = 0f;
			MarbleValue = 0f;
			WaterValue = 0f;
			FleshValue = 0f;
			MetalValue = 0f;
			MudValue = 0f;
			SecretValue = 0f;
			GrassValue = 0f;
			DemakeValue = 1f;
			PalioValue = 0f;
			SnakeValue = 0f;
			break;
		case "Material:Palio":
			DirtValue = 0f;
			StoneValue = 0f;
			SnowValue = 0f;
			WoodValue = 0f;
			MarbleValue = 0f;
			WaterValue = 0f;
			FleshValue = 0f;
			MetalValue = 0f;
			MudValue = 0f;
			SecretValue = 0f;
			GrassValue = 0f;
			DemakeValue = 0f;
			PalioValue = 1f;
			SnakeValue = 0f;
			break;
		case "Material:Snake":
			DirtValue = 0f;
			StoneValue = 0f;
			SnowValue = 0f;
			WoodValue = 0f;
			MarbleValue = 0f;
			WaterValue = 0f;
			FleshValue = 0f;
			MetalValue = 0f;
			MudValue = 0f;
			SecretValue = 0f;
			GrassValue = 0f;
			DemakeValue = 0f;
			PalioValue = 0f;
			SnakeValue = 1f;
			break;
		default:
			DirtValue = 0f;
			StoneValue = 1f;
			SnowValue = 0f;
			WoodValue = 0f;
			MarbleValue = 0f;
			WaterValue = 0f;
			FleshValue = 0f;
			MetalValue = 0f;
			MudValue = 0f;
			SecretValue = 0f;
			GrassValue = 0f;
			DemakeValue = 0f;
			PalioValue = 0f;
			SnakeValue = 0f;
			break;
		}
	}

	public void PlayFootStep()
	{
		PlayOneShotEvent("PenitentFootStep", FxSoundCategory.Motion);
	}

	public void PlayJumpSound()
	{
		if (Core.Logic.CurrentState != 0)
		{
			PlayOneShotEvent("PenitentJump", FxSoundCategory.Motion);
		}
	}

	public void PlayRunStopSound()
	{
		if (Core.Logic.CurrentState != 0)
		{
			PlayOneShotEvent("PenitentRunStop", FxSoundCategory.Motion);
		}
	}

	public void PlayDashSound()
	{
		StopDashSound();
		PlayEvent(ref _dashEventInstance, "PenitentDash");
	}

	public void StopDashSound()
	{
		StopEvent(ref _dashEventInstance);
	}

	public void PlayLandingSound()
	{
		if (Core.Logic.CurrentState != 0)
		{
			PlayOneShotEvent("PenitentLanding", FxSoundCategory.Motion);
		}
	}

	public void PlayLandingForward()
	{
		PlayOneShotEvent("PenitentLandingForward", FxSoundCategory.Motion);
	}

	public void ClimbLadder()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentClimbLadder");
		}
	}

	public void GrabCliffLede()
	{
		PlayOneShotEvent("PenitentHangCliff", FxSoundCategory.Motion);
	}

	public void ClimbCliffLede()
	{
		PlayOneShotEvent("PenitentClimbCliff", FxSoundCategory.Motion);
	}

	public void JumpOff()
	{
		PlayOneShotEvent("PenitentJumpOff", FxSoundCategory.Motion);
	}

	public void SlidingLadder(out EventInstance eI)
	{
		EventInstance eventInstance = base.AudioManager.CreateCatalogEvent("PenitentSlidingLadder");
		if (eventInstance.isValid() && !base.Mute)
		{
			eventInstance.start();
		}
		eI = eventInstance;
	}

	public void StopSlidingLadder(EventInstance eventInstance)
	{
		if (eventInstance.isValid())
		{
			eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			eventInstance.release();
		}
	}

	public void SlidingLadderLanding()
	{
		PlayOneShotEvent("PenitentSlidingLadderLanding", FxSoundCategory.Motion);
	}

	public void PlayIdleModeBlood()
	{
		PlayEvent(ref _idleModeBlood, "PenitentIdleBlood");
	}

	public void StopIdleModeBlood()
	{
		StopEvent(ref _idleModeBlood);
	}

	public void PlayIdleModeSword()
	{
		PlayOneShotEvent("PenitentIdleSword", FxSoundCategory.Motion);
	}

	public void PlayStartDialogue()
	{
		PlayOneShotEvent("PenitentStartTalk", FxSoundCategory.Motion);
	}

	public void PlayEndDialogue()
	{
		PlayOneShotEvent("PenitentEndTalk", FxSoundCategory.Motion);
	}

	public void PlayRangeAttack()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentRangeAttack");
		}
	}

	public void PlayRangeAttackHit()
	{
		PlayOneShotEvent("PenitentRangeAttackHit", FxSoundCategory.Attack);
	}

	public void PlayBasicAttack1()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentAttack1");
		}
	}

	public void PlayBasicAttack2()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentAttack2");
		}
	}

	public void PlayHeavyAttack()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentHeavyAttack");
		}
	}

	public void PlayAirAttack1()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentAirAttack1");
		}
	}

	public void PlayAirAttack2()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentAirAttack2");
		}
	}

	public void PlayLoadingChargedAttack()
	{
		if (!_chargedAttackEventInstance.isValid() && !base.Mute)
		{
			_chargedAttackEventInstance = base.AudioManager.CreateCatalogEvent("PenitentLoadingChargedAttack");
			_chargedAttackEventInstance.start();
			_chargedAttackEventInstance.release();
		}
	}

	public void StopLoadingChargedAttack()
	{
		if (!(!_chargedAttackEventInstance.isValid() | base.Mute))
		{
			_chargedAttackEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_chargedAttackEventInstance.release();
			_chargedAttackEventInstance = default(EventInstance);
		}
	}

	public void UseFlask()
	{
		if (!_useFlaskEventInstance.isValid() && !base.Mute)
		{
			_useFlaskEventInstance = base.AudioManager.CreateCatalogEvent("UseFlask");
			_useFlaskEventInstance.start();
		}
	}

	public void StopUseFlask()
	{
		if (_useFlaskEventInstance.isValid() && !base.Mute)
		{
			_useFlaskEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_useFlaskEventInstance.release();
			_useFlaskEventInstance = default(EventInstance);
		}
	}

	public void EmptyFlask()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("EmptyFlask");
		}
	}

	public void HealingExplosion()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("HealingExplosion");
		}
	}

	public void PlayLoadedChargedAttack()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentLoadedChargedAttack");
		}
	}

	public void PlayReleaseChargedAttack()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentReleaseChargedAttack");
		}
	}

	public void PlayStartParry()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentStartParry");
		}
	}

	public void PlayParryHit()
	{
		StopParryFx();
		PlayEvent(ref _parryHitEventInstance, "PenitentParryHit");
	}

	public void StopParryFx()
	{
		StopEvent(ref _parryHitEventInstance);
	}

	public void PlayParryAttack()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentParry");
		}
	}

	public void PlaySimpleHitToEnemy()
	{
		PlayOneShotEvent("PenitentSimpleEnemyHit", FxSoundCategory.Attack);
	}

	public void PlayHeavyHitToEnemy()
	{
		PlayOneShotEvent("PenitentHeavyEnemyHit", FxSoundCategory.Attack);
	}

	public void PlayComboHit()
	{
		PlayOneShotEvent("PenitentComboHit", FxSoundCategory.Attack);
	}

	public void PlayComboHitUp()
	{
		PlayOneShotEvent("PenitentComboHitUp", FxSoundCategory.Attack);
	}

	public void PlayComboHitDown()
	{
		PlayOneShotEvent("PenitentComboHitDown", FxSoundCategory.Attack);
	}

	public void PlayDeath()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentDeath");
		}
	}

	public void PlayDeathSpikes()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentDeathBySpike");
		}
	}

	public void PlayDeathFall()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentDeathByFall");
		}
	}

	public void PlayOverthrow()
	{
		if (base.AudioManager != null && Owner.SpriteRenderer.isVisible && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentOverthrow");
		}
	}

	public void PlayPushBack()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentPushBack");
		}
	}

	public void PlaySimpleDamage()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentSimpleEnemyDamage");
		}
	}

	public void PlayHeavyDamage()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentHeavyEnemyDamage");
		}
	}

	public void PlayStickToWall()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			PlayOneShotEvent("PenitentStickToWall", FxSoundCategory.Climb);
		}
	}

	public void PlayUnHangFromWall()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentUnHangFromWall");
		}
	}

	public void PlayHardLanding()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentHardLanding");
		}
	}

	public void PlayVerticalAttackStart()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentVerticalAttackStart");
		}
	}

	public void PlayVerticalAttackFalling()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentVerticalAttackFalling");
		}
	}

	public void PlayVerticalAttackLanding()
	{
		if (!base.Mute)
		{
			string getLandingFxEventKey = Core.Logic.Penitent.VerticalAttack.GetLandingFxEventKey;
			Core.Audio.EventOneShotPanned(getLandingFxEventKey, base.transform.position);
		}
	}

	public void PlayFinishingComboDown()
	{
		if (!base.Mute)
		{
			Core.Audio.EventOneShotPanned(Core.Logic.Penitent.VerticalAttack.VerticalLandingFxLevel3, base.transform.position);
		}
	}

	public void ActivatePrayer()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			StopPrayerCast();
			PlayEvent(ref _prayerEventInstance, "PenitentActivatePrayer");
		}
	}

	public void StopPrayerCast()
	{
		StopEvent(ref _prayerEventInstance);
	}

	public void PrayerInvincibility()
	{
		if (base.AudioManager != null && !base.Mute)
		{
			base.AudioManager.PlaySfxOnCatalog("PenitentInvincibility");
		}
	}
}
