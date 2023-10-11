using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIPopupFitter : UIBehaviour, ILayoutSelfController, ILayoutController, ILayoutElement
{
	public bool skipParentingLogic;

	[SerializeField]
	protected float _offsetFromCanvas = 1f;

	[SerializeField]
	protected RectTransform _sizeReference;

	[SerializeField]
	protected Vector2 _size = new Vector2(0.75f, 0.75f);

	[SerializeField]
	protected RectTransform _centerReference;

	[SerializeField]
	protected Vector2 _center = new Vector2(0.5f, 0.5f);

	protected Canvas _parentCanvas;

	protected RectTransform _rt;

	protected DrivenRectTransformTracker _tracker;

	protected LayoutData _width;

	protected LayoutData _height;

	protected float _additionalOffsetFromCanvas;

	protected Vector3 _actualCenter;

	protected Vector2 _actualSize;

	private Vector3 _lastPosition;

	private float _minWorldScale;

	protected Space _space
	{
		get
		{
			if (parentCanvas.renderMode != RenderMode.WorldSpace)
			{
				return Space.Self;
			}
			return Space.World;
		}
	}

	protected Camera _camera
	{
		get
		{
			if (!parentCanvas.worldCamera)
			{
				return CameraManager.Instance.mainCamera;
			}
			return parentCanvas.worldCamera;
		}
	}

	protected float _totalOffset => (_offsetFromCanvas + _additionalOffsetFromCanvas) * _minWorldScale;

	public RectTransform rectTransform => _rt ?? (_rt = GetComponent<RectTransform>());

	public Canvas parentCanvas
	{
		get
		{
			if (_parentCanvas == null)
			{
				_parentCanvas = base.gameObject.Parents().SelectValid((GameObject go) => go.GetComponent<Canvas>()).FirstOrDefault();
				if (_parentCanvas == null)
				{
					_parentCanvas = UnityEngine.Object.FindObjectOfType<Canvas>();
					if (_parentCanvas == null)
					{
						GameObject gameObject = new GameObject("Popup Canvas");
						_parentCanvas = gameObject.AddComponent<Canvas>();
					}
				}
				_SetDirty();
			}
			return _parentCanvas;
		}
	}

	public RectTransform centerReference
	{
		get
		{
			return _centerReference ?? (_centerReference = parentCanvas.transform as RectTransform);
		}
		set
		{
			if (value != _centerReference)
			{
				_centerReference = value;
				_SetDirty();
			}
		}
	}

	public RectTransform sizeReference
	{
		get
		{
			return _sizeReference;
		}
		set
		{
			if (value != _sizeReference)
			{
				_sizeReference = value;
				_SetDirty();
			}
		}
	}

	public float offsetFromCanvas
	{
		get
		{
			return _offsetFromCanvas;
		}
		set
		{
			if (value != _offsetFromCanvas)
			{
				_offsetFromCanvas = value;
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
			if (value != _size)
			{
				_size = value;
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
			if (value != _center)
			{
				_center = value;
				_SetDirty();
			}
		}
	}

	public float flexibleHeight => _height.flexible;

	public float flexibleWidth => _width.flexible;

	public int layoutPriority => 1;

	public float minHeight => _height.min;

	public float minWidth => _width.min;

	public float preferredHeight => _height.preferred;

	public float preferredWidth => _width.preferred;

	protected void _SetDirty()
	{
		if (base.isActiveAndEnabled)
		{
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
	}

	public void PopToTop()
	{
		rectTransform.SetAsLastSibling();
	}

	protected override void OnTransformParentChanged()
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		base.OnTransformParentChanged();
		_parentCanvas = null;
		if (!skipParentingLogic && rectTransform.parent != parentCanvas.transform)
		{
			rectTransform.SetParent(parentCanvas.transform, worldPositionStays: false);
			return;
		}
		_additionalOffsetFromCanvas = 0f;
		if (!skipParentingLogic && rectTransform.GetSiblingIndex() != rectTransform.parent.childCount - 1)
		{
			rectTransform.SetAsLastSibling();
		}
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

	protected override void Awake()
	{
		base.Awake();
		OnTransformParentChanged();
	}

	protected override void OnDisable()
	{
		_tracker.Clear();
		base.OnDisable();
	}

	private void LateUpdate()
	{
		if (_rt.position != _lastPosition)
		{
			this.ForceLayoutUpdate();
			_lastPosition = _rt.position;
		}
	}

	public void CalculateLayoutInputHorizontal()
	{
		if ((bool)centerReference && parentCanvas.renderMode != 0)
		{
			_minWorldScale = base.transform.GetWorldScale().AbsMin();
			Rect rect = centerReference.rect;
			_actualCenter = centerReference.localToWorldMatrix.MultiplyPoint(rect.min.Lerp(rect.max, _center).Unproject(AxisType.Z)) - parentCanvas.transform.forward * _totalOffset;
			Camera camera = _camera;
			float magnitude = Vector3.Project(_actualCenter - camera.transform.position, camera.transform.forward).magnitude;
			float num = Mathf.Tan(camera.fieldOfView * 0.5f * (MathF.PI / 180f));
			float num2 = magnitude * num;
			float num3 = num2 + num2;
			Bounds bounds = new Bounds(camera.transform.position + camera.transform.forward * magnitude, new Vector3(num3 * camera.aspect, num3, 0f));
			Vector3 vector = ((sizeReference != null) ? sizeReference.localToWorldMatrix.MultiplyVector(sizeReference.rect.size.Unproject(AxisType.Z)) : bounds.size);
			if (sizeReference != null)
			{
				vector *= magnitude / Vector3.Project(sizeReference.localToWorldMatrix.MultiplyPoint(sizeReference.rect.center) - camera.transform.position, camera.transform.forward).magnitude;
			}
			_actualSize = new Vector2(vector.x * _size.x, vector.y * _size.y);
			_actualSize = _actualSize.Clamp(Vector2.zero, bounds.size.Project(AxisType.Z));
			_actualSize /= Math.Max(MathUtil.BigEpsilon, _minWorldScale);
		}
	}

	public void CalculateLayoutInputVertical()
	{
	}

	public void SetLayoutHorizontal()
	{
		_tracker.Clear();
		_tracker.Add(this, rectTransform, DrivenTransformProperties.AnchoredPosition3D);
		_tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors);
		_tracker.Add(this, rectTransform, DrivenTransformProperties.Pivot);
		_tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDelta);
		if (parentCanvas.renderMode != 0)
		{
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = rectTransform.anchorMin;
			rectTransform.pivot = rectTransform.anchorMin;
			rectTransform.position = _actualCenter;
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _actualSize.x);
		}
		else
		{
			rectTransform.SetWorldCornersPreserveScale((parentCanvas.transform as RectTransform).GetWorldRect3D().GetRelativeRect3D(_center, _size));
		}
	}

	public void SetLayoutVertical()
	{
		if (parentCanvas.renderMode != 0)
		{
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _actualSize.y);
		}
	}
}
