using System.Collections.Generic;
using FMOD.Studio;
using Framework.Managers;
using UnityEngine;

namespace Tools.Audio;

public class AmbientMusicSettings
{
	private class AudioParamInitializedClass
	{
		public float enterValue;

		public float exitValue;

		public float currentValue;

		public AudioParamInitializedClass(float enter, float exit)
		{
			enterValue = enter;
			exitValue = exit;
			currentValue = 0f;
		}
	}

	private enum AmbientStatus
	{
		Start,
		End
	}

	private EventInstance audioInstance;

	private EventInstance reverbInstance;

	private string trackIdentifier;

	private string reverbIdentifier;

	private string reverbIdentifierPrevious;

	private float volume;

	private Dictionary<string, float> sceneParams;

	private Dictionary<string, Dictionary<string, AudioParamInitializedClass>> modifierParams;

	private Dictionary<string, AudioParamInitializedClass> ambientParams;

	private bool ambientRunning;

	private float ambientCurrentTime;

	private float ambientStartTime;

	private float ambientEndTime;

	private AmbientStatus ambientNextStatus;

	private Dictionary<string, float> areaModifiers;

	private float posYGUI;

	public EventInstance AudioInstance => audioInstance;

	public float Volume
	{
		get
		{
			if (audioInstance.isValid())
			{
				audioInstance.getVolume(out volume, out var _);
			}
			return volume;
		}
		set
		{
			volume = Mathf.Clamp01(value);
			if (audioInstance.isValid())
			{
				audioInstance.setVolume(volume);
			}
		}
	}

	public AmbientMusicSettings()
	{
		audioInstance = default(EventInstance);
		trackIdentifier = string.Empty;
		reverbInstance = default(EventInstance);
		reverbIdentifier = string.Empty;
		reverbIdentifierPrevious = string.Empty;
		volume = 1f;
		ambientRunning = false;
		ambientCurrentTime = 0f;
		ambientStartTime = 0f;
		ambientEndTime = 0f;
		ambientNextStatus = AmbientStatus.Start;
		sceneParams = new Dictionary<string, float>();
		ambientParams = new Dictionary<string, AudioParamInitializedClass>();
		modifierParams = new Dictionary<string, Dictionary<string, AudioParamInitializedClass>>();
		areaModifiers = new Dictionary<string, float>();
	}

	public void SetSceneParams(string idTrack, string idReverb, AudioParam[] globalParameters, string newLevelName = "")
	{
		Debug.Log("** NEW Scene Audio: PlayTrack " + idTrack + " reverb " + idReverb + ",volume " + volume);
		if (idTrack != trackIdentifier)
		{
			if (audioInstance.isValid() && !newLevelName.Equals("D07Z01S04"))
			{
				StopCurrent();
				audioInstance = default(EventInstance);
			}
			trackIdentifier = idTrack;
			if (idTrack != string.Empty)
			{
				audioInstance = Core.Audio.CreateEvent(idTrack);
				audioInstance.start();
				SetAudioInstanceVolume();
			}
		}
		SetReverb(idReverb);
		reverbIdentifierPrevious = string.Empty;
		for (int i = 0; i < globalParameters.Length; i++)
		{
			audioInstance.setParameterValue(globalParameters[i].name, globalParameters[i].targetValue);
			sceneParams[globalParameters[i].name] = globalParameters[i].targetValue;
		}
	}

	public void SetSceneParam(AudioParam parameter)
	{
		if (audioInstance.isValid())
		{
			audioInstance.setParameterValue(parameter.name, parameter.targetValue);
			sceneParams[parameter.name] = parameter.targetValue;
		}
	}

	public void SetSceneParam(string key, float value)
	{
		AudioParam sceneParam = default(AudioParam);
		sceneParam.name = key;
		sceneParam.targetValue = value;
		SetSceneParam(sceneParam);
	}

