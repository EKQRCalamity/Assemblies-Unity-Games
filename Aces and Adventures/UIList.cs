using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[ScriptOrder(-1)]
public class UIList : MonoBehaviour
{
	private Func<Transform, bool> _ValidClearParentFunc = (Transform t) => (bool)t.GetComponent<UIList>() || (bool)t.GetComponent<CollapseFitter>();

	public CategorySortType categorySorting = CategorySortType.DescendingNumberInCategory;

	public bool openCategoriesByDefault;

	public GameObject itemBlueprint;

	public GameObject searchBlueprint;

	public RectTransform listContainer;

	public float indentPixels = 24f;

	[Range(10f, 200f)]
	[HideInInspectorIf("_hideSearch", false)]
	public int setDataOnlyCountThresholdWhenSearchable = 50;

	protected int selectedIndex;

	protected List<UIListItemData> data;

	protected List<UIListItemData> _originalData;

	protected List<UIListItem> items;

	protected Dictionary<string, GameObject> categories;

	protected List<GameObject> categoryRoots;

	private TMP_InputField _searchField;

	private GameObject _searchGameObject;

	private string _searchText;

	private int _frameOfLastDataSet;

	private float _searchGracePeriod;

	private bool _toggleListOnSelected = true;

	public UIListEvent OnGenerate;

	[SerializeField]
	protected UIListEvent _OnSet;

	public UnityEvent OnSelected;

	public StringEvent OnSelectedTextChanged;

	public IntEvent OnSelectedIndexChanged;

	public ObjectEvent OnSelectedValueChanged;

	private TMP_InputField searchField
	{
		get
		{
			if (!_searchField)
			{
				_searchGameObject = UnityEngine.Object.Instantiate(searchBlueprint);
				_searchGameObject.transform.SetParent(listContainer, worldPositionStays: false);
				_searchField = _searchGameObject.GetComponentInChildren<TMP_InputField>();
				_searchField.onValueChanged.AddListener(delegate
				{
					_searchGracePeriod = 0.5f;
				});
				_searchField.onSubmit.AddListener(_SelectFirstItemInSearch);
				_searchGameObject.transform.SetSiblingIndex(0);
				_searchField.gameObject.GetOrAddComponent<UIFocusRequester>();
			}
			return _searchField;
		}
	}

	private bool _canBeSearched => searchBlueprint;

	private bool _searchActive => !_searchText.IsNullOrEmpty();

	protected virtual bool _showSearchField => true;

	public bool toggleListOnSelected
	{
		get
		{
			return _toggleListOnSelected;
		}
		set
		{
			_toggleListOnSelected = value;
		}
	}

	public UIListEvent OnSet => _OnSet ?? (_OnSet = new UIListEvent());

	public event Action onManuallySelected;

	private static UIListItemData[] _GenerateGenericListItemData<T>(Dictionary<string, T> dataMap, Dictionary<string, string> categoryData, HashSet<T> exclude, bool useFriendlyName = true)
	{
		exclude = exclude ?? new HashSet<T>();
		categoryData = categoryData ?? new Dictionary<string, string>(0);
		UIListItemData[] array = new UIListItemData[dataMap.Count - exclude.Count];
		int num = 0;
		foreach (KeyValuePair<string, T> item in dataMap)
		{
			string key = item.Key;
			T value = item.Value;
			if (!exclude.Contains(value))
			{
				array[num++] = new UIListItemData(useFriendlyName ? key.FriendlyFromCamelOrPascalCase() : key, value, categoryData.ContainsKey(key) ? categoryData[key] : null);
				if (num == array.Length)
				{
					return array;
				}
			}
		}
		return array;
	}

	public static void GenerateGeneric<T>(UIList list, Dictionary<string, T> data, T defaultSelected = default(T), CategorySortType categorySort = CategorySortType.Alphabetical, Dictionary<string, string> categoryData = null, HashSet<T> exclude = null, bool useFriendlyName = true)
	{
		list.categorySorting = categorySort;
		if (categorySort == CategorySortType.Alphabetical)
		{
			data = data.OrderBy((KeyValuePair<string, T> p) => p.Key).ToDictionary((KeyValuePair<string, T> p) => p.Key, (KeyValuePair<string, T> p) => p.Value);
		}
		list.Set(_GenerateGenericListItemData(data, categoryData, exclude, useFriendlyName));
		ComboBox comboBox = list as ComboBox;
		if (!(comboBox == null) && !ReflectionUtil.SafeEquals(defaultSelected, default(T)))
		{
			comboBox.defaultSelectedObject = defaultSelected;
		}
	}

