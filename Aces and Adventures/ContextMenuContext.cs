using System;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenuContext : IEquatable<ContextMenuContext>
{
	private List<Component> _selection;

	private Vector3 _mouseWorldPosition;

	private Dictionary<Type, object> _selectionByType;

	public List<Component> selection => _selection ?? (_selection = new List<Component>());

	public Vector3 mouseWorldPosition => _mouseWorldPosition;

	static ContextMenuContext()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new ContextMenuContext(), delegate(ContextMenuContext c)
		{
			c._Clear();
		}, delegate(ContextMenuContext c)
		{
			c._OnUnpool();
		});
	}

	public static ContextMenuContext Create(List<Component> selection = null, Vector3? mouseWorldPosition = null)
	{
		return Pools.Unpool<ContextMenuContext>().SetData(selection, mouseWorldPosition ?? Vector3.zero);
	}

	private ContextMenuContext()
	{
	}

	private void _Clear()
	{
		Pools.Repool(ref _selection);
		foreach (object value in _selectionByType.Values)
		{
			Pools.RepoolObject(value);
		}
		Pools.Repool(ref _selectionByType);
	}

	private void _OnUnpool()
	{
		Pools.TryUnpool(ref _selection);
		Pools.TryUnpool(ref _selectionByType);
	}

	public ContextMenuContext SetData(List<Component> selectionData, Vector3 mouseWorldPosition)
	{
		if (selectionData != null)
		{
			selection.CopyFrom(selectionData);
		}
		_mouseWorldPosition = mouseWorldPosition;
		return this;
	}

	public List<T> GetSelection<T>() where T : Component
	{
		Type typeFromHandle = typeof(T);
		if (!_selectionByType.ContainsKey(typeFromHandle))
		{
			List<T> list = Pools.TryUnpoolList<T>();
			foreach (Component item in selection)
			{
				if (item is T)
				{
					list.Add((T)item);
				}
			}
			_selectionByType[typeFromHandle] = list;
		}
		return _selectionByType[typeFromHandle] as List<T>;
	}

	public ContextMenuContext SetMousePosition(Vector3? mouseWorldPosition)
	{
		_mouseWorldPosition = mouseWorldPosition ?? _mouseWorldPosition;
		return this;
	}

	public bool IsMousePositionInRect(RectTransform rectTransform, Camera eventCamera)
	{
		if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, Input.mousePosition, eventCamera, out var worldPoint))
		{
			return false;
		}
		_mouseWorldPosition = worldPoint;
		return rectTransform.GetWorldRect3D().ContainsProjection(_mouseWorldPosition);
	}

	public bool Equals(ContextMenuContext other)
	{
		if (other != null)
		{
			return selection.SequenceEqual(other.selection, ReferenceEqualityComparer<Component>.Default);
		}
		return false;
	}
}
