using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class AudioRefControl : MonoBehaviour
{
	private static readonly Vector2[] EMPTY_WAVE_FORM = new Vector2[1] { Vector2.zero };

	[Header("Buttons")]
	public GameObject clearButton;

	public GameObject overwriteButton;

	public GameObject renameButton;

	public GameObject editTagsButton;

	public GameObject uploadButton;

	public GameObject downloadButton;

	public GameObject inspectButton;

	public AudioCategoryType category;

	public AudioCategoryType? defaultCategory;

	public AudioClipEvent onAudioClipChanged;

	public UnityEvent onPlayRequested;

	public UnityEvent onStopRequested;

	public ObjectEvent onAudioRefChanged;

	public Vector2ArrayEvent onAudioVertexDataChanged;

	public Action onValidateImportCategory;

	private AudioRef _audioRef;

	private bool _isOverwrite;

	public AudioRef audioRef => _audioRef;

	private void _GetAudioVertexData()
	{
		_audioRef.GetWaveFormData(delegate(Vector2[] vData)
		{
			onAudioVertexDataChanged.Invoke(vData);
		});
	}

	private void _RefreshAudioRef()
	{
		AudioRef audioRef = _audioRef;
		SetAudioRef(new AudioRef(category), requestPlay: false);
		SetAudioRef(audioRef, requestPlay: false);
	}

	private void _UpdateButtons()
	{
		if ((bool)this)
		{
			bool flag = _audioRef;
			bool active = flag && _audioRef.belongsToCurrentCreator;
			clearButton.SetActive(flag);
			overwriteButton.SetActive(active);
			renameButton.SetActive(active);
			editTagsButton.SetActive(active);
			uploadButton.SetActive(_audioRef.CanUpload());
			downloadButton.SetActive(Steam.CanUseWorkshop);
			inspectButton.SetActive(_audioRef.CanInspectWorkshopItem());
		}
	}

	private void _OnImport(string path)
	{
		UIUtil.ConfirmImportPopup(path, OnImport, _SetCategory, _isOverwrite, category, defaultCategory, "Audio", base.transform);
	}

	private void _OnUpload(bool success)
	{
		if (success)
		{
			_UpdateButtons();
		}
	}

	public void SetAudioRef(AudioRef audioRef, bool requestPlay = true)
	{
		if (ReflectionUtil.SafeEquals(_audioRef, audioRef))
		{
			return;
		}
		audioRef = ProtoUtil.Clone(audioRef);
		_audioRef = audioRef;
		_audioRef.category = category;
		if (_audioRef.IsValid())
		{
			_audioRef.GetAudioClip(delegate(AudioClip clip)
			{
				onAudioRefChanged.Invoke(_audioRef);
				clip.name = _audioRef.name;
				onAudioClipChanged.Invoke(clip);
				_GetAudioVertexData();
				if (requestPlay)
				{
					onPlayRequested.Invoke();
				}
			});
		}
		else
		{
			onAudioRefChanged.Invoke(_audioRef);
			onAudioClipChanged.Invoke(null);
			onAudioVertexDataChanged.Invoke(EMPTY_WAVE_FORM);
		}
		_UpdateButtons();
	}

	private void _SetCategory(AudioCategoryType newCategory)
	{
		if (newCategory != category)
		{
			category = newCategory;
			_audioRef = new AudioRef(newCategory);
		}
	}

	public void SetCategoryAndPickFirstAudio(AudioCategoryType newCategory)
	{
		if (newCategory != category)
		{
			category = newCategory;
			SetAudioRef(AudioRef.Search(newCategory).FirstOrDefault() ?? new AudioRef(newCategory), requestPlay: false);
		}
	}

	public void OnSelectSound()
	{
		onStopRequested.Invoke();
		UIUtil.CreateAudioSearchPopup(category, delegate(AudioRef aRef)
		{
			SetAudioRef(aRef);
		}, base.transform);
	}

	public void ShowBrowser()
	{
		onStopRequested.Invoke();
		UIUtil.CreateAudioBrowserPopup(category, _OnImport, base.transform, _isOverwrite ? this : null);
	}

	public void SetIsOverwrite(bool isOverwrite)
	{
		_isOverwrite = isOverwrite;
	}

	public void OnImport(string path)
	{
		AudioRef audioRef = ((_isOverwrite && (bool)_audioRef) ? _audioRef._Reference<AudioRef>() : null);
		_audioRef = audioRef ?? new AudioRef(_audioRef.category);
		UIUtil.BeginProcessJob(base.transform).Afterward().DoJob(_audioRef.Import(path))
			.Afterward()
			.DoJob(_audioRef.RetrieveContent())
			.Afterward()
			.ResultAction(delegate(AudioClip clip)
			{
				clip.name = _audioRef.friendlyName;
				GameObject editor = UIUtil.CreateAudioEditor(clip, path, _audioRef.quality, _audioRef.savePath, _audioRef.maxClipLength, _audioRef.forceMono, _audioRef.loudnessTarget, _audioRef.maxPeak, _audioRef.dualMono, "UI/Audio Editor Standard", base.transform);
				UIUtil.CreatePopup("Trim and Fade Audio", editor, null, null, null, null, null, delegate
				{
					UIUtil.BeginProcessJob(base.transform).Afterward().DoProcess(editor.GetComponentInChildren<AudioEditorControl>().ApplyEditsProcess())
						.Afterward()
						.DoJob(_audioRef.Save())
						.Afterward()
						.DoJob(_audioRef.ReloadContent())
						.Afterward()
						.ResultAction<AudioClip>(delegate
						{
							AudioRef audioRef2 = _audioRef;
							_audioRef = null;
							onAudioClipChanged.Invoke(null);
							SetAudioRef(audioRef2);
							if (onValidateImportCategory != null)
							{
								onValidateImportCategory();
							}
							_UpdateButtons();
						})
						.Afterward()
						.Do(UIUtil.EndProcess);
				}, false, true, null, null, null, base.transform, null, null, "Finished Editing");
			})
			.Afterward()
			.Do(UIUtil.EndProcess);
	}

	public void OnEditTags()
	{
		if ((bool)_audioRef)
		{
			UIUtil.CreateContentRefTagsPopup(_audioRef, base.transform);
		}
	}

	public void OnRename()
	{
		if ((bool)_audioRef)
		{
			UIUtil.CreateContentRefRenamePopup(_audioRef, base.transform, delegate
			{
				_RefreshAudioRef();
			});
		}
	}

	public void Clear()
	{
		SetAudioRef(new AudioRef(category), requestPlay: false);
	}

	public void Upload()
	{
		if (_audioRef.CanUpload())
		{
			UIUtil.CreateContentRefUploadPopup(_audioRef, base.transform, _OnUpload);
		}
	}

	public void Download()
	{
		UIUtil.CreateWorkshopAudioSearchPopup(_audioRef, delegate(AudioRef aRef)
		{
			SetAudioRef(aRef);
		}, base.transform);
	}

	public void InspectWorkshopItem()
	{
		if (_audioRef.CanInspectWorkshopItem())
		{
			UIUtil.CreateSteamWorkshopItemInspectPopup(_audioRef.workshopFileId, base.transform);
		}
	}

	private void Awake()
	{
		DataRefControl.SetWorkshopButtonTooltips(uploadButton, downloadButton, inspectButton);
	}

	private void Start()
	{
		_audioRef = _audioRef ?? new AudioRef(category);
		_UpdateButtons();
	}
}
