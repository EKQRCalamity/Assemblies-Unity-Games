using System;
using UnityEngine;

[RequireComponent(typeof(AudioSourcePlayer))]
public class AudioPlayerControl : MonoBehaviour
{
	public const float SET_POSITION_DELTA_THRESHOLD = 0.05f;

	[SerializeField]
	protected AudioClip _clip;

	protected TimeSpan _clipTimeSpan;

	protected Vector2 _currentZoom;

	public FloatEvent OnAudioPositionChanged;

	public BoolEvent OnIsPlayingChanged;

	public BoolEvent OnIsLoopingChanged;

	public AudioClipEvent OnAudioClipChanged;

	public StringEvent OnNameChanged;

	public StringEvent OnLengthStringChanged;

	public Vector2Event onZoomRangeChanged;

	protected AudioSourcePlayer _player;

	public Vector2 currentZoom
	{
		get
		{
			return _currentZoom;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _currentZoom, value))
			{
				onZoomRangeChanged.Invoke(value);
			}
		}
	}

	protected void _OnAudioClipChanged()
	{
		if (_clip != null && _player.source.clip != _clip)
		{
			_player.Stop();
			_player.source.clip = _clip;
			OnAudioClipChanged.Invoke(_clip);
			OnIsPlayingChanged.Invoke(arg0: false);
			currentZoom = new Vector2(0f, _clip.length);
			_OnClipChangedUnique();
			OnAudioPositionChanged.Invoke(_GetPlayFromBeginningsTime());
			OnNameChanged.Invoke(_clip.name);
			_clipTimeSpan = TimeSpan.FromSeconds(_clip.length);
			OnLengthStringChanged.Invoke(_GetClipLengthString());
		}
		else if (!_clip)
		{
			_player.Stop();
			_player.source.clip = null;
			OnNameChanged.Invoke("<i>No Audio Clip Selected</i>");
			OnLengthStringChanged.Invoke("");
		}
	}

	protected virtual string _GetClipLengthString()
	{
		return $"{_clipTimeSpan.Minutes:D2}:{_clipTimeSpan.Seconds:D2}.{_clipTimeSpan.Milliseconds:D3}";
	}

	protected virtual float _GetPlayFromBeginningsTime()
	{
		return 0f;
	}

	protected virtual void _OnClipChangedUnique()
	{
	}

	public void PlayFromBeginning()
	{
		_player.source.time = _GetPlayFromBeginningsTime();
		SetIsPlaying(isPlaying: true);
	}

	public void Stop()
	{
		SetIsPlaying(isPlaying: false);
		_player.source.time = _GetPlayFromBeginningsTime();
	}

	public void SetIsPlaying(bool isPlaying)
	{
		if (_player.state == AudioPlayState.Playing != isPlaying)
		{
			OnIsPlayingChanged.Invoke(isPlaying);
		}
		_player.SetPause(!isPlaying);
		if (isPlaying && (bool)_clip)
		{
			OnLengthStringChanged.Invoke(_GetClipLengthString());
		}
		else if ((bool)_clip)
		{
			OnLengthStringChanged.Invoke(_player.source.time.ToString("N2"));
		}
	}

	public void SetIsLooping(bool isLooping)
	{
		if (_player.loop != isLooping)
		{
			_player.loop = isLooping;
			OnIsLoopingChanged.Invoke(isLooping);
		}
	}

	public void SetAudioPositon(float position)
	{
		if (_player.source.clip == null)
		{
			return;
		}
		float position2 = _player.position;
		if (_player.state == AudioPlayState.Playing)
		{
			if (!(Math.Abs(position2 - position) <= 0.05f))
			{
				_player.source.time = Mathf.Clamp(position, 0f, _player.source.clip.length - 0.05f);
			}
		}
		else if (!_player.justStopped)
		{
			_player.PlaySampleAtNormalizedPosition(position / _player.source.clip.length);
			OnLengthStringChanged?.Invoke(position.ToString("N2"));
		}
	}

	public void SetAudioClip(AudioClip clip)
	{
		_clip = clip;
		_OnAudioClipChanged();
	}

	protected virtual void Awake()
	{
		_player = GetComponent<AudioSourcePlayer>();
		_player.onSoundFinished.AddListener(delegate
		{
			OnIsPlayingChanged.Invoke(arg0: false);
			OnAudioPositionChanged.Invoke(0f);
		});
		_player.source.bypassEffects = true;
		_player.source.bypassListenerEffects = true;
		_player.source.bypassReverbZones = true;
		_player.source.spatialBlend = 0f;
	}

	protected virtual void Start()
	{
		_player.source.clip = null;
		SetIsLooping(_player.loop);
		SetAudioClip(_clip);
	}

	protected virtual void Update()
	{
		if (_player.state == AudioPlayState.Playing)
		{
			OnAudioPositionChanged.Invoke(_player.position);
		}
	}
}
