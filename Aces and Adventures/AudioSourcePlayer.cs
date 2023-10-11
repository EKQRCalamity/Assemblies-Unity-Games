using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class AudioSourcePlayer : MonoBehaviour
{
	private const float SCRUB_DURATION = 0.1f;

	public UnityEvent onSoundFinished;

	private int _scrubRequests;

	private AudioSource _source;

	public bool justStopped { get; private set; }

	public AudioSource source => _source ?? (_source = GetComponent<AudioSource>());

	public AudioPlayState state { get; private set; }

	public float position
	{
		get
		{
			if (!(source.clip == null))
			{
				return source.time;
			}
			return 0f;
		}
	}

	public bool loop
	{
		get
		{
			return source.loop;
		}
		set
		{
			source.loop = value;
		}
	}

	private IEnumerator _BeginScrub()
	{
		_scrubRequests++;
		yield return new WaitForSeconds(0.1f);
		_scrubRequests--;
		if (_scrubRequests == 0)
		{
			source.Pause();
		}
	}

	public void Play()
	{
		source.Play();
		state = AudioPlayState.Playing;
	}

	public void SetPause(bool paused)
	{
		if (source.isPlaying)
		{
			if (paused)
			{
				if (state == AudioPlayState.Stopped)
				{
					source.Play();
				}
				source.Pause();
				state = AudioPlayState.Paused;
			}
		}
		else if (!paused)
		{
			if (state == AudioPlayState.Stopped)
			{
				source.time = 0f;
				source.Play();
			}
			else
			{
				source.UnPause();
			}
			state = AudioPlayState.Playing;
		}
	}

	public void Stop()
	{
		source.Stop();
		justStopped = true;
		state = AudioPlayState.Stopped;
		onSoundFinished.Invoke();
	}

	public void PlaySampleAtNormalizedPosition(float normalizedPosition)
	{
		if (!(source.clip == null))
		{
			source.time = Mathf.Clamp(source.clip.length * normalizedPosition, 0f, source.clip.length - 0.05f);
			if (!source.isPlaying)
			{
				source.Play();
			}
			else
			{
				source.UnPause();
			}
			StartCoroutine(_BeginScrub());
		}
	}

	private void Awake()
	{
		state = (source.isPlaying ? AudioPlayState.Playing : AudioPlayState.Stopped);
	}

	private void Update()
	{
		justStopped = false;
		if (!source.isPlaying && state == AudioPlayState.Playing)
		{
			Stop();
		}
	}
}
