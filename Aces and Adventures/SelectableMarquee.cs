using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform), typeof(SelectableGroup))]
public class SelectableMarquee : MonoBehaviour, IInitializePotentialDragHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[EnumFlags]
	public KeyModifiers keyModifiers;

	public RectTransform visualRect;

	[EnumFlags]
	public PointerInputButtonFlags dragButtons = PointerInputButtonFlags.Left;

	public PointerEventBubbleType dragBubbleType = PointerEventBubbleType.Hierarchy;

	private SelectableGroup _group;

	private HashSet<SelectableItem> _previouslySelected;

	private HashSet<SelectableItem> _itemsInRect;

	private Vector3 _startDragPosition;

	private KeyModifiers _previousKeyModifiers;

	private Comparison<SelectableItem> _sortComparison;

	public RectTransform rectTransform => base.transform as RectTransform;

	public SelectableGroup group => this.CacheComponentInParent(ref _group);

	public bool dragging
	{
		get
		{
			return visualRect.gameObject.activeSelf;
		}
		private set
		{
			if (value != dragging)
			{
				itemsInRect.Clear();
			}
			visualRect.gameObject.SetActive(value);
		}
	}

	protected HashSet<SelectableItem> previouslySelected => _previouslySelected ?? (_previouslySelected = new HashSet<SelectableItem>());

	protected HashSet<SelectableItem> itemsInRect => _itemsInRect ?? (_itemsInRect = new HashSet<SelectableItem>());

	private Comparison<SelectableItem> sortComparison => (SelectableItem a, SelectableItem b) => (a.transform.position - _startDragPosition).sqrMagnitude.CompareTo((b.transform.position - _startDragPosition).sqrMagnitude);

	private void OnEnable()
	{
		dragging = false;
	}

	private void OnDisable()
	{
		dragging = false;
		_group = null;
	}

	private void OnTransformParentChanged()
	{
		_group = null;
	}

	private void Update()
	{
		if (dragging && _previousKeyModifiers != InputManager.I.keyModifiers)
		{
			OnDrag(null);
		}
	}

	private void _RemoveItemsInRect()
	{
		foreach (SelectableItem item in previouslySelected)
		{
			group[item] = !itemsInRect.Contains(item);
		}
	}

	private void _ToggleItemsInRect()
	{
		foreach (SelectableItem item in previouslySelected.SymmetricExcept(itemsInRect))
		{
			group[item] = true;
		}
	}

	private void _AddItemsInRect()
	{
		foreach (SelectableItem item in previouslySelected)
		{
			group[item] = true;
		}
		_SelectItemsInRect();
	}

	private void _SelectItemsInRect()
	{
		foreach (SelectableItem item in itemsInRect)
		{
			group[item] = true;
		}
	}

	private bool _IsMarqueeRectValid(SelectableItem selectable, Rect3D worldMarqueeRect)
	{
		foreach (ISelectableMarqueeFilter item in selectable.gameObject.GetInterfacesPooled<ISelectableMarqueeFilter>())
		{
			if (!item.IsMarqueeRectValid(worldMarqueeRect))
			{
				return false;
			}
		}
		return true;
	}

	private bool _IsValidDrag(PointerEventData eventData, bool checkKeyModifiers = true)
	{
		if ((bool)group && EnumUtil.HasFlagConvert(dragButtons, eventData.button))
		{
			if (checkKeyModifiers)
			{
				return InputManager.I[keyModifiers];
			}
			return true;
		}
		return false;
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		if (_IsValidDrag(eventData))
		{
			eventData.useDragThreshold = false;
		}
		else
		{
			dragBubbleType.BubblePointerDrag(this, eventData, ExecuteEvents.initializePotentialDrag);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (_IsValidDrag(eventData))
		{
			previouslySelected.CopyFrom(group.selectedItems, clearExisting: true);
			_startDragPosition = rectTransform.GetPlane(PlaneAxes.XY).Raycast(eventData.pressEventCamera.ScreenPointToRay(eventData.position.Unproject(AxisType.Z))).Value;
			dragging = true;
			visualRect.SetAsLastSibling();
		}
		else
		{
			dragBubbleType.BubblePointerDrag(this, eventData, ExecuteEvents.beginDragHandler);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!group)
		{
			return;
		}
		if (eventData != null)
		{
			Vector3? vector = rectTransform.GetPlane(PlaneAxes.XY).Raycast(eventData.pressEventCamera.ScreenPointToRay(eventData.position.Unproject(AxisType.Z)));
			if (!vector.HasValue)
			{
				return;
			}
			Rect3D rect3D = new Rect3D(-base.transform.forward, base.transform.up, _startDragPosition, vector.Value);
			visualRect.SetWorldCornersPreserveScale(rect3D);
			Rect rect = rect3D.WorldToViewportRect(eventData.pressEventCamera).Project2D();
			using PoolKeepItemListHandle<SelectableItem> poolKeepItemListHandle = Pools.UseKeepItemList<SelectableItem>();
			foreach (SelectableItem item in group.gameObject.GetComponentsInChildrenPooled<SelectableItem>())
			{
				if (rect.Overlaps(new Rect3D(item.transform as RectTransform).WorldToViewportRect(eventData.pressEventCamera).Project2D()) && _IsMarqueeRectValid(item, rect3D))
				{
					poolKeepItemListHandle.Add(item);
				}
			}
			poolKeepItemListHandle.value.Sort(sortComparison);
			foreach (SelectableItem item2 in itemsInRect.EnumerateSafe())
			{
				if (!poolKeepItemListHandle.value.Contains(item2))
				{
					itemsInRect.Remove(item2);
				}
			}
			foreach (SelectableItem item3 in poolKeepItemListHandle.value)
			{
				itemsInRect.Add(item3);
			}
		}
		_previousKeyModifiers = InputManager.I.keyModifiers;
		group.ClearSelected();
		if (InputManager.I[KeyModifiers.Shift | KeyModifiers.Control])
		{
			_RemoveItemsInRect();
		}
		else if (InputManager.I[KeyModifiers.Control])
		{
			_ToggleItemsInRect();
		}
		else if (InputManager.I[KeyModifiers.Shift])
		{
			_AddItemsInRect();
		}
		else
		{
			_SelectItemsInRect();
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		dragging = false;
	}
}
