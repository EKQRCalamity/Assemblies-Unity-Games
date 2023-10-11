using UnityEngine;
using UnityEngine.EventSystems;

public class BubblePointerOver3D : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public GameObject bubbleTo;

	public void OnPointerEnter(PointerEventData eventData)
	{
		ExecuteEvents.Execute(bubbleTo, eventData, ExecuteEvents.pointerEnterHandler);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ExecuteEvents.Execute(bubbleTo, eventData, ExecuteEvents.pointerExitHandler);
	}
}
