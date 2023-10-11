using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ContentRefSearcher : MonoBehaviour
{
	private static ResourceBlueprint<GameObject> _Blueprint = "UI/Content/ContentRefSearcher";

	private const int DEFAULT_PAGE_SIZE = 60;

	private static Dictionary<Type, int> _PageSizes;

	private const float DEFAULT_SCALE = 1f;

	private static Dictionary<Type, float> _DefaultScales;

	private static HashSet<Type> _HiddenResourceDataTypes;

	public RectTransform resultsContainer;

	[SerializeField]
	[Range(1f, 1000f)]
	private int _pageSize = 50;

	[SerializeField]
	[EnumFlags]
	private ContentCreatorTypeFlags _creatorTypes;

	[SerializeField]
	private ContentRefSortType _sort;

	public TMP_InputField searchInputField;

	[Range(0f, 1f)]
	public float searchGracePeriod = 0.5f;

	public RectTransform creatorTypesContainer;

	public RectTransform sortTypeContainer;

	[Range(0f, 1024f)]
	public float scrollSensitivity = 128f;

	[Header("Events")]
	public StringEvent onSearchTextChange;

	public IntEvent onPageChange;

	public IntEvent onPageMaxChange;

	public StringEvent onPageTextChange;

	public FloatEvent onScaleChange;

	public ContentRefEvent onContentSelected;

	public FloatEvent onScrollSensitivityChange;

	public BoolEvent onCanUseCreatorFilterChange;

	public BoolEvent onCanSortResultsChange;

	public StringEvent onLoadMessageChange;

	private string _searchText;

	private int _pageNumber;

	private float _scale = 1f;

	private float? _searchGracePeriodRemaining;

	private ImageCategoryType? _imageCategory;

	private AudioCategoryType? _audioCategory;

	private Type _dataType;

	private Func<ContentRef, bool> _validContent;

	private bool _mustBeCommitted = true;

	private bool _isDirty;

	private List<ContentRef> _results;

	private HashSet<ContentRef> _loadedContent;

	private bool _allowHiddenResources;

	private bool _unloadContent;

	private DynamicGridLayout _resultsLayout;

	private GameState _state;

	public static ContentRefSearcher Instance { get; private set; }

	public static bool IsSearchingAbilities
	{
		get
		{
			if ((bool)Instance)
			{
				return Instance._dataType == typeof(AbilityData);
			}
			return false;
		}
	}

	private static Dictionary<Type, int> PageSizes
	{
		get
		{
			object obj = _PageSizes;
			if (obj == null)
			{
				obj = new Dictionary<Type, int>
				{
					{
						typeof(ImageRef),
						30
					},
					{
						typeof(AudioRef),
						50
					}
				};
				_PageSizes = (Dictionary<Type, int>)obj;
			}
			return (Dictionary<Type, int>)obj;
		}
	}

	private static Dictionary<Type, float> DefaultScales
	{
		get
		{
			object obj = _DefaultScales;
			if (obj == null)
			{
				obj = new Dictionary<Type, float>
				{
					{
						typeof(ImageRef),
						0.5f
					},
					{
						typeof(AudioRef),
						0.9f
					}
				};
				_DefaultScales = (Dictionary<Type, float>)obj;
			}
			return (Dictionary<Type, float>)obj;
		}
	}

	private static HashSet<Type> HiddenResourceDataTypes => _HiddenResourceDataTypes ?? (_HiddenResourceDataTypes = new HashSet<Type>());

	private List<ContentRef> results => _results ?? (_results = new List<ContentRef>());

	public int pageSize
	{
		get
		{
			return _pageSize;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _pageSize, Mathf.Max(1, value)))
			{
				_ResetSearch();
			}
		}
	}

	public string searchText
	{
		get
		{
			return _searchText;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _searchText, value))
			{
				_OnSearchTextChange();
			}
		}
	}

	public int pageNumber
	{
		get
		{
			return _pageNumber;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _pageNumber, Mathf.Clamp(value, 1, maxPageNumber)))
			{
				_OnPageChange();
			}
		}
	}

	public int maxPageNumber => Math.Max(0, results.Count - 1) / pageSize + 1;

	public float scale
	{
		get
		{
			return _scale;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _scale, value))
			{
				_OnScaleChange();
			}
		}
	}

	public ContentCreatorTypeFlags creatorTypes
	{
		get
		{
			return _creatorTypes;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _creatorTypes, value))
			{
				_OnCreatorTypesChange();
			}
		}
	}

	public ContentRefSortType sortType
	{
		get
		{
			return _sort;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _sort, value))
			{
				_OnSortTypeChange();
			}
		}
	}

	private int _pageIndexOffset => (pageNumber - 1) * pageSize;

	private HashSet<ContentRef> loadedContent => _loadedContent ?? (_loadedContent = new HashSet<ContentRef>());

	private DynamicGridLayout resultsLayout => resultsContainer.CacheComponent(ref _resultsLayout);

	public static GameObject Create(ImageCategoryType imageCategory, UnityAction<ContentRef> onSelected, Transform parent = null)
	{
		return Pools.Unpool(_Blueprint, parent).GetComponent<ContentRefSearcher>().SetData(imageCategory)
			.SetOnSelected(onSelected)
			.gameObject;
	}

	public static GameObject Create(AudioCategoryType audioCategory, UnityAction<ContentRef> onSelected, Transform parent = null)
	{
		return Pools.Unpool(_Blueprint, parent).GetComponent<ContentRefSearcher>().SetData(audioCategory)
			.SetOnSelected(onSelected)
			.gameObject;
	}

	public static GameObject Create(Type dataType, UnityAction<ContentRef> onSelected, Func<ContentRef, bool> validContent = null, bool mustBeCommitted = true, Transform parent = null, bool ignoreCreatorTypeFilter = false, bool allowHiddenResources = false, bool unloadContent = true)
	{
		return Pools.Unpool(_Blueprint, parent).GetComponent<ContentRefSearcher>().SetData(dataType, validContent, mustBeCommitted, ignoreCreatorTypeFilter, allowHiddenResources, unloadContent)
			.SetOnSelected(onSelected)
			.gameObject;
	}

	private static int GetPageSize(Type contentType)
	{
		if (!PageSizes.ContainsKey(contentType))
		{
			return 60;
		}
		return PageSizes[contentType];
	}

	private static int? GetPageSizeOverride(ImageCategoryType? imageCategory)
	{
		return imageCategory switch
		{
			ImageCategoryType.Enemy => 18, 
			ImageCategoryType.Adventure => 24, 
			ImageCategoryType.Ability => 18, 
			_ => null, 
		};
	}

	public static int GetPageSize(ContentRef cRef)
	{
		return ((cRef is ImageRef) ? GetPageSizeOverride(((ImageRef)cRef).category) : null) ?? GetPageSize(cRef.isDataRef ? cRef.dataType : cRef.GetType());
	}

	private static float GetScale(Type contentType)
	{
		if (!DefaultScales.ContainsKey(contentType))
		{
			return 1f;
		}
		return DefaultScales[contentType];
	}

	private static float? GetScaleOverride(ImageCategoryType? imageCategory)
	{
		return imageCategory switch
		{
			ImageCategoryType.Ability => 0.59f, 
			ImageCategoryType.Adventure => 0.59f, 
			ImageCategoryType.Enemy => 0.59f, 
			_ => null, 
		};
	}

	public static float GetScale(ContentRef cRef)
	{
		return ((cRef is ImageRef) ? GetScaleOverride(((ImageRef)cRef).category) : null) ?? GetScale(cRef.isDataRef ? cRef.dataType : cRef.GetType());
	}

	public static Vector2? GetCellSize(ImageCategoryType? imageCategory)
	{
		if (!imageCategory.HasValue)
		{
			return null;
		}
		Ushort2? @ushort = imageCategory.Value.PreferredSize();
		if (!@ushort.HasValue)
		{
			return null;
		}
		return @ushort.Value.ToVector2() * ((float)imageCategory.Value.MaxSaveResolution() / (float)(int)@ushort.Value.Max()) + new Vector2(0f, 32f);
	}

	public static Vector2? GetCellSize(ContentRef cRef)
	{
		if (!(cRef is ImageRef))
		{
			return null;
		}
		return GetCellSize(((ImageRef)cRef).category);
	}

	private ContentRefSearcher _SetContentType(Type contentType)
	{
		pageSize = GetPageSizeOverride(_imageCategory) ?? GetPageSize(contentType);
		scale = GetScaleOverride(_imageCategory) ?? GetScale(contentType);
		resultsLayout.SetCellSize(GetCellSize(_imageCategory));
		return this;
	}

	private void _OnPageChange()
	{
		onPageChange.Invoke(pageNumber);
		_SetDirty();
	}

	private void _OnSearchTextChange()
	{
		onSearchTextChange.Invoke(searchText);
		_searchGracePeriodRemaining = searchGracePeriod;
		onCanSortResultsChange.Invoke(!searchText.HasVisibleCharacter());
	}

	private void _OnCreatorTypesChange()
	{
		_ResetSearch();
	}

	private void _OnSortTypeChange()
	{
		_ResetSearch();
	}

	private void _OnScaleChange()
	{
		onScaleChange.Invoke(_scale);
		onScrollSensitivityChange.Invoke(_scale * scrollSensitivity);
	}

	private void _ResetSearch()
	{
		pageNumber = 1;
		_SetDirty();
	}

	private void _SetDirty()
	{
		_isDirty = true;
	}

	private void _RepoolResultViews()
	{
		foreach (Transform item in resultsContainer)
		{
			item.GetComponentInChildren<PointerClick3D>(includeInactive: true).OnClick.RemoveAllListeners();
			item.gameObject.SetActive(value: false);
		}
		foreach (ContentRef item2 in loadedContent.EnumerateSafe())
		{
			if (item2.Unload())
			{
				loadedContent.Remove(item2);
			}
		}
	}

	private void _CalculateResults()
	{
		IEnumerable<ContentRef> enumerable = (_imageCategory.HasValue ? ImageRef.Search(_imageCategory.Value).Cast<ContentRef>() : (_audioCategory.HasValue ? AudioRef.Search(_audioCategory.Value).Cast<ContentRef>() : ContentRef.SearchData(_dataType, _mustBeCommitted)));
		if (_validContent != null)
		{
			enumerable = enumerable.Where(_validContent);
		}
		ContentCreatorTypeFlags creatorFilter = creatorTypes;
		if (!Application.isEditor && _dataType != null && !_allowHiddenResources && HiddenResourceDataTypes.Contains(_dataType))
		{
			EnumUtil.Subtract(ref creatorFilter, ContentCreatorTypeFlags.Ours);
		}
		if (!EnumUtil.HasAllFlags(creatorFilter))
		{
			enumerable = enumerable.Where((ContentRef cRef) => EnumUtil.HasFlagConvert(creatorFilter, cRef.creatorType));
		}
		results.ClearAndCopyFrom(searchText.FuzzyMatchSort(enumerable, out var effectiveSearchString, (ContentRef c) => c.GetSearchString()).AsEnumerable());
		if (effectiveSearchString.IsNullOrEmpty())
		{
			_SortResults();
		}
	}

	private void _CreateResultViews()
	{
		onLoadMessageChange.Invoke("");
		_RepoolResultViews();
		_CalculateResults();
		if (_imageCategory.HasValue)
		{
			_CreateImageViews(_GetPagedResults().Cast<ImageRef>());
		}
		else if (_audioCategory.HasValue)
		{
			_CreateAudioViews(_GetPagedResults().Cast<AudioRef>());
		}
		else
		{
			_CreateDataViews(_GetPagedResults());
		}
		_AddOnClickLogicToViews();
		onPageMaxChange.Invoke(maxPageNumber);
		onPageTextChange.Invoke($"<size=60%>Page</size>\n{pageNumber}/{maxPageNumber}");
		(searchInputField.placeholder as TextMeshProUGUI).text = "Type To Search " + results.Count + " Items…";
		if (InputManager.EventSystemEnabled)
		{
			InputManager.EventSystem.SetSelectedGameObject(searchInputField.gameObject);
		}
		_isDirty = false;
	}

	private void _SortResults()
	{
		switch (sortType)
		{
		case ContentRefSortType.Recent:
			results.Sort((ContentRef a, ContentRef b) => b.lastUpdateTime.CompareTo(a.lastUpdateTime));
			break;
		case ContentRefSortType.Alphabetical:
			results.Sort((ContentRef a, ContentRef b) => string.Compare(a.name, b.name, StringComparison.CurrentCulture));
			break;
		case ContentRefSortType.Old:
			results.Sort((ContentRef a, ContentRef b) => a.lastUpdateTime.CompareTo(b.lastUpdateTime));
			break;
		}
	}

	private IEnumerable<ContentRef> _GetPagedResults()
	{
		for (int x = _pageIndexOffset; x < Mathf.Min(results.Count, pageNumber * pageSize); x++)
		{
			yield return results[x];
		}
	}

	private void _CreateImageViews(IEnumerable<ImageRef> imageRefs)
	{
		foreach (ImageRef imageRef in imageRefs)
		{
			ImageRefSearchView.Create(imageRef, resultsContainer);
		}
	}

	private void _CreateAudioViews(IEnumerable<AudioRef> audioRefs)
	{
		foreach (AudioRef audioRef in audioRefs)
		{
			AudioRefSearchView.Create(audioRef, resultsContainer);
		}
	}

	private void _CreateDataViews(IEnumerable<ContentRef> dataRefs)
	{
		foreach (ContentRef dataRef in dataRefs)
		{
			DataRefSearchView.Create(dataRef, resultsContainer);
		}
	}

	private void _AddOnClickLogicToViews()
	{
		int num = 0;
		foreach (Transform item in resultsContainer)
		{
			if (item.gameObject.activeSelf)
			{
				int i = num++;
				item.GetComponentInChildren<PointerClick3D>(includeInactive: true).OnClick.AddListener(delegate
				{
					onContentSelected.Invoke(results[i + _pageIndexOffset]);
				});
			}
		}
	}

	private void _OnContentRequestLoad(ContentRef contentRef)
	{
		if (_unloadContent && !contentRef.isDataRef)
		{
			loadedContent.Add(contentRef);
		}
	}

	private void _RemoveFromContentToUnload(ContentRef contentRef)
	{
		if (loadedContent.Count <= 0)
		{
			return;
		}
		foreach (ContentRef item in contentRef.GetDependenciesDeep())
		{
			loadedContent.Remove(item);
		}
	}

	private void _GenerateGameStateIfNeeded()
	{
	}

	private void Awake()
	{
		UIUtil.CreateEnumComboBox(delegate(ContentCreatorTypeFlags c)
		{
			creatorTypes = c;
		}, creatorTypesContainer, _creatorTypes = ContentCreatorTypeFlags.Ours);
		UIUtil.CreateEnumComboBox(delegate(ContentRefSortType c)
		{
			sortType = c;
		}, sortTypeContainer, _sort = ContentRefSortType.Recent);
		onCanUseCreatorFilterChange.Invoke(!Application.isEditor);
	}

	private void OnDisable()
	{
		_RepoolResultViews();
		_imageCategory = null;
		_audioCategory = null;
		_dataType = null;
		_validContent = null;
		_mustBeCommitted = true;
		results.Clear();
		onContentSelected.RemoveAllListeners();
		ContentRef.OnRequestLoad -= _OnContentRequestLoad;
		loadedContent.Clear();
		Pools.Repool(ref _state);
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void OnEnable()
	{
		Instance = this;
		_GenerateGameStateIfNeeded();
		ContentRef.OnRequestLoad += _OnContentRequestLoad;
		_ResetSearch();
		searchText = "";
		_searchGracePeriodRemaining = null;
		onContentSelected.AddListener(_RemoveFromContentToUnload);
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
		if (_searchGracePeriodRemaining.HasValue && (_searchGracePeriodRemaining -= ((!Input.GetKey(KeyCode.Backspace) || !searchText.HasVisibleCharacter()) ? Time.unscaledDeltaTime : 0f)) <= 0f)
		{
			float? num = (_searchGracePeriodRemaining = null);
			if (!num.HasValue)
			{
				_ResetSearch();
			}
		}
		if (_isDirty)
		{
			_CreateResultViews();
		}
	}

	public ContentRefSearcher SetData(ImageCategoryType imageCategory)
	{
		onLoadMessageChange.Invoke($"Loading {EnumUtil.FriendlyName(imageCategory)} Images... This could take awhile...");
		_imageCategory = imageCategory;
		return _SetContentType(typeof(ImageRef));
	}

	public ContentRefSearcher SetData(AudioCategoryType audioCategory)
	{
		onLoadMessageChange.Invoke($"Loading {EnumUtil.FriendlyName(audioCategory)} Audio... This could take awhile...");
		_audioCategory = audioCategory;
		return _SetContentType(typeof(AudioRef));
	}

	public ContentRefSearcher SetData(Type dataType, Func<ContentRef, bool> validContent, bool mustBeCommitted, bool ignoreCreatorTypeFilter, bool allowHiddenResources, bool unloadContent)
	{
		onLoadMessageChange.Invoke($"Loading {dataType.FriendlyName().FriendlyFromCamelOrPascalCase()}... This could take awhile...");
		_dataType = dataType;
		_validContent = validContent;
		_mustBeCommitted = mustBeCommitted;
		if (ignoreCreatorTypeFilter)
		{
			_creatorTypes = EnumUtil<ContentCreatorTypeFlags>.AllFlags;
			onCanUseCreatorFilterChange.Invoke(arg0: false);
		}
		_allowHiddenResources = allowHiddenResources;
		_unloadContent = unloadContent;
		return _SetContentType(dataType);
	}

	public ContentRefSearcher SetOnSelected(UnityAction<ContentRef> onSelected)
	{
		onContentSelected.AddListener(onSelected);
		return this;
	}

	public void SetSearchText(string text, bool clearGracePeriod = true)
	{
		searchText = text;
		if (clearGracePeriod)
		{
			_searchGracePeriodRemaining = null;
		}
	}

	public void SelectFirstResult()
	{
		if (_searchGracePeriodRemaining.HasValue)
		{
			_CalculateResults();
		}
		if (results.Count > 0)
		{
			onContentSelected.Invoke(_GetPagedResults().First());
		}
	}

	public void PreviousPage()
	{
		int num = pageNumber - 1;
		pageNumber = num;
	}

	public void NextPage()
	{
		int num = pageNumber + 1;
		pageNumber = num;
	}
}
