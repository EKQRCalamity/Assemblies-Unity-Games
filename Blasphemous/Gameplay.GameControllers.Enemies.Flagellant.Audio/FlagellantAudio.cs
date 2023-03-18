using System;
using System.Collections.Generic;
using FMOD.Studio;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Flagellant.Audio;

public class FlagellantAudio : EntityAudio
{
	public const string FootStepEventKey = "FlagellantFootStep";

	public const string RunnigEventKey = "FlagellantRunning";

	public const string LandingEventKey = "FlagellantLanding";

	public const string BasicAttackEventKey = "FlagellantBasicAttack";

	public const string BasicAttackHitEventKey = "FlagellantAttackHit";

	public const string SelfLashEventKey = "FlagellantSelfLash";

	public const string BloodDecalEventKey = "FlagellantBloodDecal";

	public const string DeathEventKey = "FlagellantDeath";

	public const string OverthrowEventKey = "FlagellantOverthrow";

	public const string VaporizationDeathEventKey = "FlagellantVaporizationDeath";

	private EventInstance _basicAttackEventInstance;

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
		catch
		{
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
		Owner.OnDeath += OwnerOnDeath;
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

	private void OwnerOnDeath()
	{
		DeathValue = 1f;
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

	public void PlayFootStep()
	{
		PlayOneShotEvent("FlagellantFootStep", FxSoundCategory.Motion);
	}

	public void PlayRunning()
	{
		PlayOneShotEvent("FlagellantRunning", FxSoundCategory.Motion);
	}

	public void PlayLandingSound()
	{
		PlayOneShotEvent("FlagellantLanding", FxSoundCategory.Motion);
	}

	public void PlayBasicAttack()
	{
		PlayOneShotEvent("FlagellantBasicAttack", FxSoundCategory.Attack);
	}

	public void PlayAttackHit()
	{
		PlayOneShotEvent("FlagellantAttackHit", FxSoundCategory.Attack);
	}

	public void PlaySelfLash()
	{
		if (base.AudioManager != null)
		{
			base.AudioManager.PlaySfxOnCatalog("FlagellantSelfLash");
		}
	}

	public void PlayBloodDecal()
	{
		if (base.AudioManager != null)
		{
			base.AudioManager.PlaySfxOnCatalog("FlagellantBloodDecal");
		}
	}

	public void PlayDeath()
	{
		if (base.AudioManager != null)
		{
			base.AudioManager.PlaySfxOnCatalog("FlagellantDeath");
		}
	}

	public void PlayOverThrow()
	{
		if (base.AudioManager != null)
		{
			base.AudioManager.PlaySfxOnCatalog("FlagellantOverthrow");
		}
	}

	public void PlayVaporizationDeath()
	{
		if (base.AudioManager != null)
		{
			base.AudioManager.PlaySfxOnCatalog("FlagellantVaporizationDeath");
		}
	}
}
