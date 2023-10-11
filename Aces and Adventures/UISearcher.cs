using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UISearcher : MonoBehaviour
{
	private static readonly TextBuilder _Builder = new TextBuilder(clearOnToString: true);

	public int searchTextComponentIndex = -1;

	[SerializeField]
	protected Transform _parent;

	[SerializeField]
	[HideInInspector]
	protected string _text;

	public bool removeRichText = true;

	public string getTextBetweenStart;

	public string getTextBetweenEnd;

	[SerializeField]
	private StringEvent _onSearchChanged;

	[SerializeField]
	private UnityEvent _onSearchStarted;

	[SerializeField]
	private UnityEvent _onSearchCleared;

	private List<Transform> _siblingOrder;

	public Transform parent
	{
		get
		{
			if (!_parent)
			{
				return _parent = (base.transform.parent ? base.transform.parent : base.transform);
			}
			return _parent;
		}
	}

	public string text
	{
		get
		{
			return _text;
		}
		set
		{
			string previousText = _text;
			if (SetPropertyUtility.SetObjectEQ(ref _text, (value ?? "").Trim(), StringComparer.OrdinalIgnoreCase))
			{
				_OnSearchTextChange(previousText);
			}
		}
	}

	public StringEvent onSearchChanged => _onSearchChanged ?? (_onSearchChanged = new StringEvent());

	public UnityEvent onSearchStarted => _onSearchStarted ?? (_onSearchStarted = new UnityEvent());

	public UnityEvent onSearchCleared => _onSearchCleared ?? (_onSearchCleared = new UnityEvent());

	private Transform _ignoredSibling => base.transform.GetSiblingTransformRelativeTo((Transform t) => t == parent, returnNullOnFail: true);

	private string _ProcessText(string t)
	{
		if (!getTextBetweenStart.IsNullOrEmpty() && !getTextBetweenEnd.IsNullOrEmpty())
		{
			t = t.GetTextBetween(getTextBetweenStart, getTextBetweenEnd);
		}
		if (!removeRichText)
		{
			return t;
		}
		return t.RemoveRichText();
	}

	private void _OnSearchTextChange(string previousText)
	{
		if (previousText.IsNullOrEmpty() && !_text.IsNullOrEmpty())
		{
			onSearchStarted.Invoke();
		}
		onSearchChanged.Invoke(_text);
		if (_text.IsNullOrEmpty())
		{
			_OnSearchCleared();
			return;
		}
		_CacheSiblingOrder();
		Transform ignoredSibling = _ignoredSibling;
		using (PoolDictionaryValuesHandle<string, List<Transform>> poolDictionaryValuesHandle = Pools.UseDictionaryValues<string, List<Transform>>())
		{
			foreach (Transform item in parent.ChildrenSafe())
			{
				if (item == ignoredSibling)
				{
					continue;
				}
				using (PoolKeepItemListHandle<TextMeshProUGUI> poolKeepItemListHandle = item.gameObject.GetComponentsInChildrenPooled<TextMeshProUGUI>())
				{
					if (searchTextComponentIndex < 0)
					{
						foreach (TextMeshProUGUI item2 in poolKeepItemListHandle.value)
						{
							_Builder.Append(_ProcessText(item2.text)).Space();
						}
						_Builder.RemoveFromEnd(1);
					}
					else if (poolKeepItemListHandle.value.Count > searchTextComponentIndex)
					{
						_Builder.Append(_ProcessText(poolKeepItemListHandle[searchTextComponentIndex].text));
					}
					string key = _Builder.ToString();
					if (!poolDictionaryValuesHandle.ContainsKey(key))
					{
						poolDictionaryValuesHandle.Add(key, Pools.TryUnpool<List<Transform>>());
					}
					poolDictionaryValuesHandle[key].Add(item);
				}
				_SetSiblingActive(item, 0, active: false);
			}
			int num = 0;
			string effectiveSearchString;
			foreach (string item3 in _text.FuzzyMatchSort(poolDictionaryValuesHandle.value.Keys, out effectiveSearchString))
			{
				foreach (Transform item4 in poolDictionaryValuesHandle[item3])
				{
					_SetSiblingActive(item4, num++, active: true);
				}
			}
		}
		if ((bool)ignoredSibling)
		{
			ignoredSibling.SetAsFirstSibling();
		}
	}

	private void _CacheSiblingOrder()
	{
		if (_siblingOrder == null)
		{
			Pools.TryUnpool(ref _siblingOrder);
			for (int i = 0; i < parent.childCount; i++)
			{
				_siblingOrder.Add(parent.GetChild(i));
			}
		}
	}

	private void _RestoreSiblings()
	{
		if (_siblingOrder == null)
		{
			return;
		}
		int num = 0;
		foreach (Transform item in _siblingOrder)
		{
			if ((bool)item && item.parent == parent)
			{
				_SetSiblingActive(item, num++, active: true);
			}
		}
		Pools.TryRepool(ref _siblingOrder);
	}

	private void _SetSiblingActive(Transform sibling, int siblingIndex, bool active)
	{
		sibling.GetOrAddComponent<LayoutElement>().ignoreLayout = !active;
		sibling.GetOrAddComponent<CanvasGroup>().alpha = active.ToFloat();
		sibling.GetOrAddComponent<CanvasGroup>().blocksRaycasts = active;
		if (active)
		{
			sibling.SetSiblingIndex(siblingIndex);
		}
	}

	private void _OnSearchCleared()
	{
		onSearchCleared.Invoke();
		_RestoreSiblings();
	}

	public void Clear()
	{
		text = "";
	}
}
