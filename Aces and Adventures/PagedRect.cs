using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PagedRect : MonoBehaviour, IScrollHandler, IEventSystemHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
	public RectTransform content;

	public RectTransform scrollDividerContainer;

	public RectTransform scrollKnob;

	public GameObject scrollDividerBlueprint;

	[Range(1f, 100f)]
	[SerializeField]
	protected int _pageSize = 10;

	public bool destroyOldPageItems;

	private int _page = 1;

	private readonly List<Func<Transform, GameObject>> _items = new List<Func<Transform, GameObject>>();

	private bool _pageDirty;

	private bool _pagesDirty;

	private Vector3 _dragOffset;

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
				_OnPageSizeChange();
			}
		}
	}

	public int page
	{
		get
		{
			return _page;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _page, Mathf.Clamp(value, 1, maxPage)))
			{
				_OnPageChange();
			}
		}
	}

	public int maxPage => _items.GetMaxPageNumber(_pageSize);

	public int count => _items.Count;

	private void _OnPageSizeChange()
	{
		_pageDirty = (_pagesDirty = true);
	}

	private void _OnPageChange()
	{
		_pageDirty = true;
	}

	private void _UpdatesPages()
	{
		scrollDividerContainer.SetChildrenActive(active: false);
		if ((bool)scrollDividerBlueprint)
		{
			for (int i = 1; i <= maxPage; i++)
			{
				Pools.Unpool(scrollDividerBlueprint, scrollDividerContainer);
			}
		}
		_pagesDirty = false;
	}

	private void _UpdatePage()
	{
		if (destroyOldPageItems)
		{
			content.gameObject.DestroyChildren();
		}
		else
		{
			content.gameObject.SetActiveChildren(active: false);
		}
		foreach (Func<Transform, GameObject> pagedResult in _items.GetPagedResults(page, pageSize))
		{
			pagedResult(content);
		}
		scrollKnob.SetWorldCornersPreserveScale(new Rect3D(scrollDividerContainer).GetRelativeRect3D(new Vector2(0.5f, 1f - ((float)page - 0.5f) / (float)maxPage), new Vector2(1f, 1f / (float)maxPage)));
		_pageDirty = false;
	}

	private void LateUpdate()
	{
		if (_pagesDirty)
		{
			_UpdatesPages();
		}
		if (_pageDirty)
		{
			_UpdatePage();
		}
	}

	public void ClearItems()
	{
		int num = maxPage;
		_items.Clear();
		_pageDirty = true;
		if (maxPage != num)
		{
			_pagesDirty = true;
		}
	}

	public void SetItems(IEnumerable<Func<Transform, GameObject>> itemGenerators)
	{
		int num = maxPage;
		_items.ClearAndCopyFrom(itemGenerators);
		_pageDirty = true;
		if (maxPage != num)
		{
			_pagesDirty = true;
		}
	}

	public void AddItem(Func<Transform, GameObject> generator)
	{
		int num = maxPage;
		_items.Add(generator);
		_pageDirty = true;
		if (maxPage != num)
		{
			_pagesDirty = true;
		}
	}

	public int GetPageOfItem(int itemIndex)
	{
		return itemIndex / pageSize + 1;
	}

	public void OnScroll(PointerEventData eventData)
	{
		page += ((eventData.scrollDelta.y > 0f) ? (-1) : ((eventData.scrollDelta.y < 0f) ? 1 : 0));
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_dragOffset = (content.IsMouseInRect() ? (scrollKnob.GetWorldRect3D().center - scrollDividerContainer.GetPlane(PlaneAxes.XY).ClosestPointOnPlane(eventData.pressEventCamera.ScreenPointToRay(Input.mousePosition))) : Vector3.zero);
	}

	public void OnDrag(PointerEventData eventData)
	{
		page = 1 + (int)((1f - new Rect3D(scrollDividerContainer).GetLerpAmount(scrollDividerContainer.GetPlane(PlaneAxes.XY).ClosestPointOnPlane(eventData.pressEventCamera.ScreenPointToRay(Input.mousePosition)) + _dragOffset).y) * (float)maxPage);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		_dragOffset = Vector3.zero;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (scrollDividerContainer.IsMouseInRect())
		{
			OnDrag(eventData);
		}
	}
}
