using Rewired.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper;

[AddComponentMenu("")]
[RequireComponent(typeof(Selectable))]
public class ScrollRectSelectableChild : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	public bool useCustomEdgePadding;

	public float customEdgePadding = 50f;

	private ScrollRect parentScrollRect;

	private Selectable _selectable;

	private RectTransform parentScrollRectContentTransform => parentScrollRect.content;

	private Selectable selectable => _selectable ?? (_selectable = GetComponent<Selectable>());

	private RectTransform rectTransform => base.transform as RectTransform;

	private void Start()
	{
		parentScrollRect = base.transform.GetComponentInParent<ScrollRect>();
		if (parentScrollRect == null)
		{
			UnityEngine.Debug.LogError("Rewired Control Mapper: No ScrollRect found! This component must be a child of a ScrollRect!");
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (!(parentScrollRect == null) && eventData is AxisEventData)
		{
			RectTransform rectTransform = parentScrollRect.transform as RectTransform;
			Rect child = MathTools.TransformRect(this.rectTransform.rect, this.rectTransform, rectTransform);
			Rect rect = rectTransform.rect;
			Rect rect2 = rectTransform.rect;
			float num = ((!useCustomEdgePadding) ? child.height : customEdgePadding);
			rect2.yMax -= num;
			rect2.yMin += num;
			if (!MathTools.RectContains(rect2, child) && MathTools.GetOffsetToContainRect(rect2, child, out var offset))
			{
				Vector2 anchoredPosition = parentScrollRectContentTransform.anchoredPosition;
				anchoredPosition.x = Mathf.Clamp(anchoredPosition.x + offset.x, 0f, Mathf.Abs(rect.width - parentScrollRectContentTransform.sizeDelta.x));
				anchoredPosition.y = Mathf.Clamp(anchoredPosition.y + offset.y, 0f, Mathf.Abs(rect.height - parentScrollRectContentTransform.sizeDelta.y));
				parentScrollRectContentTransform.anchoredPosition = anchoredPosition;
			}
		}
	}
}
