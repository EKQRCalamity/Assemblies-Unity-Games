using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Processioner.Audio;

public class ProcesionerAudio : EntityAudio
{
	private const string WalkEventKey = "ProcessionerFootsteps";

	private const string AttackEventKey = "ProcessionerAttack";

	private const string DeathEventKey = "ProcessionerDeath";

	private const string ChargeLoop = "FireChargeLoop";

	private EventInstance _attackEventInstance;

	private EventInstance _chargeLoopEventInstance;

	private const string MoveParameterKey = "Moves";

	public void PlayAttack()
	{
		StopAttack();
		PlayEvent(ref _attackEventInstance, "ProcessionerAttack");
	}

	public void StopAttack()
	{
		StopEvent(ref _attackEventInstance);
	}

	public void PlayWalk()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("ProcessionerFootsteps", FxSoundCategory.Motion);
		}
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("ProcessionerDeath", FxSoundCategory.Damage);
	}

	public void SetAttackParam(float value)
	{
		SetMoveParam(_attackEventInstance, value);
	}

	public void StartChargeLoop()
	{
		PlayEvent(ref _chargeLoopEventInstance, "FireChargeLoop");
	}

	public void StopChargeLoop()
	{
		StopEvent(ref _chargeLoopEventInstance);
	}

	public void SetMoveParam(EventInstance eventInstance, float value)
	{
		try
		{
			eventInstance.getParameter("Moves", out var instance);
			instance.setValue(value);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message + ex.StackTrace);
		}
	}
}
