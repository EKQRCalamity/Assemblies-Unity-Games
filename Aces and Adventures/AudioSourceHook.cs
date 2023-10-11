using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class AudioSourceHook : MonoBehaviour
{
	public enum RequestType
	{
		Stop,
		Play
	}

	public float volume = 1f;

	[Range(0.01f, 10f)]
	public float fadeTime = 0.25f;

	[Range(0.01f, 10f)]
	public float fadeOutTime = 0.25f;

	private AudioSource _source;

	private RequestType _lastRequest;

	public AudioSource source => this.CacheComponent(ref _source);

	private void Update()
	{
		if (!source.isPlaying)
		{
			return;
		}
		switch (_lastRequest)
		{
		case RequestType.Stop:
			source.volume = Math.Max(0f, source.volume - Time.deltaTime / fadeOutTime);
			if (source.volume <= 0f)
			{
				source.Stop();
			}
			break;
		case RequestType.Play:
			source.volume = Math.Min(volume, source.volume + Time.deltaTime / fadeTime);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void Play()
	{
		_lastRequest = RequestType.Play;
		source.volume = 0f;
		source.Play();
	}

	public void Stop()
	{
		_lastRequest = RequestType.Stop;
	}

	public void SetVolume(float newVolume)
	{
		source.volume = (volume = newVolume);
	}
}
