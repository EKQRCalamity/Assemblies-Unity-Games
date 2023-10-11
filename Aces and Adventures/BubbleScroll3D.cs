using UnityEngine;
using UnityEngine.EventSystems;

public class BubbleScroll3D : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	public PointerEventBubbleType bubbleType = PointerEventBubbleType.Hierarchy;

	public void OnScroll(PointerEventData eventData)
	{
		bubbleType.Bubble(this, eventData, ExecuteEvents.scrollHandler);
	}
}
