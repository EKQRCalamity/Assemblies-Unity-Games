using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPool : MonoBehaviour
{
	public const float FADE_OUT = 0.2f;

	public const float MIN_DISTANCE = 1.5f;

	public const float MAX_DISTANCE = 50f;

	public const int PRIORITY = 128;

	private static AudioPool _Instance;

	private static GameObject _Blueprint;

	private Dictionary<PooledAudioCategory, Limiter<AudioLimiterData>> _limiters;

	public static AudioPool Instance => ManagerUtil.GetSingletonInstance(ref _Instance, createSeparateGameObject: true);

	public static GameObject Blueprint
	{
		get
		{
			if (!(_Blueprint != null))
			{
				return _Blueprint = Resources.Load<GameObject>("Audio/PooledAudioSource");
			}
			return _Blueprint;
		}
	}

	public static void StopAllSafe(float fadeTime = 0.2f)
	{
		if ((bool)_Instance)
		{
			_Instance.StopAll(fadeTime);
		}
	}

	private void Awake()
	{
		_limiters = new Dictionary<PooledAudioCategory, Limiter<AudioLimiterData>>(EnumUtil<PooledAudioCategory>.equalityComparer);
		PooledAudioCategory[] values = EnumUtil<PooledAudioCategory>.Values;
		foreach (PooledAudioCategory key in values)
		{
			_limiters.Add(key, Limiter<AudioLimiterData>.Create(AudioLimiterData.Alive, AudioLimiterData.Alike));
		}
	}

	private PooledAudioSource _CreateSource(AudioClip clip, float volume, float pitch, float dopplerLevel, AudioMixerGroup outputGroup, float spatialBlend, Vector3 position, Transform attachTo, float startAtTime, float? fadeInDuration, bool loop, float maxDistance, int priority, float createVolumeThreshold, float minDistance)
	{
		if (!clip || volume < createVolumeThreshold)
		{
			return null;
		}
		PooledAudioSource component = Pools.Unpool(Blueprint, position, null, base.transform).GetComponent<PooledAudioSource>();
		component.volume = volume;
		component.source.pitch = pitch;
		component.source.dopplerLevel = dopplerLevel;
		component.source.outputAudioMixerGroup = outputGroup;
		component.source.spatialBlend = spatialBlend;
		component.attachTo = attachTo;
		component.source.loop = loop;
		component.source.minDistance = minDistance;
		component.source.maxDistance = maxDistance;
		component.source.priority = priority;
		component.Play(clip, startAtTime, fadeInDuration);
		return component;
	}

	public PooledAudioSource Play(AudioClip clip, AudioMixerGroup outputGroup = null, float volume = 1f, float pitch = 1f, bool loop = false, float fadeInDuration = 0f, float startAtTime = 0f, float maxDistance = 50f, int priority = 128, float createVolumeThreshold = 0f, float minDistance = 1.5f)
	{
		return _CreateSource(clip, volume, pitch, 0f, outputGroup, 0f, Vector3.zero, null, startAtTime, fadeInDuration, loop, maxDistance, priority, createVolumeThreshold, minDistance);
	}

	public PooledAudioSource Play(AudioClip clip, Vector3 position, AudioMixerGroup outputGroup = null, float volume = 1f, float pitch = 1f, float dopplerLevel = 1f, bool loop = false, float fadeInDuration = 0f, float startAtTime = 0f, float maxDistance = 50f, int priority = 128, float createVolumeThreshold = 0f, PooledAudioCategory? category = null, float minDistance = 1.5f)
	{
		if (!ShouldPlay(category, clip, position))
		{
			return null;
		}
		return _CreateSource(clip, volume, pitch, dopplerLevel, outputGroup, 1f, position, null, startAtTime, fadeInDuration, loop, maxDistance, priority, createVolumeThreshold, minDistance);
	}

	public PooledAudioSource Play(AudioClip clip, Transform attachSoundTo, AudioMixerGroup outputGroup = null, float volume = 1f, float pitch = 1f, float dopplerLevel = 1f, bool loop = false, float fadeInDuration = 0f, float startAtTime = 0f, float maxDistance = 50f, int priority = 128, float createVolumeThreshold = 0f, float minDistance = 1.5f)
	{
		return _CreateSource(clip, volume, pitch, dopplerLevel, outputGroup, 1f, attachSoundTo.position, attachSoundTo, startAtTime, fadeInDuration, loop, maxDistance, priority, createVolumeThreshold, minDistance);
	}

	public void Stop(ref PooledAudioSource source, float fadeTime = 0.2f, long? playId = null)
	{
		if ((bool)source && source.isPlaying && (!playId.HasValue || source.playId == playId.Value))
		{
			source.Stop(fadeTime);
		}
		source = null;
	}

	public void Stop(PooledAudioSource source, float fadeTime = 0.2f, long? playId = null)
	{
		Stop(ref source, fadeTime, playId);
	}

	public void StopAll(float fadeTime = 0.2f)
	{
		PooledAudioSource[] componentsInChildren = GetComponentsInChildren<PooledAudioSource>();
		foreach (PooledAudioSource source in componentsInChildren)
		{
			Stop(source, fadeTime);
		}
	}

	public bool ShouldPlay(PooledAudioCategory? category, AudioClip clip, Vector3 position)
	{
		if (category.HasValue)
		{
			return _limiters[category.Value].ShouldAdd(new AudioLimiterData(clip, position, Time.time));
		}
		return true;
	}
}
