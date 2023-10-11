using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FileBrowserAudio : FileBrowserItem
{
	private Action<object> _onContent;

	private AudioClipWithWaveForm _audioClipAndWaveForm;

	private IEnumerator _RequestClip()
	{
		using (_request = UnityWebRequestMultimedia.GetAudioClip(_uri, AudioType.UNKNOWN))
		{
			yield return _request.SendWebRequest();
			AudioClip content = DownloadHandlerAudioClip.GetContent(_request);
			if ((bool)content)
			{
				_SetAudioClip(content);
			}
			_request = null;
		}
		_DeleteTempFile();
	}

	private IEnumerator _GetClip()
	{
		_tempFileName = "";
		_uri = base.uri;
		if (!IOUtil.UNITY_RUNTIME_AUDIO_EXTENSIONS.Contains(base.extension))
		{
			_tempFileName = IOUtil.GetUniqueTempFilepath("Process", ".wav");
			yield return ToBackgroundThread.Create();
			FFMpeg.Convert(base.filePath, _tempFileName);
			yield return ToMainThread.Create();
			_uri = IOUtil.ToURI(_tempFileName);
		}
		if ((bool)this && base.isActiveAndEnabled)
		{
			StartCoroutine(_RequestClip());
		}
		else
		{
			_DeleteTempFile();
		}
	}

	private void _SetAudioClip(AudioClip audioClip)
	{
		audioClip.name = base.fileName;
		if (_audioClipAndWaveForm != null)
		{
			_audioClipAndWaveForm.clip = audioClip;
		}
		else
		{
			UnityEngine.Object.Destroy(audioClip);
		}
	}

	private void _SetWaveForm(Texture2D texture)
	{
		if (_audioClipAndWaveForm != null)
		{
			_audioClipAndWaveForm.texture = texture;
		}
		else
		{
			UnityEngine.Object.Destroy(texture);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_audioClipAndWaveForm != null)
		{
			_audioClipAndWaveForm.Destroy();
		}
		_audioClipAndWaveForm = null;
	}

	private void Update()
	{
		if (_onContent != null && (bool)_audioClipAndWaveForm)
		{
			_onContent(_audioClipAndWaveForm);
			_onContent = null;
		}
	}

	public override void RequestPreviewContent(Action<object> onContent)
	{
		if (_audioClipAndWaveForm == null)
		{
			_audioClipAndWaveForm = new AudioClipWithWaveForm();
			Job.Process(_GetClip());
			FFMpeg.GetWaveFormTexture(base.filePath).Immediately().ResultAction<Texture2D>(_SetWaveForm);
		}
		_onContent = onContent;
	}
}
