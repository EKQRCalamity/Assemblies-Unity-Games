using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioCurves : ACurves
{
	[Header("Audio=======================================================================================================")]
	public bool playOnEnable = true;

	public FloatCurve volume;

	public FloatCurve pitch;

	private AudioSource _audioSource;

	protected AudioSource audioSource
	{
		get
		{
			if (!(_audioSource != null))
			{
				return _InitializeValues();
			}
			return _audioSource;
		}
	}

	private AudioSource _InitializeValues()
	{
		_audioSource = GetComponent<AudioSource>();
		volume.initialValue = _audioSource.volume;
		pitch.initialValue = _audioSource.pitch;
		return _audioSource;
	}

	private void Awake()
	{
		_ = audioSource;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (playOnEnable && !audioSource.isPlaying)
		{
			audioSource.Play();
		}
	}

	protected override void _Input(float t)
	{
		if (volume.enabled)
		{
			audioSource.volume = volume.GetValue(t);
		}
		if (pitch.enabled)
		{
			audioSource.pitch = pitch.GetValue(t);
		}
	}
}