	public void SetAmbientParams(AudioParamInitialized[] parameters, float startTime, float endTime)
	{
		ambientRunning = parameters.Length > 0;
		if (!ambientRunning)
		{
			return;
		}
		ambientStartTime = startTime;
		ambientEndTime = endTime;
		for (int i = 0; i < parameters.Length; i++)
		{
			AudioParamInitialized audioParamInitialized = parameters[i];
			float currentValue = 0f;
			if (ambientParams.ContainsKey(audioParamInitialized.name))
			{
				currentValue = ambientParams[audioParamInitialized.name].currentValue;
			}
			ambientParams[audioParamInitialized.name] = new AudioParamInitializedClass(audioParamInitialized.enterValue, audioParamInitialized.exitValue);
			ambientParams[audioParamInitialized.name].currentValue = currentValue;
		}
	}

	public void AddAreaModifier(string name, float value)
	{
		if (value == 0f && areaModifiers.ContainsKey(name))
		{
			areaModifiers.Remove(name);
		}
		else
		{
			areaModifiers[name] = value;
		}
	}

	public void StartModifierParams(string name, string newReverb, AudioParamInitialized[] parameters)
	{
		Dictionary<string, AudioParamInitializedClass> dictionary = new Dictionary<string, AudioParamInitializedClass>();
		for (int i = 0; i < parameters.Length; i++)
		{
			AudioParamInitialized audioParamInitialized = parameters[i];
			dictionary[audioParamInitialized.name] = new AudioParamInitializedClass(audioParamInitialized.enterValue, audioParamInitialized.exitValue);
			dictionary[audioParamInitialized.name].currentValue = audioParamInitialized.enterValue;
			audioInstance.setParameterValue(audioParamInitialized.name, audioParamInitialized.enterValue);
		}
		modifierParams[name] = dictionary;
		if (newReverb != string.Empty && newReverb != reverbIdentifier)
		{
			reverbIdentifierPrevious = reverbIdentifier;
			SetReverb(newReverb);
		}
	}

	public void StopModifierParams(string name)
	{
		if (!modifierParams.ContainsKey(name))
		{
			return;
		}
		foreach (KeyValuePair<string, AudioParamInitializedClass> item in modifierParams[name])
		{
			audioInstance.setParameterValue(item.Key, item.Value.exitValue);
		}
		modifierParams.Remove(name);
		if (reverbIdentifierPrevious != string.Empty)
		{
			SetReverb(reverbIdentifierPrevious);
			reverbIdentifierPrevious = string.Empty;
		}
	}

	public void StopCurrent()
	{
		StopReverb();
		if (audioInstance.isValid())
		{
			Debug.Log("** Fadeout last " + trackIdentifier);
			audioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			audioInstance.release();
			audioInstance = default(EventInstance);
			trackIdentifier = string.Empty;
			reverbIdentifierPrevious = string.Empty;
			modifierParams.Clear();
			ambientParams.Clear();
			sceneParams.Clear();
			areaModifiers.Clear();
			ambientRunning = false;
			ambientCurrentTime = 0f;
			ambientNextStatus = AmbientStatus.Start;
		}
	}

	private void StopReverb()
	{
		if (reverbInstance.isValid())
		{
			reverbInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			reverbInstance.release();
			reverbInstance = default(EventInstance);
			reverbIdentifier = string.Empty;
		}
	}

	public float GetParameterValue(string param)
	{
		float value = -1f;
		float finalvalue = -1f;
		if (audioInstance.isValid())
		{
			audioInstance.getParameterValue(param, out value, out finalvalue);
		}
		return value;
	}

	public void SetParameter(string param, float value)
	{
		if (audioInstance.isValid())
		{
			audioInstance.setParameterValue(param, value);
		}
	}

