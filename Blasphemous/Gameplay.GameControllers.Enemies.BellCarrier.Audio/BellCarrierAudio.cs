using System;
using System.Collections.Generic;
using FMOD.Studio;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.BellCarrier.Animator;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.BellCarrier.Audio;

public class BellCarrierAudio : EntityAudio
{
	private BellCarrierAnimatorInyector _animatorInyector;

	public const string RunEventKey = "BellCarrierRun";

	public const string RunStopEventKey = "BellCarrierRunStop";

	public const string StartToRunEventKey = "BellCarrierStartToRun";

	public const string TurnAroundEventKey = "BellCarrierTurnAround";

	public const string TurnAroundRunEventKey = "BellCarrierTurnAroundRun";

	public const string DropBellEventKey = "BellCarrierDropBell";

	public const string WakeUpEventKey = "BellCarrierWakeUp";

	public const string FrontHitEventKey = "BellCarrierFrontHit";

	public const string DeathEventKey = "BellCarrierDeath";

	public const string WallCrashEventKey = "BellCarrierWallCrush";

	private void SetFloorMaterialParams(EventInstance eventInstance)
	{
		try
		{
			eventInstance.getParameter("Dirt", out var instance);
			instance.setValue(DirtValue);
			eventInstance.getParameter("Snow", out var instance2);
			instance2.setValue(SnowValue);
			eventInstance.getParameter("Stone", out var instance3);
			instance3.setValue(StoneValue);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message + ex.StackTrace);
		}
	}

	private void OnDestroy()
	{
		if (FloorSensorEmitter != null)
		{
			FloorSensorEmitter.OnStay -= FloorSensorListenerOnStay;
			FloorSensorEmitter.OnExit -= FloorSensorListenerOnExit;
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
			FloorSensorEmitter.OnStay += FloorSensorListenerOnStay;
			FloorSensorEmitter.OnExit += FloorSensorListenerOnExit;
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
		_animatorInyector = Owner.GetComponentInChildren<BellCarrierAnimatorInyector>();
	}

	private void WeaponSensorEmitterOnExit(object sender, Collider2DParam e)
	{
		if ((WeaponLayerMask.value & (1 << e.Collider2DArg.gameObject.layer)) > 0)
		{
		}
	}

	private void WeaponSensorEmitterOnStay(object sender, Collider2DParam e)
	{
		if ((WeaponLayerMask.value & (1 << e.Collider2DArg.gameObject.layer)) > 0)
		{
			SetParametersValuesByEnemy(e.Collider2DArg);
		}
	}

	private void FloorSensorListenerOnExit(object sender, Collider2DParam e)
	{
		if ((FloorLayerMask.value & (1 << e.Collider2DArg.gameObject.layer)) > 0)
		{
		}
	}

	private void FloorSensorListenerOnStay(object sender, Collider2DParam e)
	{
		if ((FloorLayerMask.value & (1 << e.Collider2DArg.gameObject.layer)) > 0)
		{
			SetParameterValuesByFloor(e.Collider2DArg);
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
			break;
		case "Material:Stone":
			DirtValue = 0f;
			StoneValue = 1f;
			SnowValue = 0f;
			break;
		case "Material:Snow":
			DirtValue = 0f;
			StoneValue = 0f;
			SnowValue = 1f;
			break;
		default:
			DirtValue = 0f;
			StoneValue = 1f;
			SnowValue = 0f;
			break;
		}
	}

	public void PlayRun()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("BellCarrierRun", FxSoundCategory.Motion);
		}
	}

	public void DropBell()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("BellCarrierDropBell", FxSoundCategory.Motion);
		}
	}

	public void PlayRunStop()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("BellCarrierRunStop", FxSoundCategory.Motion);
		}
	}

	public void PlayStartToRun()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("BellCarrierStartToRun", FxSoundCategory.Motion);
		}
	}

	public void PlayTurnAround()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("BellCarrierTurnAround", FxSoundCategory.Motion);
		}
	}

	public void PlayTurnAroundRun()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("BellCarrierTurnAroundRun", FxSoundCategory.Motion);
		}
	}

	public void PlayBellCarrierWallCrush()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("BellCarrierWallCrush", FxSoundCategory.Motion);
		}
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("BellCarrierDeath", FxSoundCategory.Motion);
	}

	public void PlayWakeUp()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("BellCarrierWakeUp", FxSoundCategory.Motion);
		}
	}

	public void PlayDamageSound()
	{
		Hit lastHit = Owner.EntityDamageArea.LastHit;
		if (_animatorInyector.CurrentDamageSoundType == BellCarrierAnimatorInyector.DamageSoundType.AfterTurnAround)
		{
			PlayBellCarrierFrontHit();
		}
		else
		{
			PlayDamageSoundByHit(lastHit);
		}
	}

	public void PlayBellCarrierFrontHit()
	{
		PlayOneShotEvent("BellCarrierFrontHit", FxSoundCategory.Motion);
	}

	public void PlayDamageSoundByHit(Hit hit)
	{
		Core.Audio.EventOneShotPanned(hit.HitSoundId, Owner.transform.position);
	}
}
