using System;
using System.Collections.Generic;
using FMOD.Studio;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Acolyte.Audio;

public class AcolyteAudio : EntityAudio
{
	public const string FootstepEventKey = "AcolyteFootstep";

	public const string RunningEventKey = "AcolyteRunning";

	public const string StopRunningEventKey = "AcolyteStopRunning";

	public const string LandingEventKey = "AcolyteLanding";

	public const string ChargeAttackEventKey = "AcolyteChargeAttack";

	public const string ReleaseAttackEventKey = "AcolyteReleaseAttack";

	public const string BloodDecalEventKey = "AcolyteBloodDecal";

	public const string DeathEventKey = "AcolyteDeath";

	public const string DeathOnCliffLedeEventKey = "AcolyteDeathOnCliffLede";

	public const string OverThrowEventKey = "AcolyteOverthrow";

	public const string VaporizationDeathEventKey = "AcolyteVaporizationDeath";

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

	public void PlayFootStep()
	{
		PlayOneShotEvent("AcolyteFootstep", FxSoundCategory.Motion);
	}

	public void PlayRunning()
	{
		PlayOneShotEvent("AcolyteRunning", FxSoundCategory.Motion);
	}

	public void PlayStopRunning()
	{
		PlayOneShotEvent("AcolyteStopRunning", FxSoundCategory.Motion);
	}

	public void PlayLanding()
	{
		PlayOneShotEvent("AcolyteLanding", FxSoundCategory.Motion);
	}

	public void PlayChargeAttack()
	{
		PlayOneShotEvent("AcolyteChargeAttack", FxSoundCategory.Motion);
	}

	public void PlayReleaseAttack()
	{
		PlayOneShotEvent("AcolyteReleaseAttack", FxSoundCategory.Motion);
	}

	public void PlayBloodDecal()
	{
		PlayOneShotEvent("AcolyteBloodDecal", FxSoundCategory.Damage);
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("AcolyteDeath", FxSoundCategory.Damage);
	}

	public void PlayDeathOnCliffLede()
	{
		PlayOneShotEvent("AcolyteDeathOnCliffLede", FxSoundCategory.Damage);
	}

	public void PlayOverthrow()
	{
		PlayOneShotEvent("AcolyteOverthrow", FxSoundCategory.Damage);
	}

	public void PlayVaporizationDeath()
	{
		PlayOneShotEvent("AcolyteVaporizationDeath", FxSoundCategory.Damage);
	}
}
