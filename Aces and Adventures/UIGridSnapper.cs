using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIGridSnapper : MonoBehaviour
{
	[SerializeField]
	[Range(1f, 1000f)]
	protected float _gridSizeX = 10f;

	[SerializeField]
	[Range(1f, 1000f)]
	protected float _gridSizeY = 10f;

	[SerializeField]
	[Range(0f, 1000f)]
	protected float _paddingX;

	[SerializeField]
	[Range(0f, 1000f)]
	protected float _paddingY;

	[SerializeField]
	protected TransformEvent _onGridItemPositionUpdate;

	private HashSet<RectTransform> _gridItems;

	private bool _isDirty;

	public float gridSizeX
	{
		get
		{
			return _gridSizeX;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _gridSizeX, value))
			{
				SetDirty();
			}
		}
	}

	public float gridSizeY
	{
		get
		{
			return _gridSizeY;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _gridSizeY, value))
			{
				SetDirty();
			}
		}
	}

	public Vector2 gridSize
	{
		get
		{
			return new Vector2(gridSizeX, gridSizeY);
		}
		set
		{
			gridSizeX = value.x;
			gridSizeY = value.y;
		}
	}

	public float paddingX
	{
		get
		{
			return _paddingX;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _paddingX, value))
			{
				SetDirty();
			}
		}
	}

	public float paddingY
	{
		get
		{
			return _paddingY;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _paddingY, value))
			{
				SetDirty();
			}
		}
	}

	public Vector2 padding
	{
		get
		{
			return new Vector2(paddingX, paddingY);
		}
		set
		{
			paddingX = value.x;
			paddingY = value.y;
		}
	}

	public TransformEvent onGridItemPositionUpdate => _onGridItemPositionUpdate ?? (_onGridItemPositionUpdate = new TransformEvent());

	protected RectTransform rectTransform => base.transform as RectTransform;

	protected HashSet<RectTransform> gridItems => _gridItems ?? (_gridItems = new HashSet<RectTransform>());

	private void _UpdateGridElements()
	{
		_isDirty = false;
		using PoolKeepItemDictionaryHandle<RectTransform, Rect3D> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<RectTransform, Rect3D>();
		Rect3D? rect3D = null;
		foreach (RectTransform gridItem in gridItems)
		{
			Rect3D rect3D2 = (poolKeepItemDictionaryHandle[gridItem] = gridItem.GetWorldRect3D());
			Rect3D rect3D3 = rect3D2;
			rect3D = (rect3D.HasValue ? rect3D.Value.Encapsulate(rect3D3) : rect3D3);
		}
		rect3D = rect3D ?? new Rect3D(base.transform.position, -base.transform.forward, base.transform.up, gridSize);
		rect3D = rect3D.Value.Pad(Vector2.Max(padding * base.transform.lossyScale.AbsMax() * 2f, gridSize).RoundToNearestMultipleOf(gridSize));
		rectTransform.SetWorldCornersPreserveScale(rect3D.Value);
		rectTransform.offsetMin = rectTransform.offsetMin.RoundToNearestMultipleOf(gridSize);
		rectTransform.offsetMax = rectTransform.offsetMax.RoundToNearestMultipleOf(gridSize);
		rectTransform.localPosition = rectTransform.localPosition.Project(AxisType.Z).RoundToNearestMultipleOf(gridSize).Unproject(AxisType.Z, rectTransform.localPosition.z);
		foreach (KeyValuePair<RectTransform, Rect3D> item in poolKeepItemDictionaryHandle.value)
		{
			item.Key.SetWorldCornersPreserveScale(item.Value);
			item.Key.localPosition = item.Key.localPosition.Project(AxisType.Z).RoundToNearestMultipleOf(gridSize).Unproject(AxisType.Z);
			onGridItemPositionUpdate.Invoke(item.Key);
		}
	}

	private void OnEnable()
	{
		SetDirty();
	}

	private void LateUpdate()
	{
		if (_isDirty)
		{
			_UpdateGridElements();
		}
	}

	public bool SetDirty()
	{
		return _isDirty = true;
	}

	public Vector2 WorldToGridPosition(Vector3 worldPosition)
	{
		return rectTransform.worldToLocalMatrix.MultiplyPoint(worldPosition).Project(AxisType.Z).RoundToNearestMultipleOf(gridSize);
	}

	public void ForceUpdate()
	{
		_UpdateGridElements();
	}

	public bool AddGridItem(RectTransform itemRectTransform)
	{
		if (gridItems.Add(itemRectTransform))
		{
			return SetDirty();
		}
		return false;
	}

	public T AddGridItem<T>(T item) where T : Component
	{
		AddGridItem(item.transform as RectTransform);
		return item;
	}

	public bool RemoveGridItem(RectTransform itemRectTransform)
	{
		if (gridItems.Remove(itemRectTransform))
		{
			return SetDirty();
		}
		return false;
	}

	public bool RemoveGridItem<T>(T item) where T : Component
	{
		return RemoveGridItem(item.transform as RectTransform);
	}
}
