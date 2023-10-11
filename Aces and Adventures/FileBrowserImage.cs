using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FileBrowserImage : FileBrowserItem
{
	[Range(128f, 4096f)]
	public int maxResolution = 512;

	public Texture2DEvent OnImageChange;

	public BoolEvent OnHasImageChange;

	private Texture2D _texture;

	private IEnumerator _RequestTexture()
	{
		using (_request = UnityWebRequestTexture.GetTexture(_uri, nonReadable: true))
		{
			yield return _request.SendWebRequest();
			_SetTexture(DownloadHandlerTexture.GetContent(_request));
			_request = null;
		}
		_DeleteTempFile();
	}

	private IEnumerator _GetTexture()
	{
		_tempFileName = "";
		_uri = base.uri;
		if (!IOUtil.UNITY_RUNTIME_IMAGE_EXTENSIONS.Contains(base.extension))
		{
			_tempFileName = IOUtil.GetUniqueTempFilepath("Process", ".png");
			yield return ToBackgroundThread.Create();
			NConvert.Convert("png", base.filePath, _tempFileName);
			yield return ToMainThread.Create();
			_uri = IOUtil.ToURI(_tempFileName);
		}
		if ((bool)this && base.isActiveAndEnabled)
		{
			StartCoroutine(_RequestTexture());
		}
		else
		{
			_DeleteTempFile();
		}
	}

	private void _SetTexture(Texture2D texture)
	{
		if ((bool)texture)
		{
			texture.name = base.fileName;
		}
		_texture = texture;
		OnImageChange.Invoke(texture);
		OnHasImageChange.Invoke(texture);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)_texture)
		{
			UnityEngine.Object.Destroy(_texture);
		}
		_SetTexture(null);
	}

	protected override void _OnFilePathChange()
	{
		base._OnFilePathChange();
		Job.Process(_GetTexture());
	}

	public override void RequestPreviewContent(Action<object> onContent)
	{
		if ((bool)_texture)
		{
			onContent(_texture);
			return;
		}
		OnImageChange.AddSingleFireListener(delegate(Texture2D texture)
		{
			onContent(texture);
		});
	}
}
