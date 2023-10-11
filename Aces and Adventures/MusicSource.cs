using System;
using UnityEngine;

public class MusicSource : MonoBehaviour
{
	[SerializeField]
	protected AudioClip _clip;

	public double introBoundary;

	public double loopBoundary = 9999.0;

	[Range(0f, 1f)]
	public float volume = 1f;

	[Range(0f, 10f)]
	public float fadeInTime;

	[Range(0f, 10f)]
	public float fadeOutTime = 1f;

	private AudioSource[] _tracks;

	private int _trackIndex;

	private double _dspTimeOfPlay;

	private float? _timeOfLastPlay;

	private float? _timeOfLastStop;

	private double _loopBoundary;

	public AudioClip clip
	{
		get
		{
			return _clip;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _clip, value))
			{
				_OnClipChange();
			}
		}
	}

	public bool isPlaying
	{
		get
		{
			if (_timeOfLastPlay.HasValue)
			{
				return !_timeOfLastStop.HasValue;
			}
			return false;
		}
	}

	public bool finished
	{
		get
		{
			if (_timeOfLastStop.HasValue)
			{
				return Time.unscaledTime - _timeOfLastStop.Value >= fadeOutTime;
			}
			return false;
		}
	}

	private AudioSource[] tracks => _tracks ?? (_tracks = _InitializeTracks());

	private AudioSource _track => tracks[_trackIndex];

	private AudioSource _nextTrack => tracks[_nextTrackIndex];

	private int _nextTrackIndex => (_trackIndex + 1) % 2;

	public double time { get; private set; }

	private double _dspIntroEndTime => _dspTimeOfPlay + introBoundary;

	private double _dspLoopBoundaryTime => _dspTimeOfPlay + _loopBoundary;

	private double _loopLength => _loopBoundary - introBoundary;

	private AudioSource[] _InitializeTracks()
	{
		return new AudioSource[2]
		{
			_InitializeAudioSource(base.gameObject.AddComponent<AudioSource>()),
			_InitializeAudioSource(base.gameObject.AddComponent<AudioSource>())
		};
	}

	private AudioSource _InitializeAudioSource(AudioSource source)
	{
		source.outputAudioMixerGroup = MasterMixManager.ResourceController.Music;
		source.playOnAwake = false;
		return source;
	}

	private void _OnClipChange()
	{
		if ((bool)clip)
		{
			_loopBoundary = Math.Min(loopBoundary, clip.Length());
		}
		AudioSource[] array = tracks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].clip = clip;
		}
	}

	private void _Play(double dspTransitionTime)
	{
		_track.Play();
		_track.SetScheduledEndTime(dspTransitionTime);
	}

	private void _PlayFromIntroBoundary(double dspTransitionTime)
	{
		_nextTrack.SetTime(introBoundary);
		_nextTrack.PlayScheduled(dspTransitionTime);
		_nextTrack.SetScheduledEndTime(dspTransitionTime + _loopLength);
		_trackIndex = _nextTrackIndex;
	}

	private void Update()
	{
		if ((bool)clip && !AudioListener.pause)
		{
			float num = volume;
			if (_timeOfLastPlay.HasValue && Time.unscaledTime - _timeOfLastPlay.Value < fadeInTime)
			{
				num *= (Time.unscaledTime - _timeOfLastPlay.Value) / fadeInTime;
			}
			if (_timeOfLastStop.HasValue && Time.unscaledTime - _timeOfLastStop.Value < fadeOutTime)
			{
				num *= 1f - (Time.unscaledTime - _timeOfLastStop.Value) / fadeOutTime;
			}
			AudioSource[] array = tracks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].volume = num;
			}
			time = _track.GetTime();
			if (!(_dspLoopBoundaryTime - AudioSettings.dspTime > 1.0))
			{
				_PlayFromIntroBoundary(_dspLoopBoundaryTime);
				_dspTimeOfPlay = _dspLoopBoundaryTime - introBoundary;
			}
		}
	}

	private void OnDisable()
	{
		clip = null;
		_timeOfLastPlay = (_timeOfLastStop = null);
	}

	public void PlayFrom(double playFrom)
	{
		if ((bool)clip)
		{
			playFrom = Math.Max(0.0, Math.Min(playFrom, clip.Length()));
			_timeOfLastPlay = Time.unscaledTime;
			_timeOfLastStop = null;
			_dspTimeOfPlay = AudioSettings.dspTime - playFrom;
			_track.SetTime(playFrom);
			_Play(_dspLoopBoundaryTime);
			Update();
		}
	}

	public void Play()
	{
		PlayFrom(0.0);
	}

	public void Stop()
	{
		if (isPlaying)
		{
			_timeOfLastStop = Time.unscaledTime;
			_track.SetScheduledEndTime(AudioSettings.dspTime + (double)fadeOutTime);
			_nextTrack.SetScheduledEndTime(AudioSettings.dspTime + (double)fadeOutTime);
		}
	}
}
