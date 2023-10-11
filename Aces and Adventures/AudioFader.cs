using UnityEngine;

public class AudioFader : MonoBehaviour
{
	private enum FadeType
	{
		None,
		Play,
		Resume,
		Pause,
		Stop
	}

	[Range(0f, 5f)]
	public float fadeDuration = 0.5f;

	[Range(0f, 1f)]
	public float volume = 1f;

	private AudioSource _audioSource;

	private FadeType _activeFade;

	private float _activeFadeDuration;

	private float _timeAtStartFade;

	public AudioSource audioSource
	{
		get
		{
			if (!(_audioSource != null))
			{
				return _audioSource = base.gameObject.AddComponent<AudioSource>();
			}
			return _audioSource;
		}
		set
		{
			_audioSource = value;
		}
	}

	public bool isPlaying
	{
		get
		{
			if (audioSource.IsPlayingOrPaused())
			{
				return _activeFade <= FadeType.Resume;
			}
			return false;
		}
	}

	private float _currentFadeLerp
	{
		get
		{
			if (!(_activeFadeDuration <= 0f))
			{
				return Mathf.Clamp01((Time.unscaledTime - _timeAtStartFade) / _activeFadeDuration);
			}
			return 1f;
		}
	}

	private float _currentVolumeLerp => _currentFadeLerp.OneMinusIf(_activeFade > FadeType.Resume);

	private bool _BeginFade(FadeType type, float? fadeDurationOverride = null)
	{
		float num = (audioSource.isPlaying ? _currentVolumeLerp : 0f);
		_activeFade = type;
		if (!audioSource.isPlaying)
		{
			if (_activeFade == FadeType.Play)
			{
				audioSource.Play();
			}
			else if (_activeFade == FadeType.Resume)
			{
				audioSource.UnPause();
			}
		}
		_timeAtStartFade = Time.unscaledTime;
		_activeFadeDuration = Mathf.Max(0f, fadeDurationOverride ?? fadeDuration);
		float currentVolumeLerp = _currentVolumeLerp;
		_timeAtStartFade -= Mathf.Abs(currentVolumeLerp - num) * _activeFadeDuration;
		Update();
		return true;
	}

	private void Update()
	{
		if (_activeFade == FadeType.None)
		{
			return;
		}
		audioSource.volume = _currentVolumeLerp * volume;
		if (_currentFadeLerp >= 1f)
		{
			if (_activeFade == FadeType.Pause)
			{
				audioSource.Pause();
			}
			else if (_activeFade == FadeType.Stop)
			{
				audioSource.Stop();
			}
			_activeFade = FadeType.None;
		}
	}

	private void OnDisable()
	{
		_activeFade = FadeType.None;
	}

	public void Play(AudioClip clip, float startAtTime = 0f, float? fadeDurationOverride = null)
	{
		audioSource.clip = clip;
		audioSource.time = Mathf.Clamp(startAtTime, 0f, clip.length);
		_BeginFade(FadeType.Play, fadeDurationOverride);
	}

	public bool Resume(float? fadeDurationOverride = null)
	{
		if (!isPlaying)
		{
			return _BeginFade(FadeType.Resume, fadeDurationOverride);
		}
		return false;
	}

	public bool Pause(float? fadeDurationOverride = null)
	{
		if (isPlaying)
		{
			return _BeginFade(FadeType.Pause, fadeDurationOverride);
		}
		return false;
	}

	public bool Stop(float? fadeDurationOverride = null)
	{
		if (isPlaying)
		{
			return _BeginFade(FadeType.Stop, fadeDurationOverride);
		}
		return false;
	}
}
