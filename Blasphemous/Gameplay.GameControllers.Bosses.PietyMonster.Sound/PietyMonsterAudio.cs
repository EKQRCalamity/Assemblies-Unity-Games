using System;
using System.Collections.Generic;
using FMOD.Studio;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Sound;

public class PietyMonsterAudio : EntityAudio
{
	public const string WalkEventKey = "PietatWalk";

	public const string TurnEventKey = "PietatTurn";

	public const string StopEventKey = "PietatStop";

	public const string StepEventKey = "PietatStep";

	public const string SlashEventKey = "PietatSlash";

	public const string StompEventKey = "PietatStomp";

	public const string SmashEventKey = "PietatSmash";

	public const string SmashScreamEventKey = "PietatSmashScream";

	public const string SmashGetUpEventKey = "PietatSmashGetUp";

	public const string ReadyToSpitEventKey = "PietatReadyToSpit";

	public const string SpitEventKey = "PietatSpit";

	public const string SpitExplosionEventKey = "PietatSpitExplosion";

	public const string SpitGrowEventKey = "PietatSpitGrow";

	public const string SpitHitEventKey = "PietatSpitHit";

	public const string SpitDestroyEventKey = "PietatSpitDestroyHit";

	public const string PietatRootAttackEventKey = "PietatRootAttack";

	public const string PietatStompHitEventKey = "PietatStompHit";

	public const string DeathEventKey = "PietatDeath";

	protected override void OnWake()
	{
		base.OnWake();
		EventInstances = new List<EventInstance>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (FloorCollider != null)
		{
			FloorSensorEmitter = FloorCollider.GetComponent<ICollisionEmitter>();
		}
		if (WeaponCollider != null)
		{
			WeaponSensorEmitter = WeaponCollider.GetComponent<ICollisionEmitter>();
		}
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

	public void PlayWalk()
	{
		PlayOneShotEvent("PietatWalk", FxSoundCategory.Motion);
	}

	public void PlayStepStomp()
	{
		PlayOneShotEvent("PietatStep", FxSoundCategory.Motion);
	}

	public void PlayTurn()
	{
		PlayOneShotEvent("PietatTurn", FxSoundCategory.Motion);
	}

	public void PlayStop()
	{
		PlayOneShotEvent("PietatStop", FxSoundCategory.Motion);
	}

	public void PlaySlash()
	{
		PlayOneShotEvent("PietatSlash", FxSoundCategory.Motion);
	}

	public void PlayStomp()
	{
		PlayOneShotEvent("PietatStomp", FxSoundCategory.Motion);
	}

	public void PlayDead()
	{
		PlayOneShotEvent("PietatDeath", FxSoundCategory.Damage);
	}

	public void PlayScream()
	{
		PlayOneShotEvent("PietatSmashScream", FxSoundCategory.Damage);
	}

	public void PlaySmash()
	{
		PlayOneShotEvent("PietatSmash", FxSoundCategory.Attack);
	}

	public void PlayStompHit()
	{
		PlayOneShotEvent("PietatStompHit", FxSoundCategory.Attack);
	}

	public void PlayGetUp()
	{
		PlayOneShotEvent("PietatSmashGetUp", FxSoundCategory.Attack);
	}

	public void ReadyToSpit()
	{
		PlayOneShotEvent("PietatReadyToSpit", FxSoundCategory.Attack);
	}

	public void Spit()
	{
		PlayOneShotEvent("PietatSpit", FxSoundCategory.Attack);
	}

	public void RootAttack()
	{
		PlayOneShotEvent("PietatRootAttack", FxSoundCategory.Attack);
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
}
