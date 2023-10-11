using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Layout/Collapse Fitter", 142)]
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
[ScriptOrder(1)]
public class CollapseFitter : ContentSizeFitter, ILayoutElement, IUIContentContainer
{
	public RectTransform.Edge expandFrom = RectTransform.Edge.Bottom;

	[Header("Padding")]
	public float horizontalPadding;

	public float verticalPadding;

	private float _expandRatio;

	private RectTransform _rect;

	private DrivenRectTransformTracker tracker;

	private LayoutData defaultWidth;

	private LayoutData defaultHeight;

	private LayoutData width;

	private LayoutData height;

	private ToggleFloatData _toggleFloat = new ToggleFloatData(0.15f);

	private CanvasGroup _canvasGroup;

	private string _path;

	private string _label;

	public FloatEvent OnExpandChange;

	public RectTransform rect => this.CacheComponent(ref _rect);

	public CanvasGroup canvasGroup => this.CacheComponent(ref _canvasGroup);

	public float expandRatio
	{
		get
		{
			return _expandRatio;
		}
		set
		{
			if (value != _expandRatio)
			{
				_expandRatio = value;
				canvasGroup.alpha = value;
				canvasGroup.blocksRaycasts = value == 1f;
				OnExpandChange.Invoke(_expandRatio);
				SetDirty();
			}
		}
	}

	public ToggleFloatData toggleFloat
	{
		get
		{
			return _toggleFloat;
		}
		set
		{
			_toggleFloat = value;
			expandRatio = _toggleFloat.value;
		}
	}

	public bool isOpen
	{
		get
		{
			return _toggleFloat.isOn;
		}
		set
		{
			_toggleFloat.isOn = value;
		}
	}

	public float flexibleHeight => height.flexible;

	public float flexibleWidth => width.flexible;

	public int layoutPriority => 1;

	public float minHeight => height.min;

	public float minWidth => width.min;

	public float preferredHeight => height.preferred;

	public float preferredWidth => width.preferred;

	public Transform uiContentParent => base.transform;

	protected override void Awake()
	{
		base.Awake();
		canvasGroup.blocksRaycasts = false;
		canvasGroup.alpha = 0f;
		HorizontalOrVerticalLayoutGroup component = GetComponent<HorizontalOrVerticalLayoutGroup>();
		if (component != null)
		{
			horizontalPadding = component.padding.left + component.padding.right;
			verticalPadding = component.padding.bottom + component.padding.top;
		}
	}

	protected override void OnDisable()
	{
		tracker.Clear();
		LayoutRebuilder.MarkLayoutForRebuild(rect);
		base.OnDisable();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		SetDirty();
	}

	protected virtual void OnTransformChildrenChanged()
	{
		SetDirty();
	}

	private void Update()
	{
		if (_toggleFloat.Update())
		{
			expandRatio = _toggleFloat.value;
		}
	}

	public void Toggle()
	{
		_SetToggleState(!_toggleFloat.isOn);
	}

	public void Open()
	{
		_SetToggleState(isOn: true);
	}

	public void ForceOpen()
	{
		toggleFloat = ToggleFloatData.Open;
	}

	public void Close()
	{
		_SetToggleState(isOn: false);
	}

	public void ForceClose()
	{
		toggleFloat = ToggleFloatData.Close;
	}

	public void ClearPath()
	{
		_label = null;
		_path = null;
	}

	public string GetPath()
	{
		if (_path == null)
		{
			_path = GetLabel();
			CollapseFitter collapseFitter = (base.transform.parent ? base.transform.parent.GetComponentInParent<CollapseFitter>(includeInactive: true) : null);
			if ((bool)collapseFitter)
			{
				_path += collapseFitter.GetPath();
			}
		}
		return _path;
	}

	public string GetLabel()
	{
		return _label ?? (_label = base.transform.parent.gameObject.GetUILabel().GetPersistedCollapseString());
	}

	protected new void SetDirty()
	{
		if (IsActive())
		{
			LayoutRebuilder.MarkLayoutForRebuild(rect);
		}
	}

	protected void _SetToggleState(bool isOn)
	{
		if (_toggleFloat.isOn != isOn)
		{
			_toggleFloat.isOn = isOn;
		}
	}

	public override void SetLayoutHorizontal()
	{
		base.SetLayoutHorizontal();
		if (expandFrom == RectTransform.Edge.Left || expandFrom == RectTransform.Edge.Right)
		{
			tracker.Clear();
			tracker.Add(this, rect, DrivenTransformProperties.SizeDeltaX);
			tracker.Add(this, rect, DrivenTransformProperties.PivotX);
			rect.pivot = new Vector2(expandFrom.Pivot(), rect.pivot.y);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(defaultWidth.min, defaultWidth.GetSize(rect, m_HorizontalFit, 0) + horizontalPadding, _expandRatio));
		}
	}

	public override void SetLayoutVertical()
	{
		base.SetLayoutVertical();
		if (expandFrom == RectTransform.Edge.Bottom || expandFrom == RectTransform.Edge.Top)
		{
			tracker.Clear();
			tracker.Add(this, rect, DrivenTransformProperties.SizeDeltaY);
			tracker.Add(this, rect, DrivenTransformProperties.PivotY);
			rect.pivot = new Vector2(rect.pivot.x, expandFrom.Pivot());
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(defaultHeight.min, defaultHeight.GetSize(rect, m_VerticalFit, 1) + verticalPadding, _expandRatio));
		}
	}

	public void CalculateLayoutInputHorizontal()
	{
		if (expandFrom.Axis() == 0)
		{
			width = rect.GetTotalLayoutDataOfChildrenSpecial(0, (RectTransform r, float v) => (!(r.GetComponent<CollapseFitter>() != null)) ? v : 0f);
			defaultWidth = width;
			if (GetComponentInParent<CollapseFitter>() != null)
			{
				width.min = 0f;
			}
			width.preferred = Mathf.Lerp(width.min, width.GetSize(rect, m_HorizontalFit, 0) + horizontalPadding, _expandRatio);
		}
		else if (base.horizontalFit == FitMode.PreferredSize)
		{
			width = rect.GetMaxLayoutDataOfChildren(0);
		}
	}

	public void CalculateLayoutInputVertical()
	{
		if (expandFrom.Axis() == 1)
		{
			height = rect.GetTotalLayoutDataOfChildrenSpecial(1, (RectTransform r, float v) => (!(r.GetComponent<CollapseFitter>() != null)) ? v : 0f);
			defaultHeight = height;
			if (GetComponentInParent<CollapseFitter>() != null)
			{
				height.min = 0f;
			}
			height.preferred = Mathf.Lerp(height.min, height.GetSize(rect, m_VerticalFit, 1) + verticalPadding, _expandRatio);
		}
		else if (base.verticalFit == FitMode.PreferredSize)
		{
			height = rect.GetMaxLayoutDataOfChildren(1);
		}
	}
}
