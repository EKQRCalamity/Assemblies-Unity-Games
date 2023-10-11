using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class IListUIControlSearcher : MonoBehaviour
{
	private static ResourceBlueprint<GameObject> _Blueprint = "UI/Reflection/IListUIControlSearcher";

	[Range(0f, 1f)]
	public float searchGracePeriod = 0.5f;

	public TMP_InputField searchInputField;

	[Header("Events")]
	public IntListEvent onIndexMapChange;

	public StringEvent onPageTextChange;

	private IList _list;

	private int _pageSize;

	private int _pageNumber;

	private string _searchText = "";

	private float? _searchGracePeriod;

	private bool _isDirty;

	private List<object> _results;

	private List<int> _indexMap;

	private bool? _focusSearch;

	private List<object> results => _results ?? (_results = new List<object>());

	public List<int> indexMap => _indexMap ?? (_indexMap = new List<int>());

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

	public int maxPageNumber => Math.Max(0, results.Count - 1) / pageSize + 1;

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
				_OnPageNumberChange();
			}
		}
	}

	public int underlyingPageNumber
	{
		set
		{
			_pageNumber = value;
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
			if (SetPropertyUtility.SetStruct(ref _pageSize, Mathf.Max(1, value)))
			{
				_ResetSearch();
			}
		}
	}

	private int _pageIndexOffset => (pageNumber - 1) * pageSize;

	public bool isDirty
	{
		get
		{
			return _isDirty;
		}
		set
		{
			_isDirty = value;
		}
	}

	public static IListUIControlSearcher Create(IList list, int pageSize, Transform parent, Couple<string, int>? persistedSearchData = null)
	{
		return UnityEngine.Object.Instantiate((GameObject)_Blueprint, parent).GetComponent<IListUIControlSearcher>().SetData(list, pageSize, persistedSearchData);
	}

	private void _ResetSearch(bool requestFocusSearch = false)
	{
		_pageNumber = 1;
		_isDirty = true;
		_focusSearch = (requestFocusSearch ? new bool?(false) : null);
	}

	private void _OnSearchTextChange()
	{
		_searchGracePeriod = searchGracePeriod;
	}

	private void _OnPageNumberChange()
	{
		_isDirty = true;
	}

	private string _ToSearchText(object obj)
	{
		return obj.ToString().RemoveRichText().ToTagString();
	}

	private IEnumerable<object> _GetPagedResults()
	{
		for (int x = _pageIndexOffset; x < Mathf.Min(results.Count, pageNumber * pageSize); x++)
		{
			yield return results[x];
		}
	}

	private void _UpdateResults()
	{
		string effectiveSearchString = (searchText.HasVisibleCharacter() ? searchText : "");
		results.ClearAndCopyFrom(effectiveSearchString.FuzzyMatchSort(_list.Cast<object>(), out effectiveSearchString, _ToSearchText, sortOutputWhenSearchStringIsEmpty: false, 5, stableSort: true).AsEnumerable());
		_pageNumber = Mathf.Clamp(_pageNumber, 1, maxPageNumber);
		indexMap.Clear();
		foreach (object item in _GetPagedResults())
		{
			indexMap.Add(item.GetType().IsValueType ? _list.IndexOf(item) : _list.IndexOfByReference(item));
		}
		onIndexMapChange.Invoke(indexMap);
		onPageTextChange.Invoke($"<size=60%>Page</size>\n{pageNumber}/{maxPageNumber}");
		(searchInputField.placeholder as TextMeshProUGUI).text = "Type To Search " + _list.Count + " Items…";
		if (_focusSearch.HasValue)
		{
			_focusSearch = true;
		}
		_isDirty = false;
	}

	private void OnEnable()
	{
		_isDirty = true;
	}

	private void Update()
	{
		if (_searchGracePeriod.HasValue && (_searchGracePeriod -= ((!Input.GetKey(KeyCode.Backspace) || !searchText.HasVisibleCharacter()) ? Time.unscaledDeltaTime : 0f)) <= 0f)
		{
			float? num = (_searchGracePeriod = null);
			if (!num.HasValue)
			{
				_ResetSearch(requestFocusSearch: true);
			}
		}
		if (_isDirty)
		{
			_UpdateResults();
		}
	}

	private void LateUpdate()
	{
		if (_focusSearch == true)
		{
			bool? flag = (_focusSearch = null);
			if (!flag.HasValue)
			{
				searchInputField.FocusAndMoveToEnd();
			}
		}
	}

	public IListUIControlSearcher SetData(IList list, int pageSize, Couple<string, int>? persistedSearchData = null)
	{
		_list = list;
		this.pageSize = pageSize;
		if (persistedSearchData.HasValue)
		{
			searchInputField.text = (persistedSearchData.Value.a.HasVisibleCharacter() ? persistedSearchData.Value.a : "");
			_searchGracePeriod = null;
			_pageNumber = persistedSearchData.Value;
		}
		else
		{
			_pageNumber = Math.Max(0, _list.Count - 1) / this.pageSize + 1;
		}
		return this;
	}

	public void ForceUpdate()
	{
		_UpdateResults();
	}

	public void NextPage()
	{
		pageNumber = (InputManager.I[KeyModifiers.Shift] ? maxPageNumber : (pageNumber + 1));
	}

	public void PreviousPage()
	{
		pageNumber = (InputManager.I[KeyModifiers.Shift] ? 1 : (pageNumber - 1));
	}
}
