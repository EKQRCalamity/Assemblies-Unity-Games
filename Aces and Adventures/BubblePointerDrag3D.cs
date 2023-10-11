using UnityEngine;
using UnityEngine.EventSystems;

public class BubblePointerDrag3D : MonoBehaviour, IInitializePotentialDragHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[EnumFlags]
	public PointerInputButtonFlags buttonsToBubble = EnumUtil<PointerInputButtonFlags>.AllFlags;

	public PointerEventBubbleType bubbleType = PointerEventBubbleType.Hierarchy;

	private bool _ShouldBubble(PointerEventData eventData)
	{
		return EnumUtil.HasFlagConvert(buttonsToBubble, eventData.button);
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		if (_ShouldBubble(eventData))
		{
			bubbleType.BubblePointerDrag(this, eventData, ExecuteEvents.initializePotentialDrag);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (_ShouldBubble(eventData))
		{
			bubbleType.BubblePointerDrag(this, eventData, ExecuteEvents.beginDragHandler);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
	}

	public void OnEndDrag(PointerEventData eventData)
	{
	}
}
