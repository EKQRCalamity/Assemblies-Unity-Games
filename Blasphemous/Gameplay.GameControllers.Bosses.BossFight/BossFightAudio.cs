using System;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BossFight;

public class BossFightAudio : MonoBehaviour
{
	[EventRef]
	public string BossTrackId;

	private EventInstance _bossMusicInstance;

	public event Action OnBossMusicStarts;

	public EventInstance GetCurrentMusicInstance()
	{
		return _bossMusicInstance;
	}

	private void Start()
	{
		_bossMusicInstance = GetEventInstanceByKey(BossTrackId);
	}

	[Button(ButtonSizes.Small)]
	public void PlayBossTrack()
	{
		if (_bossMusicInstance.isValid())
		{
			if (this.OnBossMusicStarts != null)
			{
				this.OnBossMusicStarts();
			}
			_bossMusicInstance.start();
		}
	}

	public void StopBossTrack()
	{
		if (_bossMusicInstance.isValid())
		{
			_bossMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_bossMusicInstance.release();
		}
	}

	public void SetBossTrackState(float paramValue)
	{
		if (_bossMusicInstance.isValid())
		{
			_bossMusicInstance.getParameter("Intensity", out var instance);
			float value = Mathf.Clamp(paramValue, 0f, 100f);
			if (instance.isValid())
			{
				instance.setValue(value);
			}
		}
	}

	public void SetBossTrackParam(string paramName, float paramValue)
	{
		if (_bossMusicInstance.isValid())
		{
			_bossMusicInstance.getParameter(paramName, out var instance);
			if (instance.isValid())
			{
				instance.setValue(paramValue);
			}
		}
	}

	public ParameterInstance GetBossTrackParam(string paramName)
	{
		ParameterInstance instance = default(ParameterInstance);
		if (_bossMusicInstance.isValid())
		{
			_bossMusicInstance.getParameter(paramName, out instance);
		}
		return instance;
	}

	public void SetBossEndingMusic(float paramValue)
	{
		if (_bossMusicInstance.isValid())
		{
			_bossMusicInstance.getParameter("Ending", out var instance);
			if (instance.isValid())
			{
				instance.setValue(Mathf.Clamp01(paramValue));
			}
		}
	}

	private EventInstance GetEventInstanceByKey(string eventInstanceId)
	{
		EventInstance result = default(EventInstance);
		if (!string.IsNullOrEmpty(eventInstanceId))
		{
			return Core.Audio.CreateEvent(eventInstanceId);
		}
		return result;
	}

	private void OnDestroy()
	{
		StopBossTrack();
	}
}
