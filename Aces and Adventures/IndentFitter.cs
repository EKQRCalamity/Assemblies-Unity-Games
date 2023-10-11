using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[AddComponentMenu("Layout/Indent Fitter", 142)]
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class IndentFitter : ContentSizeFitter
{
	public RectTransform.Edge indentFrom;

	[SerializeField]
	[FormerlySerializedAs("indentAmountPixels")]
	private float _indentAmountPixels;

	[SerializeField]
	[FormerlySerializedAs("indentAmountRatio")]
	private float _indentAmountRatio;

	private RectTransform rect;

	private DrivenRectTransformTracker tracker;

	public float indentAmountPixels
	{
		get
		{
			return _indentAmountPixels;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _indentAmountPixels, value))
			{
				SetDirty();
			}
		}
	}

	public float indentAmountRatio
	{
		get
		{
			return _indentAmountRatio;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _indentAmountRatio, value))
			{
				SetDirty();
			}
		}
	}

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
		if (indentFrom == RectTransform.Edge.Left || indentFrom == RectTransform.Edge.Right)
		{
			RectTransform rectTransform = (RectTransform)rect.parent;
			if ((bool)rectTransform && rect.rect.height > 0f)
			{
				tracker.Clear();
				tracker.Add(this, rect, DrivenTransformProperties.AnchoredPositionX);
				tracker.Add(this, rect, DrivenTransformProperties.SizeDeltaX);
				float num = indentAmountPixels + indentAmountRatio * rect.rect.width;
				rect.SetInsetAndSizeFromParentEdge(indentFrom, num, rectTransform.rect.width - num);
			}
		}
		else
		{
			base.SetLayoutHorizontal();
		}
	}

	public override void SetLayoutVertical()
	{
		if (indentFrom == RectTransform.Edge.Bottom || indentFrom == RectTransform.Edge.Top)
		{
			RectTransform rectTransform = (RectTransform)rect.parent;
			if ((bool)rectTransform && rect.rect.width > 0f)
			{
				tracker.Clear();
				tracker.Add(this, rect, DrivenTransformProperties.AnchoredPositionY);
				tracker.Add(this, rect, DrivenTransformProperties.SizeDeltaY);
				float num = indentAmountPixels + indentAmountRatio * rect.rect.height;
				rect.SetInsetAndSizeFromParentEdge(indentFrom, num, rectTransform.rect.height - num);
			}
		}
		else
		{
			base.SetLayoutVertical();
		}
	}

	public void SetIndentRatio(float indentRatio)
	{
		indentAmountRatio = indentRatio;
	}
}
