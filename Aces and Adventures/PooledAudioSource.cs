using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(AudioFader))]
public class PooledAudioSource : MonoBehaviour
{
	public const long NULL_PLAY_ID = 0L;

	private static long PlayID;

	private AudioSource _source;

	private AudioFader _fader;

	public AudioSource source => _source;

	public float volume
	{
		get
		{
			return _source.volume;
		}
		set
		{
			AudioFader fader = _fader;
			float num2 = (_source.volume = value);
			fader.volume = num2;
		}
	}

	public Transform attachTo { get; set; }

	public long playId { get; private set; }

	public bool isPlaying => _fader.isPlaying;

	public float playTimeRatio => _source.PlayTimeRatio();

	private void Awake()
	{
		_source = GetComponent<AudioSource>();
		_fader = GetComponent<AudioFader>();
		_fader.audioSource = _source;
	}

	private void LateUpdate()
	{
		if ((bool)attachTo)
		{
			base.transform.position = attachTo.position;
		}
		if (!source.IsPlayingOrPaused())
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		attachTo = null;
		source.clip = null;
	}

	public void Play(AudioClip clip, float startAtTime = 0f, float? fadeDurationOverride = null)
	{
		playId = ++PlayID;
		playId = ((playId != 0L) ? playId : (playId + 1));
		_fader.Play(clip, startAtTime, fadeDurationOverride);
	}

	public void Stop(float? fadeDurationOverride = null)
	{
		_fader.Stop(fadeDurationOverride);
	}

	public PooledAudioSource StopAndClear(float? fadeDurationOverride = null)
	{
		Stop(fadeDurationOverride);
		return null;
	}
}
