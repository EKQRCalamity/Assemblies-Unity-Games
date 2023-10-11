using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class FitIntoRect : UIBehaviour, ILayoutSelfController, ILayoutController
{
	[SerializeField]
	private RectTransform _fitInto;

	[SerializeField]
	private Vector2 _center = new Vector2(0.5f, 0.5f);

	[SerializeField]
	private Vector2 _size = Vector2.one;

	[SerializeField]
	private float _depthOffset;

	private RectTransform _rt;

	private DrivenRectTransformTracker _tracker;

	private Vector3 _lastPosition;

	private Vector3 _actualCenter;

	private Vector2 _actualSize;

	public RectTransform fitInto
	{
		get
		{
			return _fitInto;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _fitInto, value))
			{
				_SetDirty();
			}
		}
	}

	public Vector2 center
	{
		get
		{
			return _center;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _center, value))
			{
				_SetDirty();
			}
		}
	}

	public Vector2 size
	{
		get
		{
			return _size;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _size, value))
			{
				_SetDirty();
			}
		}
	}

	public float depthOffset
	{
		get
		{
			return _depthOffset;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _depthOffset, value))
			{
				_SetDirty();
			}
		}
	}

	private void _SetDirty()
	{
		if (base.isActiveAndEnabled)
		{
			LayoutRebuilder.MarkLayoutForRebuild(_rt);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_rt = base.transform as RectTransform;
	}

	protected override void OnDisable()
	{
		_tracker.Clear();
		base.OnDisable();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		_SetDirty();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_SetDirty();
	}

	private void LateUpdate()
	{
		if (_rt.position != _lastPosition)
		{
			this.ForceLayoutUpdateControl();
			_lastPosition = _rt.position;
		}
	}

	public void SetLayoutHorizontal()
	{
		if (!(_fitInto == null))
		{
			_tracker.Clear();
			_tracker.Add(this, _rt, DrivenTransformProperties.All);
			Rect rect = fitInto.rect;
			_actualCenter = fitInto.localToWorldMatrix.MultiplyPoint(rect.min.Lerp(rect.max, _center).Unproject(AxisType.Z)) - fitInto.transform.forward * depthOffset;
			Vector3 v = fitInto.localToWorldMatrix.MultiplyVector(rect.size.Unproject(AxisType.Z));
			Vector2 max = v.Project(AxisType.Z).Multiply(_size);
			_actualSize = _rt.localToWorldMatrix.MultiplyVector(_rt.rect.size.Unproject(AxisType.Z)).Project(AxisType.Z);
			_actualSize = _actualSize.Clamp(Vector2.zero, max);
			Vector4 vector = fitInto.transform.worldToLocalMatrix * _actualCenter;
			Bounds bounds = new Bounds(vector, _actualSize);
			Bounds bounds2 = new Bounds(fitInto.position, v);
			vector.x -= Math.Max(0f, bounds.max.x - bounds2.max.x) - Math.Max(0f, bounds2.min.x - bounds.min.x);
			vector.y -= Math.Max(0f, bounds.max.y - bounds2.max.y) - Math.Max(0f, bounds2.min.y - bounds.min.y);
			_actualCenter = fitInto.transform.localToWorldMatrix * vector;
			_rt.localScale = Vector3.one;
			_rt.rotation = fitInto.rotation;
			_rt.anchorMin = new Vector2(0.5f, 0.5f);
			_rt.anchorMax = _rt.anchorMin;
			_rt.pivot = _rt.anchorMin;
			_rt.position = _actualCenter;
			_rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _actualSize.x);
		}
	}

	public void SetLayoutVertical()
	{
		if (!(_fitInto == null))
		{
			_rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _actualSize.y);
		}
	}
}
