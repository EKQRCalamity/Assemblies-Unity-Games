using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragClone : MonoBehaviour
{
	protected RectTransform t;

	protected RectTransform dragPlane;

	protected Vector3? mouseOffset;

	protected bool matchRotationToDragOverRect;

	public void Init(GameObject draggedObject, GameObject visual, PointerEventData pointerEventData, float visualAlphaWhileDragging, float draggedVisualAlpha, Vector2 visualPreferredDimensionsMultiplier, bool centerDragOnDraggedObject, bool matchRotationToDragOverRect, bool destroyEventTriggerOnEndDrag)
	{
		RectTransform rectTransform = visual.transform as RectTransform;
		GameObject root = visual.GetRoot((GameObject p) => p.GetComponent<Canvas>() != null);
		if (root != null)
		{
			base.gameObject.transform.SetParent(root.transform, worldPositionStays: true);
			base.gameObject.transform.SetAsLastSibling();
		}
		LayoutElement layoutElement2 = base.gameObject.AddComponent<LayoutElement>();
		layoutElement2.minWidth = rectTransform.rect.width;
		layoutElement2.minHeight = rectTransform.rect.height;
		layoutElement2.ignoreLayout = true;
		float? existingCanvasGroupAlpha = null;
		bool addCanvasGroupToVisual = visualAlphaWhileDragging < 1f;
		if (addCanvasGroupToVisual)
		{
			CanvasGroup component = visual.GetComponent<CanvasGroup>();
			if ((bool)component)
			{
				existingCanvasGroupAlpha = component.alpha;
			}
			(component ? component : visual.AddComponent<CanvasGroup>()).alpha = visualAlphaWhileDragging;
		}
		LayoutElement layoutElement = null;
		if (visualPreferredDimensionsMultiplier != Vector2.one)
		{
			layoutElement = visual.AddComponent<LayoutElement>();
			layoutElement.preferredWidth = LayoutUtility.GetPreferredWidth(rectTransform) * visualPreferredDimensionsMultiplier.x;
			layoutElement.preferredHeight = LayoutUtility.GetPreferredHeight(rectTransform) * visualPreferredDimensionsMultiplier.y;
		}
		UIUtil.AddEventHandler<PointerEventData>(draggedObject, EventTriggerType.Drag, OnDrag);
		UIUtil.AddEventHandler<PointerEventData>(draggedObject, EventTriggerType.EndDrag, delegate
		{
			if ((bool)layoutElement)
			{
				Object.DestroyImmediate(layoutElement);
			}
			if (addCanvasGroupToVisual)
			{
				if (!existingCanvasGroupAlpha.HasValue)
				{
					Object.DestroyImmediate(visual.GetComponent<CanvasGroup>());
				}
				else
				{
					visual.GetComponent<CanvasGroup>().alpha = existingCanvasGroupAlpha.Value;
				}
			}
			if ((bool)draggedObject && destroyEventTriggerOnEndDrag)
			{
				Object.Destroy(draggedObject.GetComponent<EventTrigger>());
			}
			if ((bool)this)
			{
				Object.Destroy(base.gameObject);
			}
		});
		t = base.transform as RectTransform;
		t.pivot = new Vector2(0.5f, 0.5f);
		t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.width);
		t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rectTransform.rect.height);
		(from comp in GetComponentsInChildren(typeof(ILayoutSelfController), includeInactive: true)
			where !(comp is CollapseFitter)
			select comp).EffectAll(Object.DestroyImmediate);
		GetComponentsInChildren<EventTrigger>(includeInactive: true).EffectAll(delegate(EventTrigger e)
		{
			e.enabled = false;
		});
		GetComponentsInChildren<Selectable>(includeInactive: true).EffectAll(delegate(Selectable s)
		{
			s.enabled = false;
		});
		CanvasGroup orAddComponent = base.gameObject.GetOrAddComponent<CanvasGroup>();
		orAddComponent.blocksRaycasts = false;
		orAddComponent.alpha = draggedVisualAlpha;
		dragPlane = rectTransform;
		this.matchRotationToDragOverRect = matchRotationToDragOverRect;
		if (pointerEventData != null)
		{
			RectTransformUtility.ScreenPointToWorldPointInRectangle(dragPlane, pointerEventData.position, pointerEventData.pressEventCamera, out var worldPoint);
			mouseOffset = base.transform.position - worldPoint + (visual.transform.position - base.transform.position);
			if (centerDragOnDraggedObject)
			{
				mouseOffset += worldPoint - draggedObject.transform.position;
			}
		}
	}

	protected void OnDrag(PointerEventData data)
	{
		if (!this)
		{
			return;
		}
		if (data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
		{
			dragPlane = data.pointerEnter.transform as RectTransform;
		}
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(dragPlane, data.position, data.pressEventCamera, out var worldPoint))
		{
			t.position = worldPoint + (mouseOffset ?? Vector3.zero);
			if (matchRotationToDragOverRect)
			{
				t.rotation = dragPlane.rotation;
			}
		}
	}
}
