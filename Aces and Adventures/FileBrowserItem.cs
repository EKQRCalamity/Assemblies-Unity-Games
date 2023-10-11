using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(SelectableItem))]
public abstract class FileBrowserItem : MonoBehaviour
{
	[SerializeField]
	private string _filePath;

	[Header("Events")]
	public StringEvent onNameChange;

	protected string _tempFileName;

	protected string _uri;

	protected UnityWebRequest _request;

	public string filePath
	{
		get
		{
			return _filePath;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _filePath, value) && File.Exists(value))
			{
				_OnFilePathChange();
			}
		}
	}

	public string uri => IOUtil.ToURI(filePath);

	public string fileName => Path.GetFileName(filePath);

	public string directory => Path.GetDirectoryName(filePath);

	public string extension => Path.GetExtension(filePath);

	protected virtual void OnDisable()
	{
		_filePath = null;
		if (_request != null)
		{
			_request.Abort();
		}
		_request = null;
	}

	protected void _DeleteTempFile()
	{
		if (File.Exists(_tempFileName))
		{
			File.Delete(_tempFileName);
		}
	}

	protected virtual void _OnFilePathChange()
	{
		onNameChange.Invoke(Path.GetFileName(filePath));
		TooltipCreator.CreateTextTooltip(base.transform, filePath ?? "", beginShowTimer: false, 0.5f, backgroundEnabled: true, TextAlignmentOptions.Center, 0f, 12f, TooltipDirection.Vertical);
	}

	public virtual void RequestPreviewContent(Action<object> onContent)
	{
	}

	public FileBrowserItem SetData(string filePath)
	{
		this.filePath = filePath;
		return this;
	}

	public static implicit operator string(FileBrowserItem item)
	{
		if (!item)
		{
			return "";
		}
		return item.filePath;
	}
}
