using System;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Runner.Audio;

public class RunnerAudio : EntityAudio
{
	private const string DeathEventKey = "RunnerDeath";

	private const string RunEventKey = "RunnerRun";

	private const string SlideEventKey = "RunnerSlide";

	[EventRef]
	[SerializeField]
	private string ScreamEventKey;

	private const string MoveParameterKey = "Moves";

	private EventInstance _screamEventInstance;

	public void PlayDeath()
	{
		StopScream();
		PlayOneShotEvent("RunnerDeath", FxSoundCategory.Damage);
	}

	public void PlayRun()
	{
		PlayOneShotEvent("RunnerRun", FxSoundCategory.Motion);
	}

	public void PlaySlide()
	{
		PlayOneShotEvent("RunnerSlide", FxSoundCategory.Motion);
	}

	public void PlayScream()
	{
		StopScream();
		Core.Audio.PlayEventNoCatalog(ref _screamEventInstance, ScreamEventKey);
	}

	public void StopScream()
	{
		StopEvent(ref _screamEventInstance);
	}

	public void SetScreamParam(float value)
	{
		SetMoveParam(_screamEventInstance, value);
	}

	private void SetMoveParam(EventInstance eventInstance, float value)
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
