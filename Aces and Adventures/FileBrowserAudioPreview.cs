using System;
using UnityEngine;

public class FileBrowserAudioPreview : MonoBehaviour
{
	private static PooledAudioPlayer.ActivePooledAudioSource _ActiveAudioSource;

	public AudioClipEvent onAudioClipChange;

	public StringEvent onNameChange;

	public StringEvent onLengthChange;

	public Vector2ArrayEvent onAudioVertexDataChanged;

	public Texture2DEvent onWaveFormImageChanged;

	public BoolEvent onIsPlayingChange;

	private AudioClip _audioClip;

	private Vector2[] _waveFormData;

	private PooledAudioPlayer.ActivePooledAudioSource _audioSource;

	private bool _isPlaying;

	public AudioClip audioClip
	{
		get
		{
			return _audioClip;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _audioClip, value))
			{
				_OnAudioClipChange();
			}
		}
	}

	public bool isPlaying => _isPlaying;

	private void _OnAudioClipChange()
	{
		if ((bool)_audioSource)
		{
			_audioSource.Stop();
		}
		onAudioClipChange.Invoke(audioClip);
		if ((bool)audioClip)
		{
			onNameChange.Invoke(audioClip.name);
			TimeSpan timeSpan = TimeSpan.FromSeconds(audioClip.length);
			onLengthChange.Invoke($"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}");
			if (_waveFormData == null)
			{
				UIAudioMesh.GetUnitizedBarGraph(ref _waveFormData, audioClip, 300);
			}
			else if (_waveFormData.Length != 0)
			{
				onAudioVertexDataChanged.Invoke(_waveFormData);
			}
		}
	}

	private void _PlayClip()
	{
		if ((bool)audioClip)
		{
			_ActiveAudioSource.Stop();
			_audioSource = (_ActiveAudioSource = AudioPool.Instance.Play(audioClip, MasterMixManager.UI));
		}
	}

	private void OnEnable()
	{
		onIsPlayingChange.Invoke(_isPlaying = false);
	}

	private void OnDisable()
	{
		_audioClip = null;
		_waveFormData = null;
		_OnAudioClipChange();
	}

	private void Update()
	{
		if (SetPropertyUtility.SetStruct(ref _isPlaying, _audioSource == _ActiveAudioSource && (bool)_ActiveAudioSource))
		{
			onIsPlayingChange.Invoke(isPlaying);
		}
	}

	public void PreviewClip(AudioClip audioClip, bool playClip)
	{
		this.audioClip = audioClip;
		if (playClip)
		{
			_PlayClip();
		}
	}

	public void PreviewAudioRef(AudioRefControl audioRefControl, bool playClip)
	{
		audioRefControl.audioRef.GetWaveFormData(delegate(Vector2[] waveForm)
		{
			_waveFormData = waveForm;
		}, forceImmediate: true);
		audioClip = audioRefControl.audioRef.audioClip;
		if (playClip)
		{
			_PlayClip();
		}
	}

	public void PreviewClipWithWaveForm(AudioClipWithWaveForm clipWithWaveForm, bool playClip)
	{
		onWaveFormImageChanged.Invoke(clipWithWaveForm);
		_waveFormData = new Vector2[0];
		audioClip = clipWithWaveForm;
		if (playClip)
		{
			_PlayClip();
		}
	}

	public void PlayButtonClick()
	{
		_ActiveAudioSource.Stop();
		if (!isPlaying)
		{
			_PlayClip();
		}
	}
}
