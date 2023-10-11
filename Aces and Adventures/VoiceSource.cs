using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class VoiceSource : MonoBehaviour
{
	private const float INTERRUPT_TIME = 0.2f;

	public const long NULL_PLAY_ID = 0L;

	private static long PlayID;

	[Range(0f, 1f)]
	public float maxSpatialBlend = 0.5f;

	[Range(1f, 10f)]
	public float spatialBlendDistance = 3f;

	private AudioSource _source;

	private float _volume;

	private float _timeAtInterrupt;

	public AudioSource source => _source;

	protected Transform _sourceTransform { get; private set; }

	protected float _interruptTime => Mathf.Max(0f, Time.unscaledTime - _timeAtInterrupt);

	public bool finished
	{
		get
		{
			if (source.IsPlayingOrPaused() && !(_interruptTime > 0f))
			{
				return !base.isActiveAndEnabled;
			}
			return true;
		}
	}

	public float spatialBlend
	{
		get
		{
			return _source.spatialBlend;
		}
		set
		{
			_source.spatialBlend = value;
		}
	}

	public AudioMixerGroup outputGroup
	{
		get
		{
			return _source.outputAudioMixerGroup;
		}
		set
		{
			_source.outputAudioMixerGroup = value;
		}
	}

	public long playId { get; private set; }

	protected float _dynamicVolume => _volume;

	private void Awake()
	{
		_source = GetComponent<AudioSource>();
	}

	private void OnEnable()
	{
		_timeAtInterrupt = float.MaxValue;
	}

	private void LateUpdate()
	{
		if ((bool)_sourceTransform)
		{
			base.transform.position = _sourceTransform.position;
		}
		float interruptTime = _interruptTime;
		if (interruptTime > 0f)
		{
			float num = Mathf.Clamp01(interruptTime / 0.2f);
			source.volume = _dynamicVolume * (1f - num);
			if (num >= 1f)
			{
				_Finish();
				return;
			}
		}
		else
		{
			source.volume = _dynamicVolume;
		}
		if (!source.IsPlayingOrPaused())
		{
			_Finish();
		}
		_UpdateSpatialBlend();
	}

	private void _Finish()
	{
		source.Stop();
		base.gameObject.SetActive(value: false);
	}

	private void _UpdateSpatialBlend()
	{
		if (spatialBlend > 0f && (bool)CameraManager.Instance.audioListener)
		{
			spatialBlend = Mathf.Lerp(0.0001f, maxSpatialBlend, (base.transform.position - CameraManager.Instance.audioListener.transform.position).magnitude / spatialBlendDistance);
		}
	}

	public void Play(SoundPack.SoundData soundData, Transform sourceTransform, float? pitchShift = null)
	{
		playId = ++PlayID;
		playId = ((playId != 0L) ? playId : (playId + 1));
		_sourceTransform = sourceTransform;
		_volume = soundData.volume;
		source.volume = _dynamicVolume;
		_UpdateSpatialBlend();
		source.pitch = pitchShift ?? 1f;
		source.clip = soundData.audioRef.audioClip;
		source.Play();
	}

	public void Interrupt()
	{
		_timeAtInterrupt = Time.unscaledTime;
	}

	public VoiceSource SetVolume(float volume)
	{
		_volume = volume;
		return this;
	}
}
