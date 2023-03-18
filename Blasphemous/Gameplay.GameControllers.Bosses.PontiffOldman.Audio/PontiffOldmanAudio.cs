using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffOldman.Audio;

public class PontiffOldmanAudio : EntityAudio
{
	private const string PontiffOldman_DEATH = "PontiffOldmanDeath";

	private const string PontiffOldman_TELEPORT_IN = "PontiffOldmanTeleportIn";

	private const string PontiffOldman_TELEPORT_OUT = "PontiffOldmanTeleportOut";

	private const string PontiffOldman_CAST = "PontiffOldmanCast";

	private const string PontiffOldman_CAST_LOOP = "PontiffOldmanCastLoop";

	private const string PontiffOldman_WIND = "PontiffOldmanWindLoop";

	private const string EndParamKey = "End";

	private EventInstance _castEventInstance;

	private EventInstance _windEventInstance;

	public void PlayDeath_AUDIO()
	{
		StopAll();
		PlayOneShotEvent("PontiffOldmanDeath", FxSoundCategory.Damage);
	}

	public void StopAll()
	{
		StopEvent(ref _castEventInstance);
		StopWind_AUDIO();
	}

	public void PlayTeleportIn_AUDIO()
	{
		PlayOneShotEvent("PontiffOldmanTeleportIn", FxSoundCategory.Motion);
	}

	public void PlayTeleportOut_AUDIO()
	{
		PlayOneShotEvent("PontiffOldmanTeleportOut", FxSoundCategory.Motion);
	}

	public void PlayStartCast_AUDIO()
	{
		PlayOneShotEvent("PontiffOldmanCast", FxSoundCategory.Attack);
	}

	public void PlayStartCastLoop_AUDIO()
	{
		StopEvent(ref _castEventInstance);
		PlayEvent(ref _castEventInstance, "PontiffOldmanCastLoop");
	}

	public void PlayStopCastLoop_AUDIO()
	{
		SetParam(_castEventInstance, "End", 1f);
	}

	public void PlayWind_AUDIO()
	{
		StopEvent(ref _windEventInstance);
		PlayEvent(ref _windEventInstance, "PontiffOldmanWindLoop");
	}

	public void StopWind_AUDIO()
	{
		SetParam(_windEventInstance, "End", 1f);
	}

	public void SetParam(EventInstance eventInstance, string paramKey, float value)
	{
		try
		{
			eventInstance.getParameter(paramKey, out var instance);
			instance.setValue(value);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message + ex.StackTrace);
		}
	}

	private void OnDestroy()
	{
		StopEvent(ref _windEventInstance);
		StopEvent(ref _castEventInstance);
	}
}
