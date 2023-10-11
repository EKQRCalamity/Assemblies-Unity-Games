using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ContextMenuWindow : UIBehaviour, ILayoutIgnorer, IDeepPointerPropagation, IDeepPointerEventHandler, IEventSystemHandler
{
	private const float OUT_OF_BOUNDS_THRESHOLD = 0.0001f;

	[SerializeField]
	protected Vector2 _rectLerp;

	[SerializeField]
	protected RectTransform _anchorRect;

	[SerializeField]
	protected Vector2 _anchorRectLerp;

	[SerializeField]
	protected bool _flipRectLerpXOnOutOfBounds = true;

	[SerializeField]
	protected bool _flipRectLerpYOnOutOfBounds = true;

	[SerializeField]
	[Range(0f, 100f)]
	protected float _rootMenuPadding = 1f;

	[SerializeField]
	protected UnityEvent _OnClose;

	private bool _isHierarchyDirty;

	private bool _isPositionDirty;

	private Camera _canvasCamera;

	private Canvas _parentCanvas;

	private Vector3 _outOfBoundsOffset;

	private List<ContextMenuActionView> _contextMenuActionViews;

	public RectTransform rectTransform => base.transform as RectTransform;

	public Vector2 rectLerp
	{
		get
		{
			return _rectLerp;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _rectLerp, value))
			{
				_SetPositionDirty();
			}
		}
	}

	public RectTransform anchorRect
	{
		get
		{
			return _anchorRect;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _anchorRect, value))
			{
				_SetPositionDirty();
			}
		}
	}

	public Vector2 anchorRectLerp
	{
		get
		{
			return _anchorRectLerp;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _anchorRectLerp, value) && (bool)anchorRect)
			{
				_SetPositionDirty();
			}
		}
	}

	public bool flipRectLerpXOnOutOfBounds
	{
		get
		{
			return _flipRectLerpXOnOutOfBounds;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _flipRectLerpXOnOutOfBounds, value))
			{
				_SetPositionDirty();
			}
		}
	}

	public bool flipRectLerpYOnOutOfBounds
	{
		get
		{
			return _flipRectLerpYOnOutOfBounds;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _flipRectLerpYOnOutOfBounds, value))
			{
				_SetPositionDirty();
			}
		}
	}

	public float rootMenuPadding
	{
		get
		{
			return _rootMenuPadding;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _rootMenuPadding, value))
			{
				_SetPositionDirty();
			}
		}
	}

	public ContextMenuWindow rootWindow => base.gameObject.GetRootComponent<ContextMenuWindow>();

	public int subMenuDepth => base.gameObject.GetComponentsInParentCount<ContextMenuWindow>() - 1;

	public UnityEvent OnClose => _OnClose ?? (_OnClose = new UnityEvent());

	protected Camera canvasCamera
	{
		get
		{
			if (!_canvasCamera)
			{
				return _canvasCamera = CameraManager.Instance.mainCamera;
			}
			return _canvasCamera;
		}
	}

	public List<ContextMenuActionView> contextMenuActionViews => _contextMenuActionViews ?? (_contextMenuActionViews = new List<ContextMenuActionView>());

	private float _padding
	{
		get
		{
			if (!anchorRect.GetComponentInParent<ContextMenuWindow>())
			{
				return rootMenuPadding * ((_parentCanvas.renderMode != RenderMode.WorldSpace) ? _parentCanvas.transform.lossyScale.Average() : 1f);
			}
			return 0f;
		}
	}

	public bool ignoreLayout => true;

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		_SetHierarchyDirty();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		_SetPositionDirty();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_SetDirty();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_canvasCamera = null;
		_parentCanvas = null;
		foreach (Transform item in base.transform.ChildrenSafe())
		{
			item.gameObject.SetActive(value: false);
		}
		OnClose.Invoke();
	}

	private void LateUpdate()
	{
		if ((bool)anchorRect)
		{
			_UpdateHierarchy();
			_UpdatePosition();
		}
	}

	private void _SetDirty()
	{
		_isPositionDirty = (_isHierarchyDirty = true);
	}

	private void _SetHierarchyDirty()
	{
		_isHierarchyDirty = true;
	}

	private void _SetPositionDirty()
	{
		_isPositionDirty = true;
	}

	private bool _ShouldUpdate(ref bool isDirty)
	{
		if (!isDirty)
		{
			return false;
		}
		isDirty = false;
		if ((bool)this)
		{
			return base.isActiveAndEnabled;
		}
		return false;
	}

	private void _UpdateHierarchy()
	{
		if (_ShouldUpdate(ref _isHierarchyDirty))
		{
			_parentCanvas = rectTransform.GetCanvas();
			if ((bool)_parentCanvas)
			{
				_canvasCamera = _parentCanvas.worldCamera;
				ContextMenuWindow componentInParent = anchorRect.GetComponentInParent<ContextMenuWindow>();
				base.transform.SetParent(componentInParent ? componentInParent.transform : _parentCanvas.transform, worldPositionStays: false);
				base.transform.SetAsLastSibling();
				_isHierarchyDirty = false;
				_UpdatePosition();
			}
		}
	}

	private void _UpdatePosition()
	{
		if (_ShouldUpdate(ref _isPositionDirty))
		{
			base.transform.rotation = anchorRect.rotation;
			_UpdatePositionInner(Vector3.zero);
			if (_FixRectLerpForOutOfBounds())
			{
				_UpdatePositionInner(_outOfBoundsOffset);
			}
		}
	}

	private void _UpdatePositionInner(Vector3 offset)
	{
		base.transform.position += anchorRect.GetWorldRect3D().Lerp(anchorRectLerp) - rectTransform.GetWorldRect3D().Pad(Vector2.one * _padding).Lerp(rectLerp) + offset;
	}

	private bool _FixRectLerpForOutOfBounds()
	{
		_outOfBoundsOffset = rectTransform.GetOutOfCameraBoundsWorldOffsetCorrection(canvasCamera);
		if (_outOfBoundsOffset.sqrMagnitude < 0.0001f)
		{
			return false;
		}
		float num = Vector3.Dot(_outOfBoundsOffset, base.transform.right);
		bool flag = flipRectLerpXOnOutOfBounds && Mathf.Abs(num) >= 0.0001f && Math.Sign(num) == Math.Sign(rectLerp.x - 0.5f);
		float num2 = Vector3.Dot(_outOfBoundsOffset, base.transform.up);
		bool flag2 = flipRectLerpYOnOutOfBounds && Mathf.Abs(num2) >= 0.0001f && Math.Sign(num2) == Math.Sign(rectLerp.y - 0.5f);
		_rectLerp = rectLerp.OneMinus(flag, flag2);
		_anchorRectLerp = anchorRectLerp.OneMinus(flag, flipRectLerpXOnOutOfBounds && flag2);
		if (flag)
		{
			_outOfBoundsOffset -= Vector3.Project(_outOfBoundsOffset, base.transform.right);
		}
		if (flag2)
		{
			_outOfBoundsOffset -= Vector3.Project(_outOfBoundsOffset, base.transform.up);
		}
		return true;
	}

	public ContextMenuWindow SetData(Vector2 rectLerp, RectTransform anchorRect, Vector2 anchorRectLerp, bool flipRectLerpXOnOutOfBounds, bool flipRectLerpYOnOutOfBounds)
	{
		this.rectLerp = rectLerp;
		this.anchorRect = anchorRect;
		this.anchorRectLerp = anchorRectLerp;
		this.flipRectLerpXOnOutOfBounds = flipRectLerpXOnOutOfBounds;
		this.flipRectLerpYOnOutOfBounds = flipRectLerpYOnOutOfBounds;
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		LateUpdate();
		GetComponentsInChildren(contextMenuActionViews);
		return this;
	}

	public bool ShouldDeepPointerContinuePropagation(PointerEventData eventData)
	{
		return false;
	}
}
