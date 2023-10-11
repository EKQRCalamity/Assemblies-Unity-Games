using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProtoBuf;
using UnityEngine;

public class ContentRefManager : MonoBehaviour
{
	[Flags]
	public enum TypeFlags : byte
	{
		Audio = 1,
		Data = 2,
		Image = 4
	}

	public enum SortType : byte
	{
		FewestDependents,
		MostDependents,
		Recent,
		Old,
		Alphabetical
	}

	public class ContentRefComparer : IComparer<ContentRef>
	{
		public readonly ContentRefManager manager;

		public ContentRefComparer(ContentRefManager manager)
		{
			this.manager = manager;
		}

		public int Compare(ContentRef a, ContentRef b)
		{
			switch (manager.sortType)
			{
			case SortType.FewestDependents:
			{
				int num = manager.IsUsedByDefaults(a).ToInt() - manager.IsUsedByDefaults(b).ToInt();
				if (num == 0)
				{
					return manager.GetDependentsCount(a) - manager.GetDependentsCount(b);
				}
				return num;
			}
			case SortType.MostDependents:
				return manager.GetDependentsCount(b) - manager.GetDependentsCount(a);
			case SortType.Recent:
				return b.lastUpdateTime.CompareTo(a.lastUpdateTime);
			case SortType.Old:
				return a.lastUpdateTime.CompareTo(b.lastUpdateTime);
			case SortType.Alphabetical:
				return StringComparer.CurrentCulture.Compare(a.name, b.name);
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public class SearchData
	{
		public string searchText;

		public int pageNumber;

		public TypeFlags typeFilter;

		public ContentCreatorTypeFlags creatorFilter;

		public SortType sortType;

		public bool showDeletableOnly;

		public bool showLocalizableOnly;

		public SearchData(ContentRefManager manager)
		{
			searchText = manager.searchText;
			pageNumber = manager.pageNumber;
			typeFilter = manager.typeFilter;
			creatorFilter = manager.creatorFilter;
			sortType = manager.sortType;
			showDeletableOnly = manager.showDeletableOnly;
			showLocalizableOnly = manager.showLocalizableOnly;
		}

		public void Restore(ContentRefManager manager)
		{
			manager.searchText = searchText;
			manager.showDeletableOnly = showDeletableOnly;
			manager.showLocalizableOnly = showLocalizableOnly;
			manager._SetDirty();
			manager._pageNumber = pageNumber;
			manager._searchGracePeriod = 0f;
		}

		public static implicit operator TypeFlags(SearchData data)
		{
			return data?.typeFilter ?? TypeFlags.Data;
		}

		public static implicit operator ContentCreatorTypeFlags(SearchData data)
		{
			return data?.creatorFilter ?? EnumUtil<ContentCreatorTypeFlags>.AllFlagsExcept(ContentCreatorTypeFlags.Ours);
		}

		public static implicit operator SortType(SearchData data)
		{
			return data?.sortType ?? EnumUtil<SortType>.Min;
		}
	}

	private static bool _ContentScanRequested;

	private static HashSet<ContentRef.Key> _ContentRefDefaults = new HashSet<ContentRef.Key>();

	private static Dictionary<ContentRefType, Sprite> _ContentTypeSpriteMap;

	private static SearchData _RestoreData;

	private static bool DEBUG_AUDIO_CLIP_LENGTH = false;

	[SerializeField]
	[Range(10f, 100f)]
	private int _pageSize = 20;

	public RectTransform managementViewsContainer;

	public RectTransform contentTypeFilterContainer;

	public RectTransform contentCreatorFilterContainer;

	public RectTransform sortTypeContainer;

	[Range(0.1f, 2f)]
	public float searchGracePeriod = 0.5f;

	[Range(1f, 1000f)]
	public int getDependencyPerFrame = 100;

	[Header("Scan Events")]
	public StringEvent onProgressMessageChange;

	public FloatEvent onProgressRatioChange;

	public StringEvent onProgressCountChange;

	public BoolEvent onScanRequiredChange;

	public BoolEvent onScanActiveChange;

	[Header("Search Events")]
	public StringEvent onPageTextChange;

	public StringEvent onSearchPlaceHolderTextChange;

	public BoolEvent onCanSortResultsChange;

	public BoolEvent onCanUseCreatorFilterChange;

	[Header("Restore Events")]
	public StringEvent onSearchTextChange;

	public BoolEvent onShowDeletableOnlyChange;

	public BoolEvent onShowLocalizableOnlyChange;

	private Dictionary<ContentRef.Key, HashSet<ContentRef.Key>> _dependentsCache;

	private readonly Dictionary<ContentRef.Key, bool?> _isLocalized = new Dictionary<ContentRef.Key, bool?>();

	private string _searchText = "";

	private TypeFlags _typeFilter = EnumUtil<TypeFlags>.AllFlags;

	private ContentCreatorTypeFlags _creatorFilter;

	private SortType _sortType;

	private bool _showDeletableOnly;

	private bool _showLocalizableOnly;

	private int _pageNumber = 1;

	private List<ContentRef> _results;

	private bool _filteredContentDirty;

	private bool _resultsListDirty;

	private bool _resultViewsDirty;

	private float _searchGracePeriod;

	private ContentRefComparer _comparer;

	private List<ContentRef> _filteredContent;

	private CanvasInputFocus _canvasFocus;

	public static ContentRefManager Instance { get; private set; }

	public static Dictionary<ContentRefType, Sprite> ContentTypeSpriteMap => _ContentTypeSpriteMap ?? (_ContentTypeSpriteMap = ReflectionUtil.CreateEnumResourceMap<ContentRefType, Sprite>("UI/Content/ContentRefType"));

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

	public TypeFlags typeFilter
	{
		get
		{
			return _typeFilter;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _typeFilter, value))
			{
				_SetDirty(resetPageNumber: true, setFilteredContentDirty: true);
			}
		}
	}