	public void Update()
	{
		if (ambientRunning && audioInstance.isValid())
		{
			ambientCurrentTime += Time.deltaTime;
			if (ambientCurrentTime >= ambientStartTime && ambientNextStatus == AmbientStatus.Start)
			{
				ambientNextStatus = AmbientStatus.End;
				SetAmbientParameters(enableParam: true);
			}
			if (ambientCurrentTime >= ambientEndTime && ambientNextStatus == AmbientStatus.End)
			{
				ambientNextStatus = AmbientStatus.Start;
				ambientCurrentTime = 0f;
				SetAmbientParameters(enableParam: false);
			}
		}
	}

	public void OnGUI()
	{
		posYGUI = 10f;
		DrawTextLine("Backgorund Music -------------------------------------");
		DrawTextLine("Sound:" + trackIdentifier);
		DrawTextLine("Reverb:" + reverbIdentifier);
		DrawTextLine("***** Params: Gloabal");
		foreach (KeyValuePair<string, float> sceneParam in sceneParams)
		{
			DrawTextLine(sceneParam.Key + ": " + sceneParam.Value);
		}
		DrawTextLine("Ambient ------------");
		DrawTextLine("Time:" + ambientCurrentTime);
		DrawTextLine("Start:" + ambientStartTime);
		DrawTextLine("End:" + ambientEndTime);
		DrawTextLine("***** Params: AmbientMusic->" + ambientRunning);
		foreach (KeyValuePair<string, AudioParamInitializedClass> ambientParam in ambientParams)
		{
			DrawTextLine(ambientParam.Key + ": " + ambientParam.Value.currentValue);
		}
		DrawTextLine("Modifiers ------------");
		foreach (KeyValuePair<string, Dictionary<string, AudioParamInitializedClass>> modifierParam in modifierParams)
		{
			DrawTextLine("*** Modifier: " + modifierParam.Key);
			foreach (KeyValuePair<string, AudioParamInitializedClass> item in modifierParam.Value)
			{
				DrawTextLine(item.Key + ": " + item.Value.currentValue);
			}
		}
		DrawTextLine("Area ------------");
		foreach (KeyValuePair<string, float> areaModifier in areaModifiers)
		{
			DrawTextLine("* " + areaModifier.Key + ": " + areaModifier.Value);
		}
		if (!audioInstance.isValid())
		{
			return;
		}
		DrawTextLine("***** ALL Instance Params");
		foreach (KeyValuePair<string, AudioParamInitializedClass> ambientParam2 in ambientParams)
		{
			float value = -1f;
			int count = 0;
			audioInstance.getParameterCount(out count);
			for (int i = 0; i < count; i++)
			{
				ParameterInstance instance = default(ParameterInstance);
				audioInstance.getParameterByIndex(i, out instance);
				instance.getDescription(out var description);
				instance.getValue(out value);
				DrawTextLine(string.Concat(description.name, ": ", value.ToString()));
			}
		}
	}

	private void SetReverb(string idReverb)
	{
		if (idReverb != reverbIdentifier)
		{
			if (reverbInstance.isValid())
			{
				StopReverb();
			}
			reverbIdentifier = idReverb;
			if (idReverb != string.Empty)
			{
				reverbInstance = Core.Audio.CreateEvent(idReverb);
				reverbInstance.start();
			}
			else
			{
				reverbInstance = default(EventInstance);
			}
		}
	}

	private void DrawTextLine(string text)
	{
		GUI.Label(new Rect(10f, posYGUI, 500f, posYGUI + 10f), text);
		posYGUI += 13f;
	}

	private void SetAudioInstanceVolume()
	{
		audioInstance.setVolume(volume);
		audioInstance.setParameterValue("VOLUME", 1f);
		audioInstance.setParameterValue("ACTIVE", 1f);
	}

	private void SetAmbientParameters(bool enableParam)
	{
		foreach (KeyValuePair<string, AudioParamInitializedClass> ambientParam in ambientParams)
		{
			ambientParam.Value.currentValue = ((!enableParam) ? ambientParam.Value.exitValue : ambientParam.Value.enterValue);
			audioInstance.setParameterValue(ambientParam.Key, ambientParam.Value.currentValue);
		}
	}
}
