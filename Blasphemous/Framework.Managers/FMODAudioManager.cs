using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DG.Tweening;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Framework.Audio;
using Framework.FrameworkCore;
using Framework.Util;
using Sirenix.Utilities;
using Tools.Audio;
using UnityEngine;

namespace Framework.Managers;

public class FMODAudioManager : GameSystem
{
	public delegate void ProgrammerSoundSeted(float time);

	public const float LeftPanValue = 0f;

	public const float RightPanValue = 1f;

	private static ProgrammerSoundSeted currentEvent;

	private EVENT_CALLBACK programmerSoundCallback;

	private Dictionary<string, EventInstance> NamedSounds;

	private float posYGUI;

	private const string LocalizationParam = "Spanish";

	private float LocalizationValue;

	private FMODAudioCatalog[] _audioCatalogs;

	public Bus Sfx { get; private set; }

	public Bus Music { get; private set; }

	public Bus Voiceover { get; private set; }

	public AmbientMusicSettings Ambient { get; private set; }

	public ChannelGroup SfxChannelGroup { get; private set; }

	public FMODAudioCatalog[] EnemiesAudioCatalogs { get; private set; }

	public float MasterVolume
	{
		get
		{
			GetMasterGroup().getVolume(out var volume);
			return volume;
		}
		set
		{
			GetMasterGroup().setVolume(value);
		}
	}

	public event ProgrammerSoundSeted OnProgrammerSoundSeted;

