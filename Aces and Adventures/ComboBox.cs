using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ComboBox : UIList
{
	public RectTransform selectedContainer;

	public GameObject selectedItemBlueprint;

	public bool setDataOnlyIfCollapsed = true;

	[HideInInspector]
	public string defaultSelected;

	[NonSerialized]
	[HideInInspector]
	public object defaultSelectedObject;

	protected UIListItem selectedItem;

	protected CollapseFitter _fitter;

	public UnityEvent OnToggle;

	protected override bool _showSearchField
	{
		get
		{
			if (Mathf.Max(data.SafeCount(), _originalData.SafeCount()) >= 7)
			{
				return !LaunchManager.InGame;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_fitter = GetComponentInChildren<CollapseFitter>(includeInactive: true);
		if (_fitter != null)
		{
			_fitter.OnExpandChange.AddListener(_OnFitterExpandChange);
		}
	}

	public override void GenerateList()
	{
		base.GenerateList();
		DoDefaultSelectedLogic();
	}

	protected virtual void DoDefaultSelectedLogic()
	{
		if (data.Count == 0 || (defaultSelectedObject != null && SelectValue(defaultSelectedObject, toggleList: false)))
		{
			return;
		}
		int index = 0;
		if (defaultSelected != "")
		{
			for (int i = 0; i < data.Count; i++)
			{
				if (data[i].value.ToString() == defaultSelected)
				{
					index = i;
					break;
				}
			}
		}
		SelectIndex(index, toggleList: false);
	}

	public override bool SetDataOnly()
	{
		if (setDataOnlyIfCollapsed)
		{
			CollapseFitter fitter = _fitter;
			if ((object)fitter == null)
			{
				return false;
			}
			return fitter.expandRatio <= 0f;
		}
		return false;
	}

	public override void Clear()
	{
		base.Clear();
		if ((bool)selectedItem)
		{
			selectedItem.gameObject.Destroy();
		}
	}

	public override void SelectIndex(int index, bool toggleList = true)
	{
		if (OnSelected != null)
		{
			OnSelected.Invoke();
		}
		selectedIndex = index;
		RefreshSelectedItem();
		SignalSelectionChanged();
		if (toggleList && !InputManager.I[KeyModifiers.Alt])
		{
			ToggleList();
		}
	}

	protected virtual void RefreshSelectedItem()
	{
		if (!this.IsDestroyed())
		{
			if ((bool)selectedItem)
			{
				selectedItem.gameObject.Destroy();
			}
			selectedItem = CreateFromData(GetSelectedItemDataFromSelection(), selectedItemBlueprint);
			selectedItem.transform.SetParent(selectedContainer.transform, worldPositionStays: false);
			selectedItem.GetComponent<Button>().onClick.AddListener(ToggleList);
		}
	}

	protected virtual UIListItemData GetSelectedItemDataFromSelection()
	{
		return data[selectedIndex];
	}

	public void ToggleList()
	{
		OnToggle.Invoke();
	}

	private void _SetDataOnCollapseOpen()
	{
		if (_fitter != null)
		{
			_fitter = null;
			if (data.Count == 0)
			{
				GenerateList();
			}
			else if (items.Count != data.Count)
			{
				Set(data.ToList());
			}
		}
	}

	private void _OnFitterExpandChange(float expandRatio)
	{
		if (!(_fitter == null) && expandRatio > 0f)
		{
			_fitter.OnExpandChange.RemoveListener(_OnFitterExpandChange);
			_SetDataOnCollapseOpen();
		}
	}

	public virtual void SetMaxCount(int maxCount)
	{
	}

	protected override void _SearchList(string searchString)
	{
		UIListItem uIListItem = selectedItem;
		selectedItem = null;
		base._SearchList(searchString);
		selectedItem = uIListItem;
	}
}
