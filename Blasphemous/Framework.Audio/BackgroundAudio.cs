using System;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using UnityEngine;

namespace Framework.Audio;

[RequireComponent(typeof(BoxCollider2D))]
public class BackgroundAudio : MonoBehaviour
{
	public const string VolumeParamLabel = "Volume";

	private float _desiredVolume;

	private EventInstance _eventInstance;

	private bool _fading;

	private float _interpolatingTime;

	private ParameterInstance _volumeParameter;

	[SerializeField]
	[EventRef]
	private string audio;

	[Range(0f, 1f)]
	public float FadingSpeed = 0.75f;

	public LayerMask TargetLayer;

	protected float volume;

	public float Volume
	{
		get
		{
			_volumeParameter.getValue(out volume);
			return volume;
		}
		set
		{
			if (_volumeParameter.isValid())
			{
				float value2 = Mathf.Clamp01(value);
				_volumeParameter.setValue(value2);
			}
		}
	}

	private void Start()
	{
		_eventInstance = Core.Audio.CreateEvent(audio);
		if (!_eventInstance.isValid())
		{
			return;
		}
		try
		{
			_eventInstance.getParameter("Volume", out _volumeParameter);
			volume = 1f;
			_volumeParameter.setValue(volume);
			_eventInstance.start();
			Volume = 0f;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + ex.StackTrace);
		}
	}

	private void Update()
	{
		if (_fading)
		{
			_interpolatingTime += Time.deltaTime * FadingSpeed;
			Volume = Mathf.Lerp(Volume, _desiredVolume, _interpolatingTime);
		}
		if (Mathf.Approximately(Volume, _desiredVolume) && _fading)
		{
			_fading = false;
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if ((TargetLayer.value & (1 << other.gameObject.layer)) > 0)
		{
			_desiredVolume = 1f;
			if (!Mathf.Approximately(Volume, _desiredVolume) && !_fading)
			{
				_interpolatingTime = 0f;
				_fading = true;
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if ((TargetLayer.value & (1 << other.gameObject.layer)) > 0)
		{
			_desiredVolume = 0f;
			if (!_fading)
			{
				_interpolatingTime = 0f;
				_fading = true;
			}
		}
	}

	private void OnDestroy()
	{
		if (_eventInstance.isValid())
		{
			_eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_eventInstance.release();
		}
	}
}
