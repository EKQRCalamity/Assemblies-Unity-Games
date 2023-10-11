using System;
using UnityEngine;
using UnityEngine.Audio;

public static class PooledAudioPackExtensions
{
	public static PooledAudioSource PlaySafe(this PooledAudioPack pack, System.Random random, AudioMixerGroup mixerGroup = null, bool loop = false, float fadeInDuration = 0f, float startAtTime = 0f, float maxDistance = 50f, int priority = 128, float createVolumeThreshold = 0f, float volumeMultiplier = 1f, float minDistance = 1.5f)
	{
		if (pack.IsNullOrEmpty() || !AudioPool.Instance)
		{
			return null;
		}
		return pack.Play(random, mixerGroup, loop, fadeInDuration, startAtTime, maxDistance, priority, createVolumeThreshold, volumeMultiplier, minDistance);
	}

	public static PooledAudioSource PlaySafe(this PooledAudioPack pack, Vector3 position, System.Random random, AudioMixerGroup mixerGroup = null, bool loop = false, float fadeInDuration = 0f, float dopplerLevel = 1f, float startAtTime = 0f, float maxDistance = 50f, int priority = 128, float createVolumeThreshold = 0f, float volumeMultiplier = 1f, float minDistance = 1.5f)
	{
		if (pack.IsNullOrEmpty() || !AudioPool.Instance)
		{
			return null;
		}
		return pack.Play(position, random, mixerGroup, loop, fadeInDuration, dopplerLevel, startAtTime, maxDistance, priority, createVolumeThreshold, volumeMultiplier, minDistance);
	}

	public static PooledAudioSource PlaySafe(this PooledAudioPack pack, Transform transform, System.Random random, AudioMixerGroup mixerGroup = null, bool loop = false, float fadeInDuration = 0f, float dopplerLevel = 1f, float startAtTime = 0f, float maxDistance = 50f, int priority = 128, float createVolumeThreshold = 0f, float volumeMultiplier = 1f, float minDistance = 1.5f)
	{
		if (pack.IsNullOrEmpty() || !AudioPool.Instance)
		{
			return null;
		}
		return pack.Play(transform, random, mixerGroup, loop, fadeInDuration, dopplerLevel, startAtTime, maxDistance, priority, createVolumeThreshold, volumeMultiplier, minDistance);
	}

	public static bool IsNullOrEmpty(this PooledAudioPack pack)
	{
		return pack?.clips.IsNullOrEmpty() ?? true;
	}
}