	public ContentCreatorTypeFlags creatorFilter
	{
		get
		{
			return _creatorFilter;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _creatorFilter, value))
			{
				_SetDirty(resetPageNumber: true, setFilteredContentDirty: true);
			}
		}
	}

	public SortType sortType
	{
		get
		{
			return _sortType;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _sortType, value))
			{
				_SetDirty(resetPageNumber: true);
			}
		}
	}

	public bool showDeletableOnly
	{
		get
		{
			return _showDeletableOnly;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _showDeletableOnly, value))
			{
				_OnShowDeletableOnlyChange();
			}
		}
	}

	public bool showLocalizableOnly
	{
		get
		{
			return _showLocalizableOnly;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _showLocalizableOnly, value))
			{
				_OnShowLocalizableOnlyChange();
			}
		}
	}

	public int pageSize
	{
		get
		{
			return _pageSize;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _pageSize, value))
			{
				_resultViewsDirty = true;
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
				_resultViewsDirty = true;
			}
		}
	}

	private int maxPageNumber => results.GetMaxPageNumber(pageSize);

	private List<ContentRef> results => _results ?? (_results = new List<ContentRef>());

	public Dictionary<ContentRef.Key, HashSet<ContentRef.Key>> dependentsCache => _dependentsCache ?? (_dependentsCache = new Dictionary<ContentRef.Key, HashSet<ContentRef.Key>>());

	private ContentRefComparer comparer => _comparer ?? (_comparer = new ContentRefComparer(this));

	private List<ContentRef> filteredContent => _filteredContent ?? (_filteredContent = new List<ContentRef>());

	private CanvasInputFocus canvasFocus => this.CacheComponentSafe(ref _canvasFocus);

	private void Awake()
	{
		UIUtil.CreateEnumComboBox(delegate(TypeFlags c)
		{
			typeFilter = c;
		}, contentTypeFilterContainer, _typeFilter = _RestoreData);
		UIUtil.CreateEnumComboBox(delegate(ContentCreatorTypeFlags c)
		{
			creatorFilter = c;
		}, contentCreatorFilterContainer, _creatorFilter = _RestoreData);
		UIUtil.CreateEnumComboBox(delegate(SortType c)
		{
			sortType = c;
		}, sortTypeContainer, _sortType = _RestoreData);
		onScanRequiredChange.Invoke(!_ContentScanRequested);
		if (_ContentScanRequested)
		{
			_BuildDependentsCache();
		}
		if (_RestoreData != null)
		{
			_RestoreData.Restore(this);
		}
		onCanUseCreatorFilterChange.Invoke(ContentRef.UGC);
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private void Update()
	{
		if (_searchGracePeriod > 0f && (_searchGracePeriod -= ((!Input.GetKey(KeyCode.Backspace) || !searchText.HasVisibleCharacter()) ? Time.unscaledDeltaTime : 0f)) <= 0f)
		{
			_SetDirty(resetPageNumber: true);
		}
		_UpdateFilteredContent();
		_UpdateResultsList();
		_CreateManagementViews();
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void OnDestroy()
	{
		_RestoreData = new SearchData(this);
	}

	private void _OnSearchTextChange()
	{
		_searchGracePeriod = searchGracePeriod;
		onCanSortResultsChange.Invoke(searchText.CanSortSearchResults());
		onSearchTextChange.Invoke(searchText);
	}

	private void _OnShowDeletableOnlyChange()
	{
		_SetDirty(resetPageNumber: true, setFilteredContentDirty: true);
		onShowDeletableOnlyChange.Invoke(showDeletableOnly);
	}

	private void _OnShowLocalizableOnlyChange()
	{
		_SetDirty(resetPageNumber: true, setFilteredContentDirty: true);
		onShowLocalizableOnlyChange?.Invoke(showLocalizableOnly);
	}

	private void _SetDirty(bool resetPageNumber = false, bool setFilteredContentDirty = false)
	{
		_resultsListDirty = (_resultViewsDirty = true);
		if (resetPageNumber)
		{
			_pageNumber = 1;
		}
		if (setFilteredContentDirty)
		{
			_filteredContentDirty = true;
		}
	}

	private IEnumerator _ScanAllContent()
	{
		int index = 0;
		ImageCategoryType[] values = EnumUtil<ImageCategoryType>.Values;
		foreach (ImageCategoryType imageCategory in values)
		{
			onProgressMessageChange.Invoke("Loading " + EnumUtil.FriendlyName(imageCategory) + " Image References");
			onProgressRatioChange.Invoke((float)index++ / (float)EnumUtil<ImageCategoryType>.Values.Length);
			onProgressCountChange.Invoke(index + " / " + EnumUtil<ImageCategoryType>.Values.Length);
			yield return null;
			ImageRef.LoadAll(imageCategory);
		}
		index = 0;
		AudioCategoryType[] values2 = EnumUtil<AudioCategoryType>.Values;
		foreach (AudioCategoryType audioCategoryType in values2)
		{
			onProgressMessageChange.Invoke("Loading " + EnumUtil.FriendlyName(audioCategoryType) + " Audio References");
			onProgressRatioChange.Invoke((float)index++ / (float)EnumUtil<AudioCategoryType>.Values.Length);
			onProgressCountChange.Invoke(index + " / " + EnumUtil<AudioCategoryType>.Values.Length);
			yield return null;
			AudioRef.LoadAll(audioCategoryType);
		}
		index = 0;
		PoolKeepItemListHandle<Type> iDataContentTypes = Pools.UseKeepItemList(from ProtoIncludeAttribute protoInclude in typeof(ContentRef).GetCustomAttributes(typeof(ProtoIncludeAttribute), inherit: true)
			select protoInclude.KnownType into t
			where t.IsGenericType && t.GetGenericTypeDefinition() == typeof(DataRef<>)
			select t.GetGenericArguments()[0]);
		foreach (Type iDataContentType in iDataContentTypes)
		{
			onProgressMessageChange.Invoke("Loading " + iDataContentType.FriendlyName().FriendlyFromCamelOrPascalCase());
			onProgressRatioChange.Invoke((float)index++ / (float)iDataContentTypes.Count);
			onProgressCountChange.Invoke(index + " / " + iDataContentTypes.Count);
			yield return null;
			ContentRef.SearchData(iDataContentType).FirstOrDefault();
		}
		index = 0;
		onProgressMessageChange.Invoke("Calculating Dependencies");
		PoolKeepItemListHandle<ContentRef> dataRefs = Pools.UseKeepItemList(from cRef in ContentRef.All()
			where cRef.isDataRef
			select cRef);
		foreach (ContentRef dataRef in dataRefs)
		{
			if (index % getDependencyPerFrame == 0)
			{
				onProgressRatioChange.Invoke((float)index / (float)dataRefs.Count);
				onProgressCountChange.Invoke(index + 1 + " / " + dataRefs.Count);
				yield return null;
			}
			int num = index + 1;
			index = num;
			foreach (ContentRef dependency in dataRef.GetDependencies())
			{
				if (!dependentsCache.ContainsKey(dependency))
				{
					dependentsCache.Add(dependency, new HashSet<ContentRef.Key>());
				}
				dependentsCache[dependency].Add(dataRef);
			}
		}
		yield return null;
		onScanActiveChange.Invoke(arg0: false);
		InputManager.SetEventSystemEnabled(this, enabled: true);
		_SetDirty(resetPageNumber: true, setFilteredContentDirty: true);
	}

	private void _ScanContent()
	{
		if (!_ContentScanRequested)
		{
			_ContentScanRequested = true;
			onScanActiveChange.Invoke(arg0: true);
			onScanRequiredChange.Invoke(arg0: false);
			InputManager.SetEventSystemEnabled(this, enabled: false);
			Job.Process(_ScanAllContent(), Department.Content);
		}
	}

	private void _BuildDependentsCache()
	{
		dependentsCache.Clear();
		foreach (ContentRef item in ContentRef.All())
		{
			foreach (ContentRef dependency in item.GetDependencies())
			{
				if (!dependentsCache.ContainsKey(dependency))
				{
					dependentsCache.Add(dependency, new HashSet<ContentRef.Key>());
				}
				dependentsCache[dependency].Add(item);
			}
		}
		_SetDirty(resetPageNumber: false, setFilteredContentDirty: true);
	}

	[Conditional("UNITY_EDITOR")]
	private void _CalculateContentRefDefaults()
	{
		_ContentRefDefaults = (from cRef in (from cRef in ReflectionUtil.FindAllInstances<ContentRef>(ContentRef.Defaults)
				where cRef.isTracked
				select cRef).Concat(from d in DataRef<AbilityData>.Search()
				where d.data.characterClass.HasValue
				select d).Concat(from d in DataRef<TutorialData>.Search()
				where d.data
				select d).Concat(DataRef<AbilityDeckData>.All)
				.Concat(DataRef<AchievementData>.All)
				.Concat(DataRef<BlobData>.All)
				.Concat(DataRef<DBlobData>.All)
				.Concat(DataRef<EBlobData>.All)
				.Concat(DataRef<MessageData>.All)
			select cRef.key).ToHash();
	}

	private void _UpdateFilteredContent()
	{
		if (_filteredContentDirty)
		{
			_filteredContentDirty = false;
			filteredContent.ClearAndCopyFrom(from cRef in ContentRef.All()
				where typeFilter.IsValid(cRef) && EnumUtil.HasFlagConvert(creatorFilter, cRef.creatorType) && (!showDeletableOnly || CanBeDeleted(cRef)) && (!showLocalizableOnly || IsLocalized(cRef))
				select cRef);
		}
	}

	private void _UpdateResultsList()
	{
		if (_resultsListDirty)
		{
			_resultsListDirty = false;
			onSearchPlaceHolderTextChange.Invoke("Type To Search " + filteredContent.Count + " Items…");
			results.ClearAndCopyFrom((!searchText.CanSortSearchResults()) ? searchText.FuzzyMatchSort(filteredContent, (ContentRef cRef) => cRef.GetSearchString() + " " + cRef.specificType).AsEnumerable() : filteredContent.OrderByComparer(comparer));
		}
	}

	private void _CreateManagementViews()
	{
		if (!_resultViewsDirty)
		{
			return;
		}
		_resultViewsDirty = false;
		results.InsureValidPageNumber(ref _pageNumber, pageSize);
		onPageTextChange.Invoke($"<size=60%>Page</size>\n{pageNumber}/{maxPageNumber}");
		managementViewsContainer.SetChildrenActive(active: false);
		foreach (ContentRef pagedResult in results.GetPagedResults(pageNumber, pageSize))
		{
			ContentRefManagementView.Create(pagedResult, managementViewsContainer);
		}
	}

	[Conditional("UNITY_EDITOR")]
	private void _DebugAudioClipLengths()
	{
		if (DEBUG_AUDIO_CLIP_LENGTH)
		{
			Job.Process(_DebugAudioClipLengthsProcess());
		}
	}

	private IEnumerator _DebugAudioClipLengthsProcess()
	{
		int loadWindowSize = 25;
		Action<AudioClip> onClip = delegate
		{
		};
		AudioCategoryType[] values = EnumUtil<AudioCategoryType>.Values;
		foreach (AudioCategoryType category in values)
		{
			float maxLength = 0f;
			float averageLength = 0f;
			using PoolKeepItemListHandle<AudioRef> audioRefsInCategory = Pools.UseKeepItemList(AudioRef.Search(category));
			using PoolKeepItemHashSetHandle<ContentRef.Key> loadedAudioRefs = Pools.UseKeepItemHashSet<ContentRef.Key>();
			for (int x = 0; x < audioRefsInCategory.Count; x++)
			{
				for (int j = x; j < Math.Min(audioRefsInCategory.Count, x + loadWindowSize); j++)
				{
					AudioRef audioRef2 = audioRefsInCategory[j];
					if (!audioRef2.contentIsReady && loadedAudioRefs.Add(audioRef2))
					{
						audioRef2.GetAudioClip(onClip);
					}
				}
				AudioRef audioRef = audioRefsInCategory[x];
				while (!audioRef.contentIsReady)
				{
					yield return null;
				}
				maxLength = Mathf.Max(maxLength, audioRef.audioClip.length);
				averageLength += audioRef.audioClip.length;
				if (loadedAudioRefs.Remove(audioRef))
				{
					audioRef.Unload();
				}
			}
			_ = averageLength / (float)Math.Max(1, audioRefsInCategory.Count);
		}
	}

	[Conditional("UNITY_EDITOR")]
	private async void _EditorOnlyActions()
	{
		if (!Application.isEditor || !canvasFocus.hasFocus || !Steam.Enabled)
		{
			return;
		}
		if (InputManager.I[KeyModifiers.Shift] && InputManager.I[KeyCode.W][KState.Down] && InputManager.I[KeyCode.D][KState.JustPressed])
		{
			UIUtil.CreatePopup("Delete All Published Workshop Items", UIUtil.CreateMessageBox("Would you like to delete all of your published workshop items?"), null, parent: base.transform, buttons: new string[2] { "Cancel", "Delete All Published Items" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
			{
				if (s == "Delete All Published Items")
				{
					UIUtil.BeginProcessJob(base.transform, "Deleting All Published Workshop Items").Then().DoProcess(Steam.Ugc.DeleteAllPublishedByUserAsync(Steam.SteamId).AsEnumerator())
						.Then()
						.Do(UIUtil.EndProcess);
				}
			});
		}
		else if (InputManager.I[KeyModifiers.Shift] && InputManager.I[KeyCode.A][KState.Down] && InputManager.I[KeyCode.C][KState.JustPressed])
		{
			UIUtil.CreatePopup("Clear Achievements", UIUtil.CreateMessageBox("Would you like to clear all current achievements?"), null, parent: base.transform, buttons: new string[2] { "Cancel", "Clear Achievements" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
			{
				_ = s == "Clear Achievements";
			});
		}
		else if (InputManager.I[KeyModifiers.Control] && InputManager.I[KeyCode.A][KState.Down])
		{
			_ = InputManager.I[KeyCode.C][KState.JustPressed];
		}
	}

	public void ScanContent()
	{
		string scanButton = "Scan All Content For Dependencies";
		UIUtil.CreatePopup(scanButton, UIUtil.CreateMessageBox("Do you wish to load ALL content references and scan them for dependencies? This process can take awhile but is necessary to insure that content can be deleted safely."), null, buttons: new string[2] { "Cancel", scanButton }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (s == scanButton)
			{
				_ScanContent();
			}
		});
	}

	public int GetDependentsCount(ContentRef contentRef)
	{
		if (!dependentsCache.ContainsKey(contentRef))
		{
			return 0;
		}
		return dependentsCache[contentRef].Count;
	}

	public bool IsUsedByDefaults(ContentRef contentRef)
	{
		return contentRef.isResource;
	}

	public bool CanBeDeleted(ContentRef contentRef)
	{
		if (!IsUsedByDefaults(contentRef))
		{
			return GetDependentsCount(contentRef) switch
			{
				1 => dependentsCache[contentRef].First() == contentRef, 
				0 => true, 
				_ => false, 
			};
		}
		return false;
	}

	public bool IsLocalized(ContentRef contentRef)
	{
		if (contentRef.isDataRef && contentRef.dataType.HasAttribute<LocalizeAttribute>())
		{
			bool? valueOrDefault = _isLocalized.GetValueOrDefault(contentRef);
			if (!valueOrDefault.HasValue)
			{
				bool? flag2 = (_isLocalized[contentRef] = ReflectionUtil.GetValuesFromUI<LocalizedStringData>(contentRef.GetDataImmediate()).Any((LocalizedStringData l) => l.id));
				return flag2 == true;
			}
			return valueOrDefault.GetValueOrDefault();
		}
		return false;
	}

	public PoolKeepItemListHandle<ContentRef> GetDependents(ContentRef contentRef)
	{
		return Pools.UseKeepItemList(dependentsCache.ContainsKey(contentRef) ? dependentsCache[contentRef].Select(ContentRef.GetByKey<ContentRef>) : Enumerable.Empty<ContentRef>());
	}

	public void Refresh()
	{
		_BuildDependentsCache();
	}

	public void NextPage()
	{
		int num = pageNumber + 1;
		pageNumber = num;
	}

	public void PreviousPage()
	{
		int num = pageNumber - 1;
		pageNumber = num;
	}
}
