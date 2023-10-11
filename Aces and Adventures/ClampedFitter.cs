using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class ClampedFitter : ContentSizeFitter, ILayoutElement
{
	public bool setPreferredWidthToMax;

	public bool setPreferredHeightToMax;

	public bool getMaxValuesFromParent;

	public bool setMinWidthToPreferred;

	public bool setMinHeightToPreferred;

	public float widthMin;

	public float widthMax;

	public float heightMin;

	public float heightMax;

	public PaddingSimple padding;

	public int priority;

	private RectTransform rect;

	private DrivenRectTransformTracker tracker;

	private LayoutData width;

	private bool widthClamped;

	private LayoutData height;

	private bool heightClamped;

	public float flexibleHeight => height.flexible;

	public float flexibleWidth => width.flexible;

	public int layoutPriority => priority;

	public float minHeight
	{
		get
		{
			if (!setMinHeightToPreferred)
			{
				return height.min;
			}
			return height.preferred;
		}
	}

	public float minWidth
	{
		get
		{
			if (!setMinWidthToPreferred)
			{
				return width.min;
			}
			return width.preferred;
		}
	}

	public float preferredHeight => height.preferred;

	public float preferredWidth => width.preferred;

	protected override void Awake()
	{
		base.Awake();
		rect = GetComponent<RectTransform>();
	}

	protected override void OnDisable()
	{
		tracker.Clear();
		base.OnDisable();
	}

	public override void SetLayoutHorizontal()
	{
		if (widthClamped)
		{
			tracker.Clear();
			tracker.Add(this, rect, DrivenTransformProperties.SizeDeltaX);
			tracker.Add(this, rect, DrivenTransformProperties.AnchoredPositionX);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width.GetSize(rect, base.horizontalFit, 0) + padding.horizontal);
		}
		base.SetLayoutHorizontal();
	}

	public override void SetLayoutVertical()
	{
		if (heightClamped)
		{
			tracker.Clear();
			tracker.Add(this, rect, DrivenTransformProperties.SizeDeltaY);
			tracker.Add(this, rect, DrivenTransformProperties.AnchoredPositionY);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height.GetSize(rect, base.verticalFit, 1) + padding.vertical);
		}
		base.SetLayoutVertical();
	}

	public void CalculateLayoutInputHorizontal()
	{
		if (getMaxValuesFromParent)
		{
			RectTransform rectTransform = base.transform.parent as RectTransform;
			if ((bool)rectTransform)
			{
				widthMax = rectTransform.rect.width;
				heightMax = rectTransform.rect.height;
			}
		}
		width = rect.GetTotalLayoutDataOfChildren(0);
		widthClamped = width.Clamp(widthMin, widthMax);
		if (setPreferredWidthToMax)
		{
			width.preferred = Mathf.Max(width.preferred, widthMax);
		}
		width.preferred += padding.horizontal;
	}

	public void CalculateLayoutInputVertical()
	{
		height = rect.GetTotalLayoutDataOfChildren(1);
		heightClamped = height.Clamp(heightMin, heightMax);
		if (setPreferredHeightToMax)
		{
			height.preferred = Mathf.Max(height.preferred, heightMax);
		}
		height.preferred += padding.vertical;
	}
}
