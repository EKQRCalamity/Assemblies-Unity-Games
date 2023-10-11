using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PooledAudioPlayer : MonoBehaviour
{
	public struct ActivePooledAudioSource : IEquatable<ActivePooledAudioSource>
	{
		public PooledAudioSource source;

		public readonly long playId;

		public ActivePooledAudioSource(PooledAudioSource source)
		{
			this.source = source;
			playId = source.playId;
		}

		public void Stop(float? fadeDurationOverride = null)
		{
			if ((bool)this)
			{
				source.Stop(fadeDurationOverride);
			}
			source = null;
		}

		public static implicit operator ActivePooledAudioSource(PooledAudioSource source)
		{
			return new ActivePooledAudioSource(source);
		}

		public static implicit operator PooledAudioSource(ActivePooledAudioSource activeSource)
		{
			return activeSource.source;
		}

		public static implicit operator long(ActivePooledAudioSource activeSource)
		{
			return activeSource.playId;
		}

		public static implicit operator bool(ActivePooledAudioSource activeSource)
		{
			if (activeSource.source != null && activeSource.source.playId == activeSource.playId)
			{
				return activeSource.source.isPlaying;
			}
			return false;
		}

		public static bool operator ==(ActivePooledAudioSource a, ActivePooledAudioSource b)
		{
			return a.playId == b.playId;
		}

		public static bool operator !=(ActivePooledAudioSource a, ActivePooledAudioSource b)
		{
			return !(a == b);
		}

		public bool Equals(ActivePooledAudioSource other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			if (obj is ActivePooledAudioSource)
			{
				return this == (ActivePooledAudioSource)obj;
			}
			return false;
		}

		public override int GetHashCode()
		{
			long num = playId;
			return num.GetHashCode();
		}
	}

	private static System.Random _Random = new System.Random();

	public AudioMixerGroup mixer;

	public bool playOnEnable = true;

	public bool is3D = true;

	public bool attachToTransform = true;

	public bool useSmartSoundOverriding;

	public bool loop;

	[Range(0f, 1f)]
	[Space(12f)]
	public float volume = 1f;

	[Range(0f, 0.99f)]
	public float createVolumeThreshold;

	[Range(0f, 3f)]
	public float fadeIn;

	[Range(0f, 3f)]
	public float fadeOut = 0.2f;

	[Space(12f)]
	public PooledAudioPack audioPack = new PooledAudioPack();

	[Range(1f, 32f)]
	[Space(12f)]
	public int maxActiveSources = 1;

	private List<ActivePooledAudioSource> _activeSources = new List<ActivePooledAudioSource>();

	public int activeSourceCount => _activeSources.Count;

	private void _UpdateActiveSources()
	{
		for (int num = _activeSources.Count - 1; num >= 0; num--)
		{
			if (!_activeSources[num])
			{
				_activeSources.RemoveAt(num);
			}
		}
	}

	private void OnEnable()
	{
		if (playOnEnable)
		{
			Play();
		}
	}

	private void Update()
	{
		_UpdateActiveSources();
	}

	public PooledAudioPlayer SetData(PooledAudioPack audioPack, AudioMixerGroup mixer = null, int maxActiveSources = 1, float fadeOut = 0.2f, bool loop = false, float fadeIn = 0f, bool is3D = false, bool attachToTransform = false, bool playOnEnable = false, float createVolumeThreshold = 0f, bool useSmartSoundOverriding = false)
	{
		this.audioPack = audioPack;
		this.mixer = mixer;
		this.maxActiveSources = maxActiveSources;
		this.fadeOut = fadeOut;
		this.loop = loop;
		this.fadeIn = fadeIn;
		this.is3D = is3D;
		this.attachToTransform = attachToTransform;
		this.playOnEnable = playOnEnable;
		this.createVolumeThreshold = createVolumeThreshold;
		this.useSmartSoundOverriding = useSmartSoundOverriding;
		return this;
	}

	public void Play()
	{
		PooledAudioSource pooledAudioSource = ((!is3D) ? audioPack.Play(_Random, mixer, loop, fadeIn, 0f, 50f, 128, createVolumeThreshold, volume) : (attachToTransform ? audioPack.Play(base.transform, _Random, mixer, loop, fadeIn, 1f, 0f, 50f, 128, createVolumeThreshold, volume) : audioPack.Play(base.transform.position, _Random, mixer, loop, fadeIn, 1f, 0f, 50f, 128, createVolumeThreshold, volume)));
		if ((bool)pooledAudioSource)
		{
			_activeSources.Add(pooledAudioSource);
			if (_activeSources.Count > maxActiveSources)
			{
				Stop();
			}
		}
	}

	public void Play(float volume)
	{
		this.volume = volume;
		Play();
	}

	public void Play(PooledAudioPack sounds, float? volume = null)
	{
		audioPack = sounds;
		Play(volume ?? this.volume);
	}

	public void Stop(long? playId)
	{
		if (_activeSources.Count == 0)
		{
			return;
		}
		playId = playId ?? ((long)_activeSources[0]);
		for (int i = 0; i < _activeSources.Count; i++)
		{
			if ((long)_activeSources[i] == playId.Value)
			{
				AudioPool.Instance.Stop(_activeSources[i], fadeOut, _activeSources[i]);
				_activeSources.RemoveAt(i);
				break;
			}
		}
	}

	public void Stop()
	{
		long? playId = null;
		if (useSmartSoundOverriding)
		{
			_UpdateActiveSources();
			float num = float.MaxValue;
			foreach (ActivePooledAudioSource activeSource in _activeSources)
			{
				float num2 = Mathf.Pow(activeSource.source.source.TimeRemainingRatio(), 2f) * activeSource.source.volume;
				if (num2 < num)
				{
					playId = activeSource;
					num = num2;
				}
			}
		}
		Stop(playId);
	}
}
