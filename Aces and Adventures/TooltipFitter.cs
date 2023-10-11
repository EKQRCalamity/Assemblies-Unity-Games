using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
[ScriptOrder(3)]
public class TooltipFitter : MonoBehaviour
{
	[SerializeField]
	protected TooltipDirection _direction;

	[SerializeField]
	protected TooltipOrthogonalDirection _orthogonalDirection = TooltipOrthogonalDirection.Center;

	[SerializeField]
	[HideInInspector]
	protected RectTransform _toolTipCreator;

	[SerializeField]
	protected UnityEvent _OnHide;

	protected RectTransform _rect;

	private float? _mainAxisPivot;

	protected float? _mainAxisPivotTarget;

	protected bool _dirty;

	private bool _hasBeenSetAsLastSiblingOfCanvas;

	private Vector3? _defaultContentScale;

	private sbyte _forcedMainAxisPivot = -1;

	public TooltipDirection direction
	{
		get
		{
			return _direction;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _direction, value))
			{
				_SetDirty();
			}
		}
	}

	public TooltipOrthogonalDirection orthogonalDirection
	{
		get
		{
			return _orthogonalDirection;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _orthogonalDirection, value))
			{
				_SetDirty();
			}
		}
	}

	public RectTransform tooltipCreator
	{
		get
		{
			return _toolTipCreator;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _toolTipCreator, value))
			{
				_SetDirty();
			}
		}
	}

	public float padding { get; set; }

	public float canvasEdgePadding { get; set; }

	public bool deactivateContentOnDisable { get; set; }

	public bool recurseTooltipCreatorRect { get; set; }

	public bool matchContentScaleWithCreator { get; set; }

	public float contentScale { get; set; }

	public bool trackCreator { get; set; }

	public bool useOppositeDirection { get; set; }

	protected int mainAxis => (int)direction;

	protected int otherAxis => 1 - mainAxis;

	public int forcedMainAxisPivot
	{
		get
		{
			return _forcedMainAxisPivot;
		}
		set
		{
			_forcedMainAxisPivot = (sbyte)Math.Sign(value);
		}
	}

	private void _SetDirty()
	{
		_dirty = true;
	}

	private IEnumerator _SetDirtyAtEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		if ((bool)this)
		{
			_SetDirty();
		}
	}

	private void _SetAsLastSiblingOfCanvas(RectTransform canvasRect)
	{
		if (!_hasBeenSetAsLastSiblingOfCanvas)
		{
			_hasBeenSetAsLastSiblingOfCanvas = true;
			_rect.SetParent(canvasRect, worldPositionStays: false);
		}
	}

	protected void _UpdatePivot(int suggestedMainAxisPivot)
	{
		_mainAxisPivotTarget = _mainAxisPivotTarget ?? ((float)suggestedMainAxisPivot);
		_mainAxisPivot = _mainAxisPivot ?? ((float)suggestedMainAxisPivot);
		if (trackCreator)
		{
			_mainAxisPivot = MathUtil.Ease(_mainAxisPivot.Value, _mainAxisPivotTarget.Value, 10f, Time.unscaledDeltaTime);
		}
	}

	protected Vector2 _SetAnchorsAndPivot()
	{
		Vector2 vector = default(Vector2);
		vector[mainAxis] = _mainAxisPivot.Value;
		vector[otherAxis] = (float)(int)orthogonalDirection * 0.5f;
		_rect.SetAnchors(vector);
		Vector2 pivot = vector;
		pivot[mainAxis] = 1f - vector[mainAxis];
		_rect.pivot = pivot;
		foreach (Transform item in _rect.Children())
		{
			(item as RectTransform).pivot = pivot;
		}
		return vector;
	}

	protected virtual void _UpdatePosition()
	{
		Canvas componentInParent = GetComponentInParent<Canvas>();
		RectTransform rectTransform = componentInParent.transform as RectTransform;
		_SetAsLastSiblingOfCanvas(rectTransform);
		Vector3 localScale = _rect.localScale;
		_rect.localScale = Vector3.one;
		Rect worldRect = rectTransform.GetWorldRect();
		Vector2 vector = worldRect.size.Multiply(rectTransform.rect.size.Inverse());
		Vector2 center = worldRect.center;
		Rect r = ((tooltipCreator != null) ? tooltipCreator.GetWorldRect(recurseTooltipCreatorRect) : worldRect.ScaleFromCenter(0f));
		r = r.Pad(padding * vector.Multiply(Vector2.one.SetAxis(otherAxis, 0f)));
		Vector2 min = r.min;
		Vector2 max = r.max;
		float num = Mathf.Abs(min[mainAxis] - center[mainAxis]);
		float num2 = Mathf.Abs(max[mainAxis] - center[mainAxis]);
		int num3 = ((forcedMainAxisPivot >= 0) ? forcedMainAxisPivot : ((!(num < num2)) ? 1 : 0).OneMinusIf(useOppositeDirection));
		_UpdatePivot(num3);
		_rect.localPosition = rectTransform.worldToLocalMatrix.MultiplyPoint(r.Lerp(_SetAnchorsAndPivot())).Project(AxisType.Z).Unproject(AxisType.Z);
		RectTransform rectTransform2 = _rect.GetChild(0) as RectTransform;
		if (matchContentScaleWithCreator && tooltipCreator.gameObject.activeInHierarchy)
		{
			rectTransform2.SetWorldScale(tooltipCreator.GetWorldScale());
		}
		else
		{
			_defaultContentScale = _defaultContentScale ?? rectTransform2.localScale;
		}
		rectTransform2.localScale = (_defaultContentScale ?? rectTransform2.localScale) * contentScale;
		if (trackCreator)
		{
			_rect.position = new Vector3(_rect.position.x, _rect.position.y, tooltipCreator.position.z);
			_rect.rotation = tooltipCreator.rotation;
		}
		Vector2 vector2 = rectTransform2.GetWorldRect().Pad(canvasEdgePadding * vector).MaximalOverlapOffset((componentInParent.renderMode != 0) ? worldRect.ScaleFromCenter(Mathf.Abs(componentInParent.worldCamera.worldToCameraMatrix.MultiplyPoint(rectTransform2.position).z / componentInParent.worldCamera.worldToCameraMatrix.MultiplyPoint(componentInParent.transform.position).z)) : worldRect);
		Vector3 vector3 = rectTransform.worldToLocalMatrix.MultiplyVector(vector2).Project(AxisType.Z).Unproject(AxisType.Z);
		_rect.localPosition += vector3;
		if (trackCreator && Mathf.Abs(vector3[mainAxis]) > MathUtil.Epsilon)
		{
			_mainAxisPivotTarget = num3;
		}
		_rect.localScale = localScale;
		_dirty = false;
	}

	public virtual void ConnectToWorldLinker()
	{
	}

	private void Awake()
	{
		_rect = base.transform as RectTransform;
	}

	private void OnEnable()
	{
		_SetDirty();
		StartCoroutine(_SetDirtyAtEndOfFrame());
	}

	private void OnDisable()
	{
		_mainAxisPivot = null;
		_mainAxisPivotTarget = null;
		_hasBeenSetAsLastSiblingOfCanvas = false;
		_defaultContentScale = null;
		if (!deactivateContentOnDisable)
		{
			return;
		}
		foreach (Transform item in base.transform.ChildrenSafe())
		{
			item.gameObject.SetActive(value: false);
		}
	}

	protected void OnTransformChildrenChanged()
	{
		_SetDirty();
	}

	private void Update()
	{
		if ((object)tooltipCreator != null && (!tooltipCreator || !tooltipCreator.gameObject.activeInHierarchy))
		{
			Hide();
			tooltipCreator = null;
		}
		else if (trackCreator && (bool)tooltipCreator && tooltipCreator.gameObject.activeInHierarchy)
		{
			_SetDirty();
		}
	}

	private void LateUpdate()
	{
		if (_dirty && _rect.childCount != 0 && (bool)tooltipCreator)
		{
			_UpdatePosition();
		}
	}

	public void Hide()
	{
		_OnHide.Invoke();
		TooltipCreator[] componentsInChildren = GetComponentsInChildren<TooltipCreator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Hide();
		}
	}
}