	public override void Initialize()
	{
		base.Initialize();
		NamedSounds = new Dictionary<string, EventInstance>();
		_audioCatalogs = new FMODAudioCatalog[0];
		Settings instance = Settings.Instance;
		if (instance.AutomaticEventLoading || instance.ImportType != 0)
		{
			UnityEngine.Debug.LogError("*** FMODAudioManager, setting must be AutomaticEventLoading=false and ImportType=StreamingAssets");
		}
		else
		{
			try
			{
				foreach (string masterBank in instance.MasterBanks)
				{
					RuntimeManager.LoadBank(masterBank + ".strings", instance.AutomaticSampleLoading);
					RuntimeManager.LoadBank(masterBank, instance.AutomaticSampleLoading);
				}
				foreach (string bank in instance.Banks)
				{
					if (!bank.ToUpper().StartsWith("VOICEOVER_"))
					{
						RuntimeManager.LoadBank(bank, instance.AutomaticSampleLoading);
					}
				}
				RuntimeManager.WaitForAllLoads();
			}
			catch (BankLoadException exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
		Sfx = RuntimeManager.GetBus("bus:/ALLSFX");
		Music = RuntimeManager.GetBus("bus:/MUSIC");
		Voiceover = RuntimeManager.GetBus("bus:/VO");
		programmerSoundCallback = ProgrammerSoundCallback;
		Ambient = new AmbientMusicSettings();
		EnemiesAudioCatalogs = Resources.LoadAll<FMODAudioCatalog>("FMODCatalogs/Enemies");
		LocalizationManager.OnLocalizeAudioEvent += OnAudioLocalizationChange;
	}

	private void OnAudioLocalizationChange(string idlang)
	{
		LocalizationValue = ((!(idlang.ToUpper() == "ES")) ? 0f : 1f);
	}

	public override void Update()
	{
		if (Ambient != null)
		{
			Ambient.Update();
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, EventInstance> namedSound in NamedSounds)
		{
			EventInstance value = namedSound.Value;
			bool flag;
			if (!value.isValid())
			{
				flag = true;
			}
			else
			{
				value.getPlaybackState(out var state);
				flag = state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING;
				value.release();
			}
			if (flag)
			{
				list.Add(namedSound.Key);
			}
		}
		list.ForEach(delegate(string element)
		{
			NamedSounds.Remove(element);
		});
	}

	public override void OnGUI()
	{
		Ambient.OnGUI();
		posYGUI = 10f;
		DrawTextLine("Named Sounds -------------------------------------");
		DrawTextLine("Total:" + NamedSounds.Count);
		DrawTextLine("----------------------");
		foreach (KeyValuePair<string, EventInstance> namedSound in NamedSounds)
		{
			string text = "NOT VALID";
			if (namedSound.Value.isValid())
			{
				namedSound.Value.getPlaybackState(out var state);
				namedSound.Value.getTimelinePosition(out var position);
				text = string.Concat(state, "  Pos:", position);
			}
			DrawTextLine(namedSound.Key + ": " + text);
		}
		DrawTextLine("Playing Sounds -------------------------------------");
		Bank[] array = null;
		RuntimeManager.StudioSystem.getBankList(out array);
		Bank[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Bank bank = array2[i];
			EventDescription[] array3 = null;
			string path = string.Empty;
			bank.getEventList(out array3);
			bank.getPath(out path);
			DrawTextLine("BANK:" + path);
			EventDescription[] array4 = array3;
			for (int j = 0; j < array4.Length; j++)
			{
				EventDescription eventDescription = array4[j];
				int count = 0;
				eventDescription.getInstanceCount(out count);
				if (count > 0)
				{
					string path2 = string.Empty;
					eventDescription.getPath(out path2);
					DrawTextLine("." + path2 + " (" + count + ")");
				}
			}
		}
	}

	private void DrawTextLine(string text)
	{
		GUI.Label(new Rect(500f, posYGUI, 1000f, posYGUI + 10f), text);
		posYGUI += 13f;
	}

	public EventInstance PlayProgrammerSound(string eventName, string keyName, ProgrammerSoundSeted eventSound)
	{
		currentEvent = eventSound;
		EventInstance eventInstance = RuntimeManager.CreateInstance(eventName);
		GCHandle value = GCHandle.Alloc(keyName, GCHandleType.Pinned);
		eventInstance.setUserData(GCHandle.ToIntPtr(value));
		eventInstance.setCallback(programmerSoundCallback);
		UpdateEventInstanceParam(eventInstance);
		eventInstance.start();
		eventInstance.release();
		return eventInstance;
	}

	private static RESULT ProgrammerSoundCallback(EVENT_CALLBACK_TYPE type, EventInstance eventInstance, IntPtr parameterPtr)
	{
		eventInstance.getUserData(out var userdata);
		GCHandle gCHandle = GCHandle.FromIntPtr(userdata);
		string text = gCHandle.Target as string;
		switch (type)
		{
		case EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
		{
			PROGRAMMER_SOUND_PROPERTIES pROGRAMMER_SOUND_PROPERTIES2 = (PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(PROGRAMMER_SOUND_PROPERTIES));
			SOUND_INFO info;
			RESULT soundInfo = RuntimeManager.StudioSystem.getSoundInfo(text, out info);
			if (soundInfo != 0)
			{
				UnityEngine.Debug.LogWarning("Programmer sound: Can't find key " + text + " ERR:" + soundInfo);
				break;
			}
			Sound sound2;
			RESULT rESULT = RuntimeManager.LowlevelSystem.createSound(info.name_or_data, info.mode, ref info.exinfo, out sound2);
			if (rESULT == RESULT.OK)
			{
				pROGRAMMER_SOUND_PROPERTIES2.sound = sound2.handle;
				pROGRAMMER_SOUND_PROPERTIES2.subsoundIndex = info.subsoundindex;
				Marshal.StructureToPtr(pROGRAMMER_SOUND_PROPERTIES2, parameterPtr, fDeleteOld: false);
				if (currentEvent != null)
				{
					sound2.getSubSound(info.subsoundindex, out var subsound);
					subsound.getLength(out var length, TIMEUNIT.MS);
					float time = (float)length / 1000f;
					currentEvent(time);
				}
			}
			else
			{
				UnityEngine.Debug.LogWarning("Programmer sound: Can't create sound, key " + text + " ERR:" + rESULT);
			}
			break;
		}
		case EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
		{
			PROGRAMMER_SOUND_PROPERTIES pROGRAMMER_SOUND_PROPERTIES = (PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(PROGRAMMER_SOUND_PROPERTIES));
			Sound sound = default(Sound);
			sound.handle = pROGRAMMER_SOUND_PROPERTIES.sound;
			sound.release();
			break;
		}
		case EVENT_CALLBACK_TYPE.DESTROYED:
			gCHandle.Free();
			break;
		}
		return RESULT.OK;
	}

	public void PlayNamedSound(string eventName, string keyName)
	{
		EventInstance eventInstance = default(EventInstance);
		if (NamedSounds.ContainsKey(keyName))
		{
			eventInstance = NamedSounds[keyName];
		}
		if (!eventInstance.isValid())
		{
			eventInstance = RuntimeManager.CreateInstance(eventName);
			UpdateEventInstanceParam(eventInstance);
			NamedSounds[keyName] = eventInstance;
		}
		RESULT rESULT = eventInstance.start();
	}

	public void StopNamedSound(string keyName, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
	{
		if (IsPlayingNamedSound(keyName))
		{
			NamedSounds[keyName].stop(stopMode);
			NamedSounds[keyName].release();
			NamedSounds.Remove(keyName);
		}
	}

	public bool IsPlayingNamedSound(string keyName)
	{
		bool result = false;
		if (NamedSounds.ContainsKey(keyName) && NamedSounds[keyName].isValid())
		{
			NamedSounds[keyName].getPlaybackState(out var state);
			result = state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING;
		}
		return result;
	}

	private FMODAudioCatalog.FMODIndexedClip FindSfxInCatalogs(string id)
	{
		return _audioCatalogs.Select((FMODAudioCatalog t) => t.GetSfx(id)).FirstOrDefault((FMODAudioCatalog.FMODIndexedClip c) => c != null);
	}

	private void UpdateEventInstanceParam(EventInstance evt)
	{
		if (evt.isValid())
		{
			evt.setParameterValue("Spanish", LocalizationValue);
		}
	}

	public void RegisterCatalog(FMODAudioCatalog cat)
	{
		if (!_audioCatalogs.Any((FMODAudioCatalog t) => t == cat))
		{
			FMODAudioCatalog[] array = new FMODAudioCatalog[_audioCatalogs.Length + 1];
			_audioCatalogs.CopyTo(array, 0);
			array[_audioCatalogs.Length] = cat;
			_audioCatalogs = array;
			cat.Initialize();
		}
	}

	public void PlayOneShot(string path, Vector3 position = default(Vector3))
	{
		try
		{
			PlayOneShot(RuntimeManager.PathToGUID(path), position);
		}
		catch (EventNotFoundException)
		{
			UnityEngine.Debug.LogWarning("[FMOD] Event not found: " + path);
		}
	}

	public void PlayOneShot(Guid guid, Vector3 position = default(Vector3))
	{
		EventInstance evt = RuntimeManager.CreateInstance(guid);
		evt.set3DAttributes(position.To3DAttributes());
		UpdateEventInstanceParam(evt);
		evt.start();
		evt.release();
	}

	public EventInstance CreateCatalogEvent(string key, Vector3 position = default(Vector3))
	{
		FMODAudioCatalog.FMODIndexedClip fMODIndexedClip = FindSfxInCatalogs(key);
		EventInstance eventInstance = ((fMODIndexedClip != null) ? RuntimeManager.CreateInstance(fMODIndexedClip.FMODKey) : default(EventInstance));
		eventInstance.set3DAttributes(position.To3DAttributes());
		UpdateEventInstanceParam(eventInstance);
		return eventInstance;
	}

	public EventInstance CreateEvent(string key, Vector3 position = default(Vector3))
	{
		EventInstance eventInstance = default(EventInstance);
		if (!key.IsNullOrWhitespace())
		{
			eventInstance = RuntimeManager.CreateInstance(key);
		}
		eventInstance.set3DAttributes(position.To3DAttributes());
		UpdateEventInstanceParam(eventInstance);
		if (!eventInstance.isValid())
		{
			Log.Error("Audio", "Imposible to create audio instance. ID: " + key);
		}
		return eventInstance;
	}

	public static EventInstance CloneInstance(EventDescription desc)
	{
		desc.createInstance(out var instance);
		return instance;
	}

	public void EventOneShot(EventInstance eventInstance)
	{
		eventInstance.getDescription(out var description);
		EventInstance eventInstance2 = CloneInstance(description);
		eventInstance2.start();
		eventInstance2.release();
	}

	public void EventOneShotPanned(string eventKey, Vector3 position)
	{
		if (!eventKey.IsNullOrWhitespace())
		{
			EventInstance evt = RuntimeManager.CreateInstance(eventKey);
			UpdateEventInstanceParam(evt);
			evt.getParameter("Panning", out var instance);
			if (instance.isValid())
			{
				float panningValueByPosition = GetPanningValueByPosition(position);
				instance.setValue(panningValueByPosition);
			}
			evt.start();
			evt.release();
		}
	}

	public void PlayOneShotFromCatalog(string eventKey, Vector3 position)
	{
		FMODAudioCatalog.FMODIndexedClip fMODIndexedClip = FindSfxInCatalogs(eventKey);
		if (fMODIndexedClip != null)
		{
			string fMODKey = fMODIndexedClip.FMODKey;
			EventOneShotPanned(fMODKey, position);
		}
	}

	public void EventOneShotPanned(string eventKey, Vector3 position, out EventInstance eventInstance)
	{
		if (eventKey.IsNullOrWhitespace())
		{
			eventInstance = default(EventInstance);
			return;
		}
		eventInstance = RuntimeManager.CreateInstance(eventKey);
		UpdateEventInstanceParam(eventInstance);
		eventInstance.getParameter("Panning", out var instance);
		if (instance.isValid())
		{
			float panningValueByPosition = GetPanningValueByPosition(position);
			instance.setValue(panningValueByPosition);
		}
		eventInstance.start();
		eventInstance.release();
	}

	public void PlaySfxOnCatalog(string id)
	{
		if (!string.IsNullOrEmpty(id))
		{
			FMODAudioCatalog.FMODIndexedClip fMODIndexedClip = FindSfxInCatalogs(id);
			if (fMODIndexedClip != null)
			{
				string fMODKey = fMODIndexedClip.FMODKey;
				RuntimeManager.PlayOneShot(fMODKey);
			}
			else
			{
				UnityEngine.Debug.LogError($"SFX {id} not defined in any AudioCatalog!");
			}
		}
	}

	public void FadeAudio(float volume, float time)
	{
		DOTween.To(() => Core.Audio.MasterVolume, delegate(float x)
		{
			Core.Audio.MasterVolume = x;
		}, volume, time);
	}

	public void FadeLevelAudio(float volume, float time)
	{
		DOTween.To(() => Core.Audio.Ambient.Volume, delegate(float x)
		{
			Core.Audio.Ambient.Volume = x;
		}, volume, time);
	}

	public void PauseAudio(bool pause)
	{
		RuntimeManager.PauseAllEvents(pause);
	}

	public void PlaySfxOnCatalog(string id, float delay)
	{
		Singleton<Core>.Instance.StartCoroutine(PlaySfxDelay(id, delay));
	}

	public void PlaySfx(string id, float delay = 0f)
	{
		if (!id.IsNullOrWhitespace())
		{
			DOTween.Sequence().SetDelay(delay).OnComplete(delegate
			{
				RuntimeManager.PlayOneShot(id);
			});
		}
	}

	public ChannelGroup GetMasterGroup()
	{
		RuntimeManager.LowlevelSystem.getMasterChannelGroup(out var channelgroup);
		return channelgroup;
	}

	public EVENT_CALLBACK ModifyPanning(EventInstance e, Transform transform)
	{
		e.getParameter("Panning", out var instance);
		if (instance.isValid())
		{
			float panningValueByPosition = GetPanningValueByPosition(transform.position);
			instance.setValue(panningValueByPosition);
		}
		return null;
	}

	public void ApplyDistanceParam(ref EventInstance ev, float minDist, float maxDist, Transform a, Transform b)
	{
		if (ev.isValid())
		{
			float distanceParam = GetDistanceParam(minDist, maxDist, a, b);
			ev.getParameter("Distance", out var instance);
			if (instance.isValid())
			{
				instance.setValue(distanceParam);
			}
		}
	}

	public void PlayEventWithCatalog(ref EventInstance eventInstance, string eventKey, Vector3 position = default(Vector3))
	{
		if (!eventInstance.isValid())
		{
			eventInstance = CreateCatalogEvent(eventKey, position);
			eventInstance.start();
		}
	}

	public void PlayEventNoCatalog(ref EventInstance eventInstance, string eventKey, Vector3 position = default(Vector3))
	{
		if (!eventInstance.isValid())
		{
			eventInstance = CreateEvent(eventKey, position);
			eventInstance.start();
		}
	}

	public void StopEvent(ref EventInstance eventInstance)
	{
		if (eventInstance.isValid())
		{
			eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			eventInstance.release();
			eventInstance.clearHandle();
		}
	}

	private float GetDistanceParam(float min, float max, Transform a, Transform b)
	{
		float value = Vector2.Distance(a.position, b.position);
		value = Mathf.Clamp(value, min, max);
		return (value - min) / (max - min);
	}

	public void PlaySfxAtPosition(string id, Vector2 position, float range)
	{
		if (!id.IsNullOrWhitespace())
		{
			EventInstance eventInstance = RuntimeManager.CreateInstance(id);
			UpdateEventInstanceParam(eventInstance);
			Vector2 a = Core.Logic.Penitent.transform.position;
			float num = Math.Max(0f, Vector2.Distance(a, position));
			float volume = 1f - Mathf.Clamp01(num / range);
			eventInstance.setVolume(volume);
			eventInstance.start();
			eventInstance.setCallback(PlaySfxAtPositionFinished(eventInstance), EVENT_CALLBACK_TYPE.STOPPED);
		}
	}

	private EVENT_CALLBACK PlaySfxAtPositionFinished(EventInstance instance)
	{
		instance.release();
		return null;
	}

	public void StopSfx(string id, bool allowFadeout = false)
	{
		FMODAudioCatalog.FMODIndexedClip fMODIndexedClip = FindSfxInCatalogs(id);
		if (fMODIndexedClip != null)
		{
			EventInstance eventInstance = RuntimeManager.CreateInstance(fMODIndexedClip.FMODKey);
			eventInstance.getPlaybackState(out var state);
			if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING)
			{
				eventInstance.stop((!allowFadeout) ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			}
			eventInstance.release();
		}
	}

	private IEnumerator PlaySfxDelay(string id, float delay)
	{
		yield return new WaitForSeconds(delay);
		PlaySfxOnCatalog(id);
	}

	public static float GetPanningValueByPosition(Vector2 pos)
	{
		float t = Mathf.Clamp01(Camera.main.WorldToViewportPoint(pos).x);
		return Mathf.Lerp(0f, 1f, t);
	}

	public void SetSfxVolume(float volume)
	{
		RESULT rESULT = Sfx.setVolume(Mathf.Clamp01(volume));
		if (rESULT != 0)
		{
			UnityEngine.Debug.LogError("SetSfxVolume error! Operation result: " + rESULT);
		}
	}

	public void SetMusicVolume(float volume)
	{
		RESULT rESULT = Music.setVolume(Mathf.Clamp01(volume));
		if (rESULT != 0)
		{
			UnityEngine.Debug.LogError("SetMusicVolume error! Operation result: " + rESULT);
		}
	}

	public void SetVoiceoverVolume(float volume)
	{
		RESULT rESULT = Voiceover.setVolume(Mathf.Clamp01(volume));
		if (rESULT != 0)
		{
			UnityEngine.Debug.LogError("SetVoiceoverVolume error! Operation result: " + rESULT);
		}
	}

	public float GetSfxVolume()
	{
		float volume;
		float finalvolume;
		RESULT volume2 = Sfx.getVolume(out volume, out finalvolume);
		if (volume2 != 0)
		{
			UnityEngine.Debug.LogError("GetSfxVolume error! Operation result: " + volume2);
		}
		return volume;
	}

	public float GetMusicVolume()
	{
		float volume;
		float finalvolume;
		RESULT volume2 = Music.getVolume(out volume, out finalvolume);
		if (volume2 != 0)
		{
			UnityEngine.Debug.LogError("GetMusicVolume error! Operation result: " + volume2);
		}
		return volume;
	}

	public float GetVoiceoverVolume()
	{
		float volume;
		float finalvolume;
		RESULT volume2 = Voiceover.getVolume(out volume, out finalvolume);
		if (volume2 != 0)
		{
			UnityEngine.Debug.LogError("GetVoiceoverVolume error! Operation result: " + volume2);
		}
		return volume;
	}
}