	protected virtual void Awake()
	{
		items = new List<UIListItem>();
		data = new List<UIListItemData>();
	}

	protected virtual void Start()
	{
		if ((bool)searchBlueprint)
		{
			_ = searchField;
			if ((bool)_searchGameObject)
			{
				_searchGameObject.SetActive(_showSearchField);
			}
		}
		if (data.Count == 0)
		{
			GenerateList();
		}
	}

	private void Update()
	{
		if (_searchGracePeriod > 0f && (_searchGracePeriod -= Time.unscaledDeltaTime) <= 0f && (bool)searchField)
		{
			_SearchList(searchField.text);
		}
	}

	public virtual void GenerateList()
	{
		Clear();
		if (OnGenerate != null)
		{
			OnGenerate.Invoke(this);
		}
		selectedIndex = -1;
	}

	public void Set(IEnumerable<UIListItemData> uiListItemData)
	{
		_frameOfLastDataSet = Time.frameCount;
		if (items.Count == data.Count)
		{
			Clear();
		}
		data = uiListItemData.ToList();
		if ((bool)searchBlueprint && (bool)searchField)
		{
			(searchField.placeholder as TextMeshProUGUI).text = "Type To Search " + data.Count + " Items…";
		}
		if (SetDataOnly())
		{
			return;
		}
		List<GameObject> list = null;
		HashSet<GameObject> hashSet = null;
		if (!_searchActive)
		{
			_RemoveRedundantCategoryData(data);
			foreach (UIListItemData datum in data)
			{
				if (!datum.category.IsNullOrEmpty())
				{
					categories = categories ?? new Dictionary<string, GameObject>();
					list = list ?? new List<GameObject>();
					hashSet = hashSet ?? new HashSet<GameObject>();
					string[] array = datum.category.Split('|');
					string text = "";
					string key = "";
					for (int i = 0; i < array.Length; i++)
					{
						string currentCategory = array[i];
						text += currentCategory;
						GameObject gameObject = categories.AddPairIfUnique(text, () => UIUtil.CreateCollapse(currentCategory).GetComponentInChildren<CollapseFitter>().gameObject);
						if (gameObject != null)
						{
							if (i > 0)
							{
								gameObject.GetRoot().transform.SetParent(categories[key].transform, worldPositionStays: false);
								hashSet.Add(gameObject);
							}
							list.Add(gameObject);
						}
						key = text;
						text += "|";
					}
				}
				else if (list != null && list.Count > 0)
				{
					GameObject gameObject2 = categories.AddPairIfUnique("Misc", () => UIUtil.CreateCollapse("Misc").GetComponentInChildren<CollapseFitter>().gameObject);
					if (gameObject2 != null)
					{
						list.Add(gameObject2);
					}
				}
			}
		}
		foreach (UIListItemData datum2 in data)
		{
			UIListItem uIListItem = CreateFromData(datum2);
			if (categories == null)
			{
				uIListItem.transform.SetParent(listContainer.transform, worldPositionStays: false);
			}
			else
			{
				uIListItem.transform.SetParent(categories[datum2.category.IsNullOrEmpty() ? "Misc" : datum2.category].transform, worldPositionStays: false);
			}
			int index = items.Count;
			items.Add(uIListItem);
			uIListItem.gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate
			{
				SelectIndex(index, toggleListOnSelected);
			});
		}
		if (list != null)
		{
			UIUtil.SortCategoryControls(list, categorySorting);
			categoryRoots = new List<GameObject>();
			for (int j = 0; j < list.Count; j++)
			{
				if (hashSet.Contains(list[j]))
				{
					list[j].transform.parent.SetAsLastSibling();
					continue;
				}
				GameObject root = list[j].GetRoot();
				root.transform.SetParent(listContainer.transform, worldPositionStays: false);
				categoryRoots.Add(root);
				if (openCategoriesByDefault)
				{
					root.GetComponentInChildren<CollapseFitter>().Open();
				}
			}
		}
		selectedIndex = -1;
		_SetUniqueLogic();
		OnSet.Invoke(this);
		Job.Process(_ShowSearchField());
	}

	private IEnumerator _ShowSearchField()
	{
		if ((bool)_searchGameObject)
		{
			_searchGameObject.SetActive(_showSearchField);
		}
		yield break;
	}

	protected virtual void _SetUniqueLogic()
	{
	}

	public virtual void Clear()
	{
		if (categories == null)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if ((bool)items[i])
				{
					UnityEngine.Object.Destroy(items[i].transform.GetSiblingTransformRelativeTo(_ValidClearParentFunc).gameObject.SetActiveAndReturn(active: false));
				}
			}
		}
		else if (categoryRoots != null)
		{
			foreach (GameObject categoryRoot in categoryRoots)
			{
				if ((bool)categoryRoot)
				{
					UnityEngine.Object.Destroy(categoryRoot.SetActiveAndReturn(active: false));
				}
			}
		}
		data.Clear();
		items.Clear();
		categories = null;
		categoryRoots = null;
	}

	public virtual bool SetDataOnly()
	{
		if (_canBeSearched && !_searchActive)
		{
			return data.Count > setDataOnlyCountThresholdWhenSearchable;
		}
		return false;
	}

	public void RemoveValue(object value)
	{
		if (value == null)
		{
			return;
		}
		data.RemoveAll((UIListItemData d) => value.Equals(d.value));
		if (_originalData != null)
		{
			_originalData.RemoveAll((UIListItemData d) => value.Equals(d.value));
		}
		foreach (UIListItem item in base.gameObject.GetComponentsInChildrenPooled<UIListItem>())
		{
			if (value.Equals(item.value))
			{
				UnityEngine.Object.Destroy(((categoryRoots == null) ? item.transform.GetSiblingTransformRelativeTo<UIList>() : item.transform.GetSiblingTransformRelativeTo<CollapseFitter>()).gameObject.SetActiveAndReturn(active: false));
			}
		}
		items = base.gameObject.GetComponentsInChildrenPooled<UIListItem>().AsEnumerable().ToList();
	}

	protected UIListItem CreateFromData(UIListItemData data, GameObject blueprint = null)
	{
		UIListItem uIListItem = UnityEngine.Object.Instantiate(blueprint ?? itemBlueprint).AddComponent<UIListItem>();
		uIListItem.Init(data);
		Button button = uIListItem.gameObject.GetComponentInChildren<Button>();
		if (!button)
		{
			button = uIListItem.gameObject.AddComponent<Button>();
		}
		if (data.tooltip.HasVisibleCharacter())
		{
			TooltipCreator.CreateTextTooltip(button.transform, data.tooltip, beginShowTimer: false, 0.2f, backgroundEnabled: true, TextAlignmentOptions.Center, 0f, 12f, TooltipDirection.Vertical, TooltipOrthogonalDirection.Center, 1.333f, matchContentScaleWithCreator: false, deactivateContentOnHide: true, recurseRect: false, trackCreator: true);
		}
		return uIListItem;
	}

	public virtual void SelectIndex(int index, bool toggleList = true)
	{
		if (OnSelected != null)
		{
			OnSelected.Invoke();
		}
		selectedIndex = index;
		SignalSelectionChanged();
	}

	public void SelectText(string text, bool toggleList = true)
	{
		for (int i = 0; i < data.Count; i++)
		{
			if (data[i].text == text)
			{
				SelectIndex(i, toggleList);
				break;
			}
		}
	}

	public bool SelectValue(object value, bool toggleList = true, bool clearSearch = false)
	{
		if (clearSearch)
		{
			SetSearchText("");
		}
		for (int i = 0; i < data.Count; i++)
		{
			if (data[i].value.Equals(value))
			{
				SelectIndex(i, toggleList);
				return true;
			}
		}
		return false;
	}

	public Transform GetValueTransform(object value)
	{
		foreach (UIListItem item in base.gameObject.GetComponentsInChildrenPooled<UIListItem>())
		{
			if (item.value.Equals(value))
			{
				return item.transform;
			}
		}
		return null;
	}

	public void SignalSelectionChanged()
	{
		if (OnSelectedIndexChanged != null)
		{
			OnSelectedIndexChanged.Invoke(selectedIndex);
		}
		if (OnSelectedTextChanged != null)
		{
			OnSelectedTextChanged.Invoke(GetSelectedText());
		}
		if (OnSelectedValueChanged != null)
		{
			OnSelectedValueChanged.Invoke(GetSelectedValue());
		}
		if (this.onManuallySelected != null && Time.frameCount > _frameOfLastDataSet)
		{
			this.onManuallySelected();
		}
	}

	protected virtual string GetSelectedText()
	{
		return data[selectedIndex].text;
	}

	protected virtual object GetSelectedValue()
	{
		return data[selectedIndex].value;
	}

	protected void _RemoveRedundantCategoryData(List<UIListItemData> uiData)
	{
		List<string> list = (from d in uiData
			select d.category into s
			where !s.IsNullOrEmpty()
			select s).Distinct().ToList();
		if (list.Count == 0)
		{
			return;
		}
		list.Sort();
		TreeNode<string> treeNode = new TreeNode<string>("");
		for (int i = 0; i < list.Count; i++)
		{
			treeNode.AddChildBranch(list[i].Split('|'));
		}
		HashSet<Tuple<string, int>> hashSet = new HashSet<Tuple<string, int>>();
		foreach (TreeNode<string> item in treeNode.DepthFirstEnumNodes())
		{
			if (item.parent != null && item.parent.children.Count == 1)
			{
				hashSet.Add(new Tuple<string, int>(item.value, item.DepthLevel - 1));
			}
		}
		if (hashSet.Count == 0)
		{
			return;
		}
		foreach (UIListItemData uiDatum in uiData)
		{
			if (uiDatum.category.IsNullOrEmpty())
			{
				continue;
			}
			StringBuilder stringBuilder = new StringBuilder();
			string[] array = uiDatum.category.Split('|');
			for (int j = 0; j < array.Length; j++)
			{
				string text = array[j];
				if (!hashSet.Contains(new Tuple<string, int>(text, j)))
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.Append('|');
					}
					stringBuilder.Append(text);
				}
			}
			uiDatum.category = stringBuilder.ToString();
		}
	}

	public virtual void SetPrefixText(string prefixText)
	{
	}

	public virtual void SetCategorySorting(string sort)
	{
		categorySorting = (CategorySortType)Enum.Parse(typeof(CategorySortType), sort);
	}

	public void SetSearchText(string searchText)
	{
		if ((bool)searchField)
		{
			searchField.text = searchText;
		}
	}

	protected virtual void _SearchList(string searchString)
	{
		_searchText = searchString;
		_originalData = _originalData ?? data.Clone();
		if (searchString.IsNullOrEmpty())
		{
			Set(_originalData);
			return;
		}
		for (int num = listContainer.childCount - 1; num >= 1; num--)
		{
			UnityEngine.Object.DestroyImmediate(listContainer.GetChild(num).gameObject);
		}
		using PoolKeepItemListHandle<UIListItemData> poolKeepItemListHandle = searchString.FuzzyMatchSort(_originalData, out _searchText, (UIListItemData d) => d.searchText);
		Set(poolKeepItemListHandle.value.Take((searchString != "*") ? (setDataOnlyCountThresholdWhenSearchable * 2) : poolKeepItemListHandle.Count));
	}

	private void _SelectFirstItemInSearch(string searchString)
	{
		if (_searchGracePeriod > 0f && (_searchGracePeriod = 0f) == 0f)
		{
			_SearchList(searchString);
		}
		if (!data.IsNullOrEmpty() && !_originalData.IsNullOrEmpty() && !searchString.IsNullOrEmpty() && !searchString.Trim().IsNullOrEmpty())
		{
			_frameOfLastDataSet = Time.frameCount - 1;
			SelectIndex(0);
		}
	}

	public void ClearSearchHistory()
	{
		_originalData = null;
		SetSearchText("");
	}

	public void FocusInputOnSearchText()
	{
		if ((bool)searchField && InputManager.EventSystemEnabled)
		{
			InputManager.EventSystem.SetSelectedGameObject(searchField.gameObject);
		}
	}
}
