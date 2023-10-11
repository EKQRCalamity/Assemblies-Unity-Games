using UnityEngine;
using UnityEngine.UI;

public class LayoutElementPostProcessor : MonoBehaviour, ILayoutElement, ILayoutIgnorer
{
	public Component baseLayoutElement;

	[Header("Maximum Dimensions")]
	public float maxWidth;

	public bool getMaxWidthFromParent;

	[HideInInspectorIf("_hideParentWidthMargin", false)]
	public float parentWidthMargin;

	[Space]
	public float maxHeight;

	public bool getMaxHeightFromParent;

	[HideInInspectorIf("_hideParentHeightMargin", false)]
	public float parentHeightMargin;

	public ILayoutElement layoutElement => baseLayoutElement as ILayoutElement;

	public float minWidth
	{
		get
		{
			if (layoutElement == null)
			{
				return 0f;
			}
			return layoutElement.minWidth;
		}
	}

	public float preferredWidth
	{
		get
		{
			if (layoutElement == null)
			{
				return 0f;
			}
			if (!(maxWidth > 0f))
			{
				return layoutElement.preferredWidth;
			}
			return Mathf.Min(maxWidth, layoutElement.preferredWidth);
		}
	}

	public float flexibleWidth
	{
		get
		{
			if (layoutElement == null)
			{
				return 0f;
			}
			return layoutElement.flexibleWidth;
		}
	}

	public float minHeight
	{
		get
		{
			if (layoutElement == null)
			{
				return 0f;
			}
			return layoutElement.minHeight;
		}
	}

	public float preferredHeight
	{
		get
		{
			if (layoutElement == null)
			{
				return 0f;
			}
			if (!(maxHeight > 0f))
			{
				return layoutElement.preferredHeight;
			}
			return Mathf.Min(maxHeight, layoutElement.preferredHeight);
		}
	}

	public float flexibleHeight
	{
		get
		{
			if (layoutElement == null)
			{
				return 0f;
			}
			return layoutElement.flexibleHeight;
		}
	}

	public int layoutPriority
	{
		get
		{
			if (layoutElement == null)
			{
				return int.MinValue;
			}
			return layoutElement.layoutPriority + 1;
		}
	}

	public bool ignoreLayout => layoutElement == null;

	public void CalculateLayoutInputHorizontal()
	{
		RectTransform rectTransform = base.transform.parent as RectTransform;
		if ((bool)rectTransform)
		{
			if (getMaxWidthFromParent)
			{
				maxWidth = rectTransform.rect.width - parentWidthMargin;
			}
			if (getMaxHeightFromParent)
			{
				maxHeight = rectTransform.rect.height - parentHeightMargin;
			}
		}
	}

	public void CalculateLayoutInputVertical()
	{
	}
}
