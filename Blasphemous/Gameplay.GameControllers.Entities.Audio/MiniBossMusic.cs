using System;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.Audio;

public class MiniBossMusic : MonoBehaviour
{
	[EventRef]
	public string SoundEvent;

	private string ParamName = "Ending";

	private EventInstance _eventInstance;

	public void PlayEvent()
	{
		StopEvent();
		if (!_eventInstance.isValid())
		{
			_eventInstance = Core.Audio.CreateEvent(SoundEvent);
			_eventInstance.start();
		}
	}

	public void StopEvent()
	{
		if (_eventInstance.isValid())
		{
			_eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_eventInstance.release();
			_eventInstance = default(EventInstance);
		}
	}

	public void SetParameter(float paramValue)
	{
		if (string.IsNullOrEmpty(ParamName) || !_eventInstance.isValid())
		{
			return;
		}
		paramValue = Mathf.Clamp01(paramValue);
		try
		{
			_eventInstance.getParameter(ParamName, out var instance);
			if (instance.isValid())
			{
				instance.setValue(paramValue);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message + ex.StackTrace);
			throw;
		}
	}

	private void OnDestroy()
	{
		StopEvent();
	}
}
