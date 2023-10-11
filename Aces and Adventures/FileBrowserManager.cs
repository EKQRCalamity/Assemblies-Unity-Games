using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FileBrowserManager : MonoBehaviour
{
	private const int MAX_DISPLAYED_DIRECTORIES = 300;

	private const int MAX_DISPLAYED_DIRECTORIES_SEARCH = 100;

	private const int SEARCHED_FILES_MAX = 10000;

	private const FileAttributes ExcludedAttributes = FileAttributes.Hidden | FileAttributes.System;

	private static HashSet<string> _ValidImageExtensions;

	private static HashSet<string> _ValidAudioExtensions;

	private static ResourceBlueprint<GameObject> _Blueprint = "UI/Content/FileBrowserManager";

	private static List<Environment.SpecialFolder> _QuickFolders;

	private static Dictionary<Environment.SpecialFolder, Sprite> _QuickFolderSpriteMap;

	[SerializeField]
	[HideInInspector]
	private FileBrowserType _type;

	[Header("Blueprints")]
	public GameObject filePathButtonBlueprint;

	public GameObject directoryBlueprint;

	public GameObject imageItemBlueprint;

	public GameObject audioItemBlueprint;

	public GameObject logicalDriveBlueprint;

	public GameObject quickDirectoryBlueprint;

	public GameObject recentDirectoryBlueprint;

	[Header("Containers")]
	public RectTransform filePathContainer;

	public RectTransform directoriesContainer;

	public SelectableGroup resultsContainer;

	public RectTransform logicalDrivesContainer;

	public RectTransform quickAccessContainer;

	public RectTransform recentDirectoryContainer;

	public RectTransform resultSortContainer;

	[Header("Search")]
	[SerializeField]
	[Range(1f, 100f)]
	private int _resultPageSize = 50;

	[Range(0f, 1f)]
	public float searchGracePeriod = 0.5f;

	[Range(0.5f, 2f)]
	public float refreshResultPeriod = 1f;

	[SerializeField]
	private ContentRefSortType _resultSortType;

	public TMP_InputField resultSearchField;

	public TMP_InputField directorySearchField;

	[Header("Preview")]
	public RectTransform overwritePreviewContainer;

	public RectTransform previewContainer;

	public GameObject imagePreviewBlueprint;

	public GameObject audioPreviewBlueprint;

	[Header("Events")]
	public BoolEvent onItemIsSelectedChange;

	public StringEvent onFileConfirm;

	public StringEvent onConfirmButtonNameChange;

	public StringEvent onResultPageTextChange;

	public StringEvent onSelectedPathChange;

	public FloatEvent onResultScaleChange;

	public BoolEvent onIsOverwriteChange;

	public BoolEvent onHasDirectoryForwardChange;

	public BoolEvent onBackGroundSearchActiveChange;

	public BoolEvent onCanSortResultsChange;

	private string _directoy;

	private int _resultPageNumber;

	private string _resultSearchText = "";

	private string _directorySearchText = "";

	private HashSet<string> _validFileExtensions;

	private string _previousResultSearchText;

	private List<DirectoryInfo> _directories;

	private List<FileInfo> _results;

	private float _resultSearchGracePeriod;

	private float _directorySearchGracePeriod;

	private bool _isDirty;

	private bool _filePathDirty;

	private bool _directoriesDirty;

	private bool _resultsDirty;

	private bool _resultsListDirty;

	private bool _hasRefreshedSinceEnable;

	private FileBrowserItem _selectedItem;

	private Stack<string> _backDirectoryStack;

	private List<FileInfo> _filesInDirectory;

	private List<FileInfo> _searchedFiles;

	private bool _backgroundSearchActive;

	private int _searchIndex;

	private FuzzyStringType _searchStringType;

	private static HashSet<string> ValidImageExtensions => _ValidImageExtensions ?? (_ValidImageExtensions = NConvert.IMPORT_IMAGE_FORMATS.ToHash(StringComparer.OrdinalIgnoreCase));

	private static HashSet<string> ValidAudioExtensions => _ValidAudioExtensions ?? (_ValidAudioExtensions = FFMpeg.IMPORT_AUDIO_FORMATS.ToHash(StringComparer.OrdinalIgnoreCase));

	private static List<Environment.SpecialFolder> QuickFolders => _QuickFolders ?? (_QuickFolders = new List<Environment.SpecialFolder>
	{
		Environment.SpecialFolder.Desktop,
		Environment.SpecialFolder.MyDocuments,
		Environment.SpecialFolder.MyPictures,
		Environment.SpecialFolder.MyMusic
	}.Where((Environment.SpecialFolder f) => !f.GetFolderPath().IsNullOrEmpty()).ToList());

	public static Dictionary<Environment.SpecialFolder, Sprite> QuickFolderSpriteMap => _QuickFolderSpriteMap ?? (_QuickFolderSpriteMap = ReflectionUtil.CreateEnumResourceMap<Environment.SpecialFolder, Sprite>("UI/Content/SpecialFolderSprites"));

	public string directory
	{
		get
		{
			return _directoy;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _directoy, Directory.Exists(value) ? value : _directoy))
			{
				_OnDirectoryChange();
			}
		}
	}

	public string resultSearchText
	{
		get
		{
			return _resultSearchText;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _resultSearchText, value.HasVisibleCharacter() ? value : ""))
			{
				_OnResultSearchTextChange();
			}
		}
	}

	public ContentRefSortType resultSortType
	{
		get
		{
			return _resultSortType;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _resultSortType, value))
			{
				_OnResultSortTypeChange();
			}
		}
	}

	public int resultPageSize
	{
		get
		{
			return _resultPageSize;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _resultPageSize, Mathf.Max(1, value)))
			{
				_SetDirty(resetPageNumber: true, setDirectoriesDirty: false, setFilePathDirty: false, setResultsDirty: true);
			}
		}
	}

	public int resultPageNumber
	{
		get
		{
			return _resultPageNumber;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _resultPageNumber, Mathf.Clamp(value, 1, maxResultsPageNumber)))
			{
				_SetDirty(resetPageNumber: false, setDirectoriesDirty: false, setFilePathDirty: false, setResultsDirty: true, setResultsListDirty: false, clearSelected: true);
			}
		}
	}

	public int maxResultsPageNumber => Math.Max(0, results.Count - 1) / resultPageSize + 1;

	public string directorySearchText
	{
		get
		{
			return _directorySearchText;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _directorySearchText, value.HasVisibleCharacter() ? value : ""))
			{
				_OnDirectorySearchTextChange();
			}
		}
	}

	public List<FileInfo> results => _results ?? (_results = new List<FileInfo>());

	public List<DirectoryInfo> directories => _directories ?? (_directories = new List<DirectoryInfo>());

	public HashSet<string> validFileExtensions => _validFileExtensions ?? (_validFileExtensions = new HashSet<string>());

	private FileBrowserItem selectedItem
	{
		get
		{
			return _selectedItem;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _selectedItem, value))
			{
				_OnSelectedItemChange();
			}
		}
	}

	private Stack<string> backDirectoryStack => _backDirectoryStack ?? (_backDirectoryStack = new Stack<string>());

	private List<FileInfo> filesInDirectory => _filesInDirectory ?? (_filesInDirectory = new List<FileInfo>());

	private List<FileInfo> searchedFiles => _searchedFiles ?? (_searchedFiles = new List<FileInfo>());

	private bool backgroundSearchActive
	{
		get
		{
			return _backgroundSearchActive;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _backgroundSearchActive, value))
			{
				onBackGroundSearchActiveChange.Invoke(value);
			}
		}
	}

	private IComparer<FileInfo> _resultSortComparer
	{
		get
		{
			if (!_searchStringType.CanSortResults())
			{
				return null;
			}
			return resultSortType switch
			{
				ContentRefSortType.Recent => FileInfoRecentComparer.Recent, 
				ContentRefSortType.Alphabetical => FileInfoNameComparer.Ascending, 
				ContentRefSortType.Old => FileInfoRecentComparer.Old, 
				_ => null, 
			};
		}
	}

	private static FileBrowserManager ShowBrowser(FileBrowserType type, string confirmButtonText, HashSet<string> validFileExtensions, Transform parent, int resultPageSize, float resultScale, UnityEngine.Object overwritten = null, string startingDirectory = null)
	{
		return Pools.Unpool(_Blueprint, parent).GetComponent<FileBrowserManager>().SetData(type, confirmButtonText, validFileExtensions, resultPageSize, resultScale, overwritten, startingDirectory);
	}

	public static FileBrowserManager ShowImageBrowser(string confirmButtonText, Transform parent, Texture2D overwritten = null, string startingDirectory = null)
	{
		return ShowBrowser(FileBrowserType.Image, confirmButtonText, ValidImageExtensions, parent, 9, 0.55f, overwritten, startingDirectory);
	}

	public static FileBrowserManager ShowAudioBrowser(string confirmButtonText, Transform parent, UnityEngine.Object overwritten = null, string startingDirectory = null)
	{
		return ShowBrowser(FileBrowserType.Audio, confirmButtonText, ValidAudioExtensions, parent, 16, 1f, overwritten, startingDirectory);
	}

	private bool _UpdateSearchGracePeriod(ref float gracePeriod, string text)
	{
		if (gracePeriod > 0f)
		{
			return (gracePeriod -= ((!Input.GetKey(KeyCode.Backspace) || !text.HasVisibleCharacter()) ? Time.unscaledDeltaTime : 0f)) <= 0f;
		}
		return false;
	}

	private void _SetDirty(bool resetPageNumber = false, bool setDirectoriesDirty = false, bool setFilePathDirty = false, bool setResultsDirty = false, bool setResultsListDirty = false, bool clearSelected = false)
	{
		_isDirty = true;
		if (resetPageNumber)
		{
			_resultPageNumber = 1;
		}
		if (setResultsDirty)
		{
			_resultsDirty = true;
		}
		if (setResultsListDirty)
		{
			_resultsListDirty = true;
		}
		if (setDirectoriesDirty)
		{
			_directoriesDirty = true;
		}
		if (setFilePathDirty)
		{
			_filePathDirty = true;
		}
		if (clearSelected)
		{
			_ClearSelectedItem();
		}
	}

	private void _SetDirectory(string newDirectory)
	{
		if (!Directory.Exists(newDirectory))
		{
			newDirectory = ProfileManager.prefs.browser.GetRecentDirectories().AsEnumerable().FirstOrDefault() ?? QuickFolders[0].GetFolderPath();
		}
		_directoy = newDirectory;
		_OnDirectoryChange();
	}

	private void _OnDirectoryChange(bool clearBackDirectoryStack = true)
	{
		_SetDirty(resetPageNumber: true, setDirectoriesDirty: true, setFilePathDirty: true, setResultsDirty: true, setResultsListDirty: true, clearSelected: true);
		directorySearchField.text = "";
		_directorySearchGracePeriod = 0f;
		if (clearBackDirectoryStack)
		{
			backDirectoryStack.Clear();
		}
		onHasDirectoryForwardChange.Invoke(backDirectoryStack.Count > 0);
		filesInDirectory.ClearAndCopyFrom(from f in new DirectoryInfo(directory).GetFiles()
			where (f.Attributes & (FileAttributes.Hidden | FileAttributes.System)) == 0
			where validFileExtensions.Contains(f.Extension)
			select f);
		Interlocked.Increment(ref _searchIndex);
		lock (searchedFiles)
		{
			searchedFiles.Clear();
		}
		Job.Process(_SearchFilesInDirectoryDeep());
	}

	private void _OnResultSearchTextChange()
	{
		_resultSearchGracePeriod = searchGracePeriod;
		_searchStringType = resultSearchText.GetSearchStringType();
		onCanSortResultsChange.Invoke(_searchStringType.CanSortResults());
		_ClearSelectedItem();
	}

	private void _OnDirectorySearchTextChange()
	{
		_directorySearchGracePeriod = searchGracePeriod;
	}

	private void _OnResultSortTypeChange()
	{
		_SortResults();
		_SetDirty(resetPageNumber: true, setDirectoriesDirty: false, setFilePathDirty: false, setResultsDirty: true, setResultsListDirty: false, clearSelected: true);
		ProfileManager.prefs.browser.SetResultSortType(resultSortType);
	}

	private void _ClearSelectedItem()
	{
		resultsContainer.ClearSelected();
		resultsContainer.ForceUpdateSelection();
	}

	private void _CreateLogicalDrives()
	{
		string[] logicalDrives = Environment.GetLogicalDrives();
		foreach (string text in logicalDrives)
		{
			if (Directory.Exists(text))
			{
				string d = text;
				GameObject obj = Pools.Unpool(logicalDriveBlueprint, logicalDrivesContainer);
				obj.GetComponentInChildren<TextMeshProUGUI>().text = d;
				PointerClick3D componentInChildren = obj.GetComponentInChildren<PointerClick3D>();
				componentInChildren.OnClick.RemoveAllListeners();
				componentInChildren.OnClick.AddListener(delegate
				{
					directory = d;
				});
			}
		}
	}

	private void _CreateQuickAccessDirectories()
	{
		foreach (Environment.SpecialFolder quickFolder in QuickFolders)
		{
			string d = quickFolder.GetFolderPath();
			GameObject obj = Pools.Unpool(quickDirectoryBlueprint, quickAccessContainer);
			obj.GetComponentInChildren<TextMeshProUGUI>().text = EnumUtil.FriendlyName(quickFolder);
			obj.GetComponentsInChildren<Image>()[1].sprite = QuickFolderSpriteMap[quickFolder];
			PointerClick3D componentInChildren = obj.GetComponentInChildren<PointerClick3D>();
			componentInChildren.OnClick.RemoveAllListeners();
			componentInChildren.OnClick.AddListener(delegate
			{
				directory = d;
			});
		}
	}

	private void _CreateRecentDirectories()
	{
		foreach (string recentDirectory in ProfileManager.prefs.browser.GetRecentDirectories())
		{
			string d = recentDirectory;
			GameObject obj = Pools.Unpool(recentDirectoryBlueprint, recentDirectoryContainer);
			obj.GetComponentInChildren<TextMeshProUGUI>().text = IOUtil.GetFolderName(recentDirectory);
			PointerClick3D componentInChildren = obj.GetComponentInChildren<PointerClick3D>();
			componentInChildren.OnClick.RemoveAllListeners();
			componentInChildren.OnClick.AddListener(delegate
			{
				directory = d;
			});
			TooltipCreator.CreateTextTooltip(obj.transform, d, beginShowTimer: false, 0.5f, backgroundEnabled: true, TextAlignmentOptions.Center, 0f);
		}
	}

	private void _Refresh()
	{
		_UpdateFilePath();
		_UpdateDirectories();
		_UpdateResults();
		_isDirty = false;
		if (!_hasRefreshedSinceEnable && (_hasRefreshedSinceEnable = true))
		{
			resultSearchField.FocusAndMoveToEnd(selectAll: true);
		}
	}

	private void _UpdateFilePath()
	{
		if (!_filePathDirty)
		{
			return;
		}
		filePathContainer.SetChildrenActive(active: false);
		foreach (DirectoryInfo item in IOUtil.GetDirectoryHierarchy(directory))
		{
			DirectoryInfo d = item;
			GameObject obj = Pools.Unpool(filePathButtonBlueprint, filePathContainer);
			Button componentInChildren = obj.GetComponentInChildren<Button>();
			componentInChildren.onClick.RemoveAllListeners();
			componentInChildren.onClick.AddListener(delegate
			{
				directory = d.FullName;
			});
			obj.GetComponentInChildren<TextMeshProUGUI>().text = d.Name.RemoveFromEnd('\\') + "/";
		}
		_filePathDirty = false;
	}

	private void _UpdateDirectoriesList()
	{
		directories.ClearAndCopyFrom(directorySearchText.FuzzyMatchSort(from d in new DirectoryInfo(directory).GetDirectories()
			where (d.Attributes & (FileAttributes.Hidden | FileAttributes.System)) == 0
			select d, (DirectoryInfo d) => d.Name.ToTagString()).AsEnumerable());
	}

	private void _UpdateDirectories()
	{
		if (!_directoriesDirty)
		{
			return;
		}
		directoriesContainer.SetChildrenActive(active: false);
		_UpdateDirectoriesList();
		foreach (DirectoryInfo item in directories.Take(directorySearchText.HasVisibleCharacter().ToInt(100, 300)))
		{
			DirectoryInfo d = item;
			GameObject obj = Pools.Unpool(directoryBlueprint, directoriesContainer);
			PointerClick3D componentInChildren = obj.GetComponentInChildren<PointerClick3D>();
			componentInChildren.OnClick.RemoveAllListeners();
			componentInChildren.OnClick.AddListener(delegate
			{
				directory = d.FullName;
			});
			TooltipCreator.CreateTextTooltip(componentInChildren.transform, d.FullName, beginShowTimer: false, 0.5f, backgroundEnabled: true, TextAlignmentOptions.Center, 0f);
			obj.GetComponentInChildren<TextMeshProUGUI>().text = d.Name;
		}
		_directoriesDirty = false;
	}

	private void _UpdateResultsList()
	{
		if (_resultsListDirty)
		{
			if (!resultSearchText.HasVisibleCharacter())
			{
				results.ClearAndCopyFrom(filesInDirectory);
				_SortResults();
			}
			else
			{
				_CalculateFuzzySearchResults();
			}
			_previousResultSearchText = resultSearchText;
			_resultsListDirty = false;
		}
	}

	private void _CalculateFuzzySearchResults()
	{
		_resultsDirty = false;
		string currentSearchText = resultSearchText;
		string currentDirectory = directory;
		PoolKeepItemListHandle<FileInfo> searchedFilesCopy;
		lock (searchedFiles)
		{
			searchedFilesCopy = Pools.UseKeepItemList(searchedFiles);
		}
		PoolHandle<StringBuilder> builder = Pools.Use<StringBuilder>();
		PoolHandle<StringBuilder> tagBuilder = Pools.Use<StringBuilder>();
		PoolKeepItemHashSetHandle<string> distinctTags = Pools.UseKeepItemHashSet<string>();
		AsyncTask<IEnumerable<FileInfo>>.Do(() => currentSearchText.FuzzyMatchSort(searchedFilesCopy.AsEnumerable(), (FileInfo f) => _ResultToSearchString(f, currentDirectory, builder, tagBuilder, distinctTags), sortOutputWhenSearchStringIsEmpty: false, 5, stableSort: true).AsEnumerable().OrderByComparer(_resultSortComparer)).GetResult(delegate(IEnumerable<FileInfo> result)
		{
			Pools.Repool(builder);
			Pools.Repool(tagBuilder);
			Pools.Repool(distinctTags);
			if (!(currentSearchText != resultSearchText) && !(currentDirectory != directory))
			{
				results.ClearAndCopyFrom(result);
				_SetDirty(resetPageNumber: false, setDirectoriesDirty: false, setFilePathDirty: false, setResultsDirty: true);
			}
		});
	}

	private string _ResultToSearchString(FileInfo result, string searchedDirectory, StringBuilder builder, StringBuilder tagBuilder, HashSet<string> distinctTagHash)
	{
		builder.Append(tagBuilder.ToTagString(result.Name, distinctTagHash));
		string text = result.DirectoryName ?? "";
		if (text.Length == searchedDirectory.Length)
		{
			return builder.ToStringClear();
		}
		builder.Append(" ");
		builder.Append(tagBuilder.ToTagString(text.Substring(searchedDirectory.Length), distinctTagHash));
		return builder.ToStringClear();
	}

	private void _SortResults()
	{
		if (_resultSortComparer != null)
		{
			results.Sort(_resultSortComparer);
		}
	}

	private void _UpdateResults()
	{
		_UpdateResultsList();
		if (!_resultsDirty)
		{
			return;
		}
		using PoolKeepItemListHandle<string> poolKeepItemListHandle = Pools.UseKeepItemList(from f in _GetPagedResults()
			select f.FullName);
		using PoolKeepItemListHandle<string> poolKeepItemListHandle2 = Pools.UseKeepItemList<string>();
		if ((bool)selectedItem && !poolKeepItemListHandle.value.Contains(selectedItem))
		{
			poolKeepItemListHandle.value.ReplaceLast(selectedItem);
		}
		poolKeepItemListHandle2.value.CopyFrom(poolKeepItemListHandle.value);
		foreach (FileBrowserItem item in resultsContainer.gameObject.GetComponentsInChildrenPooled<FileBrowserItem>())
		{
			if (!poolKeepItemListHandle2.Remove(item))
			{
				item.gameObject.SetActive(value: false);
			}
		}
		_resultPageNumber = Mathf.Clamp(_resultPageNumber, 1, maxResultsPageNumber);
		onResultPageTextChange.Invoke($"<size=60%>Page</size>\n{resultPageNumber}/{maxResultsPageNumber}");
		GameObject blueprint = _GetBrowserItemBlueprint();
		foreach (string item2 in poolKeepItemListHandle2.value)
		{
			DirtyPools.Unpool(blueprint, resultsContainer.transform).GetComponent<FileBrowserItem>().SetData(item2);
		}
		using (PoolKeepItemDictionaryHandle<string, int> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<string, int>())
		{
			int num = 0;
			foreach (string item3 in poolKeepItemListHandle.value)
			{
				poolKeepItemDictionaryHandle.Add(item3, num++);
			}
			foreach (FileBrowserItem item4 in resultsContainer.gameObject.GetComponentsInChildrenPooled<FileBrowserItem>())
			{
				item4.transform.SetSiblingIndex(poolKeepItemDictionaryHandle[item4]);
			}
		}
		_resultsDirty = false;
	}

	private IEnumerable<FileInfo> _GetPagedResults()
	{
		for (int x = (resultPageNumber - 1) * resultPageSize; x < Mathf.Min(results.Count, resultPageNumber * resultPageSize); x++)
		{
			yield return results[x];
		}
	}

	private void _RepoolResults()
	{
		resultsContainer.transform.SetChildrenActive(active: false);
	}

	private GameObject _GetBrowserItemBlueprint()
	{
		return _type switch
		{
			FileBrowserType.Image => imageItemBlueprint, 
			FileBrowserType.Audio => audioItemBlueprint, 
			_ => null, 
		};
	}

	private void _OnResultSelectionChange(List<Component> selection)
	{
		selectedItem = selection.OfType<FileBrowserItem>().FirstOrDefault();
	}

	private void _OnSelectedItemChange()
	{
		onItemIsSelectedChange.Invoke(selectedItem);
		onSelectedPathChange.Invoke(selectedItem);
		previewContainer.SetChildrenActive(active: false);
		FileBrowserItem currentlySelected = selectedItem;
		if (!currentlySelected)
		{
			return;
		}
		currentlySelected.RequestPreviewContent(delegate(object content)
		{
			if (selectedItem == currentlySelected)
			{
				_CreatePreview(content, previewContainer);
			}
		});
	}

	private void _CreatePreview(object content, Transform parent)
	{
		if (content is Texture2D)
		{
			DirtyPools.Unpool(imagePreviewBlueprint, parent).GetComponent<FileBrowserImagePreview>().texture = (Texture2D)content;
		}
		else if (content is AudioClip)
		{
			DirtyPools.Unpool(audioPreviewBlueprint, parent).GetComponent<FileBrowserAudioPreview>().PreviewClip((AudioClip)content, parent == previewContainer);
		}
		else if (content is AudioRefControl)
		{
			DirtyPools.Unpool(audioPreviewBlueprint, parent).GetComponent<FileBrowserAudioPreview>().PreviewAudioRef((AudioRefControl)content, parent == previewContainer);
		}
		else if (content is AudioClipWithWaveForm)
		{
			DirtyPools.Unpool(audioPreviewBlueprint, parent).GetComponent<FileBrowserAudioPreview>().PreviewClipWithWaveForm((AudioClipWithWaveForm)content, parent == previewContainer);
		}
	}

	private IEnumerator _SearchFilesInDirectoryDeep()
	{
		string searchedDirectory = directory;
		int searchIndex = _searchIndex;
		while (backgroundSearchActive)
		{
			yield return null;
		}
		backgroundSearchActive = true;
		yield return ToBackgroundThread.Create();
		lock (searchedFiles)
		{
			searchedFiles.Clear();
		}
		foreach (FileInfo item in IOUtil.EnumerateFiles(new DirectoryInfo(searchedDirectory)))
		{
			if (_searchIndex != searchIndex)
			{
				break;
			}
			if (!validFileExtensions.Contains(item.Extension))
			{
				continue;
			}
			lock (searchedFiles)
			{
				if (searchIndex == _searchIndex)
				{
					searchedFiles.Add(item);
					if (_resultSearchGracePeriod < 0f && _searchStringType != 0)
					{
						_resultSearchGracePeriod = refreshResultPeriod;
					}
					if (searchedFiles.Count < 10000)
					{
						continue;
					}
				}
			}
			break;
		}
		yield return ToMainThread.Create();
		backgroundSearchActive = false;
	}

	private void Awake()
	{
		resultsContainer.onContextSelectionChange.AddListener(_OnResultSelectionChange);
		_CreateQuickAccessDirectories();
		UIUtil.CreateEnumComboBox(delegate(ContentRefSortType c)
		{
			resultSortType = c;
		}, resultSortContainer, _resultSortType = ProfileManager.prefs.browser.resultSortType);
	}

	private void OnDisable()
	{
		filePathContainer.SetChildrenActive(active: false);
		directoriesContainer.SetChildrenActive(active: false);
		previewContainer.SetChildrenActive(active: false);
		overwritePreviewContainer.SetChildrenActive(active: false);
		logicalDrivesContainer.SetChildrenActive(active: false);
		recentDirectoryContainer.SetChildrenActive(active: false);
		_RepoolResults();
		onFileConfirm.RemoveAllListeners();
		Resources.UnloadUnusedAssets();
		InputManager.ReleaseInput(this);
		Interlocked.Increment(ref _searchIndex);
		_directoy = "";
	}

	private void OnEnable()
	{
		InputManager.RequestInput(this);
		_hasRefreshedSinceEnable = false;
		_CreateLogicalDrives();
		_CreateRecentDirectories();
	}

	private void Start()
	{
		UIPopupControl componentInParent = GetComponentInParent<UIPopupControl>();
		if ((bool)componentInParent)
		{
			componentInParent.window.localScale = componentInParent.window.localScale.Multiply(GetComponentInParent<CanvasScaler>().referenceResolution.Multiply(new Vector2(2560f, 1440f).Inverse()).Unproject(AxisType.Z, 1f));
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Mouse3) && InputManager.EventSystemEnabled)
		{
			DirectoryBack();
		}
		if (Input.GetKeyUp(KeyCode.Mouse4) && InputManager.EventSystemEnabled)
		{
			DirectoryForward();
		}
		if (_UpdateSearchGracePeriod(ref _resultSearchGracePeriod, resultSearchText))
		{
			_SetDirty(resultSearchText != _previousResultSearchText, setDirectoriesDirty: false, setFilePathDirty: false, setResultsDirty: true, setResultsListDirty: true);
		}
		if (_UpdateSearchGracePeriod(ref _directorySearchGracePeriod, directorySearchText))
		{
			_SetDirty(resetPageNumber: true, setDirectoriesDirty: true);
		}
		if (_isDirty)
		{
			_Refresh();
		}
	}

	public FileBrowserManager SetData(FileBrowserType type, string confirmButtonText, HashSet<string> validExtensions, int resultPageSize, float resultScale, UnityEngine.Object overwritten = null, string startingDirectory = null)
	{
		_type = type;
		onConfirmButtonNameChange.Invoke(confirmButtonText);
		_validFileExtensions = validExtensions;
		this.resultPageSize = resultPageSize;
		onResultScaleChange.Invoke(resultScale);
		_SetDirectory(startingDirectory);
		if ((bool)overwritten)
		{
			_CreatePreview(overwritten, overwritePreviewContainer);
		}
		onIsOverwriteChange.Invoke(overwritten);
		return this;
	}

	public void NextResultPage()
	{
		int num = resultPageNumber + 1;
		resultPageNumber = num;
	}

	public void PreviousResultPage()
	{
		int num = resultPageNumber - 1;
		resultPageNumber = num;
	}

	public void SelectFirstDirectory()
	{
		_UpdateDirectoriesList();
		if (directories.Count > 0)
		{
			directory = directories[0].FullName;
		}
	}

	public void GoToSelectedItemDirectory()
	{
		if ((bool)selectedItem)
		{
			directory = selectedItem.directory;
		}
	}

	public void DirectoryBack()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(directory);
		if (directoryInfo.Parent != null)
		{
			backDirectoryStack.Push(directory);
			_directoy = directoryInfo.Parent.FullName;
			_OnDirectoryChange(clearBackDirectoryStack: false);
		}
	}

	public void DirectoryForward()
	{
		if (backDirectoryStack.Count != 0)
		{
			_directoy = backDirectoryStack.Pop();
			_OnDirectoryChange(clearBackDirectoryStack: false);
		}
	}

	public void ConfirmSelection()
	{
		if ((bool)selectedItem)
		{
			onFileConfirm.Invoke(selectedItem);
			if (ProfileManager.prefs.browser.AddRecentDirectory(directory))
			{
				ProfileManager.Profile.SavePreferences();
			}
			Close();
		}
	}

	public void Close()
	{
		UIPopupControl componentInParent = GetComponentInParent<UIPopupControl>();
		if ((bool)componentInParent)
		{
			componentInParent.Close();
		}
	}
}
