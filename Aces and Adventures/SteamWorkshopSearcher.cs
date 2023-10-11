using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SteamWorkshopSearcher : MonoBehaviour
{
	public const int MAX_RESULTS = 10000;

	private static ResourceBlueprint<GameObject> _Blueprint = "UI/Content/SteamWorkshopSearcher";

	private static Dictionary<Type, GameObject> _TypeBlueprintsThatRequireSpecialSpacing;

	private static Dictionary<Type, DynamicGridLayout.PreferredSizeType> _TypesWithSpecialPreferredSizeLayout;

	[Range(0f, 1024f)]
	public float scrollSensitivity = 128f;

	[Header("Containers")]
	public SelectableGroup resultsContainer;

	public RectTransform statusContainer;

	public RectTransform queryTypeContainer;

	public RectTransform groupContainer;

	[Range(0f, 2f)]
	[Header("Search Text")]
	public float searchGradePeriod = 0.5f;

	public TMP_InputField searchField;

	private string _searchText = "";

	private float _searchGracePeriod;

	[Header("Events")]
	public StringEvent onPageTextChange;

	public FloatEvent onScaleChange;

	public FloatEvent onScrollSensitivityChange;

	public BoolEvent onItemIsSelectedChange;

	public StringEvent onDownloadButtonTextChange;

	public BoolEvent onCanScaleChange;

	public BoolEvent onNoResultsFoundChange;

	public StringEvent onNoResultsFoundTextChange;

	public BoolEvent onQueryActiveChange;

	public StringEvent onDownloadQueueTextChange;

	public BoolEvent onDownloadQueueActiveChange;

	public StringEvent onDownloadQueueButtonTextChange;

	private UnityAction<ContentRef> _onContentDownloaded;

	private SteamWorkshopItemView _selectedItem;

	private ContentRef _contentRef;

	private Steam.Ugc.Query.QueryType _queryType;

	private ContentInstallStatusFlags _statusFlags;

	private Steam.Friends.Group _group;

	private List<KeyValuePair<string, string>> _keyValueTags;

	private Steam.Ugc.IQuery _query;

	private List<Steam.Ugc.Query.Result> _results = new List<Steam.Ugc.Query.Result>();

	private HashSet<Steam.Ugc.Query.Result> _downloadQueue;

	private bool _viewDownloadQueue;

	private float _scale = 1f;

	private int _pageSize = 1;

	private int _pageNumber = 1;

	private int _maxPageNumber = 1;

	private int _lastPageNumber = 1;

	private bool _pageDirty;

	private bool _queryDirty;

	private DynamicGridLayout _resultsLayout;

	private Vector2 _defaultSpacing;

	private Vector4 _defaultPadding;

	public static Dictionary<Type, GameObject> TypeBlueprintsThatRequireSpecialSpacing => _TypeBlueprintsThatRequireSpecialSpacing ?? (_TypeBlueprintsThatRequireSpecialSpacing = new Dictionary<Type, GameObject>());

	private static Dictionary<Type, DynamicGridLayout.PreferredSizeType> TypesWithSpecialPreferredSizeLayout => _TypesWithSpecialPreferredSizeLayout ?? (_TypesWithSpecialPreferredSizeLayout = new Dictionary<Type, DynamicGridLayout.PreferredSizeType>());

	public static SteamWorkshopSearcher Instance { get; private set; }

	public string searchText
	{
		get
		{
			return _searchText;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _searchText, value.HasVisibleCharacter() ? value : ""))
			{
				_OnSearchTextChange();
			}
		}
	}

	public Steam.Ugc.Query.QueryType queryType
	{
		get
		{
			return _queryType;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _queryType, value))
			{
				_OnQueryTypeChange();
			}
		}
	}

	public ContentInstallStatusFlags statusFlags
	{
		get
		{
			return _statusFlags;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _statusFlags, value))
			{
				_OnStatusFlagsChange();
			}
		}
	}

	public Steam.Friends.Group group
	{
		get
		{
			return _group;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _group, value))
			{
				_OnGroupChange();
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
			if (SetPropertyUtility.SetStruct(ref _pageNumber, Mathf.Clamp(value, 1, _maxPageNumber)))
			{
				_OnPageNumberChange();
			}
		}
	}

	public int maxPageNumber => _maxPageNumber;

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

	public SteamWorkshopItemView selectedItem
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

	private DynamicGridLayout resultsLayout => resultsContainer.CacheComponent(ref _resultsLayout);

	private HashSet<Steam.Ugc.Query.Result> downloadQueue => _downloadQueue ?? (_downloadQueue = new HashSet<Steam.Ugc.Query.Result>());

	public bool viewDownloadQueue
	{
		get
		{
			return _viewDownloadQueue;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _viewDownloadQueue, value))
			{
				_OnViewDownloadQueueChange();
			}
		}
	}

	public static GameObject Create(ContentRef contentRef, UnityAction<ContentRef> onContentDownloaded, IEnumerable<KeyValuePair<string, string>> additionalWorkshopTags = null, Transform parent = null)
	{
		return Pools.Unpool(_Blueprint, parent).GetComponent<SteamWorkshopSearcher>()._SetData(contentRef, onContentDownloaded, additionalWorkshopTags)
			.gameObject;
	}

	private DynamicGridLayout.PreferredSizeType _GetPreferredSizeType(ContentRef contentRef)
	{
		if (!TypesWithSpecialPreferredSizeLayout.ContainsKey(contentRef.GetType()))
		{
			return DynamicGridLayout.PreferredSizeType.Average;
		}
		return TypesWithSpecialPreferredSizeLayout[contentRef.GetType()];
	}

	public static bool InDownloadQueue(Steam.Ugc.Query.Result result)
	{
		if ((bool)Instance)
		{
			return Instance.downloadQueue.Contains(result);
		}
		return false;
	}

	private SteamWorkshopSearcher _SetData(ContentRef contentRef, UnityAction<ContentRef> onContentDownloaded, IEnumerable<KeyValuePair<string, string>> additionalWorkshopTags)
	{
		resultsLayout.cellSizeType = _GetPreferredSizeType(contentRef);
		resultsLayout.SetCellSize(ContentRefSearcher.GetCellSize(contentRef));
		bool flag = !DataRefSearchView.TypeViews.ContainsKey(contentRef.GetType());
		onCanScaleChange.Invoke(flag);
		float num = ContentRefSearcher.GetScale(contentRef);
		scale = (flag ? num : 1f);
		if (!flag)
		{
			resultsLayout.spacing = _defaultSpacing * num;
			resultsLayout.padding.SetPadding(_defaultPadding * num);
		}
		_pageSize = ContentRefSearcher.GetPageSize(contentRef);
		_contentRef = contentRef;
		_keyValueTags = new List<KeyValuePair<string, string>> { contentRef.workshopTag };
		if (additionalWorkshopTags != null)
		{
			_keyValueTags.AddMany(additionalWorkshopTags);
		}
		_onContentDownloaded = onContentDownloaded;
		_SetDirty();
		_StartQuery();
		contentRef.SearchSimilar().Any();
		return this;
	}

	private void _StartQuery()
	{
		onQueryActiveChange.Invoke(arg0: true);
		onNoResultsFoundChange.Invoke(arg0: false);
		Pools.RepoolObject(ref _query);
		_results.Clear();
		PublishedFileId_t? publishedFileId_t = Steam.ParsePublishedFileId(_searchText);
		if (!publishedFileId_t.HasValue)
		{
			IEnumerable<KeyValuePair<string, string>> enumerable = _keyValueTags.AsEnumerable();
			IEnumerable<string> tags = null;
			if (_queryType == Steam.Ugc.Query.QueryType.GroupMembersOnly && (bool)_group)
			{
				IEnumerable<KeyValuePair<string, string>> first = enumerable;
				KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[1];
				ulong steamID = _group.id.m_SteamID;
				array[0] = new KeyValuePair<string, string>("G", steamID.ToString());
				enumerable = first.Concat(array);
			}
			if (_queryType == Steam.Ugc.Query.QueryType.LikedCreators)
			{
				_query = Steam.Ugc.QueryUsers.Create(ProfileManager.prefs.steam.likedAuthors.Select((ulong id) => (CSteamID)id), EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_GameManagedItems, EUserUGCListSortOrder.k_EUserUGCListSortOrder_VoteScoreDesc, enumerable, tags, matchAnyTag: true, _contentRef.usesWorkshopMetaData, returnChildren: false, returnKeyValueTags: true, _contentRef.usesAdditionalPreviews);
			}
			else
			{
				_query = Steam.Ugc.Query.Create(_queryType.Type(), EUGCMatchingUGCType.k_EUGCMatchingUGCType_GameManagedItems, enumerable, tags, matchAnyTag: true, _contentRef.usesWorkshopMetaData, returnChildren: false, returnKeyValueTags: true, _contentRef.usesAdditionalPreviews);
			}
		}
		else
		{
			_query = Steam.Ugc.QuerySpecific.Create(publishedFileId_t.Value, _keyValueTags, null, matchAnyTag: true, _contentRef.usesWorkshopMetaData, returnChildren: false, returnKeyValueTags: true, _contentRef.usesAdditionalPreviews);
		}
		int queryId = _query.id;
		_query.PageAllAsync(_OnPageResults).ContinueWith(delegate
		{
			if (_query == null || _query.id == queryId)
			{
				onQueryActiveChange.Invoke(arg0: false);
			}
		}, TaskScheduler.FromCurrentSynchronizationContext());
		_queryDirty = false;
	}

	private void _OnPageResults(IEnumerable<Steam.Ugc.Query.Result> pageResults)
	{
		int num = 0;
		foreach (Steam.Ugc.Query.Result pageResult in pageResults)
		{
			if (pageResult.Visible(_query.type) && ++num > 0)
			{
				_CheckResultAuthorVisibility(pageResult);
			}
		}
		if (_results.Count == 0 && num == 0)
		{
			_SetDirty(queryDirty: false, clearSelected: false);
		}
	}

	private async void _CheckResultAuthorVisibility(Steam.Ugc.Query.Result result)
	{
		if (ProfileManager.options.game.ugc.enableDownVotedCreators)
		{
			_AddResult(result, addSorted: false);
		}
		else if ((await Steam.Ugc.GetAuthorFile(result.ownerId)).IsNotDownVoted())
		{
			_AddResult(result, addSorted: true);
		}
	}

	private void _AddResult(Steam.Ugc.Query.Result result, bool addSorted)
	{
		if (addSorted)
		{
			_results.AddSorted(result, Comparer<Steam.Ugc.Query.Result>.Default);
		}
		else
		{
			_results.Add(result);
		}
		_SetDirty(queryDirty: false, clearSelected: false);
		if (_results.Count >= 10000)
		{
			Pools.RepoolObject(ref _query);
		}
	}

	private void _OnSearchTextChange()
	{
		_searchGracePeriod = searchGradePeriod;
		_ClearSelectedItem();
	}

	private void _OnQueryTypeChange()
	{
		_SetDirty();
		ProfileManager.prefs.steam.SetResultSortType(_queryType);
		groupContainer.gameObject.SetActive(_queryType == Steam.Ugc.Query.QueryType.GroupMembersOnly);
	}

	private void _OnStatusFlagsChange()
	{
		_SetDirty(queryDirty: false);
		ProfileManager.prefs.steam.SetStatusFlags(_statusFlags);
	}

	private void _OnGroupChange()
	{
		_SetDirty();
		ProfileManager.prefs.steam.SetSearchGroup(_group);
	}

	private void _OnPageNumberChange()
	{
		_SetDirty(queryDirty: false);
	}

	private void _OnScaleChange()
	{
		onScaleChange.Invoke(_scale);
		onScrollSensitivityChange.Invoke(_scale * scrollSensitivity);
	}

	private void _OnResultSelectionChange(List<Component> selection)
	{
		selectedItem = selection.OfType<SteamWorkshopItemView>().FirstOrDefault();
	}

	private void _OnSelectedItemChange()
	{
		if (downloadQueue.Count <= 0)
		{
			onItemIsSelectedChange.Invoke(selectedItem);
			onDownloadButtonTextChange.Invoke(selectedItem ? ("Download " + selectedItem.result.name + " " + _contentRef.specificType) : "Select Item To Download");
		}
	}

	private void _OnViewDownloadQueueChange()
	{
		int lastPageNumber = _lastPageNumber;
		_lastPageNumber = pageNumber;
		_pageNumber = lastPageNumber;
		onDownloadQueueButtonTextChange.Invoke(viewDownloadQueue ? "View Search" : "View Queue");
		_SetDirty(queryDirty: false);
	}

	private void _ClearSelectedItem()
	{
		resultsContainer.ClearSelected();
		resultsContainer.ForceUpdateSelection();
	}

	private void _SetDirty(bool queryDirty = true, bool clearSelected = true, bool pageDirty = true)
	{
		_pageDirty = pageDirty;
		_queryDirty = queryDirty;
		if (clearSelected)
		{
			_ClearSelectedItem();
		}
	}

	private void _RefreshResults()
	{
		if (_queryDirty)
		{
			_StartQuery();
		}
		if (_pageDirty)
		{
			_SortResults();
		}
	}

	private bool _IsValidResult(Steam.Ugc.Query.Result result)
	{
		if (EnumUtil.HasFlagConvert(_statusFlags, ContentRef.GetPublishedIdInstallStatus(result.id, result.timeUpdated)))
		{
			return !downloadQueue.Contains(result) ^ viewDownloadQueue;
		}
		return false;
	}

	private async void _SortResults()
	{
		_pageDirty = false;
		string searchTextCopy = _searchText;
		PoolKeepItemListHandle<Steam.Ugc.Query.Result> resultsCopy = Pools.UseKeepItemList(_results.Where(_IsValidResult));
		try
		{
			using PoolKeepItemListHandle<Steam.Ugc.Query.Result> poolKeepItemListHandle = await Task.Run(() => searchTextCopy.FuzzyMatchSort(resultsCopy.value, (Steam.Ugc.Query.Result r) => r.tags.Replace(',', ' '), sortOutputWhenSearchStringIsEmpty: false, 5, stableSort: true));
			_CreateResultViews(poolKeepItemListHandle);
		}
		finally
		{
			if (resultsCopy != null)
			{
				((IDisposable)resultsCopy).Dispose();
			}
		}
	}

	private void _UpdateResultPaging(List<Steam.Ugc.Query.Result> results)
	{
		_maxPageNumber = results.GetMaxPageNumber(_pageSize);
		_pageNumber = Mathf.Clamp(_pageNumber, 1, maxPageNumber);
		onPageTextChange.Invoke($"<size=60%>Page</size>\n{pageNumber}/{maxPageNumber}");
	}

	private void _CreateResultViews(List<Steam.Ugc.Query.Result> results)
	{
		onNoResultsFoundChange.Invoke(results.IsNullOrEmpty());
		if (results.IsNullOrEmpty())
		{
			onNoResultsFoundTextChange.Invoke(string.Format("No Results Found For {0}{1}{2}{3} {4}{5}", searchText.HasVisibleCharacter() ? ("\"" + searchText + "\" ") : "", (statusFlags != EnumUtil<ContentInstallStatusFlags>.AllFlags) ? ("[" + EnumUtil.FriendlyName(statusFlags) + "] ") : "", EnumUtil.FriendlyName(_queryType), (queryType == Steam.Ugc.Query.QueryType.GroupMembersOnly) ? (" (" + group.name + ")") : "", _contentRef.SpecificTypeFriendly(), viewDownloadQueue.ToText(" In Download Queue")));
		}
		_UpdateResultPaging(results);
		using PoolKeepItemListHandle<Steam.Ugc.Query.Result> poolKeepItemListHandle = Pools.UseKeepItemList(results.GetPagedResults(_pageNumber, _pageSize));
		if ((bool)selectedItem && !_IsValidResult(selectedItem))
		{
			selectedItem = null;
		}
		if ((bool)selectedItem && !poolKeepItemListHandle.value.Contains(selectedItem))
		{
			poolKeepItemListHandle.value.ReplaceLast(selectedItem);
		}
		using PoolKeepItemHashSetHandle<Steam.Ugc.Query.Result> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(poolKeepItemListHandle.value);
		using PoolKeepItemDictionaryHandle<Steam.Ugc.Query.Result, SteamWorkshopItemView> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<Steam.Ugc.Query.Result, SteamWorkshopItemView>();
		foreach (SteamWorkshopItemView item in resultsContainer.gameObject.GetComponentsInChildrenPooled<SteamWorkshopItemView>())
		{
			poolKeepItemDictionaryHandle.Add(item, item);
		}
		foreach (SteamWorkshopItemView value in poolKeepItemDictionaryHandle.value.Values)
		{
			if (!poolKeepItemHashSetHandle.Contains(value))
			{
				value.gameObject.SetActive(value: false);
			}
		}
		foreach (Steam.Ugc.Query.Result item2 in poolKeepItemListHandle.value)
		{
			if (!poolKeepItemDictionaryHandle.ContainsKey(item2))
			{
				poolKeepItemDictionaryHandle.Add(item2, SteamWorkshopItemView.Create(_contentRef, item2, resultsContainer.transform));
			}
		}
		for (int i = 0; i < poolKeepItemListHandle.Count; i++)
		{
			poolKeepItemDictionaryHandle[poolKeepItemListHandle[i]].transform.SetSiblingIndex(i);
		}
	}

	private IEnumerable<Steam.Ugc.Query.QueryType> _ExcludedQueryTypes()
	{
		if (ProfileManager.prefs.steam.likedAuthors.Count == 0)
		{
			yield return Steam.Ugc.Query.QueryType.LikedCreators;
		}
		if (Steam.Friends.CachedGroups.Count == 0)
		{
			yield return Steam.Ugc.Query.QueryType.GroupMembersOnly;
		}
	}

	private void Awake()
	{
		Steam.Friends.ClearFriendsAndGroupCaches();
		resultsContainer.onContextSelectionChange.AddListener(_OnResultSelectionChange);
		UIUtil.CreateEnumComboBox(delegate(ContentInstallStatusFlags c)
		{
			statusFlags = c;
		}, statusContainer, _statusFlags = ProfileManager.prefs.steam.statusFlags);
		UIUtil.CreateEnumComboBox(delegate(Steam.Ugc.Query.QueryType c)
		{
			queryType = c;
		}, queryTypeContainer, _queryType = ProfileManager.prefs.steam.queryType, fitIntoParent: true, null, 0, _ExcludedQueryTypes());
		UIUtil.CreateSteamGroupView(_group = ProfileManager.prefs.steam.group, delegate(Steam.Friends.Group g)
		{
			group = g;
		}, groupContainer, null, null, fitIntoParent: true);
		groupContainer.gameObject.SetActive(_queryType == Steam.Ugc.Query.QueryType.GroupMembersOnly);
		searchField.onValueChanged.AddListener(delegate(string s)
		{
			searchText = s;
		});
		searchField.onSubmit.AddListener(delegate
		{
			_searchGracePeriod = Mathf.Min(_searchGracePeriod, 0.001f);
		});
		_defaultSpacing = resultsLayout.spacing;
		_defaultPadding = resultsLayout.padding.ToVector4();
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
		if (_searchGracePeriod > 0f && (_searchGracePeriod -= Time.unscaledDeltaTime) <= 0f)
		{
			_SetDirty(Steam.ParsePublishedFileId(_searchText).HasValue || _query is Steam.Ugc.QuerySpecific);
			_pageNumber = 1;
		}
		_RefreshResults();
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private void OnDisable()
	{
		resultsContainer.gameObject.SetActiveChildren(active: false);
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void OnDestroy()
	{
		Pools.RepoolObject(ref _query);
	}

	public void ConfirmSelection()
	{
		if (downloadQueue.Count > 0)
		{
			bool continueDownload = true;
			int index = 0;
			Job.Process(Job.EmptyEnumerator(), Department.Content).ChainJobs(((IEnumerable<Steam.Ugc.Query.Result>)downloadQueue).Select((Func<Steam.Ugc.Query.Result, Func<Job>>)((Steam.Ugc.Query.Result r) => delegate
			{
				if (!continueDownload)
				{
					return Job.Process(Job.EmptyEnumerator());
				}
				Steam.Ugc.Query.Result result = r;
				UnityAction<ContentRef> onContentDownloaded = _onContentDownloaded;
				Transform parent = base.transform;
				Action onDownloadFailed = delegate
				{
					continueDownload = false;
				};
				string[] obj = new string[6] { r.name, " (", null, null, null, null };
				int num = index + 1;
				index = num;
				obj[2] = num.ToString();
				obj[3] = " / ";
				obj[4] = downloadQueue.Count.ToString();
				obj[5] = ")";
				return UIUtil.CreateContentRefDownloadPopup(result, onContentDownloaded, parent, null, onDownloadFailed, string.Concat(obj));
			})).Concat(new Func<Job>[1]
			{
				() => Job.Action(Close)
			}).ToArray());
		}
		else if ((bool)selectedItem)
		{
			UIUtil.CreateContentRefDownloadPopup(selectedItem, _onContentDownloaded, base.transform, Close);
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

	public void UpdateSpacing()
	{
		Vector2 preferredSize = TypeBlueprintsThatRequireSpecialSpacing[_contentRef.GetType()].GetComponent<LayoutElement>().GetPreferredSize();
		float num = ContentRefSearcher.GetScale(_contentRef);
		Vector2 vector = resultsLayout.cellSizeType.StartingValue(preferredSize);
		int num2 = 0;
		foreach (SteamWorkshopItemView item in resultsContainer.gameObject.GetComponentsInChildrenPooled<SteamWorkshopItemView>())
		{
			if (item.layoutIsReady && ++num2 > 0)
			{
				vector = resultsLayout.cellSizeType.Apply(vector, item.GetPreferredSize() * (1f / num));
			}
		}
		if (num2 != 0)
		{
			resultsLayout.spacing = (_defaultSpacing + Vector2.Min(Vector2.zero, preferredSize - vector)).Clamp(new Vector2(0f, -12f), Vector2.positiveInfinity);
			resultsLayout.padding.SetPadding(_defaultPadding * num);
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

	public void ToggleDownloadQueueView()
	{
		viewDownloadQueue = !viewDownloadQueue;
	}

	public void ToggleToDownloadQueue(Steam.Ugc.Query.Result result)
	{
		if (!downloadQueue.Add(result))
		{
			downloadQueue.Remove(result);
		}
		onDownloadQueueActiveChange.Invoke(downloadQueue.Count > 0);
		onDownloadQueueTextChange.Invoke(downloadQueue.ToStringSmart((Steam.Ugc.Query.Result r) => r.name));
		if (downloadQueue.Count > 0)
		{
			onItemIsSelectedChange.Invoke(arg0: true);
			onDownloadButtonTextChange.Invoke(string.Format("Download {0} queued {1}", downloadQueue.Count, "item".Pluralize(downloadQueue.Count)));
		}
		else
		{
			viewDownloadQueue = false;
			_OnSelectedItemChange();
		}
		_SetDirty(queryDirty: false);
	}
}
