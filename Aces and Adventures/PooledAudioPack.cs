using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
[ProtoContract]
public class PooledAudioPack
{
	[ProtoMember(1)]
	public Vector2 volumeRange = Vector2.one;

	[ProtoMember(2)]
	public Vector2 pitchRange = Vector2.one;

	[ProtoMember(3)]
	public List<AudioClip> clips;

	public PooledAudioSource Play(System.Random random, AudioMixerGroup mixerGroup = null, bool loop = false, float fadeInDuration = 0f, float startAtTime = 0f, float maxDistance = 50f, int priority = 128, float createVolumeThreshold = 0f, float volumeMultiplier = 1f, float minDistance = 1.5f)
	{
		return AudioPool.Instance.Play(GetClip(random), mixerGroup, GetVolume(random) * volumeMultiplier, GetPitch(random), loop, fadeInDuration, startAtTime, maxDistance, priority, createVolumeThreshold, minDistance);
	}

	public PooledAudioSource Play(Vector3 position, System.Random random, AudioMixerGroup mixerGroup = null, bool loop = false, float fadeInDuration = 0f, float dopplerLevel = 1f, float startAtTime = 0f, float maxDistance = 50f, int priority = 128, float createVolumeThreshold = 0f, float volumeMultiplier = 1f, float minDistance = 1.5f)
	{
		return AudioPool.Instance.Play(GetClip(random), position, mixerGroup, GetVolume(random) * volumeMultiplier, GetPitch(random), dopplerLevel, loop, fadeInDuration, startAtTime, maxDistance, priority, createVolumeThreshold, null, minDistance);
	}

	public PooledAudioSource Play(Transform transform, System.Random random, AudioMixerGroup mixerGroup = null, bool loop = false, float fadeInDuration = 0f, float dopplerLevel = 1f, float startAtTime = 0f, float maxDistance = 50f, int priority = 128, float createVolumeThreshold = 0f, float volumeMultiplier = 1f, float minDistance = 1.5f)
	{
		return AudioPool.Instance.Play(GetClip(random), transform, mixerGroup, GetVolume(random) * volumeMultiplier, GetPitch(random), dopplerLevel, loop, fadeInDuration, startAtTime, maxDistance, priority, createVolumeThreshold, minDistance);
	}

	public AudioClip GetClip(System.Random random)
	{
		return random.ItemSafe(clips);
	}

	public float GetVolume(System.Random random)
	{
		return random.Range(volumeRange);
	}

	public float GetPitch(System.Random random)
	{
		return random.Range(pitchRange);
	}
}
