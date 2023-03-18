using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.CowardTrapper.Audio;

public class CowardTrapperAudio : EntityAudio
{
	private const string IdleEventKey = "CowardTrapperIdle";

	private const string RunEventKey = "CowardTrapperRun";

	private const string DeathEventKey = "CowardTrapperDeath";

	private const string MoveParameterKey = "Moves";

	private EventInstance _idleEventInstance;

	public void PlayIdle()
	{
		StopIdle();
		PlayEvent(ref _idleEventInstance, "CowardTrapperIdle");
	}

	public void StopIdle()
	{
		StopEvent(ref _idleEventInstance);
	}

	public void PlayRun()
	{
		PlayOneShotEvent("CowardTrapperRun", FxSoundCategory.Motion);
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("CowardTrapperDeath", FxSoundCategory.Damage);
	}

	public void SetIdleParam(float value)
	{
		SetMoveParam(_idleEventInstance, value);
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
