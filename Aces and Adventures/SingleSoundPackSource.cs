using System;
using UnityEngine;
using UnityEngine.Events;

public class SingleSoundPackSource : MonoBehaviour
{
	private static System.Random _random;

	public bool playOnAwake;

	public bool playOnEnable;

	public bool stopOnDisable;

	public bool playSoundIn3D = true;

	[SerializeField]
	protected SingleSoundPack _soundPack;

	[Header("Events")]
	[Range(0f, 1f)]
	public float audioPlayTimeRatioToConsiderComplete = 0.75f;

	[SerializeField]
	protected UnityEvent _OnAudioFinished;

	private PooledAudioSource _source;

	private long _playId;

	private static System.Random _Random => _random ?? (_random = new System.Random());

	public SingleSoundPack soundPack => _soundPack.InsureValid(ref _soundPack);

	public UnityEvent OnAudioFinished => _OnAudioFinished ?? (_OnAudioFinished = new UnityEvent());

	private void Awake()
	{
		if (playOnAwake && !playOnEnable)
		{
			Play();
		}
	}

	private void OnEnable()
	{
		if (playOnEnable)
		{
			Play();
		}
	}

	private void LateUpdate()
	{
		if ((bool)_source && (_source.playId != _playId || !_source.isPlaying || !(_source.playTimeRatio < audioPlayTimeRatioToConsiderComplete)))
		{
			OnAudioFinished.Invoke();
			_source = null;
		}
	}

	private void OnDisable()
	{
		if (stopOnDisable)
		{
			_Stop();
		}
	}

	private void _Stop()
	{
		if ((bool)_source && _source.playId == _playId && _source.isPlaying)
		{
			_source.Stop();
		}
	}

	public void Play()
	{
		_Stop();
		_source = (playSoundIn3D ? soundPack.sounds.Play(base.transform, _Random, soundPack.mixerGroup) : soundPack.sounds.Play(_Random, soundPack.mixerGroup));
		_playId = _source?.playId ?? 0;
	}

	public void Stop()
	{
		_Stop();
	}
}
