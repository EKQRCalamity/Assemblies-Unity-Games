using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioCrossFader : MonoBehaviour
{
	[Range(0f, 5f)]
	public float crossFadeDuration = 0.5f;

	[Range(0f, 5f)]
	public float resumeFadeDuration = 2.5f;

	[Range(0f, 5f)]
	public float stopFadeDuration = 1f;

	private Dictionary<AudioClip, float> _timeStamps;

	private AudioFader[] _audioFaders;

	private int _activeIndex;

	private AudioFader activeFader => _audioFaders[_activeIndex];

	private AudioClip activeClip
	{
		get
		{
			if (!activeFader.isPlaying)
			{
				return null;
			}
			return activeFader.audioSource.clip;
		}
	}

	public AudioClip lastPlayedClip { get; private set; }

	private void _GetNextAudioFader()
	{
		_activeIndex = (_activeIndex + 1) % _audioFaders.Length;
	}

	private void Awake()
	{
		_timeStamps = new Dictionary<AudioClip, float>();
		_audioFaders = new AudioFader[4];
		for (int i = 0; i < _audioFaders.Length; i++)
		{
			_audioFaders[i] = base.gameObject.AddComponent<AudioFader>();
		}
	}

	public AudioCrossFader SetData(float crossFadeDuration, float resumeFadeDuration, float stopFadeDuration)
	{
		this.crossFadeDuration = crossFadeDuration;
		this.resumeFadeDuration = resumeFadeDuration;
		this.stopFadeDuration = stopFadeDuration;
		return this;
	}

	public void Play(AudioClip clip, float startAtTime = 0f, float? spatialBlend = null, bool? loop = null, AudioMixerGroup outputGroup = null)
	{
		lastPlayedClip = clip;
		if (!clip)
		{
			Stop();
			return;
		}
		AudioClip audioClip = activeClip;
		if (!(clip == audioClip))
		{
			if ((bool)audioClip)
			{
				_timeStamps[audioClip] = activeFader.audioSource.time;
				activeFader.Stop(crossFadeDuration);
			}
			_GetNextAudioFader();
			if (spatialBlend.HasValue)
			{
				activeFader.audioSource.spatialBlend = spatialBlend.Value;
			}
			if (loop.HasValue)
			{
				activeFader.audioSource.loop = loop.Value;
			}
			activeFader.audioSource.outputAudioMixerGroup = outputGroup;
			activeFader.Play(clip, startAtTime, (startAtTime > 0f) ? resumeFadeDuration : (audioClip ? crossFadeDuration : 0f));
		}
	}

	public void Pause()
	{
		if ((bool)activeClip)
		{
			_timeStamps[activeClip] = activeFader.audioSource.time;
		}
		if (activeFader.Pause(stopFadeDuration))
		{
			_GetNextAudioFader();
		}
	}

	public void Resume(AudioClip clip, float? spatialBlend = null, bool? loop = null, AudioMixerGroup outputGroup = null)
	{
		Play(clip, _timeStamps.ContainsKey(clip) ? _timeStamps[clip] : 0f, spatialBlend, loop, outputGroup);
	}

	public void Stop()
	{
		if (activeFader.Stop(stopFadeDuration))
		{
			_GetNextAudioFader();
		}
	}
}
