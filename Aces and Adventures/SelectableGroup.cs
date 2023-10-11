using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectableGroup : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerClickHandler
{
	private static readonly Predicate<SelectableItem> RemoveSelectableItemPredicate = (SelectableItem selectableItem) => !selectableItem || !selectableItem.enabled || !selectableItem.selected;

	public bool disableMultiSelect;

	[SerializeField]
	protected ComponentListEvent _onContextSelectionChange;

	[EnumFlags]
	public KeyModifiers disableSelectKeyModifiers;

	private HashSet<SelectableItem> _selectedItems;

	private SelectableItem _draggedItem;

	private bool _isDirty;

	public ComponentListEvent onContextSelectionChange => _onContextSelectionChange ?? (_onContextSelectionChange = new ComponentListEvent());

	public HashSet<SelectableItem> selectedItems => _selectedItems ?? (_selectedItems = new HashSet<SelectableItem>(ReferenceEqualityComparer<SelectableItem>.Default));

	public bool this[SelectableItem item]
	{
		get
		{
			return selectedItems.Contains(item);
		}
		set
		{
			if (value)
			{
				if (disableMultiSelect && selectedItems.Count > 0 && !selectedItems.Contains(item))
				{
					ClearSelected();
				}
				if (selectedItems.Add(item) && _SetDirty())
				{
					item.selected = true;
				}
			}
			else if (selectedItems.Remove(item) && _SetDirty())
			{
				item.selected = false;
			}
		}
	}

	private bool _ShouldDragOtherItems(SelectableItem item)
	{
		if (item.selected)
		{
			return item == _draggedItem;
		}
		return false;
	}

	private void _SignalSelectionChange()
	{
		_isDirty = false;
		_draggedItem = null;
		using PoolKeepItemListHandle<Component> poolKeepItemListHandle = Pools.UseKeepItemList<Component>();
		foreach (SelectableItem selectedItem in selectedItems)
		{
			foreach (Component contextItem in selectedItem.contextItems)
			{
				poolKeepItemListHandle.Add(contextItem);
			}
		}
		onContextSelectionChange.Invoke(poolKeepItemListHandle);
	}

	private bool _SetDirty()
	{
		return _isDirty = true;
	}

	private void LateUpdate()
	{
		if (selectedItems.RemoveWhere(RemoveSelectableItemPredicate) > 0 || _isDirty)
		{
			_SignalSelectionChange();
		}
	}

	public void ClearSelected()
	{
		foreach (SelectableItem item in selectedItems.EnumerateSafe())
		{
			this[item] = false;
		}
	}

	public void ForceUpdateSelection()
	{
		if (_isDirty)
		{
			_SignalSelectionChange();
		}
	}

	public void SelectAll()
	{
		foreach (SelectableItem item in base.gameObject.GetComponentsInChildrenPooled<SelectableItem>())
		{
			this[item] = true;
		}
	}

	public void RefreshSelection()
	{
		using PoolKeepItemListHandle<SelectableItem> poolKeepItemListHandle = Pools.UseKeepItemList(selectedItems);
		foreach (SelectableItem item in poolKeepItemListHandle.value)
		{
			this[item] = false;
		}
		_SignalSelectionChange();
		foreach (SelectableItem item2 in poolKeepItemListHandle.value)
		{
			this[item2] = true;
		}
		_SignalSelectionChange();
	}

	public void OnSelectableItemClick(SelectableItem item, PointerEventData eventData)
	{
		if (!eventData.dragging && !EnumUtil.HasFlag(disableSelectKeyModifiers, InputManager.I.keyModifiers))
		{
			if (InputManager.I[KeyModifiers.Shift | KeyModifiers.Control])
			{
				this[item] = false;
				return;
			}
			if (InputManager.I[KeyModifiers.Control])
			{
				this[item] = !this[item];
				return;
			}
			if (InputManager.I[KeyModifiers.Shift])
			{
				this[item] = true;
				return;
			}
			ClearSelected();
			this[item] = true;
		}
	}

	public void OnSelectableItemClick(SelectableItem item, PointerEventData eventData, bool signalIfDirty)
	{
		OnSelectableItemClick(item, eventData);
		if (_isDirty && signalIfDirty)
		{
			_SignalSelectionChange();
		}
	}

	public void OnSelectableItemBeginDrag(SelectableItem item, PointerEventData eventData)
	{
		_draggedItem = _draggedItem ?? item;
		if (!_ShouldDragOtherItems(item))
		{
			return;
		}
		foreach (SelectableItem selectedItem in selectedItems)
		{
			if (selectedItem != _draggedItem)
			{
				selectedItem.pointerDrag.OnBeginDrag(eventData);
			}
		}
	}

	public void OnSelectableItemDrag(SelectableItem item, PointerEventData eventData)
	{
		if (!_ShouldDragOtherItems(item))
		{
			return;
		}
		foreach (SelectableItem selectedItem in selectedItems)
		{
			if (selectedItem != _draggedItem)
			{
				selectedItem.pointerDrag.OnDrag(eventData);
			}
		}
	}

	public void OnSelectableItemEndDrag(SelectableItem item, PointerEventData eventData)
	{
		if (_ShouldDragOtherItems(item))
		{
			foreach (SelectableItem selectedItem in selectedItems)
			{
				if (selectedItem != _draggedItem)
				{
					selectedItem.pointerDrag.OnEndDrag(eventData);
				}
			}
		}
		if (item == _draggedItem)
		{
			_draggedItem = null;
		}
	}

	public Vector3? GetSelectedCentroidWorldSpace()
	{
		if (selectedItems.Count != 0)
		{
			return selectedItems.Select((SelectableItem selected) => selected.transform.position).Centroid();
		}
		return null;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!eventData.dragging && eventData.button == PointerEventData.InputButton.Left && !InputManager.I.AnyKeyModifierDown())
		{
			ClearSelected();
		}
	}
}
