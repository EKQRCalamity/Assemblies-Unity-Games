using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AudioEditorControl : AudioPlayerControl
{
	[SerializeField]
	[Range(0f, 600f)]
	private float _maxLength = 5f;

	public float normalizedSnapDistance = 0.005f;

	public string inputPath;

	public string outputPath;

	[Range(0f, 10f)]
	public int oggQuality;

	public bool forceMono;

	public float? loudnessTarget;

	public float maxPeak;

	public bool dualMono;

	public FloatEvent onClipLengthChanged;

	public FloatEvent onMaxLengthChanged;

	public BoolEvent onExceedsMaxLengthChanged;

	public FloatEvent onSnapDistanceChanged;

	public FloatEvent onTrimStartChanged;

	public FloatEvent onTrimEndChanged;

	public FloatEvent onFadeInEndChanged;

	public FloatEvent onFadeOutStartChanged;

	private float _clipLength;

	private float _snapDistance;

	private float _trimStart;

	private float _trimEnd;

	private float _fadeInEnd;

	private float _fadeOutStart;

	public UnityEvent onFinishedEditting;

	protected float clipLength
	{
		get
		{
			return _clipLength;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _clipLength, value))
			{
				onClipLengthChanged.Invoke(value);
			}
		}
	}

	protected float snapDistance
	{
		get
		{
			return _snapDistance;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _snapDistance, value))
			{
				onSnapDistanceChanged.Invoke(value);
			}
		}
	}

	public float maxLength
	{
		get
		{
			return _maxLength;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _maxLength, value))
			{
				onMaxLengthChanged.Invoke(value);
			}
		}
	}

	public float trimStart
	{
		get
		{
			return _trimStart;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _trimStart, value))
			{
				onTrimStartChanged.Invoke(value);
				_UpdateLengthString();
			}
		}
	}

	public float trimEnd
	{
		get
		{
			return _trimEnd;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _trimEnd, value))
			{
				onTrimEndChanged.Invoke(value);
				_UpdateLengthString();
			}
		}
	}

	public float fadeInEnd
	{
		get
		{
			return _fadeInEnd;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _fadeInEnd, value))
			{
				onFadeInEndChanged.Invoke(value);
			}
		}
	}

	public float fadeOutStart
	{
		get
		{
			return _fadeOutStart;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _fadeOutStart, value))
			{
				onFadeOutStartChanged.Invoke(value);
			}
		}
	}

	public bool hasEdits
	{
		get
		{
			if (trimStart == 0f && trimEnd == clipLength && fadeInEnd == 0f)
			{
				return fadeOutStart != clipLength;
			}
			return true;
		}
	}

	protected override void _OnClipChangedUnique()
	{
		if (!(_clip == null))
		{
			clipLength = _clip.length;
			trimStart = 0f;
			fadeInEnd = 0f;
			trimEnd = ((_maxLength == 0f) ? _clipLength : Math.Min(_maxLength, _clipLength));
			fadeOutStart = _trimEnd;
			onExceedsMaxLengthChanged.Invoke(clipLength >= maxLength);
		}
	}

	protected override string _GetClipLengthString()
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(_trimEnd - trimStart);
		return "[" + $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}" + "] / [" + base._GetClipLengthString() + "]";
	}

	private void _UpdateLengthString()
	{
		OnLengthStringChanged.Invoke(_GetClipLengthString());
	}

	protected override float _GetPlayFromBeginningsTime()
	{
		return _trimStart;
	}

	public void SetZoom(bool zoomIn)
	{
		base.currentZoom = (zoomIn ? new Vector2(trimStart, trimEnd) : new Vector2(0f, clipLength));
	}

	private void _ApplyEdits()
	{
		string text = IOUtil.ChangeFileName(outputPath, "tempAudioClipAndFade");
		text = FFMpeg.Transcode(inputPath, text, null, forceMono, null, (trimStart > 0f) ? new float?(trimStart) : null, (trimEnd < clipLength || fadeOutStart < clipLength) ? new float?(trimEnd) : null, (fadeInEnd != trimStart) ? new float?(fadeInEnd) : null, (fadeOutStart != trimEnd) ? new float?(fadeOutStart) : null);
		string text2 = text;
		string text3 = outputPath;
		int? quality = oggQuality;
		bool num = forceMono;
		float? normalizeLUFS = loudnessTarget;
		float? num2 = maxPeak;
		bool flag = dualMono;
		FFMpeg.Transcode(text2, text3, quality, num, normalizeLUFS, null, null, null, null, num2, flag);
	}

	public void ApplyEdits()
	{
		if (hasEdits)
		{
			_ApplyEdits();
		}
	}

	public IEnumerator ApplyEditsProcess()
	{
		yield return ToBackgroundThread.Create();
		if (hasEdits)
		{
			_ApplyEdits();
			yield break;
		}
		string text = outputPath;
		string text2 = outputPath;
		int? quality = oggQuality;
		bool num = forceMono;
		float? normalizeLUFS = loudnessTarget;
		float? num2 = maxPeak;
		bool flag = dualMono;
		FFMpeg.Transcode(text, text2, quality, num, normalizeLUFS, null, null, null, null, num2, flag);
	}

	protected override void Update()
	{
		_player.source.volume = 1f;
		if (_clip != null && _player.state == AudioPlayState.Playing)
		{
			if (_player.source.time < _trimStart)
			{
				_player.source.time = _trimStart;
			}
			else if (_player.source.time > _trimEnd)
			{
				if (_player.loop)
				{
					_player.source.time = _trimStart;
				}
				else
				{
					_player.Stop();
				}
			}
			float time = _player.source.time;
			if (time < _fadeInEnd)
			{
				_player.source.volume = Mathf.Clamp01((time - _trimStart) / (_fadeInEnd - _trimStart));
			}
			else if (time > _fadeOutStart)
			{
				_player.source.volume = Mathf.Clamp01(1f - (time - _fadeOutStart) / (_trimEnd - _fadeOutStart));
			}
			snapDistance = 0f;
		}
		else
		{
			snapDistance = normalizedSnapDistance;
		}
		base.Update();
	}
}
