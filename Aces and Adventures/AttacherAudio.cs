using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AttacherAudio : Attacher
{
	[Header("Audio=======================================================================================================")]
	public bool playOnEnable = true;

	public AttacherCurveData volume;

	public AttacherCurveData pitch;

	private AudioSource _audioSource;

	private bool _clearSpeedRelativeToOnDisable;

	public AudioSource audioSource
	{
		get
		{
			if (_audioSource == null)
			{
				_audioSource = GetComponent<AudioSource>();
				if (volume != null)
				{
					volume.initialValue = _audioSource.volume;
				}
				if (pitch != null)
				{
					pitch.initialValue = _audioSource.pitch;
				}
			}
			return _audioSource;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_clearSpeedRelativeToOnDisable)
		{
			relativeSpeed.relativeTo = null;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_clearSpeedRelativeToOnDisable = false;
		if (!relativeSpeed.relativeTo)
		{
			_clearSpeedRelativeToOnDisable = true;
			AudioListener componentInChildren = CameraManager.Instance.mainCamera.GetComponentInChildren<AudioListener>(includeInactive: true);
			relativeSpeed.relativeTo = (componentInChildren ? componentInChildren.transform : null);
		}
		if (playOnEnable && !audioSource.isPlaying)
		{
			audioSource.Play();
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (_samplesDirty)
		{
			_OnSamplesChange();
		}
	}

	protected override void _OnSamplesChange()
	{
		AudioSource audioSource = this.audioSource;
		if (volume.enabled)
		{
			audioSource.volume = volume.GetSampleValue(this);
		}
		if (pitch.enabled)
		{
			audioSource.pitch = pitch.GetSampleValue(this);
		}
	}

	public AttacherAudio ApplySettings(System.Random random, IAttacherAudioSettings settings)
	{
		settings.Apply(random, this);
		return this;
	}
}
